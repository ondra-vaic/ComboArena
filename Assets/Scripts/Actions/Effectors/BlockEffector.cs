using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.VFX;

namespace Actions.Effectors
{
    public class BlockEffector : ActionEffector
    {
        [SerializeField] private float yOffset = 0.5f;

        public override void PlaceEffector(Team team, PlayerCastState playerCastState, float castMultiplier, float playerMovementDistance)
        {
            base.PlaceEffector(team, playerCastState, castMultiplier, playerMovementDistance);
            
            transform.position = playerCastState.Player.transform.position + Vector3.up * yOffset;
            transform.localRotation = Quaternion.LookRotation(playerCastState.StartDirectionForward, Vector3.up);
        }
        
        protected override void Update()
        {
            base.Update();
            if (IsServer)
            {
                transform.position = playerCastState.Player.transform.position + Vector3.up * yOffset;   
                transform.localRotation = Quaternion.LookRotation(playerCastState.Player.MeshInfo.VisualRoot.forward, Vector3.up);
            }
        }
        
        protected override void OnTriggerEnter(Collider other)
        {
            if(!IsServer) return;
            
            ActionEffector actionEffector = other.GetComponent<ActionEffector>();
            if (actionEffector == null || actionEffector.GetTeam() == GetTeam()) return;
            
            actionEffector.Break();
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
