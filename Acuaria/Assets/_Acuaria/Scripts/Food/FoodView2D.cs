using System.Collections;
using UnityEngine;

namespace Acuaria.Food
{
    public sealed class FoodView2D : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private FoodMovement2D movement;
        public FoodRuntimeState State { get; private set; }
        public FoodDefinition Definition { get; private set; }

        public void Configure(SpriteRenderer renderer, FoodMovement2D movementComponent)
        {
            visual = renderer;
            movement = movementComponent;
        }

        public void Initialize(FoodDefinition definition, FoodRuntimeState state, AquariumFoodController owner,
            float visualSize, float driftPhase)
        {
            Definition = definition;
            State = state;
            visual.color = definition.PrototypeColor;
            transform.localScale = Vector3.one * visualSize;
            movement.Initialize(this, owner, driftPhase);
        }

        public void Consume()
        {
            movement.enabled = false;
            if (Application.isPlaying) StartCoroutine(ConsumeAnimation());
            else gameObject.SetActive(false);
        }

        private IEnumerator ConsumeAnimation()
        {
            var start = transform.localScale;
            var elapsed = 0f;
            while (elapsed < 0.16f)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(start, Vector3.zero, elapsed / 0.16f);
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
