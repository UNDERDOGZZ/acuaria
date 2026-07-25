using System.IO;
using Acuaria.Core;
using Acuaria.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AcuariaAudioSettings = Acuaria.Audio.AudioSettings;

namespace Acuaria.Editor
{
    public static class ProjectFoundationSetup
    {
        private const string Root = "Assets/_Acuaria";
        private static readonly string[] Folders =
        {
            "Art", "Audio", "Data", "Data/Definitions", "Data/Definitions/FishSpecies",
            "Data/Definitions/AquariumDefinition", "Data/Definitions/PlantDefinition",
            "Data/Definitions/DecorationDefinition", "Data/Definitions/EquipmentDefinition",
            "Docs", "Materials", "Prefabs", "Resources", "Scenes", "Settings", "Shaders",
            "Tests", "Tests/EditMode"
        };

        [MenuItem("Acuaria/Setup Project Foundation")]
        public static void Configure()
        {
            foreach (var folder in Folders)
            {
                Directory.CreateDirectory($"{Root}/{folder}");
            }

            var gameSettings = LoadOrCreate<GameSettings>($"{Root}/Settings/GameSettings.asset");
            LoadOrCreate<AcuariaAudioSettings>($"{Root}/Settings/AudioSettings.asset");
            LoadOrCreate<SimulationSettings>($"{Root}/Settings/SimulationSettings.asset");

            CreateEmptyScene($"{Root}/Scenes/Room.unity");
            CreateEmptyScene($"{Root}/Scenes/MainMenu.unity");
            CreateBootstrapScene($"{Root}/Scenes/Bootstrap.unity", gameSettings);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{Root}/Scenes/Bootstrap.unity", true),
                new EditorBuildSettingsScene($"{Root}/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene($"{Root}/Scenes/Room.unity", true)
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Acuaria project foundation configured.");
        }

        public static void ConfigureFromCommandLine()
        {
            Configure();
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void CreateEmptyScene(string path)
        {
            if (File.Exists(path))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
        }

        private static void CreateBootstrapScene(string path, GameSettings settings)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootstrapObject = new GameObject("Bootstrap");
            var bootstrapper = bootstrapObject.AddComponent<Bootstrapper>();
            var serializedBootstrapper = new SerializedObject(bootstrapper);
            serializedBootstrapper.FindProperty("gameSettings").objectReferenceValue = settings;
            serializedBootstrapper.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, path);
        }
    }
}
