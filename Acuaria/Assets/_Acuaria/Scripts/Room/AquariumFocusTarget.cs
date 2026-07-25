using UnityEngine;

namespace Acuaria.Room
{
    public sealed class AquariumFocusTarget : MonoBehaviour
    {
        [SerializeField] private string slotId = "slot-01";
        [SerializeField] private Transform focusPoint;
        [SerializeField, Min(0.1f)] private float orthographicSize = 3.25f;

        public string SlotId => slotId;
        public Vector3 Position => focusPoint != null ? focusPoint.position : transform.position;
        public float OrthographicSize => Mathf.Max(0.1f, orthographicSize);

        public void Configure(string id, Transform point, float size)
        {
            slotId = id;
            focusPoint = point;
            orthographicSize = Mathf.Max(0.1f, size);
        }
    }
}
