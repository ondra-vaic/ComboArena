using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Actions.Effectors
{
    public class DashEffector : ActionEffector
    {
        [SerializeField] private float backOffset;

        public override void PlaceEffector(Team team, PlayerCastState playerCastState, float castMultiplier, float playerMovementDistance)
        {
            base.PlaceEffector(team, playerCastState, castMultiplier, playerMovementDistance);
            
            transform.localPosition = playerCastState.Player.transform.position - playerCastState.StartDirectionForward * backOffset;
            transform.localRotation = Quaternion.LookRotation(playerCastState.StartDirectionForward, Vector3.up);
        }
        
        [ClientRpc]
        public override void StartVFXClientRPC(float castMultiplier, float playerMovementDistance)
        {
            VisualEffect vfxGraph = GetComponent<VisualEffect>();
            vfxGraph.SetFloat("Duration", timeToDestroy / castMultiplier);
            vfxGraph.SetFloat("Distance", playerMovementDistance);
            vfxGraph.SetFloat("Seed", Random.Range(0f, 100));
            vfxGraph.Play();
        }
    }
}
