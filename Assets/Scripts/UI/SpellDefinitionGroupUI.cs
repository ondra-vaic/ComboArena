using System.Collections.Generic;
using Actions;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SpellDefinitionGroupUI : MonoBehaviour
    {
        [SerializeField] private SpellCastActionUI spellCastActionUIPrefab;
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private int padding = 5;
        
        private CustomizationMenu customizationMenu;
        private Button button;
        private Image image;
        private HorizontalLayoutGroup horizontalLayoutGroup;
        private RectTransform rectTransform;
        
        private Color defaultColor;

        private int multiplier = 1;
        private int numSiblings = 0;
        private readonly List<SpellDefinitionGroupUI> children = new ();
        private int leafId = -1;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
            customizationMenu = GetComponentInParent<CustomizationMenu>();
            image = GetComponent<Image>();
            button = GetComponent<Button>();
            
            defaultColor = image.color;
            setColor(defaultColor);
        }

        private void Start()
        {
            horizontalLayoutGroup.padding = new RectOffset(padding, padding, padding, padding);
            horizontalLayoutGroup.spacing = padding;
            
            button.onClick.AddListener(() => customizationMenu.OnSpellDefinitionSelected(this));

            float parentWidth = ((RectTransform)transform.parent).rect.width;
            float parentHeight = ((RectTransform)transform.parent).rect.height;
            
            float width = multiplier == 1 ? parentWidth : (parentWidth - padding * (numSiblings + 1)) / numSiblings;
            float height = multiplier == 1 ? parentHeight : parentHeight - padding * 2;
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        public void Split(SpellDefinitionGroupUI spellDefinitionGroupUIPrefab, int numSplits)
        {
            for (int i = 0; i < numSplits; i++)
            {
                var spellDefinitionGroup = Instantiate(spellDefinitionGroupUIPrefab, transform);
                spellDefinitionGroup.SetMultiplier(multiplier * numSplits);
                spellDefinitionGroup.SetNumSiblings(numSplits);
                children.Add(spellDefinitionGroup);
            }
            
            setDisabledView();
        }

        public void SetSpellAction(CastActionPrototype spellCastAction)
        {
            SpellCastActionUI spellCastActionUI = Instantiate(spellCastActionUIPrefab, transform);
            spellCastActionUI.Initialize(spellCastAction);
            spellCastActionUI.SetSize(new Vector2(rectTransform.rect.width - padding * 2, rectTransform.rect.height - padding * 2));
            spellCastActionUI.SetMultiplier(multiplier);
            spellCastActionUI.SetDisabledView();
            leafId = spellCastAction.ID;
            setDisabledView();
        }

        public void SetSelectedView(bool selected)
        {
            setColor(selected ? selectedColor : defaultColor);            
        }

        public ActionGroupNetworkData CreateActionGroupNetworkData()
        {
            ActionGroupNetworkData actionGroupNetworkData = new ActionGroupNetworkData();
            ActionGroupNetworkData[] networkDataChildren = new ActionGroupNetworkData[children.Count];
            
            for (int i = 0; i < children.Count; i++)
            {
                networkDataChildren[i] = children[i].CreateActionGroupNetworkData();
            }
            
            actionGroupNetworkData.SetChildren(networkDataChildren);
            actionGroupNetworkData.SetLeafActionId(leafId);
            actionGroupNetworkData.SetMultiplier(multiplier);
            actionGroupNetworkData.SetLeafActionId(leafId);
            
            return actionGroupNetworkData;
        }
        
        public void SetMultiplier(int multiplier) => this.multiplier = multiplier;
        
        public void SetNumSiblings(int numSiblings) => this.numSiblings = numSiblings;
        public int GetMultiplier() => multiplier;
        
        private void setColor(Color color) => image.color = color;

        private void setDisabledView()
        {
            setColor(new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0.3f));
            Destroy(button);
        }
    }
}
