using System.IO;
using Acuaria.Room;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Acuaria.Editor
{
    public static class RoomSceneSetup
    {
        private const string Root = "Assets/_Acuaria";
        private const string PrototypeArt = Root + "/Art/Prototype/Room";
        private const string PrefabRoot = Root + "/Prefabs/Room";
        private const string MaterialRoot = Root + "/Materials/Room";
        private const string RoomScenePath = Root + "/Scenes/Room.unity";
        private const string WhiteSpritePath = PrototypeArt + "/PrototypeWhite.png";

        private static readonly string[] SortingLayers =
        {
            "RoomBackground",
            "RoomEnvironment",
            "RoomFurniture",
            "AquariumBack",
            "AquariumContents",
            "AquariumFront",
            "RoomForeground",
            "Effects",
            "UI"
        };

        private static Sprite whiteSprite;

        [MenuItem("Acuaria/Setup Cozy Room")]
        public static void Configure()
        {
            EnsureFolders();
            EnsureSortingLayers();
            whiteSprite = EnsureWhiteSprite();

            var aquariumPrefab = CreateAquariumPrefab();
            var slotPrefab = CreateSlotPrefab();
            var lampPrefab = CreateLampPrefab();
            var plantPrefab = CreatePlantPrefab();

            CreateRoomScene(aquariumPrefab, slotPrefab, lampPrefab, plantPrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Acuaria cozy room configured.");
        }

        public static void ConfigureFromCommandLine()
        {
            Configure();
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(PrototypeArt);
            Directory.CreateDirectory(PrefabRoot);
            Directory.CreateDirectory(MaterialRoot);
        }

        private static void EnsureSortingLayers()
        {
            var tagManager = new SerializedObject(
                AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layers = tagManager.FindProperty("m_SortingLayers");

            foreach (var layerName in SortingLayers)
            {
                var exists = false;
                for (var index = 0; index < layers.arraySize; index++)
                {
                    if (layers.GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue == layerName)
                    {
                        exists = true;
                        break;
                    }
                }

                if (exists)
                {
                    continue;
                }

                var newIndex = layers.arraySize;
                layers.InsertArrayElementAtIndex(newIndex);
                var layer = layers.GetArrayElementAtIndex(newIndex);
                layer.FindPropertyRelative("name").stringValue = layerName;
                layer.FindPropertyRelative("uniqueID").longValue =
                    unchecked((uint)Animator.StringToHash($"Acuaria.{layerName}"));
                layer.FindPropertyRelative("locked").boolValue = false;
            }

            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Sprite EnsureWhiteSprite()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);
            if (existing != null)
            {
                return existing;
            }

            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            var pixels = new Color[16 * 16];
            for (var index = 0; index < pixels.Length; index++)
            {
                pixels[index] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(WhiteSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(WhiteSpritePath, ImportAssetOptions.ForceSynchronousImport);

            var importer = (TextureImporter)AssetImporter.GetAtPath(WhiteSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);
        }

        private static GameObject CreateAquariumPrefab()
        {
            var root = new GameObject("AquariumRoomDisplay");
            AddSprite(root.transform, "AquariumBackground", new Vector2(4.6f, 3.2f), new Vector3(0f, 0f, 0f),
                new Color(0.07f, 0.13f, 0.25f), "AquariumBack");
            AddSprite(root.transform, "Water", new Vector2(4.2f, 2.55f), new Vector3(0f, 0.05f, -0.1f),
                new Color(0.18f, 0.62f, 0.68f, 0.82f), "AquariumContents");
            AddSprite(root.transform, "Substrate", new Vector2(4.2f, 0.4f), new Vector3(0f, -1.03f, -0.2f),
                new Color(0.34f, 0.25f, 0.38f), "AquariumContents", 1);
            AddSprite(root.transform, "Rock", new Vector2(0.85f, 0.62f), new Vector3(-0.9f, -0.72f, -0.3f),
                new Color(0.38f, 0.39f, 0.52f), "AquariumContents", 2);

            var plant = new GameObject("AquaticPlant");
            plant.transform.SetParent(root.transform, false);
            plant.transform.localPosition = new Vector3(1.1f, -0.75f, -0.3f);
            AddSprite(plant.transform, "Stem", new Vector2(0.12f, 1.15f), Vector3.zero,
                new Color(0.19f, 0.51f, 0.42f), "AquariumContents", 2);
            AddSprite(plant.transform, "LeafLeft", new Vector2(0.45f, 0.18f), new Vector3(-0.18f, 0.2f, 0f),
                new Color(0.28f, 0.66f, 0.49f), "AquariumContents", 3, 25f);
            AddSprite(plant.transform, "LeafRight", new Vector2(0.45f, 0.18f), new Vector3(0.18f, 0.45f, 0f),
                new Color(0.35f, 0.73f, 0.53f), "AquariumContents", 3, -25f);

            AddSprite(root.transform, "Glass", new Vector2(4.2f, 2.55f), new Vector3(0f, 0.05f, -0.4f),
                new Color(0.72f, 0.91f, 1f, 0.12f), "AquariumFront");
            AddSprite(root.transform, "GlassReflection", new Vector2(0.15f, 2.15f), new Vector3(1.65f, 0.2f, -0.5f),
                new Color(0.9f, 0.98f, 1f, 0.32f), "AquariumFront", 1, -8f);

            var frameColor = new Color(0.13f, 0.16f, 0.28f);
            AddSprite(root.transform, "FrameTop", new Vector2(4.8f, 0.22f), new Vector3(0f, 1.55f, -0.6f),
                frameColor, "AquariumFront", 2);
            AddSprite(root.transform, "FrameBottom", new Vector2(4.8f, 0.25f), new Vector3(0f, -1.55f, -0.6f),
                frameColor, "AquariumFront", 2);
            AddSprite(root.transform, "FrameLeft", new Vector2(0.22f, 3.1f), new Vector3(-2.3f, 0f, -0.6f),
                frameColor, "AquariumFront", 2);
            AddSprite(root.transform, "FrameRight", new Vector2(0.22f, 3.1f), new Vector3(2.3f, 0f, -0.6f),
                frameColor, "AquariumFront", 2);

            var glow = root.AddComponent<Light2D>();
            glow.lightType = Light2D.LightType.Point;
            glow.color = new Color(0.32f, 0.82f, 0.82f);
            glow.intensity = 0.35f;
            glow.pointLightInnerRadius = 1.2f;
            glow.pointLightOuterRadius = 4.8f;
            glow.shadowsEnabled = false;

            return SavePrefab(root, $"{PrefabRoot}/AquariumRoomDisplay.prefab");
        }

        private static GameObject CreateSlotPrefab()
        {
            var root = new GameObject("AquariumSlotView");
            AddSprite(root.transform, "ReservedSpace", new Vector2(4.9f, 3.4f), Vector3.zero,
                new Color(0.28f, 0.34f, 0.5f, 0.16f), "RoomFurniture");
            var content = new GameObject("Content");
            content.transform.SetParent(root.transform, false);
            var slot = root.AddComponent<AquariumSlotView>();
            slot.Configure("slot-template", content.transform);
            return SavePrefab(root, $"{PrefabRoot}/AquariumSlotView.prefab");
        }

        private static GameObject CreateLampPrefab()
        {
            var root = new GameObject("RoomLamp");
            AddSprite(root.transform, "Base", new Vector2(1.1f, 0.22f), new Vector3(0f, -1.1f, 0f),
                new Color(0.25f, 0.2f, 0.34f), "RoomForeground");
            AddSprite(root.transform, "Stand", new Vector2(0.14f, 1.7f), new Vector3(0f, -0.25f, 0f),
                new Color(0.33f, 0.26f, 0.42f), "RoomForeground");
            AddSprite(root.transform, "Shade", new Vector2(1.15f, 0.72f), new Vector3(0f, 0.72f, 0f),
                new Color(0.92f, 0.59f, 0.55f), "RoomForeground", 1);

            var lightObject = new GameObject("WarmLight");
            lightObject.transform.SetParent(root.transform, false);
            lightObject.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            var light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Point;
            light.color = new Color(1f, 0.62f, 0.34f);
            light.intensity = 0.75f;
            light.pointLightInnerRadius = 0.3f;
            light.pointLightOuterRadius = 3.8f;
            light.shadowsEnabled = false;
            return SavePrefab(root, $"{PrefabRoot}/RoomLamp.prefab");
        }

        private static GameObject CreatePlantPrefab()
        {
            var root = new GameObject("DecorativePlant");
            AddSprite(root.transform, "Pot", new Vector2(1.05f, 0.8f), new Vector3(0f, -0.75f, 0f),
                new Color(0.72f, 0.38f, 0.48f), "RoomForeground");
            AddSprite(root.transform, "Stem", new Vector2(0.16f, 1.5f), new Vector3(0f, 0.2f, 0f),
                new Color(0.22f, 0.47f, 0.38f), "RoomForeground");
            AddSprite(root.transform, "LeafA", new Vector2(0.9f, 0.38f), new Vector3(-0.35f, 0.3f, 0f),
                new Color(0.33f, 0.63f, 0.48f), "RoomForeground", 1, 28f);
            AddSprite(root.transform, "LeafB", new Vector2(0.95f, 0.4f), new Vector3(0.37f, 0.62f, 0f),
                new Color(0.39f, 0.7f, 0.51f), "RoomForeground", 1, -25f);
            AddSprite(root.transform, "LeafC", new Vector2(0.8f, 0.36f), new Vector3(-0.28f, 0.95f, 0f),
                new Color(0.28f, 0.58f, 0.44f), "RoomForeground", 1, 38f);
            return SavePrefab(root, $"{PrefabRoot}/DecorativePlant.prefab");
        }

        private static void CreateRoomScene(
            GameObject aquariumPrefab,
            GameObject slotPrefab,
            GameObject lampPrefab,
            GameObject plantPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var room = new GameObject("Room");

            var systems = CreateGroup(room.transform, "Systems");
            var environment = CreateGroup(room.transform, "Environment");
            var aquariumArea = CreateGroup(room.transform, "AquariumArea");
            var ambientEffects = CreateGroup(room.transform, "AmbientEffects");
            var cameras = CreateGroup(room.transform, "Cameras");

            AddSprite(environment, "Background", new Vector2(24f, 14f), new Vector3(0f, 0f, 2f),
                new Color(0.055f, 0.075f, 0.16f), "RoomBackground");
            AddSprite(environment, "Wall", new Vector2(22f, 8.4f), new Vector3(0f, 1.35f, 1f),
                new Color(0.24f, 0.2f, 0.38f), "RoomEnvironment");
            AddSprite(environment, "Floor", new Vector2(22f, 3.2f), new Vector3(0f, -4.45f, 0.8f),
                new Color(0.19f, 0.13f, 0.25f), "RoomEnvironment", 1);
            AddSprite(environment, "FloorTrim", new Vector2(22f, 0.2f), new Vector3(0f, -2.84f, 0.6f),
                new Color(0.54f, 0.34f, 0.5f), "RoomEnvironment", 2);

            var window = CreateGroup(environment, "Window");
            window.localPosition = new Vector3(-6.8f, 1.45f, 0.4f);
            AddSprite(window, "WindowFrame", new Vector2(3.6f, 3.9f), Vector3.zero,
                new Color(0.48f, 0.37f, 0.57f), "RoomFurniture");
            AddSprite(window, "NightSky", new Vector2(3.15f, 3.45f), new Vector3(0f, 0f, -0.1f),
                new Color(0.08f, 0.17f, 0.32f), "RoomEnvironment", 2);
            AddSprite(window, "Moon", new Vector2(0.55f, 0.55f), new Vector3(0.75f, 0.8f, -0.2f),
                new Color(0.96f, 0.89f, 0.66f), "RoomEnvironment", 3);
            AddSprite(window, "CrossVertical", new Vector2(0.12f, 3.45f), new Vector3(0f, 0f, -0.3f),
                new Color(0.48f, 0.37f, 0.57f), "RoomFurniture", 1);
            AddSprite(window, "CrossHorizontal", new Vector2(3.15f, 0.12f), new Vector3(0f, 0f, -0.3f),
                new Color(0.48f, 0.37f, 0.57f), "RoomFurniture", 1);

            var furniture = CreateGroup(environment, "Furniture");
            AddSprite(furniture, "AquariumCabinetTop", new Vector2(7.3f, 0.35f), new Vector3(0.5f, -2.15f, 0f),
                new Color(0.55f, 0.32f, 0.43f), "RoomFurniture", 3);
            AddSprite(furniture, "AquariumCabinet", new Vector2(6.7f, 2.2f), new Vector3(0.5f, -3.4f, 0.1f),
                new Color(0.34f, 0.22f, 0.38f), "RoomFurniture", 2);
            AddSprite(furniture, "CabinetDivider", new Vector2(0.12f, 2f), new Vector3(0.5f, -3.4f, 0f),
                new Color(0.55f, 0.32f, 0.43f), "RoomFurniture", 3);

            var shelves = CreateGroup(environment, "Shelves");
            AddSprite(shelves, "Shelf", new Vector2(4.5f, 0.24f), new Vector3(5.9f, 2.65f, 0f),
                new Color(0.58f, 0.36f, 0.47f), "RoomFurniture", 3);
            AddSprite(shelves, "BookA", new Vector2(0.42f, 1.15f), new Vector3(5.1f, 3.35f, 0f),
                new Color(0.45f, 0.67f, 0.66f), "RoomFurniture", 4);
            AddSprite(shelves, "BookB", new Vector2(0.36f, 0.9f), new Vector3(5.55f, 3.22f, 0f),
                new Color(0.76f, 0.47f, 0.57f), "RoomFurniture", 4);
            AddSprite(shelves, "BookC", new Vector2(0.5f, 1.05f), new Vector3(6.02f, 3.3f, 0f),
                new Color(0.7f, 0.59f, 0.39f), "RoomFurniture", 4);

            var plants = CreateGroup(environment, "DecorativePlants");
            var plant = (GameObject)PrefabUtility.InstantiatePrefab(plantPrefab);
            plant.name = "DecorativePlant";
            plant.transform.SetParent(plants, false);
            plant.transform.localPosition = new Vector3(7.6f, -2.2f, 0f);

            var lamps = CreateGroup(environment, "Lamps");
            var lamp = (GameObject)PrefabUtility.InstantiatePrefab(lampPrefab);
            lamp.name = "RoomLamp";
            lamp.transform.SetParent(lamps, false);
            lamp.transform.localPosition = new Vector3(-3.9f, -1.05f, 0f);

            var slotPositions = new[]
            {
                new Vector3(0.5f, 0f, 0f),
                new Vector3(-5.9f, -0.25f, 0f),
                new Vector3(6.1f, -0.25f, 0f)
            };

            AquariumSlotView initialSlot = null;
            for (var index = 0; index < slotPositions.Length; index++)
            {
                var slotObject = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab);
                slotObject.name = $"AquariumSlot_{index + 1:00}";
                slotObject.transform.SetParent(aquariumArea, false);
                slotObject.transform.localPosition = slotPositions[index];
                if (index > 0)
                {
                    slotObject.transform.localScale = Vector3.one * 0.66f;
                }

                var slot = slotObject.GetComponent<AquariumSlotView>();
                var content = slotObject.transform.Find("Content");
                slot.Configure($"slot-{index + 1:00}", content);

                if (index == 0)
                {
                    var aquarium = (GameObject)PrefabUtility.InstantiatePrefab(aquariumPrefab);
                    aquarium.name = "AquariumRoomDisplay";
                    slot.AssignView(aquarium);
                    initialSlot = slot;
                }
            }

            var windowGlow = AddSprite(ambientEffects, "WindowGlow", new Vector2(4.4f, 4.6f),
                new Vector3(-6.8f, 1.45f, -0.7f), new Color(0.35f, 0.54f, 0.86f, 0.1f), "Effects");
            windowGlow.SetActive(true);
            AddSprite(ambientEffects, "LightAccents", new Vector2(7.4f, 0.22f),
                new Vector3(0.5f, -1.9f, -0.8f), new Color(0.36f, 0.83f, 0.78f, 0.24f), "Effects", 1);

            var globalLightObject = new GameObject("AmbientNightLight");
            globalLightObject.transform.SetParent(ambientEffects, false);
            var globalLight = globalLightObject.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.color = new Color(0.55f, 0.59f, 0.82f);
            globalLight.intensity = 0.58f;

            var windowLightObject = new GameObject("WindowLight");
            windowLightObject.transform.SetParent(ambientEffects, false);
            windowLightObject.transform.localPosition = new Vector3(-6.8f, 1.45f, 0f);
            var windowLight = windowLightObject.AddComponent<Light2D>();
            windowLight.lightType = Light2D.LightType.Point;
            windowLight.color = new Color(0.38f, 0.58f, 1f);
            windowLight.intensity = 0.4f;
            windowLight.pointLightInnerRadius = 1f;
            windowLight.pointLightOuterRadius = 5.2f;
            windowLight.shadowsEnabled = false;

            var cameraObject = new GameObject("MainCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(cameras, false);
            cameraObject.transform.localPosition = new Vector3(0f, 0f, -10f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.625f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.1f);
            camera.allowHDR = false;
            camera.allowMSAA = false;
            cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraObject.AddComponent<RoomCameraFitter>();
            cameraObject.AddComponent<AudioListener>();

            var controller = systems.gameObject.AddComponent<RoomCompositionController>();
            var serializedController = new SerializedObject(controller);
            serializedController.FindProperty("initialAquariumSlot").objectReferenceValue = initialSlot;
            serializedController.FindProperty("ambientEffects").objectReferenceValue = ambientEffects.gameObject;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, RoomScenePath);
        }

        private static Transform CreateGroup(Transform parent, string name)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            return group.transform;
        }

        private static GameObject AddSprite(
            Transform parent,
            string name,
            Vector2 size,
            Vector3 localPosition,
            Color color,
            string sortingLayer,
            int sortingOrder = 0,
            float rotation = 0f)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
            child.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = whiteSprite;
            renderer.color = color;
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = sortingOrder;
            return child;
        }

        private static GameObject SavePrefab(GameObject root, string path)
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}
