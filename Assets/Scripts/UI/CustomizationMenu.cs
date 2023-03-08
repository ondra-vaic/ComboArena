using System.Collections.Generic;
using System.Linq;
using Actions;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CustomizationMenu : MonoBehaviour
    {
        [SerializeField] private int maxMultiplier = 3;
        [SerializeField] private SpellDefinitionGroupUI spellDefinitionGroupUIPrefab;
        [SerializeField] private SpellCastActionUI spellCastActionUIPrefab;

        [SerializeField] private Transform spellCastActionUIParent;
        [SerializeField] private Button splitButton;
        [SerializeField] private Button saveAndPlayButton;
        [SerializeField] private ToggleGroup splitsToggleGroup;
        [SerializeField] private GameObject menuRoot;
        
        private Libraries libraries;
        private PlayerManager playerManager;
        private Toggle[] splitToggles;
        
        private SpellDefinitionGroupUI selectedSpellDefinitionGroup;
        
        private readonly Dictionary<KeyType, SpellComboUI> keyToSpellComboUIDictionary = new();
        
        private void Awake()
        {
            menuRoot.SetActive(true);
            
            playerManager = FindObjectOfType<PlayerManager>();
            libraries = FindObjectOfType<Libraries>();
            splitToggles = splitsToggleGroup.GetComponentsInChildren<Toggle>();

            playerManager.OnRematchSessionStart.AddListener(() => menuRoot.SetActive(true));
            playerManager.OnLocalPlayerClientIdSet.AddListener(() => menuRoot.SetActive(true));
            splitButton.onClick.AddListener(SplitSelected);
            saveAndPlayButton.onClick.AddListener(SaveAndPlay);
            
            initializeSpellComboUIDictionary();
            setFirstSpellComboSelected();
            enableSpellDefinitionSelection(false);
            initializeCastActionsScroll();
            
            menuRoot.SetActive(false);
        }

        private void Start()
        {
            menuRoot.SetActive(true);
// #if UNITY_EDITOR
//             foreach (var foo in keyToSpellComboUIDictionary)
//             {
//                 foo.Value.GetComponentInChildren<SpellDefinitionGroupUI>().SetSpellAction(libraries.ActionsLibrary.ActionsList.Last());
//             }
// #endif
             menuRoot.SetActive(false);
        }

        private void setFirstSpellComboSelected()
        {
            OnSpellDefinitionSelected(keyToSpellComboUIDictionary[KeyType.Q].GetComponentInChildren<SpellDefinitionGroupUI>());
        }
        
        private void initializeSpellComboUIDictionary()
        {
            SpellComboUI[] spellComboUIs = GetComponentsInChildren<SpellComboUI>();

            foreach (var spellComboUI in spellComboUIs)
            {
                keyToSpellComboUIDictionary.Add(spellComboUI.KeyType, spellComboUI);
            }
        }

        private void initializeCastActionsScroll()
        {
            foreach (var action in libraries.ActionsLibrary.ActionsList)
            {
                if(!action.ShowInUI) continue;
                
                Instantiate(spellCastActionUIPrefab, spellCastActionUIParent).Initialize(action);
            }
        }

        public void OnSpellDefinitionSelected(SpellDefinitionGroupUI spellDefinitionGroup)
        {
            selectedSpellDefinitionGroup?.SetSelectedView(false);
            selectedSpellDefinitionGroup = spellDefinitionGroup;
            selectedSpellDefinitionGroup.SetSelectedView(true);
            enableSpellDefinitionSelection(false);
        }
        
        public void OnSpellCastActionSelected(CastActionPrototype castActionPrototype)
        {
            if(selectedSpellDefinitionGroup == null) return;
            
            selectedSpellDefinitionGroup.SetSpellAction(castActionPrototype);
            selectedSpellDefinitionGroup = null;
            enableSpellDefinitionSelection(true);
        }

        public void SplitSelected()
        {
            int numSplits = int.Parse(splitsToggleGroup.GetFirstActiveToggle().GetComponentInChildren<TextMeshProUGUI>().text);
            enableSpellDefinitionSelection(true);
            selectedSpellDefinitionGroup.Split(spellDefinitionGroupUIPrefab, numSplits);
            selectedSpellDefinitionGroup = null;
        }

        public void Unselect()
        {
            selectedSpellDefinitionGroup.SetSelectedView(false);
            selectedSpellDefinitionGroup = null;
            enableSpellDefinitionSelection(true);
        }

        private void enableSpellDefinitionSelection(bool enable)
        {
            splitButton.interactable = !enable && selectedSpellDefinitionGroup != null && selectedSpellDefinitionGroup.GetMultiplier() < maxMultiplier;
            foreach (var toggle in splitToggles)
            {
                toggle.interactable = splitButton.interactable;
            }

            UpdateSaveAndPlayInteractability();
        }

        private bool areAllSpellCombosValid()
        {
            foreach (var keySpellComboPair in keyToSpellComboUIDictionary)
            {
                if (!keySpellComboPair.Value.IsValid()) return false;
            }

            return true;
        }

        public void UpdateSaveAndPlayInteractability() => saveAndPlayButton.interactable = areAllSpellCombosValid();

        public void SaveAndPlay()
        {
            foreach (var keySpellComboPair in keyToSpellComboUIDictionary)
            {
                ActionGroupNetworkData actionGroupNetworkData = keySpellComboPair.Value.CreateActionGroupNetworkData();
                playerManager.SetPlayerActionGroup(keySpellComboPair.Key, actionGroupNetworkData);
            }

            menuRoot.SetActive(false);
            playerManager.SetLocalPlayerInitialized();
        }
    }
}
