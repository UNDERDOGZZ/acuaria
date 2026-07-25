using System.IO;
using Acuaria.Fish;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Acuaria.Editor
{
    public static class FirstFishSetup
    {
        private const string Root = "Assets/_Acuaria";
        private const string FishPrefabPath = Root + "/Prefabs/Fish/Fish2D.prefab";
        private const string AquariumPrefabPath = Root + "/Prefabs/Room/AquariumRoomDisplay.prefab";
        private const string RoomScenePath = Root + "/Scenes/Room.unity";
        private const string WhiteSpritePath = Root + "/Art/Prototype/Room/PrototypeWhite.png";
        private const string SpeciesRoot = Root + "/Data/FishSpecies";

        [MenuItem("Acuaria/Setup First Fish")]
        public static void Configure()
        {
            EnsureFolders();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);
            var fishPrefab = CreateFishPrefab(sprite);
            var blue = CreateSpecies("blue-dart", "Blue Dart", new Vector2(0.68f, 0.92f),
                new Vector2(0.72f, 0.82f), new Vector2(2.3f, 4.2f), 0.2f,
                new Color(0.23f, 0.72f, 0.95f), SwimmingLevel.Upper);
            var orange = CreateSpecies("amber-calm", "Amber Calm", new Vector2(0.42f, 0.62f),
                new Vector2(0.94f, 1.05f), new Vector2(3.4f, 5.8f), 0f,
                new Color(1f, 0.55f, 0.24f), SwimmingLevel.Middle);
            var violet = CreateSpecies("violet-bottom", "Violet Bottom", new Vector2(0.5f, 0.72f),
                new Vector2(0.78f, 0.9f), new Vector2(2.8f, 5.1f), -0.25f,
                new Color(0.66f, 0.4f, 0.92f), SwimmingLevel.Lower);

            ConfigureAquariumPrefab(fishPrefab, blue, orange, violet);
            var scene = EditorSceneManager.OpenScene(RoomScenePath, OpenSceneMode.Single);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("First fish configured: exactly three deterministic placeholder fish.");
        }

        public static void ConfigureFromCommandLine() => Configure();

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(Root + "/Prefabs/Fish");
            Directory.CreateDirectory(Root + "/Data/FishSpecies");
            Directory.CreateDirectory(Root + "/Art/Prototype/Fish");
        }

        private static FishView CreateFishPrefab(Sprite sprite)
        {
            var root = new GameObject("Fish2D");
            var movement = root.AddComponent<FishMovement2D>();
            var visual = root.AddComponent<FishVisual2D>();
            var view = root.AddComponent<FishView>();
            var visualRoot = new GameObject("VisualRoot").transform;
            visualRoot.SetParent(root.transform, false);

            var body = AddPart(visualRoot, "Body", sprite, new Vector2(0.72f, 0.34f), Vector2.zero, 20);
            var tail = AddPart(visualRoot, "Tail", sprite, new Vector2(0.3f, 0.3f), new Vector2(-0.43f, 0f), 19, 45f);
            var topFin = AddPart(visualRoot, "TopFin", sprite, new Vector2(0.24f, 0.13f), new Vector2(-0.02f, 0.22f), 19, 18f);
            var sideFin = AddPart(visualRoot, "SideFin", sprite, new Vector2(0.22f, 0.1f), new Vector2(0.02f, -0.08f), 21, -20f);
            var eye = AddPart(visualRoot, "Eye", sprite, new Vector2(0.07f, 0.07f), new Vector2(0.25f, 0.07f), 22);
            eye.GetComponent<SpriteRenderer>().color = new Color(0.04f, 0.05f, 0.1f);
            visual.Configure(visualRoot, tail.transform, sideFin.transform,
                new[] { body.GetComponent<SpriteRenderer>(), tail.GetComponent<SpriteRenderer>(),
                    topFin.GetComponent<SpriteRenderer>(), sideFin.GetComponent<SpriteRenderer>() });
            view.Configure(movement, visual);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, FishPrefabPath).GetComponent<FishView>();
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject AddPart(Transform parent, string name, Sprite sprite, Vector2 scale,
            Vector2 position, int order, float rotation = 0f)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            part.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            var renderer = part.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "AquariumContents";
            renderer.sortingOrder = order;
            return part;
        }

        private static FishSpeciesDefinition CreateSpecies(string id, string label, Vector2 speed,
            Vector2 scale, Vector2 duration, float preference, Color color, SwimmingLevel level)
        {
            var path = $"{SpeciesRoot}/{id}.asset";
            var species = AssetDatabase.LoadAssetAtPath<FishSpeciesDefinition>(path);
            if (species == null)
            {
                species = ScriptableObject.CreateInstance<FishSpeciesDefinition>();
                AssetDatabase.CreateAsset(species, path);
            }
            species.Configure(id, label, speed, scale, duration, preference, color, level);
            EditorUtility.SetDirty(species);
            return species;
        }

        private static void ConfigureAquariumPrefab(FishView fishPrefab, params FishSpeciesDefinition[] species)
        {
            var root = PrefabUtility.LoadPrefabContents(AquariumPrefabPath);
            var oldFish = root.transform.Find("FishPopulation");
            if (oldFish != null) Object.DestroyImmediate(oldFish.gameObject);
            var population = new GameObject("FishPopulation");
            population.transform.SetParent(root.transform, false);
            population.transform.localPosition = new Vector3(0f, -0.08f, -0.65f);
            var area = population.AddComponent<AquariumSwimArea2D>();
            area.Configure(new Vector2(7.65f, 3.55f), new Vector4(0.25f, 0.25f, 0.2f, 0.3f));
            var spawner = population.AddComponent<FishSpawner2D>();
            spawner.Configure(area, new[]
            {
                new FishSpawnEntry(species[0], 1, fishPrefab, 17011),
                new FishSpawnEntry(species[1], 1, fishPrefab, 29027),
                new FishSpawnEntry(species[2], 1, fishPrefab, 43037)
            });
            PrefabUtility.SaveAsPrefabAsset(root, AquariumPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
