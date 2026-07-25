using System.Linq;
using Acuaria.Room;
using Acuaria.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Acuaria.Editor
{
    public static class AquariumFocusInteractionSetup
    {
        private const string AquariumPrefabPath = "Assets/_Acuaria/Prefabs/Room/AquariumRoomDisplay.prefab";
        private const string RoomScenePath = "Assets/_Acuaria/Scenes/Room.unity";

        [MenuItem("Acuaria/Setup Aquarium Focus Interaction")]
        public static void Configure()
        {
            ConfigureAquariumPrefab();
            var scene = EditorSceneManager.OpenScene(RoomScenePath, OpenSceneMode.Single);
            var camera = Object.FindAnyObjectByType<Camera>();
            if (camera.GetComponent<Physics2DRaycaster>() == null)
            {
                camera.gameObject.AddComponent<Physics2DRaycaster>();
            }

            var interactables = Object.FindObjectsByType<AquariumInteractable>();
            var ui = CreateUi();
            var controllerObject = new GameObject("RoomViewController");
            controllerObject.transform.SetParent(GameObject.Find("Systems").transform, false);
            var controller = controllerObject.AddComponent<RoomViewController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("roomCamera").objectReferenceValue = camera;
            var array = serialized.FindProperty("interactables");
            array.arraySize = interactables.Length;
            for (var index = 0; index < interactables.Length; index++)
            {
                array.GetArrayElementAtIndex(index).objectReferenceValue = interactables[index];
            }
            serialized.FindProperty("focusedUi").objectReferenceValue = ui.focusedUi;
            serialized.FindProperty("backButton").objectReferenceValue = ui.backButton;
            serialized.FindProperty("transitionVeil").objectReferenceValue = ui.veil;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Aquarium focus interaction configured.");
        }

        public static void ConfigureFromCommandLine() => Configure();

        private static void ConfigureAquariumPrefab()
        {
            var root = PrefabUtility.LoadPrefabContents(AquariumPrefabPath);
            var focusPoint = root.transform.Find("FocusPoint");
            if (focusPoint == null)
            {
                focusPoint = new GameObject("FocusPoint").transform;
                focusPoint.SetParent(root.transform, false);
            }

            var target = root.GetComponent<AquariumFocusTarget>();
            if (target == null)
            {
                target = root.AddComponent<AquariumFocusTarget>();
            }
            target.Configure("slot-01", focusPoint, 3.25f);
            var collider = root.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = root.AddComponent<BoxCollider2D>();
            }
            collider.size = new Vector2(9.2f, 5f);
            var interactable = root.GetComponent<AquariumInteractable>();
            if (interactable == null)
            {
                interactable = root.AddComponent<AquariumInteractable>();
            }
            interactable.Configure(target, true);
            PrefabUtility.SaveAsPrefabAsset(root, AquariumPrefabPath);
            PrefabUtility.UnloadPrefabContents(root);
        }

        private static (GameObject focusedUi, Button backButton, CanvasGroup veil) CreateUi()
        {
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var canvasObject = new GameObject("FocusCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var safeArea = new GameObject("SafeArea", typeof(RectTransform), typeof(SafeAreaPanel));
            safeArea.transform.SetParent(canvasObject.transform, false);
            var safeRect = (RectTransform)safeArea.transform;
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;

            var buttonObject = new GameObject("BackButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(safeArea.transform, false);
            var buttonRect = (RectTransform)buttonObject.transform;
            buttonRect.anchorMin = new Vector2(0f, 1f);
            buttonRect.anchorMax = new Vector2(0f, 1f);
            buttonRect.pivot = new Vector2(0f, 1f);
            buttonRect.anchoredPosition = new Vector2(32f, -32f);
            buttonRect.sizeDelta = new Vector2(112f, 80f);
            buttonObject.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.28f, 0.88f);

            var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(buttonObject.transform, false);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var text = label.GetComponent<Text>();
            text.text = "←";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 42;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.85f, 0.95f, 1f);

            var veilObject = new GameObject("TransitionVeil", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            veilObject.transform.SetParent(canvasObject.transform, false);
            var veilRect = (RectTransform)veilObject.transform;
            veilRect.anchorMin = Vector2.zero;
            veilRect.anchorMax = Vector2.one;
            veilRect.offsetMin = veilRect.offsetMax = Vector2.zero;
            veilObject.GetComponent<Image>().color = new Color(0.03f, 0.04f, 0.1f, 1f);
            var veil = veilObject.GetComponent<CanvasGroup>();
            veil.alpha = 0f;
            veil.blocksRaycasts = false;
            veilObject.SetActive(false);

            safeArea.SetActive(false);
            return (safeArea, buttonObject.GetComponent<Button>(), veil);
        }
    }
}
