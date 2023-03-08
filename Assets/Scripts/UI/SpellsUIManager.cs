using System;
using Actions;
using Networking;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    
    [Serializable] public class KeyToCastDictionary : SerializableDictionaryBase<KeyType, SpellUI> {}
    
    public class SpellsUIManager : MonoBehaviour
    {
        [SerializeField] private float coolDownMaskWidth = 100;
        [SerializeField] private KeyToCastDictionary keyToCastDictionary;
        
        private PlayerManager playerManager;
        
        private void Awake()
        {
            playerManager = FindObjectOfType<PlayerManager>();
            playerManager.OnAllPlayersInitialized.AddListener(onLocalPlayerInitialized);
            playerManager.OnRematchSessionStart.AddListener(() => showUI(false));

            foreach (var keySpellUIPair in keyToCastDictionary)
            {
                keySpellUIPair.Value.SetWidth(coolDownMaskWidth);
            }
        }

        private void onLocalPlayerInitialized()
        {
            Player.Player player = playerManager.GetMyPlayer();
            player.OnCast.AddListener(onCast);
            showUI(true);
            
#if UNITY_ANDROID
            foreach (var keySpellUIPair in keyToCastDictionary)
            {
                Button keyButton = keySpellUIPair.Value.GetComponent<Button>();
                keyButton.enabled = true;
                keyButton.onClick.AddListener(() => player.TryCast(keySpellUIPair.Key));
                keyButton.GetComponent<Image>().raycastTarget = true;
            }
#endif
        }

        private void showUI(bool show)
        {
            foreach (var keySpellUIPair in keyToCastDictionary)
            {
                keySpellUIPair.Value.gameObject.SetActive(show);
            }
        }

        private void onCast(KeyType keyType, float coolDown)
        {
            if (keyToCastDictionary.ContainsKey(keyType))
            {
                keyToCastDictionary[keyType].onCast(coolDown);
            }
        }
    }
}
