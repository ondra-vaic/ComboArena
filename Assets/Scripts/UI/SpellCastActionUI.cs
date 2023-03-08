using Actions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SpellCastActionUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI spellNameText;
        [SerializeField] private TextMeshProUGUI spellDescriptionText;
        
        private CastActionPrototype castActionPrototype;
        
        private CustomizationMenu customizationMenu;
        private Button button;
        private Image image;
        private int multiplier;
        
        private void Awake()
        {
            customizationMenu = GetComponentInParent<CustomizationMenu>();
            button = GetComponentInChildren<Button>();
            image = GetComponentInChildren<Image>();

            button.onClick.AddListener(() => customizationMenu.OnSpellCastActionSelected(castActionPrototype));
        }

        public void Initialize(CastActionPrototype castActionPrototype)
        {
            this.castActionPrototype = castActionPrototype;
            spellNameText.text = castActionPrototype.ActionUIName.ToUpper();
            spellDescriptionText.text = castActionPrototype.ActionUIDescription.Replace("\n", "").Replace("\r", "");
            spellNameText.color = castActionPrototype.ActionUIColor;
        }

        public void SetSize(Vector2 size) => ((RectTransform)transform).sizeDelta = size;

        public void SetMultiplier(int multiplier) => this.multiplier = multiplier;

        public void SetDisabledView()
        {
            image.rectTransform.offsetMin = new Vector2(0, 0);
            image.rectTransform.offsetMax = new Vector2(0, 0);
            Destroy(spellDescriptionText.gameObject);
            Destroy(button);
        }
    }
}
