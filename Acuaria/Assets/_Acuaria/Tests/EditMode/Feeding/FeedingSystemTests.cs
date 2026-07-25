using Acuaria.Food;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Feeding.Tests
{
    public sealed class FeedingSystemTests
    {
        [Test]
        public void Definition_ClampsValuesButRequiresId()
        {
            var definition = ScriptableObject.CreateInstance<FoodDefinition>();
            try
            {
                definition.Configure("", "Invalid", -1f, -1f, -1f, -1f, new Vector2(-1f, -2f),
                    Color.white, -1f, FoodTargetZone.Surface);
                Assert.That(definition.IsValid, Is.False);
                definition.Configure("flakes", "Flakes", -1f, -1f, -1f, -1f, new Vector2(-1f, -2f),
                    Color.white, -1f, FoodTargetZone.Surface);
                Assert.That(definition.IsValid, Is.True);
                Assert.That(definition.FallSpeed, Is.Positive);
                Assert.That(definition.MaximumLifetime, Is.Positive);
                Assert.That(definition.DetectionRadius, Is.Positive);
                Assert.That(definition.ConsumptionRadius, Is.Positive);
                Assert.That(definition.MaximumVisualSize, Is.GreaterThanOrEqualTo(definition.MinimumVisualSize));
                Assert.That(definition.Nutrition, Is.GreaterThanOrEqualTo(0f));
            }
            finally { Object.DestroyImmediate(definition); }
        }

        [Test]
        public void RuntimeState_ClaimsReleasesConsumesAndExpiresSafely()
        {
            var state = new FoodRuntimeState();
            state.Initialize("food-1", "flakes", new Vector2(1f, 2f), 0.2f, 10f);
            Assert.That(state.State, Is.EqualTo(FoodState.Falling));
            Assert.That(state.TryClaim("fish-a"), Is.True);
            Assert.That(state.TryClaim("fish-b"), Is.False);
            state.Release("fish-a");
            Assert.That(state.State, Is.EqualTo(FoodState.Available));
            Assert.That(state.TryClaim("fish-b"), Is.True);
            Assert.That(state.Consume("fish-a"), Is.False);
            Assert.That(state.Consume("fish-b"), Is.True);
            Assert.That(state.IsConsumed, Is.True);
            Assert.That(float.IsNaN(state.Position.x), Is.False);

            var expiring = new FoodRuntimeState();
            expiring.Initialize("food-2", "flakes", Vector2.zero, 0.2f, 1f);
            expiring.Expire();
            Assert.That(expiring.State, Is.EqualTo(FoodState.Expired));
        }

        [Test]
        public void FeedingArea_ValidatesAndProjectsSurface()
        {
            var root = new GameObject("Area");
            try
            {
                var area = root.AddComponent<AquariumFeedingArea2D>();
                area.Configure(new Vector2(8f, 4f), 0.5f, 0.9f);
                Assert.That(area.ContainsWorldPoint(Vector2.zero), Is.True);
                Assert.That(area.ContainsWorldPoint(new Vector2(20f, 0f)), Is.False);
                var projected = area.ProjectWorldToSurface(new Vector2(2f, -1f));
                Assert.That(projected.x, Is.EqualTo(2f).Within(0.001f));
                Assert.That(projected.y, Is.GreaterThan(1f));
                Assert.That(float.IsNaN(projected.y), Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void FoodController_LimitsClaimsAndConsumesUnits()
        {
            var definition = ScriptableObject.CreateInstance<FoodDefinition>();
            var prefabObject = new GameObject("FoodPrefab");
            var controllerObject = new GameObject("Controller");
            try
            {
                definition.Configure("flakes", "Flakes", 0.2f, 10f, 4f, 0.3f,
                    new Vector2(0.1f, 0.2f), Color.yellow, 0.2f, FoodTargetZone.Surface);
                var renderer = prefabObject.AddComponent<SpriteRenderer>();
                var movement = prefabObject.AddComponent<FoodMovement2D>();
                var view = prefabObject.AddComponent<FoodView2D>();
                view.Configure(renderer, movement);
                var controller = controllerObject.AddComponent<AquariumFoodController>();
                controller.Configure(definition, view, 8, new Vector2(-3f, -1f), new Vector2(3f, 1f));
                Assert.That(controller.SpawnPortion(Vector2.zero, 1, 4), Is.EqualTo(4));
                Assert.That(controller.SpawnPortion(Vector2.zero, 2, 4), Is.EqualTo(4));
                Assert.That(controller.SpawnPortion(Vector2.zero, 3, 4), Is.Zero);
                Assert.That(controller.ActiveCount, Is.EqualTo(8));
                var food = controller.ActiveFood[0];
                Assert.That(controller.TryClaim(food, "fish-a"), Is.True);
                Assert.That(controller.TryClaim(food, "fish-b"), Is.False);
                Assert.That(controller.Consume(food, "fish-b"), Is.False);
                Assert.That(controller.Consume(food, "fish-a"), Is.True);
                Assert.That(controller.ActiveCount, Is.EqualTo(7));
            }
            finally
            {
                Object.DestroyImmediate(controllerObject);
                Object.DestroyImmediate(prefabObject);
                Object.DestroyImmediate(definition);
            }
        }
    }
}
