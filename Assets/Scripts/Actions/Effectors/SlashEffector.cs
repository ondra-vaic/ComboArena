using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Actions.Effectors
{
    public class SlashEffector : ActionEffector
    {
        [SerializeField] protected Vector3 startOffset;
        [SerializeField] protected Vector3 rotation;

        public override void PlaceEffector(Team team, PlayerCastState playerCastState, float castMultiplier, float playerMovementDistance)
        {
            base.PlaceEffector(team, playerCastState, castMultiplier, playerMovementDistance);

            Quaternion playerTargetRotation = Quaternion.LookRotation(playerCastState.Player.MeshInfo.VisualRoot.forward, Vector3.up);
            transform.localRotation = playerTargetRotation * Quaternion.Euler(rotation);
            transform.localPosition = playerCastState.Player.transform.position + playerTargetRotation * startOffset;
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
