using UnityEngine;
namespace Acuaria.UI.Maintenance
{
    public sealed class MaintenanceDebugController:MonoBehaviour
    {
        [SerializeField] AquariumMaintenanceController controller;
        public void Configure(AquariumMaintenanceController value)=>controller=value;
        [ContextMenu("Filter/Add Dirt")] public void AddDirt(){var f=controller?.FilterState;if(f!=null)f.Apply(Mathf.Clamp01(f.DirtLevel+0.2f),f.CurrentEfficiency,f.BiologicalCapacity,f.HoursSinceMaintenance,f.Status,true);}
    }
}
