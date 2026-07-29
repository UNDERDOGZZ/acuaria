using UnityEngine;

namespace Acuaria.Offline
{
    [CreateAssetMenu(menuName="Acuaria/Simulation/Offline Progress",fileName="OfflineSimulationDefinition")]
    public sealed class OfflineSimulationDefinition:ScriptableObject
    {
        [SerializeField] bool enableOfflineProgress=true, simulateFishNeeds=true, simulateWaterChemistry=true,
            simulateNitrogenCycle=true, simulateFilter=true, simulateMaintenance=true, simulateWelfare=true,
            generateJournalEntries=true, saveImmediatelyAfterSimulation=true, showOfflineSummary=true,
            allowOfflineFishDeath=false, allowSimulationOnFocusResume=true, allowSimulationOnColdStart=true;
        [SerializeField,Min(0)] float minimumOfflineDurationSeconds=300;
        [SerializeField,Min(1)] float maximumOfflineDurationHours=48,clockRollbackToleranceMinutes=5,
            largeClockJumpWarningHours=72,simulationStepHours=1;
        [SerializeField,Range(1,10)] int maximumGeneratedJournalEntries=4,maximumSummaryItems=5;
        [SerializeField,Range(0,1)] float maximumFishHungerIncrease=.75f,maximumFishHealthLoss=.2f,
            minimumOfflineFishHealth=.55f,maximumFilterDirtIncrease=.55f,maximumWelfareDrop=.35f;
        [SerializeField,Min(0)] float maximumAmmoniaIncrease=1.5f,maximumNitriteIncrease=1.2f,maximumNitrateIncrease=35f;
        public bool Enabled=>enableOfflineProgress; public bool Fish=>simulateFishNeeds; public bool Water=>simulateWaterChemistry;
        public bool Cycle=>simulateNitrogenCycle; public bool Filter=>simulateFilter; public bool Maintenance=>simulateMaintenance;
        public bool Welfare=>simulateWelfare; public bool Journal=>generateJournalEntries; public bool SaveImmediately=>saveImmediatelyAfterSimulation;
        public bool ShowSummary=>showOfflineSummary; public bool AllowDeath=>allowOfflineFishDeath;
        public bool AllowResume=>allowSimulationOnFocusResume; public bool AllowColdStart=>allowSimulationOnColdStart;
        public float MinimumSeconds=>Mathf.Max(0,minimumOfflineDurationSeconds); public float MaximumHours=>Mathf.Max(1,maximumOfflineDurationHours);
        public float RollbackToleranceMinutes=>Mathf.Max(0,clockRollbackToleranceMinutes); public float LargeJumpHours=>Mathf.Max(1,largeClockJumpWarningHours);
        public float StepHours=>Mathf.Clamp(simulationStepHours,.25f,6); public int MaxJournal=>Mathf.Clamp(maximumGeneratedJournalEntries,1,10);
        public int MaxSummary=>Mathf.Clamp(maximumSummaryItems,1,10); public float MaxHunger=>Mathf.Clamp01(maximumFishHungerIncrease);
        public float MaxHealthLoss=>Mathf.Clamp01(maximumFishHealthLoss); public float MinHealth=>Mathf.Clamp01(minimumOfflineFishHealth);
        public float MaxFilterDirt=>Mathf.Clamp01(maximumFilterDirtIncrease); public float MaxWelfareDrop=>Mathf.Clamp01(maximumWelfareDrop);
        public float MaxAmmonia=>Mathf.Max(0,maximumAmmoniaIncrease); public float MaxNitrite=>Mathf.Max(0,maximumNitriteIncrease);
        public float MaxNitrate=>Mathf.Max(0,maximumNitrateIncrease);
    }
}
