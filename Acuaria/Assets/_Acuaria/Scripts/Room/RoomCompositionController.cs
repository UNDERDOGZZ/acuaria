using UnityEngine;

namespace Acuaria.Room
{
    public sealed class RoomCompositionController : MonoBehaviour
    {
        [SerializeField] private AquariumSlotView initialAquariumSlot;
        [SerializeField] private GameObject ambientEffects;
        [SerializeField] private bool ambientEffectsEnabled = true;

        private void OnEnable()
        {
            if (initialAquariumSlot == null)
            {
                Debug.LogError("Room composition requires an initial aquarium slot.", this);
                return;
            }

            if (ambientEffects != null)
            {
                ambientEffects.SetActive(ambientEffectsEnabled);
            }
        }
    }
}
