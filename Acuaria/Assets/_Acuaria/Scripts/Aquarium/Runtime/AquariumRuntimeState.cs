using System;

namespace Acuaria.Aquarium
{
    [Serializable]
    public sealed class AquariumRuntimeState
    {
        public string InstanceId { get; private set; }
        public string DefinitionId { get; private set; }
        public float CurrentTemperature { get; private set; }
        public int CurrentFishCount { get; private set; }
        public bool IsInitialized { get; private set; }
        public long LogicalTimestamp { get; private set; }
        public bool IsAvailable { get; private set; }
        public bool IsFocused { get; private set; }

        public event Action Changed;

        public void Initialize(string instanceId, AquariumDefinition definition, int fishCount = 0)
        {
            if (string.IsNullOrWhiteSpace(instanceId)) throw new ArgumentException("Instance ID is required.");
            if (definition == null || !definition.IsValid) throw new ArgumentException("A valid definition is required.");
            InstanceId = instanceId;
            DefinitionId = definition.AquariumId;
            CurrentTemperature = definition.InitialTemperature;
            CurrentFishCount = Math.Max(0, fishCount);
            IsAvailable = true;
            IsFocused = false;
            IsInitialized = true;
            Touch();
        }

        public void SetTemperature(float value)
        {
            if (!float.IsFinite(value)) return;
            CurrentTemperature = Math.Clamp(value, -10f, 50f);
            Touch();
        }

        public void SetFishCount(int value)
        {
            CurrentFishCount = Math.Max(0, value);
            Touch();
        }

        public void SetFocused(bool focused)
        {
            if (IsFocused == focused) return;
            IsFocused = focused;
            Touch();
        }

        public void SetAvailable(bool available)
        {
            if (IsAvailable == available) return;
            IsAvailable = available;
            Touch();
        }

        private void Touch()
        {
            LogicalTimestamp++;
            Changed?.Invoke();
        }
    }
}
