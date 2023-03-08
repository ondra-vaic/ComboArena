using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EndGameMenu : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject winText;
        [SerializeField] private GameObject looseTest;
        
        [SerializeField] private Button exitButton;
        [SerializeField] private Button rematchButton;

        private void Awake()
        {
            PlayerManager playerManager = FindObjectOfType<PlayerManager>();
            playerManager.OnGameEnded.AddListener(showUI);
            exitButton.onClick.AddListener(Application.Quit);
            rematchButton.onClick.AddListener(playerManager.RequestRematchServerRpc);
            rematchButton.onClick.AddListener(() => root.SetActive(false));
        }

        private void showUI(bool isWin)
        {
            root.SetActive(true);
            winText.SetActive(isWin);
            looseTest.SetActive(!isWin);
        }
    }
}
