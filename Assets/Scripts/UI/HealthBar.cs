using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private float smoothingTime = 0.05f;
        [SerializeField] private float displayHealthSmoothingTime = 0.5f;
        [SerializeField] private Image fillArea;
        [SerializeField] private Slider slider;
        
        private Player.Player player;
        private RectTransform canvasRectTransform;
        private UnityEngine.Camera mainCamera;
        private RectTransform rectTransform;

        private float headHeight;
        private Vector3 smoothingVelocity;
        private Vector3 targetWorldPosition;
        private Vector3 currentWorldPosition;

        private int targetDisplayHealth;
        private float healthSmoothingVelocity;

        private void Awake()
        {
            canvasRectTransform = GetComponentInParent<Canvas>().transform as RectTransform;
            mainCamera = FindObjectOfType<UnityEngine.Camera>();
            rectTransform = transform as RectTransform;
        }

        void Update()
        {
            if (player == null) return;
            updatePosition();
            updateDisplayedHealth();
        }

        private void updateDisplayedHealth()
        {
            slider.value = Mathf.SmoothDamp(slider.value, targetDisplayHealth, ref healthSmoothingVelocity, displayHealthSmoothingTime);
        }

        private void updatePosition()
        {
            targetWorldPosition = player.transform.position + Vector3.up * headHeight;
            currentWorldPosition = Vector3.SmoothDamp(currentWorldPosition, targetWorldPosition, ref smoothingVelocity, smoothingTime);
            
            Vector2 viewportPosition = mainCamera.WorldToViewportPoint(currentWorldPosition);

            rectTransform.anchoredPosition = new Vector2(
                viewportPosition.x * canvasRectTransform.sizeDelta.x, viewportPosition.y * canvasRectTransform.sizeDelta.y);
        }

        public void Initialize(Player.Player player, Color healthBarColor)
        {
            this.player = player;
            this.headHeight = Vector3.Distance(player.transform.position, player.MeshInfo.Head.position);
            
            fillArea.color = healthBarColor;
            slider.maxValue = player.MaxHealth;
            slider.value = player.MaxHealth;
            targetDisplayHealth = player.MaxHealth;
            
            targetWorldPosition = player.transform.position;
            currentWorldPosition = player.transform.position;

            player.Health.OnValueChanged += (_, newValue) => targetDisplayHealth = newValue;
        }
    }
}
