using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Acuaria.Room
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class AquariumInteractable : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private AquariumFocusTarget focusTarget;
        [SerializeField] private bool available = true;

        public event Action<AquariumFocusTarget> Selected;
        public bool IsSelectable => available && focusTarget != null && !string.IsNullOrWhiteSpace(focusTarget.SlotId);

        public void Configure(AquariumFocusTarget target, bool isAvailable)
        {
            focusTarget = target;
            available = isAvailable;
        }

        public void SetAvailable(bool value) => available = value;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsSelectable && eventData.button == PointerEventData.InputButton.Left)
            {
                Selected?.Invoke(focusTarget);
            }
        }
    }
}
