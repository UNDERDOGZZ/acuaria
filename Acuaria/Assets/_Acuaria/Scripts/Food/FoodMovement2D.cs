using UnityEngine;

namespace Acuaria.Food
{
    public sealed class FoodMovement2D : MonoBehaviour
    {
        private FoodView2D view;
        private AquariumFoodController owner;
        private float driftPhase;
        private float age;

        public void Initialize(FoodView2D foodView, AquariumFoodController controller, float phase)
        {
            view = foodView;
            owner = controller;
            driftPhase = phase;
            transform.localPosition = new Vector3(view.State.Position.x, view.State.Position.y, transform.localPosition.z);
        }

        private void Update()
        {
            if (view?.State == null || view.State.IsTerminal) return;
            age += Time.deltaTime;
            view.State.RemainingLifetime -= Time.deltaTime;
            if (view.State.RemainingLifetime <= 0f)
            {
                view.State.Expire();
                owner.NotifyExpired(view);
                Destroy(gameObject);
                return;
            }

            var position = view.State.Position;
            position.y -= view.State.CurrentSpeed * Time.deltaTime;
            position.x += Mathf.Sin(age * 1.7f + driftPhase) * 0.025f * Time.deltaTime;
            position = owner.ClampLocal(position);
            if (position.y <= owner.Bottom + 0.01f) view.State.MakeAvailable();
            view.State.Position = position;
            transform.localPosition = new Vector3(position.x, position.y, transform.localPosition.z);
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(age * 2f + driftPhase) * 12f);
        }
    }
}
