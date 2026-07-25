using UnityEngine;

namespace Acuaria.Fish
{
    public sealed class FishVisual2D : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform tail;
        [SerializeField] private Transform sideFin;
        [SerializeField] private SpriteRenderer[] coloredRenderers;
        [SerializeField, Min(0f)] private float flipDeadZone = 0.06f;
        private float phase;
        private bool facingRight = true;
        private float baseScale = 1f;
        private float eatingPulseUntil;

        public void PlayEatingPulse() => eatingPulseUntil = Time.time + 0.22f;

        public void Initialize(FishSpeciesDefinition species, int seed, float scale)
        {
            phase = Mathf.Abs(seed % 997) * 0.019f;
            baseScale = scale;
            for (var index = 0; index < coloredRenderers.Length; index++)
            {
                if (coloredRenderers[index] != null) coloredRenderers[index].color = species.PrototypeColor;
            }
        }

        public void Render(Vector2 direction, float time)
        {
            if (direction.x > flipDeadZone) facingRight = true;
            else if (direction.x < -flipDeadZone) facingRight = false;

            var wave = Mathf.Sin(time * 7f + phase);
            var pulse = Time.time < eatingPulseUntil ? 1f + Mathf.Sin((eatingPulseUntil - Time.time) * 28f) * 0.08f : 1f;
            visualRoot.localScale = new Vector3(facingRight ? baseScale * pulse : -baseScale * pulse,
                baseScale * pulse, 1f);
            visualRoot.localPosition = new Vector3(0f, Mathf.Sin(time * 1.8f + phase) * 0.025f, 0f);
            if (tail != null) tail.localRotation = Quaternion.Euler(0f, 0f, wave * 16f);
            if (sideFin != null) sideFin.localRotation = Quaternion.Euler(0f, 0f, wave * 10f);
        }

        public void Configure(Transform root, Transform tailTransform, Transform fin, SpriteRenderer[] renderers)
        {
            visualRoot = root;
            tail = tailTransform;
            sideFin = fin;
            coloredRenderers = renderers;
        }
    }
}
