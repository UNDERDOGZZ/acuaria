using System;
using System.Collections.Generic;
using Acuaria.Fish.Care;
using UnityEngine;
using Acuaria.Aquarium.MultiAquarium;

namespace Acuaria.Aquarium.Decorations
{
    public sealed class AquariumHabitatController : MonoBehaviour
    {
        [SerializeField] Acuaria.Aquarium.AquariumDefinition aquariumDefinition;
        [SerializeField] DecorationRegistry registry;
        [SerializeField] Transform decorationRoot;
        [SerializeField] DecorationSpawner2D decorationSpawner;
        readonly List<DecorationDefinition> installed = new();
        readonly List<DecorationPlacementData> placements = new();
        int nextRuntimeId;
        public IReadOnlyList<DecorationDefinition> Installed => installed;
        public IReadOnlyList<DecorationPlacementData> Placements => placements;
        public AquariumHabitatProfile CurrentProfile { get; private set; } = new();
        public event Action<AquariumHabitatProfile> Changed;
        public event Action<DecorationPlacementData> DecorationAdded, DecorationRemoved;
        public event Action DecorationsChanged;
        public DecorationRegistry Registry=>registry;
        public DecorationSpawner2D Spawner=>decorationSpawner;
        public AquariumInstance BoundAquarium { get; private set; }

        public void Bind(AquariumInstance aquarium)
        {
            if (aquarium == null || ReferenceEquals(BoundAquarium, aquarium)) return;
            BoundAquarium = aquarium;
            aquariumDefinition = aquarium.Definition;
            ApplyPlacements(aquarium.DecorationCollection.Placements);
            aquarium.SetHabitat(CurrentProfile);
        }

        public void Configure(Acuaria.Aquarium.AquariumDefinition definition, DecorationRegistry source, Transform root = null)
        { aquariumDefinition = definition; registry = source; decorationRoot = root; decorationSpawner=root!=null?root.GetComponentInParent<DecorationSpawner2D>():null;ResetHabitat(); }
        public void ConfigureVisuals(DecorationSpawner2D spawner){decorationSpawner=spawner;decorationRoot=spawner!=null?spawner.transform:null;SynchronizeViews();}

        void Awake() => ResetHabitat();
        void Start() => ResetHabitat();
        public void ResetHabitat()
        {
            installed.Clear();
            placements.Clear();nextRuntimeId=0;
            if (aquariumDefinition != null)
            {
                placements.AddRange(aquariumDefinition.DecorationPlacements);
                if(placements.Count==0)for(var i=0;i<aquariumDefinition.InstalledDecorations.Count;i++)
                    placements.Add(CreatePlacement(aquariumDefinition.InstalledDecorations[i],i));
            }
            RebuildDefinitions();
            Recalculate();
        }
        public bool AddById(string id)
        {
            var item = registry?.FindById(id); if (item == null) return false;
            var placement=CreatePlacement(item,placements.Count);placements.Add(placement);installed.Add(item);DecorationAdded?.Invoke(placement);Recalculate(); return true;
        }
        public bool RemoveById(string id)
        {
            for (var i = installed.Count - 1; i >= 0; i--)
                if (installed[i]?.DecorationId == id) {var placement=placements[i];installed.RemoveAt(i);placements.RemoveAt(i);DecorationRemoved?.Invoke(placement);Recalculate();return true;}
            return false;
        }
        public void AddPlant() => AddFirst(DecorationCategory.Plant);
        public void RemovePlant() => RemoveFirst(DecorationCategory.Plant);
        public void AddRock() => AddFirst(DecorationCategory.Rock);
        public void RemoveRock() => RemoveFirst(DecorationCategory.Rock);
        public void AddCave() => AddFirst(DecorationCategory.Cave);
        void AddFirst(DecorationCategory category)
        { if (registry == null) return; for (var i = 0; i < registry.Decorations.Count; i++) if (registry.Decorations[i]?.Category == category) {AddById(registry.Decorations[i].DecorationId);return;} }
        void RemoveFirst(DecorationCategory category)
        { for (var i = installed.Count - 1; i >= 0; i--) if (installed[i]?.Category == category) {RemoveById(installed[i].DecorationId);return;} }
        void Recalculate()
        {
            CurrentProfile = AquariumHabitatCalculator.Calculate(installed);
            if(BoundAquarium!=null)
            {
                BoundAquarium.DecorationCollection.Replace(placements);
                BoundAquarium.SetHabitat(CurrentProfile);
            }
            Changed?.Invoke(CurrentProfile);
            DecorationsChanged?.Invoke();
            SynchronizeViews();
        }
        void RebuildDefinitions(){installed.Clear();for(var i=0;i<placements.Count;i++)if(placements[i]?.Definition!=null)installed.Add(placements[i].Definition);}
        void SynchronizeViews()=>decorationSpawner?.SynchronizeInstalledDecorations(placements);
        public void PreviewPlacements(IReadOnlyList<DecorationPlacementData> values)=>decorationSpawner?.SynchronizeInstalledDecorations(values);
        public bool ApplyPlacements(IReadOnlyList<DecorationPlacementData> values)
        {
            if(values==null)return false;placements.Clear();for(var i=0;i<values.Count;i++)if(values[i]?.IsValid==true)placements.Add(values[i].Clone());
            RebuildDefinitions();Recalculate();return true;
        }
        DecorationPlacementData CreatePlacement(DecorationDefinition item,int index)
        {
            var category=item!=null?item.Category:DecorationCategory.Artificial;
            var count=++nextRuntimeId;var x=.16f+((index*29+count*17)%68)/100f;
            var y=category==DecorationCategory.Substrate?.03f:category==DecorationCategory.Plant?.18f:.1f;
            var scale=category switch{DecorationCategory.Substrate=>new Vector2(7.1f,.16f),DecorationCategory.Plant=>new Vector2(.45f,1.15f),DecorationCategory.Wood=>new Vector2(1.35f,.3f),DecorationCategory.Cave=>new Vector2(.8f,.48f),_=>new Vector2(.65f,.42f)};
            var layer=category==DecorationCategory.Substrate?DecorationVisualLayer.Substrate:DecorationVisualLayer.Midground;
            return new DecorationPlacementData($"runtime-{count:0000}",item,new Vector2(x,y),scale,category==DecorationCategory.Wood?12:0,index%2==0,0,layer);
        }
    }
}
