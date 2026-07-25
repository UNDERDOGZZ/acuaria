using System.Collections.Generic;
using Acuaria.Simulation.Time;
using Acuaria.Simulation.Waste;
using Acuaria.Simulation.Water;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Aquarium.Tests
{
    public sealed class WaterChemistryTests
    {
        private WaterChemistryDefinition definition;

        [SetUp]
        public void SetUp()
        {
            definition = ScriptableObject.CreateInstance<WaterChemistryDefinition>();
            definition.Configure("test", 50f, new Vector3(0f, 0f, 7f), new Vector2(0.55f, 0.55f),
                new Vector2(0.5f, 0.5f), new Vector2(0.1f, 0.01f),
                new Vector3(0.1f, 0.2f, 0.01f), new Vector2(100f, 100f),
                new Vector2(1f, 60f), 3, 0.0001f, WaterQualityThresholds.Default);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(definition);

        [Test]
        public void Definition_ValidatesRequiredAndOrderedConfiguration()
        {
            var issues = new List<string>();
            Assert.That(definition.Validate(issues), Is.True);
            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void State_ClampsInvalidValuesAndSnapshotIsIndependent()
        {
            var state = NewState();
            state.SetDevelopmentValues(float.NaN, -2f, float.PositiveInfinity, 2f, -1f, -4f, definition);
            var snapshot = state.Snapshot();
            state.AddAmmonia(1f, definition);
            Assert.That(snapshot.AmmoniaMgPerLiter, Is.Zero);
            Assert.That(state.AmmoniaMgPerLiter, Is.EqualTo(1f));
            Assert.That(state.NitriteMgPerLiter, Is.Zero);
            Assert.That(state.NitrateMgPerLiter, Is.Zero);
            Assert.That(state.AmmoniaOxidizingBacteria, Is.EqualTo(1f));
            Assert.That(state.NitriteOxidizingBacteria, Is.Zero);
            Assert.That(state.OrganicWaste, Is.Zero);
        }

        [Test]
        public void Waste_ScalesWithFishAndVolumeSafely()
        {
            var zero = AquariumWasteModel.FishWaste(0, 1f, definition);
            var three = AquariumWasteModel.FishWaste(3, 1f, definition);
            Assert.That(zero, Is.Zero);
            Assert.That(three, Is.GreaterThan(0f));
            Assert.That(AquariumWasteModel.ToConcentration(three, 100f),
                Is.LessThan(AquariumWasteModel.ToConcentration(three, 50f)));
            Assert.That(AquariumWasteModel.ExpiredFoodWaste(1, definition), Is.EqualTo(0.2f));
        }

        [Test]
        public void NitrogenCycle_IsDeterministicConservativeAndNonNegative()
        {
            var source = NewState();
            source.SetDevelopmentValues(1f, 0.5f, 7f, 0.8f, 0.8f, 2f, definition);
            var model = new NitrogenCycleModel();
            var first = model.Step(source, definition, 3600f);
            var second = model.Step(source, definition, 3600f);
            Assert.That(first.AmmoniaMgPerLiter, Is.EqualTo(second.AmmoniaMgPerLiter));
            Assert.That(first.AmmoniaMgPerLiter, Is.LessThan(1.1f));
            Assert.That(first.NitriteMgPerLiter, Is.GreaterThanOrEqualTo(0f));
            Assert.That(first.NitrateMgPerLiter, Is.GreaterThan(7f));
            Assert.That(first.OrganicWaste, Is.LessThan(2f));
        }

        [Test]
        public void NitrogenCycle_ZeroDeltaDoesNotChangeValues()
        {
            var source = NewState();
            source.AddAmmonia(1f, definition);
            var next = new NitrogenCycleModel().Step(source, definition, 0f);
            Assert.That(next.AmmoniaMgPerLiter, Is.EqualTo(source.AmmoniaMgPerLiter));
            Assert.That(next.TotalSimulatedSeconds, Is.EqualTo(source.TotalSimulatedSeconds));
        }

        [Test]
        public void Clock_TicksCapsPausesAndResumes()
        {
            var clock = new AquariumSimulationClock(1f, 60f, 2);
            var ticks = 0;
            var simulated = 0f;
            Assert.That(clock.Advance(0.5f, value => { ticks++; simulated += value; }), Is.Zero);
            Assert.That(clock.Advance(3f, value => { ticks++; simulated += value; }), Is.EqualTo(2));
            Assert.That(simulated, Is.EqualTo(120f));
            clock.Pause();
            Assert.That(clock.Advance(2f, _ => ticks++), Is.Zero);
            clock.Resume();
            Assert.That(clock.Advance(1f, _ => ticks++), Is.GreaterThan(0));
        }

        [Test]
        public void Evaluators_WorstParameterDominatesAndViewModelFormatsUnits()
        {
            var state = NewState();
            state.SetDevelopmentValues(0.6f, 0f, 7f, 0.8f, 0.8f, 0f, definition);
            var cycle = AquariumCycleEvaluator.Evaluate(state, definition);
            var quality = WaterQualityEvaluator.Evaluate(state, definition, cycle);
            var viewModel = new WaterChemistryViewModel(state, definition);
            Assert.That(quality.Status, Is.EqualTo(WaterQualityStatus.Dangerous));
            Assert.That(viewModel.AmmoniaText, Does.Contain("mg/L"));
            Assert.That(viewModel.NitriteText, Does.Contain("mg/L"));
            Assert.That(viewModel.NitrateText, Does.Contain("mg/L"));
            Assert.That(viewModel.DetailsText, Does.Contain("NH₃/NH₄"));
            Assert.That(viewModel.ContextualTip, Is.Not.Empty);
        }

        private WaterChemistryState NewState()
        {
            var state = new WaterChemistryState();
            state.Initialize("test-state", definition);
            return state;
        }
    }
}
