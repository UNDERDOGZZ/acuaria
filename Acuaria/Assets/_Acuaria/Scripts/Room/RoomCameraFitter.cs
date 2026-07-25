using UnityEngine;

namespace Acuaria.Room
{
    [RequireComponent(typeof(Camera))]
    public sealed class RoomCameraFitter : MonoBehaviour
    {
        [SerializeField] private Vector2 referenceWorldSize = new(20f, 11.25f);

        private Camera targetCamera;

        private void OnEnable()
        {
            targetCamera = GetComponent<Camera>();
            Refresh();
        }

        private void OnValidate()
        {
            referenceWorldSize.x = Mathf.Max(0.01f, referenceWorldSize.x);
            referenceWorldSize.y = Mathf.Max(0.01f, referenceWorldSize.y);
        }

        public void Refresh()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            targetCamera.orthographic = true;
            targetCamera.orthographicSize = CalculateOrthographicSize(
                referenceWorldSize.x,
                referenceWorldSize.y,
                targetCamera.aspect);
        }

        public static float CalculateOrthographicSize(
            float referenceWidth,
            float referenceHeight,
            float viewportAspect)
        {
            if (referenceWidth <= 0f || referenceHeight <= 0f || viewportAspect <= 0f)
            {
                return 0.01f;
            }

            var halfReferenceHeight = referenceHeight * 0.5f;
            var halfHeightRequiredForWidth = referenceWidth / (2f * viewportAspect);
            return Mathf.Max(halfReferenceHeight, halfHeightRequiredForWidth);
        }
    }
}
