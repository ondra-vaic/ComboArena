using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerAnimator : NetworkBehaviour
    {
        [SerializeField] private float toIdleTransitionTime = 0.25f;
        [SerializeField] private float toRunningTransitionTime = 0.25f;
        [SerializeField] private float toCastingTransitionTime = 0.1f;
    
        private Animator animator;
    
        private static readonly int SpellAnimationSpeedHash = Animator.StringToHash("SpellAnimationSpeed");
        private static readonly int IdlyingAnimationHash = Animator.StringToHash("Idlying");
        private static readonly int RunningAnimationHash = Animator.StringToHash("Running");

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>();
        }

        [ClientRpc]
        public void PlayAnimationClientRpc(int animationNameHash, float castMultiplier)
        {
            AnimatorStateInfo animatorStateInfo = getCurrentAnimatorStateInfo();
        
            int currentStateHash = animatorStateInfo.shortNameHash;
            bool isCurrentStateLooping = animatorStateInfo.loop;
        
            if (animationNameHash == currentStateHash) 
            {
                if(isCurrentStateLooping) return;
            }

            float transitionTime = getTransitionTime(animationNameHash);
            animator.SetFloat(SpellAnimationSpeedHash, castMultiplier);
            animator.CrossFadeInFixedTime(animationNameHash, transitionTime / castMultiplier);
        }

        private AnimatorStateInfo getCurrentAnimatorStateInfo()
        {
            AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(0);
            if (nextStateInfo.shortNameHash != 0) return nextStateInfo;
        
            return animator.GetCurrentAnimatorStateInfo(0);
        }
    
        private float getTransitionTime(int animationHash)
        {
            if (animationHash == IdlyingAnimationHash) return toIdleTransitionTime;
            if (animationHash == RunningAnimationHash) return toRunningTransitionTime;
            return toCastingTransitionTime;
        }
    }
}
