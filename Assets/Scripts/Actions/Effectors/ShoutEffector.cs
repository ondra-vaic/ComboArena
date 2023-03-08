using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Actions.Effectors
{
    public class ShoutEffector : ActionEffector
    {
        [SerializeField] private float upOffset;
        [SerializeField] private float range;
        [SerializeField] private AnimationCurve ballPositionAnimationCurve;
        
        private Vector3 targetPosition;
        private Vector3 startPosition;
        
        protected override void Update()
        {
            base.Update();
            if (IsServer)
            {
                float progress = (Time.time - spawnTime) / (timeToDestroy / castMultiplier);
                transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, ballPositionAnimationCurve.Evaluate(progress));
            }
        }

        public override void PlaceEffector(Team team, PlayerCastState playerCastState, float castMultiplier, float playerMovementDistance)
        {
            base.PlaceEffector(team, playerCastState, castMultiplier, playerMovementDistance);
            
            this.castMultiplier = castMultiplier;
            startPosition = playerCastState.Player.transform.position + Vector3.up * upOffset;
            range /= castMultiplier;
            targetPosition = startPosition + playerCastState.StartDirectionForward * range;
            spawnTime = Time.time;
            
            transform.position = startPosition;
            transform.localRotation = Quaternion.LookRotation(playerCastState.StartDirectionForward, Vector3.up);
        }
        
        [ClientRpc]
        public override void StartVFXClientRPC(float castMultiplier, float playerMovementDistance)
        {
            VisualEffect vfxGraph = GetComponent<VisualEffect>();
            vfxGraph.SetFloat("Duration", timeToDestroy / castMultiplier);
            vfxGraph.SetFloat("CastMultiplier", castMultiplier);
            vfxGraph.Play();
        }
    }
}
