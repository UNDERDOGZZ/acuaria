using System.Collections.Generic;
using Acuaria.Fish;
using Acuaria.UI.Aquarium;
using NUnit.Framework;
using UnityEngine;

namespace Acuaria.Aquarium.Tests
{
    public sealed class AquariumSystemTests
    {
        private const string Tip =
            "El tamaño del acuario importa: un mayor volumen suele ofrecer más estabilidad.";

        [Test]
        public void Definition_ValidatesRequiredFieldsAndNormalizesRanges()
        {
            var definition = ScriptableObject.CreateInstance<AquariumDefinition>();
            try
            {
                definition.Configure("", "", -1f, new Vector2(28f, 20f), 100f, -2, "", Tip, Color.cyan);
                Assert.That(definition.IsValid, Is.False);
                definition.Configure("starter", "Acuario Inicial", 50f, new Vector2(24f, 26f), 25f, 3,
                    "Inicial", Tip, Color.cyan);
                Assert.That(definition.IsValid, Is.True);
                Assert.That(definition.NominalVolumeLitres, Is.Positive);
                Assert.That(definition.TargetTemperatureMax, Is.GreaterThanOrEqualTo(definition.TargetTemperatureMin));
                Assert.That(definition.InitialTemperature, Is.InRange(-10f, 50f));
                Assert.That(definition.RecommendedFishCapacity, Is.GreaterThanOrEqualTo(0));
            }
            finally { Object.DestroyImmediate(definition); }
        }

        [Test]
        public void RuntimeState_InitializesAndClampsMutableValues()
        {
            var definition = CreateDefinition();
            try
            {
                var state = new AquariumRuntimeState();
                state.Initialize("instance-01", definition, -4);
                Assert.That(state.InstanceId, Is.EqualTo("instance-01"));
                Assert.That(state.DefinitionId, Is.EqualTo("starter"));
                Assert.That(state.CurrentTemperature, Is.EqualTo(25f));
                Assert.That(state.CurrentFishCount, Is.Zero);
                Assert.That(state.IsInitialized, Is.True);
                state.SetFishCount(-1);
                state.SetTemperature(float.NaN);
                state.SetFocused(true);
                Assert.That(state.CurrentFishCount, Is.Zero);
                Assert.That(float.IsNaN(state.CurrentTemperature), Is.False);
                Assert.That(state.IsFocused, Is.True);
            }
            finally { Object.DestroyImmediate(definition); }
        }

        [Test]
        public void StatusEvaluator_CoversTemperatureAndCapacitySeverities()
        {
            var definition = CreateDefinition();
            try
            {
                var state = new AquariumRuntimeState();
                state.Initialize("instance", definition, 2);
                Assert.That(AquariumStatusEvaluator.Evaluate(definition, state).Status,
                    Is.EqualTo(AquariumStatus.Excellent));
                state.SetFishCount(3);
                Assert.That(AquariumStatusEvaluator.Evaluate(definition, state).Status,
                    Is.EqualTo(AquariumStatus.Good));
                state.SetTemperature(27f);
                Assert.That(AquariumStatusEvaluator.Evaluate(definition, state).Status,
                    Is.EqualTo(AquariumStatus.Attention));
                state.SetTemperature(30f);
                Assert.That(AquariumStatusEvaluator.Evaluate(definition, state).Status,
                    Is.EqualTo(AquariumStatus.Critical));
                state.SetTemperature(25f);
                state.SetFishCount(4);
                Assert.That(AquariumStatusEvaluator.Evaluate(definition, state).Status,
                    Is.EqualTo(AquariumStatus.Attention));
                state.SetFishCount(5);
                Assert.That(AquariumStatusEvaluator.Evaluate(definition, state).Status,
                    Is.EqualTo(AquariumStatus.Critical));

                definition.Configure("zero", "Sin capacidad", 50f, new Vector2(24f, 26f), 25f, 0, "", Tip,
                    Color.cyan);
                state.SetFishCount(0);
                Assert.That(AquariumStatusEvaluator.Evaluate(definition, state).Severity, Is.GreaterThanOrEqualTo(0));
            }
            finally { Object.DestroyImmediate(definition); }
        }

        [Test]
        public void ViewModel_FormatsAquariumAndInhabitants()
        {
            var definition = CreateDefinition();
            try
            {
                var state = new AquariumRuntimeState();
                state.Initialize("instance", definition, 3);
                var inhabitants = new List<AquariumInhabitant>
                {
                    new("blue", "Pez azul", "Zona superior", 2),
                    new("amber", "Pez naranja", "Zona media", 1)
                };
                var model = new AquariumViewModel(definition, state, inhabitants);
                Assert.That(model.DisplayName, Is.EqualTo("Acuario Inicial"));
                Assert.That(model.VolumeText, Is.EqualTo("50 L"));
                Assert.That(model.TemperatureText, Is.EqualTo("25 °C"));
                Assert.That(model.CapacityText, Is.EqualTo("3 de 3 peces"));
                Assert.That(model.InhabitantsText, Does.Contain("Pez azul x2"));
                Assert.That(model.StatusLabel, Is.EqualTo("Estable"));
                Assert.That(model.EducationTip, Is.Not.Empty);
            }
            finally { Object.DestroyImmediate(definition); }
        }

        [Test]
        public void InhabitantProvider_GroupsValidSpeciesAndEmitsUpdates()
        {
            var root = new GameObject("Provider");
            var provider = root.AddComponent<AquariumInhabitantProvider>();
            var blue = CreateSpecies("blue", "Pez azul", SwimmingLevel.Upper);
            var amber = CreateSpecies("amber", "Pez naranja", SwimmingLevel.Middle);
            var updates = 0;
            provider.PopulationChanged += () => updates++;
            try
            {
                provider.Rebuild(new List<FishSpeciesDefinition> { blue, blue, null, amber });
                Assert.That(provider.TotalCount, Is.EqualTo(3));
                Assert.That(provider.Inhabitants.Count, Is.EqualTo(2));
                Assert.That(provider.Inhabitants[0].Count, Is.EqualTo(2));
                Assert.That(updates, Is.EqualTo(1));
                provider.Rebuild(null);
                Assert.That(provider.TotalCount, Is.Zero);
                Assert.That(provider.Inhabitants, Is.Empty);
                Assert.That(updates, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(blue);
                Object.DestroyImmediate(amber);
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void HudController_VisibilityFollowsFocusedState()
        {
            var root = new GameObject("HUD Controller");
            var compact = new GameObject("Compact HUD");
            compact.transform.SetParent(root.transform);
            var controller = root.AddComponent<AquariumHUDController>();
            controller.Configure(null, null, null, null, compact, null, null, null, null, null, null, null);
            try
            {
                controller.SetAquariumFocused(false);
                Assert.That(controller.IsVisible, Is.False);
                controller.SetAquariumFocused(true);
                Assert.That(controller.IsVisible, Is.True);
                controller.SetInteractionEnabled(false);
                controller.SetAquariumFocused(false);
                Assert.That(controller.IsVisible, Is.False);
                Assert.That(controller.AreDetailsOpen, Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static AquariumDefinition CreateDefinition()
        {
            var definition = ScriptableObject.CreateInstance<AquariumDefinition>();
            definition.Configure("starter", "Acuario Inicial", 50f, new Vector2(24f, 26f), 25f, 3,
                "Inicial", Tip, Color.cyan);
            return definition;
        }

        private static FishSpeciesDefinition CreateSpecies(string id, string name, SwimmingLevel level)
        {
            var species = ScriptableObject.CreateInstance<FishSpeciesDefinition>();
            species.Configure(id, name, new Vector2(0.4f, 0.6f), new Vector2(0.8f, 1f),
                new Vector2(2f, 4f), 0f, Color.white, level);
            return species;
        }
    }
}
