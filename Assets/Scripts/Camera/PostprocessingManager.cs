using Networking;
using UnityEngine;
using UnityEngine.Rendering;

namespace Camera
{
    public class PostprocessingManager : MonoBehaviour
    {
        private Animator animator;
        private PlayerManager playerManager;
        
        private static readonly int HitTriggerHash = Animator.StringToHash("Hit");

        void Awake()
        {
            animator = GetComponent<Animator>();
            playerManager = FindObjectOfType<PlayerManager>();
            playerManager.OnLocalPlayerInitialized.AddListener(registerHitEvent);
        }

        private void registerHitEvent()
        {
            Player.Player player = playerManager.GetMyPlayer();
            player.Health.OnValueChanged += onHealthChanged;
        }
        
        private void onHealthChanged(int oldHealth, int newHealth)
        {
            if (oldHealth > newHealth)
            {
                animator.SetTrigger(HitTriggerHash);
            }
        }
    }
}
