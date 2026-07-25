using System.IO;
using Acuaria.Fish;
using Acuaria.Food;
using Acuaria.Room;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.Editor
{
    public static class FeedingSystemSetup
    {
        private const string Root = "Assets/_Acuaria";
        private const string FoodPrefabPath = Root + "/Prefabs/Food/FoodParticle2D.prefab";
        private const string FoodDefinitionPath = Root + "/Data/Food/BasicFlakes.asset";
        private const string FishPrefabPath = Root + "/Prefabs/Fish/Fish2D.prefab";
        private const string AquariumPrefabPath = Root + "/Prefabs/Room/AquariumRoomDisplay.prefab";
        private const string RoomScenePath = Root + "/Scenes/Room.unity";
        private const string WhiteSpritePath = Root + "/Art/Prototype/Room/PrototypeWhite.png";

        [MenuItem("Acuaria/Setup Feeding System")]
        public static void Configure()
        {
            EnsureFolders();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(WhiteSpritePath);
            var definition = CreateDefinition();
            var foodPrefab = CreateFoodPrefab(sprite);
            ConfigureFishPrefab();
            ConfigureAquariumPrefab(definition, foodPrefab, sprite);
            ConfigureRoomScene();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Feeding system configured.");
        }

        public static void ConfigureFromCommandLine() => Configure();

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(Root + "/Art/Prototype/Food");
            Directory.CreateDirectory(Root + "/Data/Food");
            Directory.CreateDirectory(Root + "/Prefabs/Food");
        }

        private static FoodDefinition CreateDefinition()
        {
            var definition = AssetDatabase.LoadAssetAtPath<FoodDefinition>(FoodDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<FoodDefinition>();
                AssetDatabase.CreateAsset(definition, FoodDefinitionPath);
            }
            definition.Configure("basic-flakes", "Basic Flakes", 0.22f, 18f, 4f, 0.32f,
                new Vector2(0.08f, 0.14f), new Color(0.96f, 0.69f, 0.22f), 0.22f, FoodTargetZone.Surface);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static FoodView2D CreateFoodPrefab(Sprite sprite)
        {
            var root = new GameObject("FoodParticle2D");
            var movement = root.AddComponent<FoodMovement2D>();
            var view = root.AddComponent<FoodView2D>();
            var visualObject = new GameObject("Visual");
            visualObject.transform.SetParent(root.transform, false);
            visualObject.transform.localScale = new Vector3(1.35f, 0.55f, 1f);
            visualObject.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
            var renderer = visualObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "AquariumContents";
            renderer.sortingOrder = 14;
            view.Configure(renderer, movement);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, FoodPrefabPath).GetComponent<FoodView2D>();
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void ConfigureFishPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(FishPrefabPath);
            var view = root.GetComponent<FishView>();
            var movement = root.GetComponent<FishMovement2D>();
            var visual = root.GetComponent<FishVisual2D>();
            var behaviour = root.GetComponent<FishFeedingBehaviour>();
            if (behaviour == null) behaviour = root.AddComponent<FishFeedingBehaviour>();
            behaviour.Configure(movement, visual);
            view.ConfigureFeeding(behaviour);
            PrefabUtility.SaveAsPrefabAsset(root, FishPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ConfigureAquariumPrefab(FoodDefinition definition, FoodView2D foodPrefab, Sprite sprite)
        {
            var root = PrefabUtility.LoadPrefabContents(AquariumPrefabPath);
            var population = root.transform.Find("FishPopulation");
            var controller = population.GetComponent<AquariumFoodController>();
            if (controller == null) controller = population.gameObject.AddComponent<AquariumFoodController>();
            controller.Configure(definition, foodPrefab, 12, new Vector2(-3.55f, -1.45f), new Vector2(3.55f, 1.45f));
            population.GetComponent<FishSpawner2D>().SetFoodController(controller);

            var area = population.GetComponent<AquariumFeedingArea2D>();
            if (area == null) area = population.gameObject.AddComponent<AquariumFeedingArea2D>();
            area.Configure(new Vector2(7.15f, 3.2f), 0.25f, 0.9f);

            var input = root.GetComponent<FeedingInputController>();
            if (input == null) input = root.AddComponent<FeedingInputController>();
            input.Configure(null, area, controller);

            var highlight = root.transform.Find("FeedingSurfaceHighlight");
            if (highlight == null)
            {
                highlight = new GameObject("FeedingSurfaceHighlight").transform;
                highlight.SetParent(root.transform, false);
                highlight.localPosition = new Vector3(0f, 1.45f, -0.72f);
                highlight.localScale = new Vector3(7.1f, 0.08f, 1f);
                var renderer = highlight.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = new Color(1f, 0.82f, 0.25f, 0.62f);
                renderer.sortingLayerName = "AquariumContents";
                renderer.sortingOrder = 12;
                renderer.enabled = false;
            }
            PrefabUtility.SaveAsPrefabAsset(root, AquariumPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static void ConfigureRoomScene()
        {
            var scene = EditorSceneManager.OpenScene(RoomScenePath, OpenSceneMode.Single);
            var camera = Object.FindAnyObjectByType<Camera>();
            var area = Object.FindAnyObjectByType<AquariumFeedingArea2D>();
            var controller = Object.FindAnyObjectByType<AquariumFoodController>();
            var aquarium = Object.FindAnyObjectByType<AquariumInteractable>().gameObject;
            var input = aquarium.GetComponent<FeedingInputController>();
            if (input == null) input = aquarium.AddComponent<FeedingInputController>();
            input.Configure(camera, area, controller);
            var safeArea = FindSceneObject("SafeArea");
            var oldUi = safeArea.transform.Find("FeedingUI");
            if (oldUi != null) Object.DestroyImmediate(oldUi.gameObject);
            var uiRoot = new GameObject("FeedingUI", typeof(RectTransform), typeof(FeedingUIController));
            uiRoot.transform.SetParent(safeArea.transform, false);
            Stretch((RectTransform)uiRoot.transform);

            var button = CreateButton(uiRoot.transform);
            var buttonLabel = button.GetComponentInChildren<Text>();
            var instruction = CreateText(uiRoot.transform, "Instruction", "Toca el agua para alimentar",
                new Vector2(0.5f, 0.12f), new Vector2(580f, 58f), 27, TextAnchor.MiddleCenter);
            instruction.GetComponent<Text>().color = new Color(0.96f, 0.92f, 0.7f);
            var feedback = CreateText(uiRoot.transform, "Feedback", "", new Vector2(0.5f, 0.84f),
                new Vector2(900f, 72f), 25, TextAnchor.MiddleCenter).GetComponent<Text>();
            feedback.gameObject.SetActive(false);
            instruction.SetActive(false);
            var highlight = GameObject.Find("FeedingSurfaceHighlight").GetComponent<SpriteRenderer>();
            var ui = uiRoot.GetComponent<FeedingUIController>();
            ui.Configure(button, buttonLabel, instruction, feedback, input, controller, highlight);
            button.gameObject.SetActive(false);

            var roomController = Object.FindAnyObjectByType<RoomViewController>();
            var serialized = new SerializedObject(roomController);
            serialized.FindProperty("feedingUi").objectReferenceValue = ui;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);
        }

        private static Button CreateButton(Transform parent)
        {
            var root = new GameObject("FeedButton", typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-32f, 32f);
            rect.sizeDelta = new Vector2(220f, 84f);
            root.GetComponent<Image>().color = new Color(0.78f, 0.42f, 0.18f, 0.94f);
            var label = CreateText(root.transform, "Label", "Alimentar", new Vector2(0.5f, 0.5f),
                Vector2.zero, 28, TextAnchor.MiddleCenter);
            Stretch((RectTransform)label.transform);
            return root.GetComponent<Button>();
        }

        private static GameObject CreateText(Transform parent, string name, string value, Vector2 anchor,
            Vector2 size, int fontSize, TextAnchor alignment)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Text));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            var text = root.GetComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            return root;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static GameObject FindSceneObject(string objectName)
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (var index = 0; index < objects.Length; index++)
            {
                if (objects[index].name == objectName && objects[index].scene.IsValid()) return objects[index];
            }
            return null;
        }
    }
}
