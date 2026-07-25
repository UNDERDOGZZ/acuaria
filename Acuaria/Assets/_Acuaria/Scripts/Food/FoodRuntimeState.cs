using System;
using UnityEngine;

namespace Acuaria.Food
{
    [Serializable]
    public sealed class FoodRuntimeState
    {
        public string InstanceId { get; private set; }
        public string DefinitionId { get; private set; }
        public Vector2 Position { get; set; }
        public float CurrentSpeed { get; set; }
        public float RemainingLifetime { get; set; }
        public FoodState State { get; private set; }
        public string ClaimedByFishId { get; private set; }
        public bool IsConsumed => State == FoodState.Consumed;
        public bool IsTerminal => State is FoodState.Consumed or FoodState.Expired;

        public void Initialize(string instanceId, string definitionId, Vector2 position, float speed, float lifetime)
        {
            if (string.IsNullOrWhiteSpace(instanceId) || string.IsNullOrWhiteSpace(definitionId))
                throw new ArgumentException("Food instance and definition IDs are required.");
            InstanceId = instanceId;
            DefinitionId = definitionId;
            Position = position;
            CurrentSpeed = Mathf.Max(0f, speed);
            RemainingLifetime = Mathf.Max(0f, lifetime);
            State = FoodState.Falling;
            ClaimedByFishId = null;
        }

        public void MakeAvailable()
        {
            if (!IsTerminal && State != FoodState.Claimed) State = FoodState.Available;
        }

        public bool TryClaim(string fishId)
        {
            if (string.IsNullOrWhiteSpace(fishId) || IsTerminal) return false;
            if (State == FoodState.Claimed) return ClaimedByFishId == fishId;
            if (State is not (FoodState.Available or FoodState.Falling)) return false;
            State = FoodState.Claimed;
            ClaimedByFishId = fishId;
            return true;
        }

        public void Release(string fishId)
        {
            if (State == FoodState.Claimed && ClaimedByFishId == fishId)
            {
                ClaimedByFishId = null;
                State = FoodState.Available;
            }
        }

        public bool Consume(string fishId)
        {
            if (State != FoodState.Claimed || ClaimedByFishId != fishId) return false;
            State = FoodState.Consumed;
            return true;
        }

        public void Expire()
        {
            if (State != FoodState.Consumed) State = FoodState.Expired;
        }
    }
}
