using System;
using Acuaria.Aquarium;
using Acuaria.Aquarium.MultiAquarium;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.Room
{
    public sealed class MultiAquariumRoomController : MonoBehaviour
    {
        [SerializeField] AquariumManager manager;
        [SerializeField] AquariumDefinition definition;
        [SerializeField] AquariumDefinition[] definitions=Array.Empty<AquariumDefinition>();
        [SerializeField] AquariumNavigationCoordinator navigation;
        [SerializeField] Button[] slotButtons = Array.Empty<Button>();
        [SerializeField] Text[] slotLabels = Array.Empty<Text>();
        [SerializeField] Color activeColor=new(.12f,.55f,.62f);
        [SerializeField] Color inactiveColor=new(.08f,.3f,.36f);
        readonly AquariumSlot[] slots={new("slot-01"),new("slot-02"),new("slot-03")};
        public AquariumSlot[] Slots=>slots;

        public void Configure(AquariumManager source,AquariumDefinition aquariumDefinition,Button[] buttons,Text[] labels)
        {manager=source;definition=aquariumDefinition;slotButtons=buttons??Array.Empty<Button>();slotLabels=labels??Array.Empty<Text>();}
        public void SetDefinitions(AquariumDefinition[] values)=>definitions=values??Array.Empty<AquariumDefinition>();
        public void SetNavigation(AquariumNavigationCoordinator value)=>navigation=value;
        void Start()
        {
            if(manager==null)manager=AquariumManager.Instance;
            if(manager==null||definition==null){Debug.LogError("MultiAquariumRoomController is not configured.",this);return;}
            if(manager.ActiveAquarium==null)manager.CreateAquarium(definition,"aquarium-01","Acuario Inicial");
            slots[0].Assign(manager.ActiveAquarium); ConfigureSlot(slots[0], 0);
            var second=manager.Find("aquarium-02")??manager.CreateAquarium(DefinitionAt(1),"aquarium-02","Acuario 2");
            if(second.FishCollection.Count==0)second.FishCollection.Add("aquarium-02-fish-01");
            slots[1].Assign(second); ConfigureSlot(slots[1], 1);
            var third=manager.Find("aquarium-03")??manager.CreateAquarium(DefinitionAt(2),"aquarium-03","Acuario 3");
            slots[2].Assign(third); ConfigureSlot(slots[2], 2);
            for(var i=0;i<slotButtons.Length&&i<slots.Length;i++){var index=i;slotButtons[i]?.onClick.AddListener(()=>UseSlot(index));}
            manager.OnActiveAquariumChanged+=OnActiveChanged;Refresh();
            navigation?.RefreshVisualBindings();
        }
        AquariumDefinition DefinitionAt(int index)=>definitions!=null&&index>=0&&index<definitions.Length&&definitions[index]!=null
            ?definitions[index]:definition;
        static void ConfigureSlot(AquariumSlot slot,int index)
        {
            if(slot?.Aquarium==null)return;
            slot.Aquarium.AssignPresentation(slot.SlotId,$"aquarium-view-{index+1:00}");
        }
        void OnDestroy()
        {
            if(manager!=null)manager.OnActiveAquariumChanged-=OnActiveChanged;
            for(var i=0;i<slotButtons.Length;i++)slotButtons[i]?.onClick.RemoveAllListeners();
        }
        public void UseSlot(int index)
        {
            if(index<0||index>=slots.Length||manager==null)return;var slot=slots[index];
            if(slot.State==AquariumSlotState.Locked)return;
            if(slot.State==AquariumSlotState.Occupied&&navigation!=null){navigation.Request(index);return;}
            if(slot.State==AquariumSlotState.Empty)
            {
                var number=index+1;var aquarium=manager.CreateAquarium(definition,$"aquarium-{number:00}",$"Acuario {number}");
                slot.Assign(aquarium);manager.Activate(aquarium.InstanceId);
            }
            else manager.Activate(slot.Aquarium.InstanceId);
            Refresh();
        }
        public bool EmptySlot(int index)
        {
            if(index<=0||index>=slots.Length||slots[index].Aquarium==null)return false;
            var id=slots[index].Aquarium.InstanceId;if(!manager.RemoveAquarium(id))return false;
            slots[index].Clear();Refresh();return true;
        }
        void OnActiveChanged(AquariumInstance previous,AquariumInstance next)=>Refresh();
        void Refresh()
        {
            for(var i=0;i<slots.Length;i++)
            {
                var slot=slots[i];var active=ReferenceEquals(slot.Aquarium,manager?.ActiveAquarium);
                if(i<slotLabels.Length&&slotLabels[i]!=null)slotLabels[i].text=slot.State switch
                {
                    AquariumSlotState.Locked=>"Bloqueado",
                    AquariumSlotState.Empty=>"+ Crear Acuario",
                    _=>$"{(active?"▶ ":"")}{slot.Aquarium.Name}\n{slot.Aquarium.FishCollection.Count} peces · {slot.Aquarium.RuntimeState.CurrentTemperature:0.#} °C · {slot.Aquarium.Definition.NominalVolumeLitres:0} L"
                };
                if(i<slotButtons.Length&&slotButtons[i]!=null&&slotButtons[i].targetGraphic is Graphic graphic)
                    graphic.color=active?activeColor:inactiveColor;
            }
        }
    }
}
