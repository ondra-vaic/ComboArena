using Networking;
using UnityEngine;

namespace UI
{
    public class WaitingMenu : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject text;
        
        private void Awake()
        {
            PlayerManager playerManager = FindObjectOfType<PlayerManager>();
            playerManager.OnLocalPlayerClientIdSet.AddListener(() => root.SetActive(false));
            playerManager.OnRematchSessionStart.AddListener(() => root.SetActive(false));
            playerManager.OnAllPlayersInitialized.AddListener(() => root.SetActive(false));
            playerManager.OnLocalPlayerInitialized.AddListener(() => root.SetActive(true));
        }
    }
}
