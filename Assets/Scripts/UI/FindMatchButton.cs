using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FindMatchButton : MonoBehaviour
    {
        [SerializeField] private GameObject parentMenu;
        private MatchMaker matchMaker;
        private FadeOutPanel panel;
        private Button button;
        
        void Awake()
        {
            matchMaker = FindObjectOfType<MatchMaker>();
            button = GetComponent<Button>();
            button.onClick.AddListener(matchMaker.FindMatch);
            button.onClick.AddListener(() => parentMenu.SetActive(false));
        }
    }
}
