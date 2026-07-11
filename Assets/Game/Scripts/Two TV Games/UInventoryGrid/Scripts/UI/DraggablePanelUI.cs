using UnityEngine;
using UnityEngine.EventSystems;

namespace UInventoryGrid
{
    public class DraggablePanelUI : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        private RectTransform panelRectTransform;
        private Canvas canvas;
        private Vector2 initialPointerPosition;
        private Vector3 initialPanelPosition;

        void Awake()
        {
            panelRectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            initialPointerPosition = eventData.position;
            initialPanelPosition = panelRectTransform.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 pointerOffset = eventData.position - initialPointerPosition;
            panelRectTransform.position = initialPanelPosition + new Vector3(pointerOffset.x, pointerOffset.y, 0);
        }

        public void OnEndDrag(PointerEventData eventData)
        {

        }
    }

}
