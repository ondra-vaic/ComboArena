using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SpellUI : MonoBehaviour
    {
        private RectTransform maskRectTransform;
        
        private float cooldownStartTime;
        private float cooldownEndTime;
        private float spellUIWidth;
        
        private void Awake()
        {
            maskRectTransform = GetComponentInChildren<Mask>().transform.GetChild(0).transform as RectTransform;
            cooldownStartTime = Time.time;
            cooldownEndTime = Time.time + 0.0001f;
        }
        
        private void Update()
        {
            float cooldownFraction = Mathf.Clamp01((Time.time - cooldownStartTime) / (cooldownEndTime - cooldownStartTime));
            maskRectTransform.offsetMax = new Vector2(-spellUIWidth * cooldownFraction, 0);
        }

        public void onCast(float coolDown)
        {
            cooldownStartTime = Time.time;
            cooldownEndTime = cooldownStartTime + coolDown;
        }

        public void SetWidth(float spellUIWidth)
        {
            this.spellUIWidth = spellUIWidth;
        }
    }
}
