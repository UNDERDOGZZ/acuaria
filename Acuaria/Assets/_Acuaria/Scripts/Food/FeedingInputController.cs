using UnityEngine;
using UnityEngine.EventSystems;

namespace Acuaria.Food
{
    public sealed class FeedingInputController : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private AquariumFeedingArea2D feedingArea;
        [SerializeField] private AquariumFoodController foodController;
        private int portionSequence;
        public bool IsFeedingMode { get; private set; }

        private void OnEnable()
        {
            if (worldCamera == null) worldCamera = Camera.main;
        }

        public void Configure(Camera cameraReference, AquariumFeedingArea2D area, AquariumFoodController controller)
        {
            worldCamera = cameraReference;
            feedingArea = area;
            foodController = controller;
        }

        public void SetFeedingMode(bool active) => IsFeedingMode = active;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsFeedingMode || worldCamera == null || feedingArea == null || foodController == null) return;
            if (eventData.pointerCurrentRaycast.gameObject != null &&
                eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<RectTransform>() != null) return;
            var screen = new Vector3(eventData.position.x, eventData.position.y,
                Mathf.Abs(worldCamera.transform.position.z - feedingArea.transform.position.z));
            var world = (Vector2)worldCamera.ScreenToWorldPoint(screen);
            if (!feedingArea.ContainsWorldPoint(world)) return;
            var local = (Vector2)foodController.transform.InverseTransformPoint(feedingArea.ProjectWorldToSurface(world));
            foodController.SpawnPortion(local, 7919 + ++portionSequence * 104729, 3);
        }
    }
}
