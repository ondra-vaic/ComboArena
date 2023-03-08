using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Actions.Effectors
{
    public class JumpUpEffector : ActionEffector
    {
        [SerializeField] private float forwardEndOffset;
        [SerializeField] private AnimationCurve positionAnimationCurve;

        [SerializeField] private float timeToStartFollowing;
        [SerializeField] private float followSpeed;
        
        private Vector3 startPosition;
        
        protected override void Update()
        {
            base.Update();
            if (IsServer)
            {
                if (Time.time - spawnTime < timeToStartFollowing / castMultiplier)
                {
                    
                    Vector3 targetPosition = startPosition + Vector3.up * 0.05f + playerCastState.Player.MeshInfo.VisualRoot.forward * (playerMovementDistance + forwardEndOffset);
                    
                    float progress = (Time.time - spawnTime) / (timeToDestroy / castMultiplier);
                    transform.position = Vector3.LerpUnclamped(startPosition, targetPosition, positionAnimationCurve.Evaluate(progress));
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, playerCastState.Player.transform.position + Vector3.up * 0.05f, followSpeed * Time.deltaTime);   
                }
            }
        }
        
        public override void PlaceEffector(Team team, PlayerCastState playerCastState, float castMultiplier, float playerMovementDistance)
        {
            base.PlaceEffector(team, playerCastState, castMultiplier, playerMovementDistance);
            
            this.castMultiplier = castMultiplier;

            Vector3 playerPosition = playerCastState.Player.transform.position;
            startPosition = playerPosition + Vector3.up * 0.05f;

            transform.position = startPosition;
        }
        
        [ClientRpc]
        public override void StartVFXClientRPC(float castMultiplier, float playerMovementDistance)
        {
            VisualEffect vfxGraph = GetComponent<VisualEffect>();
            vfxGraph.SetFloat("Duration", timeToDestroy / castMultiplier);
            vfxGraph.Play();
        }
    }
}
