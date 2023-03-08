using System;
using Actions;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SpellComboUI : MonoBehaviour
    {
        [SerializeField] private Button resetButton;
        [SerializeField] private SpellDefinitionGroupUI spellDefinitionGroupUIPrefab;
        [SerializeField] private KeyType keyType;

        private CustomizationMenu customizationMenu;
        private SpellDefinitionGroupUI rootSpellDefinitionGroup;
        
        public KeyType KeyType => keyType;

        private void Awake()
        {
            customizationMenu = GetComponentInParent<CustomizationMenu>();
            resetButton.onClick.AddListener(ResetCombo);
            rootSpellDefinitionGroup = GetComponentInChildren<SpellDefinitionGroupUI>();
        }

        public void ResetCombo()
        {
            Transform parent = rootSpellDefinitionGroup.transform.parent;
            Destroy(rootSpellDefinitionGroup.gameObject);
            rootSpellDefinitionGroup = Instantiate(spellDefinitionGroupUIPrefab, parent);
            customizationMenu.UpdateSaveAndPlayInteractability();
            customizationMenu.OnSpellDefinitionSelected(rootSpellDefinitionGroup);
        }

        public ActionGroupNetworkData CreateActionGroupNetworkData() => rootSpellDefinitionGroup.CreateActionGroupNetworkData();

        public bool IsValid()
        {
            SpellDefinitionGroupUI[] spellDefinitionGroupUIs = GetComponentsInChildren<SpellDefinitionGroupUI>();
            foreach (var spellDefinitionGroupUI in spellDefinitionGroupUIs)
            {
                SpellDefinitionGroupUI[] spellDefinitionGroupUIChildren = spellDefinitionGroupUI.GetComponentsInChildren<SpellDefinitionGroupUI>();
                SpellCastActionUI childSpellCastActionUI = spellDefinitionGroupUI.GetComponentInChildren<SpellCastActionUI>();
                
                if (spellDefinitionGroupUIChildren.Length == 1 && childSpellCastActionUI == null)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
