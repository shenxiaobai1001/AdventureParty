using UnityEngine;

namespace UInventoryGrid
{
    public class SearchIconAnimation : MonoBehaviour
    {
        public float rotationSpeed = 100f;
        public float pulseSpeed = 0.5f;
        public float pulseScale = 0.1f;

        private RectTransform rectTransform;
        private Vector3 baseScale;

        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            baseScale = rectTransform.localScale;
        }

        void Update()
        {
            // Rotation animation
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            // Pulse animation
            float scaleMultiplier = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            rectTransform.localScale = baseScale * scaleMultiplier;
        }
    }
}
