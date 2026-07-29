using System;
using UnityEngine;
namespace Acuaria.Simulation.Filtration
{
    public enum FilterOperatingStatus { Off, Running, ReducedEfficiency, MaintenanceRecommended, Blocked }
    [Serializable] public sealed class FilterRuntimeState
    {
        public string InstanceId { get; private set; } public string DefinitionId { get; private set; }
        public bool IsActive { get; private set; } public float CurrentEfficiency { get; private set; }
        public float DirtLevel { get; private set; } public float BiologicalCapacity { get; private set; }
        public float HoursSinceMaintenance { get; private set; } public int MaintenanceCount { get; private set; }
        public FilterOperatingStatus Status { get; private set; } public bool MaintenanceRecommended { get; private set; }
        public bool IsInitialized { get; private set; }
        public void Initialize(string id, FilterDefinition definition)
        { if(string.IsNullOrWhiteSpace(id)||definition==null)throw new ArgumentException(); InstanceId=id; DefinitionId=definition.FilterId;
          IsActive=true; DirtLevel=definition.InitialDirt; BiologicalCapacity=definition.AdditionalBiologicalCapacity;
          CurrentEfficiency=definition.BaseEfficiency; Status=FilterOperatingStatus.Running; IsInitialized=true; }
        public void Apply(float dirt,float efficiency,float capacity,float hours,FilterOperatingStatus status,bool recommended)
        { DirtLevel=Mathf.Clamp01(Safe(dirt));CurrentEfficiency=Mathf.Clamp01(Safe(efficiency));BiologicalCapacity=Safe(capacity);
          HoursSinceMaintenance=Safe(hours);Status=status;MaintenanceRecommended=recommended; }
        public void RecordMaintenance()=>MaintenanceCount++;
        public void SetActive(bool value)=>IsActive=value;
        public void Restore(string id,FilterDefinition definition,bool active,float dirt,float efficiency,float capacity,
            float hours,FilterOperatingStatus status,bool recommended,int maintenanceCount)
        {Initialize(id,definition);IsActive=active;Apply(dirt,efficiency,capacity,hours,status,recommended);MaintenanceCount=Math.Max(0,maintenanceCount);}
        private static float Safe(float v)=>float.IsFinite(v)?Mathf.Max(0f,v):0f;
    }
}
