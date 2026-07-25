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
            var carouselPrefab = CreateCarouselPrefab(aquariumPrefab, slotPrefab);

            CreateRoomScene(carouselPrefab, lampPrefab, plantPrefab);
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
            AddSprite(root.transform, "TankShadow", new Vector2(9.65f, 5.45f), new Vector3(0.22f, -0.22f, 0.3f),
                new Color(0.025f, 0.03f, 0.075f, 0.72f), "RoomFurniture", 1);
            AddSprite(root.transform, "AquariumBackground", new Vector2(8.9f, 5f), new Vector3(0f, 0f, 0f),
                new Color(0.035f, 0.09f, 0.18f), "AquariumBack");
            AddSprite(root.transform, "DeepWater", new Vector2(8.45f, 4.45f), new Vector3(0f, -0.02f, -0.1f),
                new Color(0.08f, 0.37f, 0.5f, 0.96f), "AquariumContents");
            AddSprite(root.transform, "MidWater", new Vector2(8.45f, 2.85f), new Vector3(0f, 0.35f, -0.2f),
                new Color(0.12f, 0.55f, 0.62f, 0.7f), "AquariumContents", 1);
            AddSprite(root.transform, "SurfaceWater", new Vector2(8.45f, 1.25f), new Vector3(0f, 1.55f, -0.3f),
                new Color(0.28f, 0.75f, 0.75f, 0.42f), "AquariumContents", 2);
            AddSprite(root.transform, "WaterSurfaceLine", new Vector2(8.3f, 0.08f), new Vector3(0f, 2.12f, -0.4f),
                new Color(0.68f, 0.94f, 0.9f, 0.72f), "AquariumContents", 4);
            AddSprite(root.transform, "Substrate", new Vector2(8.45f, 0.62f), new Vector3(0f, -1.9f, -0.4f),
                new Color(0.4f, 0.23f, 0.34f), "AquariumContents", 5);
            AddSprite(root.transform, "SubstrateHighlight", new Vector2(8.15f, 0.12f), new Vector3(0f, -1.63f, -0.5f),
                new Color(0.67f, 0.42f, 0.43f, 0.65f), "AquariumContents", 6);
            AddSprite(root.transform, "Rock", new Vector2(1.4f, 0.9f), new Vector3(-1.65f, -1.45f, -0.6f),
                new Color(0.38f, 0.39f, 0.52f), "AquariumContents", 2);

            var plant = new GameObject("AquaticPlant");
            plant.transform.SetParent(root.transform, false);
            plant.transform.localPosition = new Vector3(2.1f, -1.35f, -0.7f);
            AddSprite(plant.transform, "Stem", new Vector2(0.16f, 2.05f), Vector3.zero,
                new Color(0.19f, 0.51f, 0.42f), "AquariumContents", 2);
            AddSprite(plant.transform, "LeafLeft", new Vector2(0.78f, 0.28f), new Vector3(-0.3f, 0.35f, 0f),
                new Color(0.28f, 0.66f, 0.49f), "AquariumContents", 3, 25f);
            AddSprite(plant.transform, "LeafRight", new Vector2(0.78f, 0.28f), new Vector3(0.3f, 0.78f, 0f),
                new Color(0.35f, 0.73f, 0.53f), "AquariumContents", 3, -25f);

            AddSprite(root.transform, "Glass", new Vector2(8.45f, 4.45f), new Vector3(0f, -0.02f, -0.8f),
                new Color(0.72f, 0.91f, 1f, 0.09f), "AquariumFront");
            AddSprite(root.transform, "GlassReflection", new Vector2(0.22f, 3.85f), new Vector3(3.1f, 0.15f, -0.9f),
                new Color(0.9f, 0.98f, 1f, 0.34f), "AquariumFront", 1, -11f);
            AddSprite(root.transform, "DiagonalReflection", new Vector2(0.16f, 4.4f), new Vector3(2.35f, 0.2f, -0.9f),
                new Color(0.72f, 0.95f, 1f, 0.16f), "AquariumFront", 1, -28f);

            var frameColor = new Color(0.13f, 0.16f, 0.28f);
            AddSprite(root.transform, "FrameTop", new Vector2(9.2f, 0.28f), new Vector3(0f, 2.5f, -1f),
                frameColor, "AquariumFront", 2);
            AddSprite(root.transform, "FrameBottom", new Vector2(9.2f, 0.32f), new Vector3(0f, -2.5f, -1f),
                frameColor, "AquariumFront", 2);
            AddSprite(root.transform, "FrameLeft", new Vector2(0.28f, 4.95f), new Vector3(-4.45f, 0f, -1f),
                frameColor, "AquariumFront", 2);
            AddSprite(root.transform, "FrameRight", new Vector2(0.28f, 4.95f), new Vector3(4.45f, 0f, -1f),
                frameColor, "AquariumFront", 2);
            AddSprite(root.transform, "ActiveBaseGlow", new Vector2(8.4f, 0.18f), new Vector3(0f, -2.78f, -1.1f),
                new Color(0.34f, 0.9f, 0.82f, 0.55f), "Effects", 2);

            var glow = root.AddComponent<Light2D>();
            glow.lightType = Light2D.LightType.Point;
            glow.color = new Color(0.32f, 0.82f, 0.82f);
            glow.intensity = 0.5f;
            glow.pointLightInnerRadius = 2.1f;
            glow.pointLightOuterRadius = 6.8f;
            glow.shadowsEnabled = false;

            return SavePrefab(root, $"{PrefabRoot}/AquariumRoomDisplay.prefab");
        }

        private static GameObject CreateSlotPrefab()
        {
            var root = new GameObject("AquariumSlotView");
            AddSprite(root.transform, "SlotShadow", new Vector2(9.45f, 5.25f), new Vector3(0.18f, -0.18f, 0.2f),
                new Color(0.025f, 0.03f, 0.07f, 0.5f), "RoomFurniture");
            AddSprite(root.transform, "ReservedSpace", new Vector2(9.05f, 4.95f), Vector3.zero,
                new Color(0.16f, 0.2f, 0.36f, 0.58f), "RoomFurniture", 1);
            AddSprite(root.transform, "ReservedInner", new Vector2(8.35f, 4.25f), new Vector3(0f, 0f, -0.1f),
                new Color(0.08f, 0.12f, 0.25f, 0.72f), "RoomFurniture", 2);
            AddSprite(root.transform, "FutureSurface", new Vector2(7.9f, 0.08f), new Vector3(0f, 1.72f, -0.2f),
                new Color(0.36f, 0.54f, 0.68f, 0.32f), "RoomFurniture", 3);
            AddSprite(root.transform, "PreparedBase", new Vector2(9.3f, 0.28f), new Vector3(0f, -2.62f, 0f),
                new Color(0.35f, 0.24f, 0.42f, 0.8f), "RoomFurniture", 2);
            var content = new GameObject("Content");
            content.transform.SetParent(root.transform, false);
            var slot = root.AddComponent<AquariumSlotView>();
            slot.Configure("slot-template", content.transform);
            return SavePrefab(root, $"{PrefabRoot}/AquariumSlotView.prefab");
        }

        private static GameObject CreateCarouselPrefab(GameObject aquariumPrefab, GameObject slotPrefab)
        {
            var root = new GameObject("AquariumCarouselRoot");
            var definitions = new[]
            {
                ("AquariumSlot_02", "slot-02", new Vector3(-11.2f, 0.1f, 0f), false),
                ("AquariumSlot_01", "slot-01", Vector3.zero, true),
                ("AquariumSlot_03", "slot-03", new Vector3(11.2f, 0.1f, 0f), false)
            };

            foreach (var definition in definitions)
            {
                var slotObject = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab);
                slotObject.name = definition.Item1;
                slotObject.transform.SetParent(root.transform, false);
                slotObject.transform.localPosition = definition.Item3;
                slotObject.transform.localScale = definition.Item4 ? Vector3.one : Vector3.one * 0.72f;

                var slot = slotObject.GetComponent<AquariumSlotView>();
                slot.Configure(definition.Item2, slotObject.transform.Find("Content"));
                if (definition.Item4)
                {
                    var aquarium = (GameObject)PrefabUtility.InstantiatePrefab(aquariumPrefab);
                    aquarium.name = "AquariumRoomDisplay";
                    slot.AssignView(aquarium);
                }
            }

            return SavePrefab(root, $"{PrefabRoot}/AquariumCarouselRoot.prefab");
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
            GameObject carouselPrefab,
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
            window.localPosition = new Vector3(-7.55f, 2.15f, 0.4f);
            AddSprite(window, "WindowFrame", new Vector2(2.8f, 3.25f), Vector3.zero,
                new Color(0.48f, 0.37f, 0.57f), "RoomFurniture");
            AddSprite(window, "NightSky", new Vector2(2.4f, 2.85f), new Vector3(0f, 0f, -0.1f),
                new Color(0.08f, 0.17f, 0.32f), "RoomEnvironment", 2);
            AddSprite(window, "Moon", new Vector2(0.55f, 0.55f), new Vector3(0.75f, 0.8f, -0.2f),
                new Color(0.96f, 0.89f, 0.66f), "RoomEnvironment", 3);
            AddSprite(window, "CrossVertical", new Vector2(0.1f, 2.85f), new Vector3(0f, 0f, -0.3f),
                new Color(0.48f, 0.37f, 0.57f), "RoomFurniture", 1);
            AddSprite(window, "CrossHorizontal", new Vector2(2.4f, 0.1f), new Vector3(0f, 0f, -0.3f),
                new Color(0.48f, 0.37f, 0.57f), "RoomFurniture", 1);

            var furniture = CreateGroup(environment, "Furniture");
            AddSprite(furniture, "CabinetFloorShadow", new Vector2(9.8f, 0.45f), new Vector3(0f, -4.35f, 0.3f),
                new Color(0.03f, 0.025f, 0.07f, 0.65f), "RoomFurniture", 1);
            AddSprite(furniture, "AquariumCabinetTop", new Vector2(9.45f, 0.3f), new Vector3(0f, -2.78f, 0f),
                new Color(0.55f, 0.32f, 0.43f), "RoomFurniture", 3);
            AddSprite(furniture, "AquariumCabinet", new Vector2(8.65f, 1.45f), new Vector3(0f, -3.62f, 0.1f),
                new Color(0.34f, 0.22f, 0.38f), "RoomFurniture", 2);
            AddSprite(furniture, "CabinetDivider", new Vector2(0.1f, 1.25f), new Vector3(0f, -3.62f, 0f),
                new Color(0.55f, 0.32f, 0.43f), "RoomFurniture", 3);

            var shelves = CreateGroup(environment, "Shelves");
            AddSprite(shelves, "Shelf", new Vector2(3.2f, 0.2f), new Vector3(7.4f, 3.1f, 0f),
                new Color(0.58f, 0.36f, 0.47f), "RoomFurniture", 3);
            AddSprite(shelves, "BookA", new Vector2(0.34f, 0.85f), new Vector3(6.8f, 3.62f, 0f),
                new Color(0.45f, 0.67f, 0.66f), "RoomFurniture", 4);
            AddSprite(shelves, "BookB", new Vector2(0.3f, 0.7f), new Vector3(7.17f, 3.55f, 0f),
                new Color(0.76f, 0.47f, 0.57f), "RoomFurniture", 4);
            AddSprite(shelves, "BookC", new Vector2(0.38f, 0.78f), new Vector3(7.55f, 3.59f, 0f),
                new Color(0.7f, 0.59f, 0.39f), "RoomFurniture", 4);

            var plants = CreateGroup(environment, "DecorativePlants");
            var plant = (GameObject)PrefabUtility.InstantiatePrefab(plantPrefab);
            plant.name = "DecorativePlant";
            plant.transform.SetParent(plants, false);
            plant.transform.localPosition = new Vector3(8.15f, -2.15f, 0f);
            plant.transform.localScale = Vector3.one * 0.82f;

            var lamps = CreateGroup(environment, "Lamps");
            var lamp = (GameObject)PrefabUtility.InstantiatePrefab(lampPrefab);
            lamp.name = "RoomLamp";
            lamp.transform.SetParent(lamps, false);
            lamp.transform.localPosition = new Vector3(-7.1f, -1.95f, 0f);
            lamp.transform.localScale = Vector3.one * 0.78f;

            AddSprite(ambientEffects, "AquariumWallGlow", new Vector2(10.8f, 6.25f),
                new Vector3(0f, 0f, 0.5f), new Color(0.2f, 0.67f, 0.72f, 0.1f), "RoomEnvironment", 4);

            var carousel = (GameObject)PrefabUtility.InstantiatePrefab(carouselPrefab);
            carousel.name = "AquariumCarouselRoot";
            carousel.transform.SetParent(aquariumArea, false);
            carousel.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            var initialSlot = carousel.transform.Find("AquariumSlot_01").GetComponent<AquariumSlotView>();

            var windowGlow = AddSprite(ambientEffects, "WindowGlow", new Vector2(3.5f, 4.1f),
                new Vector3(-7.55f, 2.15f, -0.7f), new Color(0.35f, 0.54f, 0.86f, 0.08f), "Effects");
            windowGlow.SetActive(true);
            AddSprite(ambientEffects, "LightAccents", new Vector2(9.25f, 0.18f),
                new Vector3(0f, -2.58f, -0.8f), new Color(0.36f, 0.83f, 0.78f, 0.32f), "Effects", 1);

            var globalLightObject = new GameObject("AmbientNightLight");
            globalLightObject.transform.SetParent(ambientEffects, false);
            var globalLight = globalLightObject.AddComponent<Light2D>();
            globalLight.lightType = Light2D.LightType.Global;
            globalLight.color = new Color(0.55f, 0.59f, 0.82f);
            globalLight.intensity = 0.58f;

            var windowLightObject = new GameObject("WindowLight");
            windowLightObject.transform.SetParent(ambientEffects, false);
            windowLightObject.transform.localPosition = new Vector3(-7.55f, 2.15f, 0f);
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
