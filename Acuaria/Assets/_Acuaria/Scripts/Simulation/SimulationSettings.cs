using UnityEngine;

namespace Acuaria.Simulation
{
    [CreateAssetMenu(fileName = "SimulationSettings", menuName = "Acuaria/Settings/Simulation")]
    public sealed class SimulationSettings : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float simulationTickSeconds = 1f;

        public float SimulationTickSeconds => simulationTickSeconds;
    }
}
