using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class WelcomeMenu : MonoBehaviour
    {
        [SerializeField] private Button findMatchButton;
        [SerializeField] private Button soloPracticeButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject menuHolder;
    
        void Awake()
        {
            MatchMaker matchMaker = FindObjectOfType<MatchMaker>();
            findMatchButton.onClick.AddListener(matchMaker.FindMatch);
            findMatchButton.onClick.AddListener(() => menuHolder.SetActive(false));
            
            soloPracticeButton.onClick.AddListener(matchMaker.SetSoloPractice);
            soloPracticeButton.onClick.AddListener(matchMaker.FindMatch);
            soloPracticeButton.onClick.AddListener(() => menuHolder.SetActive(false));
            
            exitButton.onClick.AddListener(Application.Quit);
            
            menuHolder.SetActive(true);
        }
    }
}
