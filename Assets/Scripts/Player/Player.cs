using System;
using System.Collections.Generic;
using Actions;
using Actions.Effectors;
using Camera;
using Networking;
using RotaryHeart.Lib.SerializableDictionary;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.TouchPhase;

namespace Player
{
    public enum PlayerState
    {
        Idling,
        Running,
        Dying,
        Casting,
        Blocked
    }

    public enum Team
    {
        None,
        Blue,
        Red
    }
    
    [Serializable] public class KeyToCastDictionary : SerializableDictionaryBase<KeyType, KeyCast> {}

    public class Player : NetworkBehaviour
    {
        [SerializeField] private KeyToCastDictionary castInputActions;
        [SerializeField] private LayerMask GroundLayer;
        [SerializeField] private CastActionGroup idleCastAction;
        [SerializeField] private int maxHealth;

        public PlayerMeshInfo MeshInfo => meshInfo;
        public PlayerMovement Movement => movement;
        public int MaxHealth => maxHealth;
        public NetworkVariable<int> Health => health;
        public UnityEvent<KeyType, float> OnCast = new();

        private UnityEngine.Camera mainCamera;
        private CameraController cameraController;
        private PlayerMovement movement;
        private PlayerAnimator animator;
        private PlayerMeshInfo meshInfo;
        private PlayerManager playerManager;
        private Libraries libraries;

        private PlayerCastState currentPlayerCastState;
        private readonly NetworkVariable<Team> team = new(Team.None);
        private readonly NetworkVariable<int> health = new();

        private readonly CastActionGroup mainActionGroup = new(
            new List<CastActionGroup>(),
            null,
            null,
            1);

        private Vector2 lastTouchPosition = Vector2.zero;

        private void Awake()
        {
            animator = GetComponent<PlayerAnimator>();
            movement = GetComponent<PlayerMovement>();
            meshInfo = GetComponent<PlayerMeshInfo>();
            mainCamera = FindObjectOfType<UnityEngine.Camera>();
            playerManager = FindObjectOfType<PlayerManager>();
            cameraController = mainCamera.GetComponent<CameraController>();
            libraries = FindObjectOfType<Libraries>();
            health.Value = maxHealth;
            currentPlayerCastState = new PlayerCastState(this, Vector3.zero);
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                Destroy(GetComponent<PlayerInput>());
            }

            playerManager.AddPlayer(OwnerClientId, this);

            if (IsOwner)
            {
                playerManager.OnAllPlayersInitialized.AddListener(() =>
                    cameraController.SetTarget(transform, MeshInfo.VisualRoot));
                playerManager.OnRematchSessionStart.AddListener(() => cameraController.SetTarget(null, null));
                team.OnValueChanged += (_, _) => OnMyTeamSet();
            }

            if (IsServer)
            {
                CastActionGroup running = new(
                    new List<CastActionGroup>(),
                    libraries.ActionsLibrary.GetActionPrototype("Running"),
                    null,
                    1);

                castInputActions[KeyType.MOUSE_R].SetCastActionGroup(running);
                playerManager.SetTeam(this);
            }
        }

        public override void OnNetworkDespawn()
        {
            playerManager.RemovePlayer(OwnerClientId);
        }

        private void Update()
        {
            if (IsClient && IsOwner)
            {
                CalculateCurrentCastState();
                var playerCastStateNetworkData = CalculateCurrentCastState().ToNetworkData(OwnerClientId);
                SendCurrentPlayerCastStateServerRpc(playerCastStateNetworkData);
            }

            if (IsServer)
            {
                tryChangeAction();
                spawnEffectors();
            }

            if (IsOwner)
            {
#if UNITY_ANDROID
                updateTouchPosition();
#endif   
            }
        }

        void updateTouchPosition()
        {
            if (Input.touchCount > 0)
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    Touch touch = Input.GetTouch(0);
                    lastTouchPosition = touch.position;
                    tryEnqueue(KeyType.MOUSE_R, true);
                }
            }
        }

        [ServerRpc]
        private void SendCurrentPlayerCastStateServerRpc(PlayerCastStateNetworkData playerCastStateNetworkData)
        {
            currentPlayerCastState = playerCastStateNetworkData.ToPlayerCastState(this);
        }

        private void tryChangeAction()
        {
            if (mainActionGroup.IsDone())
            {
                mainActionGroup.Enqueue((CastActionGroup)idleCastAction.Clone(mainActionGroup));
            }

            bool castChanged = mainActionGroup.TryChangeCast(currentPlayerCastState);
            if (castChanged)
            {
                startAction(mainActionGroup.GetCurrentLeaf());
            }
        }

        private void spawnEffectors()
        {
            PlayerCastState playerCastState = mainActionGroup.GetLeafPlayerCastState();
            float castMultiplier = mainActionGroup.GetLeafCastMultiplier();
            float distance = mainActionGroup.GetLeafMovementData().GetMaxDistanceBlockingDistance();

            Queue<ActionEffectorPrototype> actionEffectors = mainActionGroup.DequeueLeafEffectors() ?? new();
            foreach (var actionEffector in actionEffectors)
            {
                ActionEffector effector = Instantiate(actionEffector.GetPrefab(), Vector3.zero, Quaternion.identity)
                    .GetComponent<ActionEffector>();
                effector.PlaceEffector(team.Value, playerCastState, castMultiplier, distance);
                effector.GetComponent<NetworkObject>().Spawn();
                effector.StartVFXClientRPC(castMultiplier, distance);
            }
        }

        private void startAction(ICastable castable)
        {
            animator.PlayAnimationClientRpc(castable.GetLeafAnimationNameHash(), castable.GetLeafCastMultiplier());
            movement.SetCurrentCastable(castable);
        }

        [ServerRpc]
        private void tryEnqueueActionServerRpc(KeyType keyType)
        {
            if (!playerManager.IsBattleOn) return;

            KeyCast keyCast = castInputActions[keyType];
            bool canEnqueueNext = mainActionGroup.CanEnqueueNext(keyCast.CastPrototype.GetLeafPriority());

            if (keyCast.CanCast() && canEnqueueNext)
            {
                CastActionGroup castable = keyCast.Cast(mainActionGroup);
                mainActionGroup.Enqueue(castable);

                ClientRpcParams rpcParams = new ClientRpcParams();
                rpcParams.Send.TargetClientIds = new[] { OwnerClientId };
                TriggerOnCastClientRpc(keyType, keyCast.CoolDown, rpcParams);
            }
        }

        [ClientRpc]
        private void TriggerOnCastClientRpc(KeyType keyType, float keyCastCoolDown, ClientRpcParams rpcParams = default)
        {
            OnCast.Invoke(keyType, keyCastCoolDown);
        }

        private void tryEnqueue(KeyType keyType, bool performed)
        {
            if (!performed) return;
            tryEnqueueActionServerRpc(keyType);
        }

        public void SetStartTransform(Vector3 startPoint, Vector3 startForward)
        {
            transform.position = startPoint;
            transform.forward = startForward;
        }

        public void TakeDamage(int damage)
        {
            int damageTaken = Mathf.Min(damage, health.Value);
            health.Value -= damageTaken;
            if (health.Value <= 0)
            {
                playerManager.OnPlayerDeath(this);
            }
        }

        public void SetActionGroup(KeyType key, ActionGroupNetworkData castActionGroupNetworkData)
        {
            CastActionGroup castActionGroup =
                castActionGroupNetworkData.CreateCastActionGroup(libraries.ActionsLibrary, null);
            castInputActions[key].SetCastActionGroup(castActionGroup);
        }

        public void ResetHealth()
        {
            health.Value = maxHealth;
        }

        public void OnMyTeamSet() => playerManager.SetClientId(OwnerClientId);
        public void SetTeam(Team team) => this.team.Value = team;
        public Team GetTeam() => team.Value;

        public PlayerCastState CalculateCurrentCastState() => new(this, getMousePosition());

        private Vector3 getMousePosition()
        {
            Vector2 mouseScreenPosition = Vector2.zero;

#if UNITY_ANDROID
            mouseScreenPosition = lastTouchPosition;
#else
            mouseScreenPosition = Mouse.current.position.ReadValue();
#endif
            Ray ray = mainCamera.ScreenPointToRay(mouseScreenPosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, GroundLayer.value))
            {
                return hit.point;
            }

            return transform.position;
        }

        public void HandleMoveAction(InputAction.CallbackContext value) => tryEnqueue(KeyType.MOUSE_R, value.performed);

        public void HandleQAction(InputAction.CallbackContext value) => tryEnqueue(KeyType.Q, value.performed);

        public void HandleWAction(InputAction.CallbackContext value) => tryEnqueue(KeyType.W, value.performed);

        public void HandleEAction(InputAction.CallbackContext value) => tryEnqueue(KeyType.E, value.performed);

        public void HandleRAction(InputAction.CallbackContext value) => tryEnqueue(KeyType.R, value.performed);

        public void HandleSpaceAction(InputAction.CallbackContext value) => cameraController.HandleLockInput(value);

        public ICastable GetCurrentCastable() => mainActionGroup.GetCurrentLeaf();

        public void TryCast(KeyType key)
        {
            tryEnqueue(key, true);
        }
    }
}