using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Fish.Tests
{
    public sealed class FishArchitectureTests
    {
        [Test]
        public void Species_ClampsAuthoringValuesAndRequiresId()
        {
            var species = ScriptableObject.CreateInstance<FishSpeciesDefinition>();
            try
            {
                species.Configure("", "Invalid", new Vector2(-1f, -2f), new Vector2(-1f, -2f),
                    new Vector2(-1f, -2f), 0f, Color.white, SwimmingLevel.Any);
                Assert.That(species.IsValid, Is.False);
                species.Configure("valid", "Valid", new Vector2(-1f, -2f), new Vector2(-1f, -2f),
                    new Vector2(-1f, -2f), 0f, Color.white, SwimmingLevel.Any);
                Assert.That(species.IsValid, Is.True);
                Assert.That(species.MaximumSpeed, Is.GreaterThanOrEqualTo(species.MinimumSpeed));
                Assert.That(species.MinimumScale, Is.GreaterThan(0f));
            }
            finally { Object.DestroyImmediate(species); }
        }

        [Test]
        public void RuntimeState_InitializesWithoutVisualDependencies()
        {
            var state = new FishRuntimeState();
            state.Initialize("fish-1", "species-1", new Vector2(1f, 2f), 42);
            Assert.That(state.IsInitialized, Is.True);
            Assert.That(state.InstanceId, Is.EqualTo("fish-1"));
            Assert.That(state.RandomSeed, Is.EqualTo(42));
            Assert.That(float.IsNaN(state.Position.x), Is.False);
            Assert.That(float.IsNaN(state.Direction.x), Is.False);
        }

        [TestCase(SwimmingLevel.Upper)]
        [TestCase(SwimmingLevel.Middle)]
        [TestCase(SwimmingLevel.Lower)]
        public void Bounds_LevelZonesRemainInsideArea(SwimmingLevel level)
        {
            var bounds = new SwimBounds2D(-4f, 4f, -2f, 2f);
            var zone = bounds.ForLevel(level);
            Assert.That(bounds.Contains(new Vector2(zone.Left, zone.Bottom)), Is.True);
            Assert.That(bounds.Contains(new Vector2(zone.Right, zone.Top)), Is.True);
            Assert.That(float.IsNaN(zone.Top), Is.False);
        }

        [Test]
        public void Clamp_RespectsEveryBoundary()
        {
            var bounds = new SwimBounds2D(-3f, 3f, -1f, 1f);
            Assert.That(bounds.Clamp(new Vector2(-20f, 20f)), Is.EqualTo(new Vector2(-3f, 1f)));
        }

        [Test]
        public void Inset_ProducesValidPositiveBoundsAndHandlesExcessivePadding()
        {
            var bounds = new SwimBounds2D(-3f, 3f, -1f, 1f);
            var inset = bounds.Inset(100f, float.PositiveInfinity);
            Assert.That(inset.IsValid, Is.True);
            Assert.That(inset.Width, Is.GreaterThan(0f));
            Assert.That(inset.Height, Is.GreaterThan(0f));
            Assert.That(bounds.Contains(inset.Center), Is.True);
        }

        [Test]
        public void SameSeed_ProducesEquivalentSequence()
        {
            var bounds = new SwimBounds2D(-3f, 3f, -1f, 1f);
            var first = new FishMovementModel2D(1234);
            var second = new FishMovementModel2D(1234);
            for (var index = 0; index < 8; index++)
            {
                Assert.That(first.ChooseTarget(bounds, SwimmingLevel.Any, 0f),
                    Is.EqualTo(second.ChooseTarget(bounds, SwimmingLevel.Any, 0f)));
            }
        }

        [Test]
        public void DifferentSeeds_ProduceDifferentTargets()
        {
            var bounds = new SwimBounds2D(-3f, 3f, -1f, 1f);
            Assert.That(new FishMovementModel2D(1).ChooseTarget(bounds, SwimmingLevel.Any, 0f),
                Is.Not.EqualTo(new FishMovementModel2D(2).ChooseTarget(bounds, SwimmingLevel.Any, 0f)));
        }

        [TestCase(-2.8f, 1)]
        [TestCase(2.8f, -1)]
        public void WanderTarget_PrefersOppositeSideAndRespectsMinimumTravel(float currentX, int expectedDirection)
        {
            var bounds = new SwimBounds2D(-3f, 3f, -1f, 1f);
            var current = new Vector2(currentX, 0f);
            var target = new FishMovementModel2D(17).ChooseWanderTarget(
                bounds, SwimmingLevel.Any, 0f, current, 1.5f);
            Assert.That(bounds.Contains(target), Is.True);
            Assert.That(Mathf.Abs(target.x - current.x), Is.GreaterThanOrEqualTo(1.5f));
            Assert.That(Mathf.Sign(target.x - current.x), Is.EqualTo(expectedDirection));
            Assert.That(float.IsFinite(target.x) && float.IsFinite(target.y), Is.True);
        }

        [Test]
        public void WanderTarget_FallbackIsFiniteAndInsideWhenAttemptsAreLimited()
        {
            var bounds = new SwimBounds2D(-3f, 3f, -1f, 1f);
            var target = new FishMovementModel2D(4).ChooseWanderTarget(
                bounds, SwimmingLevel.Middle, 0f, Vector2.zero, 5.4f, 1);
            Assert.That(bounds.Contains(target), Is.True);
            Assert.That(float.IsFinite(target.x) && float.IsFinite(target.y), Is.True);
        }

        [Test]
        public void Arrival_UsesThresholdAndBoundaryHorizontalFallback()
        {
            var bounds = new SwimBounds2D(-3f, 3f, -1f, 1f);
            Assert.That(FishMovementModel2D.TargetReached(Vector2.zero, new Vector2(0.1f, 0.1f),
                bounds, 0.2f, 0.1f), Is.True);
            Assert.That(FishMovementModel2D.TargetReached(Vector2.zero, Vector2.one,
                bounds, 0.2f, 0.1f), Is.False);
            Assert.That(FishMovementModel2D.TargetReached(new Vector2(2.95f, 0f), new Vector2(3f, 0.8f),
                bounds, 0.2f, 0.1f), Is.True);
        }

        [TestCase(-2.95f, -1f, true)]
        [TestCase(2.95f, 1f, true)]
        [TestCase(0f, 1f, false)]
        public void BoundaryRecovery_DetectsOnlyOutwardMotion(float positionX, float directionX, bool expected)
        {
            var bounds = new SwimBounds2D(-3f, 3f, -1f, 1f);
            var target = new Vector2(positionX + directionX, 0f);
            Assert.That(FishMovementModel2D.NeedsBoundaryRecovery(
                new Vector2(positionX, 0f), new Vector2(directionX, 0f), target, bounds, 0.1f), Is.EqualTo(expected));
        }

        [TestCase(-2.95f, 1)]
        [TestCase(2.95f, -1)]
        public void BoundaryRecovery_DirectionPointsInsideAndNeverTeleports(float positionX, int expectedDirection)
        {
            var bounds = new SwimBounds2D(-3f, 3f, -1f, 1f);
            var position = new Vector2(positionX, 0f);
            var direction = FishMovementModel2D.InteriorRecoveryDirection(position, Vector2.zero, bounds);
            Assert.That(Mathf.Sign(direction.x), Is.EqualTo(expectedDirection));
            Assert.That(position.x, Is.EqualTo(positionX));
            Assert.That(direction.sqrMagnitude, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Step_StaysInsideBoundsAndLimitsVerticalDirection()
        {
            var bounds = new SwimBounds2D(-3f, 3f, -1f, 1f);
            var state = new FishRuntimeState();
            state.Initialize("fish", "species", Vector2.zero, 8);
            state.Target = new Vector2(0.01f, 1f);
            state.CurrentSpeed = 1f;
            state.TargetDuration = 10f;
            var model = new FishMovementModel2D(8);
            for (var index = 0; index < 100; index++) model.Step(state, bounds, 0.02f);
            Assert.That(bounds.Contains(state.Position), Is.True);
            Assert.That(Mathf.Abs(state.Direction.y), Is.LessThanOrEqualTo(0.5f));
            Assert.That(float.IsNaN(state.Position.x), Is.False);
        }

        [Test]
        public void Separation_AffectsOnlyNearbyFish()
        {
            var close = FishMovementModel2D.Separation(Vector2.zero, new Vector2(0.1f, 0f), 1f, 0.2f);
            var far = FishMovementModel2D.Separation(Vector2.zero, new Vector2(2f, 0f), 1f, 0.2f);
            Assert.That(close.sqrMagnitude, Is.GreaterThan(0f));
            Assert.That(far, Is.EqualTo(Vector2.zero));
            Assert.That(float.IsNaN(close.x), Is.False);
        }
    }
}
