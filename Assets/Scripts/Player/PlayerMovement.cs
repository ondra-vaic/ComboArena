using Actions;
using Actions.Movement;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [FormerlySerializedAs("rotationSpeed")] [SerializeField] private float transitionRotationSpeed = 1000f;
        [FormerlySerializedAs("rotationSpeed")] [SerializeField] private float transitionRotationDuration = 0.5f;
        
        private ICastable currentCastable;
        private float currentTransitionRotationSpeed;
        private Player player;
        private Transform visualTransform;
        
        private void Awake()
        {
            player = GetComponent<Player>();
            visualTransform = player.MeshInfo.VisualRoot;
        }

        void Update()
        {
            if(currentCastable?.GetLeafMovementData() == null) return;
        
            updateMove();
            updateRotation();
        }

        private void updateRotation()
        {
            MovementData movementData = currentCastable.GetLeafMovementData();
            
            if(movementData == null) return;

            if (currentTransitionRotationSpeed > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(currentCastable.GetLeafMovementData().GetDirection());
                visualTransform.rotation = Quaternion.RotateTowards(visualTransform.rotation, targetRotation, currentTransitionRotationSpeed * Time.deltaTime);
                currentTransitionRotationSpeed -= Time.deltaTime * transitionRotationSpeed / transitionRotationDuration;
                currentTransitionRotationSpeed = Mathf.Max(0, currentTransitionRotationSpeed);
                return;
            }

            if (movementData.GetAffectsRotation())
            {
                Quaternion targetRotation = Quaternion.LookRotation(player.CalculateCurrentCastState().StartDirectionForward);
                visualTransform.rotation = Quaternion.RotateTowards(visualTransform.rotation, targetRotation, (movementData.GetRotationSpeed()) * Time.deltaTime);
            }
        }

        private void updateMove()
        {
            MovementData movementData = currentCastable.GetLeafMovementData();
            if(movementData == null || !movementData.GetAffectsPosition()) return;
            if(movementData.IsDone()) return;
            
            transform.position += movementData.GetDirection() *
                                  movementData.GetCurrentSpeedModifier() *
                                  movementData.GetSpeed() *
                                  Time.deltaTime;
        }

        public void SetCurrentCastable(ICastable castable)
        {
            currentCastable = castable;
            
            if(currentCastable == null) return;
            if(currentCastable.GetLeafMovementData() == null) return;
            if(!currentCastable.GetLeafMovementData().GetAffectsRotation()) return;
            
            currentTransitionRotationSpeed = transitionRotationSpeed;
        }
    }
}
