using System;
using UnityEngine;

namespace Acuaria.Simulation.Maintenance
{
    public enum AquariumMaintenancePhase { Idle, Preparing, Draining, Refilling, Stabilizing, Completed, Cancelled, Failed }
    public enum AquariumMaintenanceResult { None, Success, Blocked, InvalidPercentage, CooldownActive, MissingConfiguration, Cancelled }

    [Serializable]
    public sealed class AquariumMaintenanceState
    {
        public string InstanceId { get; private set; }
        public AquariumMaintenancePhase Phase { get; private set; }
        public AquariumMaintenanceResult LastResult { get; private set; }
        public int LastPercentage { get; private set; }
        public float CooldownRemaining { get; private set; }
        public float Progress { get; private set; }
        public float TotalWaterChangedPercent { get; private set; }
        public int ChangesPerformed { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsActive => Phase is AquariumMaintenancePhase.Preparing or AquariumMaintenancePhase.Draining
            or AquariumMaintenancePhase.Refilling or AquariumMaintenancePhase.Stabilizing;

        public void Initialize(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            InstanceId = id; Phase = AquariumMaintenancePhase.Idle; LastResult = AquariumMaintenanceResult.None;
            LastPercentage = 25; CooldownRemaining = 0f; Progress = 0f; IsInitialized = true;
        }
        public bool Begin(int percentage, AquariumMaintenanceDefinition definition)
        {
            if (definition == null || !definition.IsValid) { LastResult = AquariumMaintenanceResult.MissingConfiguration; return false; }
            if (IsActive) { LastResult = AquariumMaintenanceResult.Blocked; return false; }
            if (CooldownRemaining > 0f) { LastResult = AquariumMaintenanceResult.CooldownActive; return false; }
            if (!definition.IsAllowed(percentage)) { LastResult = AquariumMaintenanceResult.InvalidPercentage; return false; }
            LastPercentage = percentage; Phase = AquariumMaintenancePhase.Preparing; Progress = 0f; LastResult = AquariumMaintenanceResult.None; return true;
        }
        public void SetPhase(AquariumMaintenancePhase phase, float progress = 0f) { Phase = phase; Progress = Mathf.Clamp01(Safe(progress)); }
        public void Complete(float cooldown)
        {
            Phase = AquariumMaintenancePhase.Completed; Progress = 1f; LastResult = AquariumMaintenanceResult.Success;
            ChangesPerformed++; TotalWaterChangedPercent += LastPercentage; CooldownRemaining = Safe(cooldown);
        }
        public void Cancel() { if (!IsActive || Phase == AquariumMaintenancePhase.Preparing) { Phase = AquariumMaintenancePhase.Cancelled; LastResult = AquariumMaintenanceResult.Cancelled; Progress = 0f; } }
        public void ReturnToIdle() { if (!IsActive) Phase = AquariumMaintenancePhase.Idle; }
        public void AdvanceCooldown(float seconds) => CooldownRemaining = Mathf.Max(0f, CooldownRemaining - Safe(seconds));
        public void RestoreStable(string id, AquariumMaintenanceResult result, int percentage, float cooldown,
            float totalChanged, int changes)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(nameof(id));
            InstanceId = id; Phase = AquariumMaintenancePhase.Idle; Progress = 0f;
            LastResult = result; LastPercentage = Mathf.Clamp(percentage, 0, 100);
            CooldownRemaining = Safe(cooldown); TotalWaterChangedPercent = Safe(totalChanged);
            ChangesPerformed = Math.Max(0, changes); IsInitialized = true;
        }
        private static float Safe(float value) => float.IsFinite(value) ? Mathf.Max(0f, value) : 0f;
    }
}
