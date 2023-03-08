using Networking;
using UnityEngine;

namespace UI
{
    public class FadeOutPanel : MonoBehaviour
    {
        private Animator animator;
        private PlayerManager playerManager;
        private MatchMaker matchMaker;

        private static readonly int FadeOutTriggerHash = Animator.StringToHash("FadeOut");
        private static readonly int FadeInTriggerHash = Animator.StringToHash("FadeIn");

        void Awake()
        {
            animator = GetComponent<Animator>();
            playerManager = FindObjectOfType<PlayerManager>();
            matchMaker = FindObjectOfType<MatchMaker>();
            
            matchMaker.OnMatchmakingStarted.AddListener(FadeOut);
            playerManager.OnLocalPlayerClientIdSet.AddListener(FadeIn);
        }

        public void FadeIn()
        {
            animator.SetTrigger(FadeInTriggerHash);
        }

        public void FadeOut()
        {
            animator.SetTrigger(FadeOutTriggerHash);
        }
    }
}
