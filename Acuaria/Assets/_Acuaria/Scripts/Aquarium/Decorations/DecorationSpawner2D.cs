using System;
using System.Collections.Generic;
using UnityEngine;

namespace Acuaria.Aquarium.Decorations
{
    public sealed class DecorationSpawner2D : MonoBehaviour
    {
        [SerializeField] Transform decorationsRoot;
        [SerializeField] AquariumDecorationArea2D area;
        [SerializeField] AquariumHabitatController source;
        [SerializeField] Acuaria.Aquarium.AquariumDefinition aquariumDefinition;
        readonly Dictionary<string, DecorationView> views = new();
        public IReadOnlyDictionary<string, DecorationView> Views => views;
        public event Action<DecorationView> ViewCreated, ViewRemoved;
        public event Action ViewsSynchronized;

        public void Configure(Transform root, AquariumDecorationArea2D decorationArea, AquariumHabitatController controller=null,
            Acuaria.Aquarium.AquariumDefinition definition=null)
        { decorationsRoot=root;area=decorationArea;source=controller;aquariumDefinition=definition;IndexExisting(); }
        void OnEnable(){if(aquariumDefinition!=null)SynchronizeInstalledDecorations(aquariumDefinition.DecorationPlacements);}
        void Start(){if(source!=null){source.DecorationsChanged+=SynchronizeFromSource;SynchronizeFromSource();}}
        void OnDisable(){if(source!=null)source.DecorationsChanged-=SynchronizeFromSource;}
        void SynchronizeFromSource()=>SynchronizeInstalledDecorations(source?.Placements);

        public void SynchronizeInstalledDecorations(IReadOnlyList<DecorationPlacementData> installed)
        {
            if(decorationsRoot==null||area==null)return;
            if(views.Count==0)IndexExisting();
            var desired=new HashSet<string>();
            if(installed!=null)for(var i=0;i<installed.Count;i++)
            {
                var placement=installed[i];if(placement==null||!placement.IsValid||!desired.Add(placement.InstanceId))continue;
                if(!views.TryGetValue(placement.InstanceId,out var view)||view==null)
                {
                    var go=new GameObject($"Decoration_{placement.InstanceId}",typeof(DecorationView));
                    go.transform.SetParent(decorationsRoot,false);view=go.GetComponent<DecorationView>();views[placement.InstanceId]=view;ViewCreated?.Invoke(view);
                }
                view.Apply(placement,area);
            }
            var removed=new List<string>();foreach(var pair in views)if(!desired.Contains(pair.Key))removed.Add(pair.Key);
            foreach(var id in removed){var view=views[id];views.Remove(id);ViewRemoved?.Invoke(view);if(view!=null)Destroy(view.gameObject);}
            ViewsSynchronized?.Invoke();
        }
        public void Clear()
        { foreach(var view in views.Values)if(view!=null)Destroy(view.gameObject);views.Clear(); }
        void OnDestroy()=>Clear();
        void IndexExisting()
        {
            views.Clear();if(decorationsRoot==null)return;
            var existing=decorationsRoot.GetComponentsInChildren<DecorationView>(true);
            for(var i=0;i<existing.Length;i++)if(existing[i]!=null&&!string.IsNullOrWhiteSpace(existing[i].InstanceId)&&!views.ContainsKey(existing[i].InstanceId))
                views.Add(existing[i].InstanceId,existing[i]);
        }
    }
}
