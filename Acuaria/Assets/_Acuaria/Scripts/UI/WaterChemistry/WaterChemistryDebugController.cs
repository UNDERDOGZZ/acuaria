using UnityEngine;

namespace Acuaria.UI.WaterChemistry
{
    [AddComponentMenu("Acuaria/Debug/Water Chemistry Debug Controller")]
    public sealed class WaterChemistryDebugController : MonoBehaviour
    {
        [SerializeField] private AquariumSimulationController simulation;
        [SerializeField, Min(0.01f)] private float ammoniaDose = 0.25f;
        [SerializeField, Min(0.01f)] private float wasteDose = 1f;

        public void Configure(AquariumSimulationController controller) => simulation = controller;

        [ContextMenu("Debug/Add Ammonia")]
        private void AddAmmonia() => simulation?.AddAmmoniaDebug(ammoniaDose);
        [ContextMenu("Debug/Add Organic Waste")]
        private void AddWaste() => simulation?.AddWaste(wasteDose);
        [ContextMenu("Debug/Reset Chemistry")]
        private void ResetChemistry() => simulation?.ResetChemistryDebug();
        [ContextMenu("Debug/Set Bacteria Minimum")]
        private void MinimumBacteria() => simulation?.SetBacteriaDebug(0.05f);
        [ContextMenu("Debug/Set Bacteria Maximum")]
        private void MaximumBacteria() => simulation?.SetBacteriaDebug(1f);
        [ContextMenu("Debug/Simulate One Hour")]
        private void SimulateHour() => simulation?.SimulateTick(3600f);
        [ContextMenu("Debug/Simulate Expired Food")]
        private void SimulateExpiredFood() => simulation?.AddWaste(wasteDose);
    }
}
