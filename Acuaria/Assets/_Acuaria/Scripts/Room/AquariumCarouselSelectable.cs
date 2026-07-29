using UnityEngine;

namespace Acuaria.Room
{
    public sealed class AquariumCarouselSelectable:MonoBehaviour
    {
        [SerializeField] AquariumInteractable interactable;
        [SerializeField] AquariumNavigationCoordinator coordinator;
        [SerializeField] RoomViewController roomView;
        [SerializeField] int index;
        public void Configure(AquariumInteractable source,AquariumNavigationCoordinator navigation,
            RoomViewController view,int slotIndex)
        {interactable=source;coordinator=navigation;roomView=view;index=slotIndex;}
        void OnEnable(){if(interactable!=null)interactable.Selected+=Select;}
        void OnDisable(){if(interactable!=null)interactable.Selected-=Select;}
        void Select(AquariumFocusTarget target)
        {
            if(coordinator!=null&&coordinator.Request(index))return;
            roomView?.FocusAquarium(target);
        }
    }
}
