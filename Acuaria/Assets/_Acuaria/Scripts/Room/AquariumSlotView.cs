using System;
using UnityEngine;

namespace Acuaria.Room
{
    public sealed class AquariumSlotView : MonoBehaviour
    {
        [SerializeField] private string slotId = "slot-01";
        [SerializeField] private Transform contentRoot;
        [SerializeField] private GameObject currentView;

        public string SlotId => slotId;
        public bool IsOccupied => currentView != null && currentView.activeSelf;
        public GameObject CurrentView => currentView;

        public void Configure(string id, Transform root)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Slot ID cannot be empty.", nameof(id));
            }

            slotId = id;
            contentRoot = root != null ? root : throw new ArgumentNullException(nameof(root));
        }

        public void AssignView(GameObject view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (contentRoot == null)
            {
                throw new InvalidOperationException("Slot requires a content root.");
            }

            if (currentView != null && currentView != view)
            {
                currentView.SetActive(false);
            }

            currentView = view;
            currentView.transform.SetParent(contentRoot, false);
            currentView.SetActive(true);
        }

        public void ClearView()
        {
            if (currentView != null)
            {
                currentView.SetActive(false);
                currentView = null;
            }
        }
    }
}
