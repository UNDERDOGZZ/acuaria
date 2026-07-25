using System.IO;
using Acuaria.Aquarium;
using Acuaria.Fish;
using Acuaria.Food;
using Acuaria.Room;
using Acuaria.UI.Aquarium;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.Editor
{
    public static class AquariumHUDSetup
    {
        private const string Root = "Assets/_Acuaria";
        private const string ScenePath = Root + "/Scenes/Room.unity";
        private const string DefinitionPath = Root + "/Data/Aquariums/StarterAquarium.asset";
        private const string HudPrefabPath = Root + "/Prefabs/UI/Aquarium/AquariumHUD.prefab";
        private const string DetailsPrefabPath = Root + "/Prefabs/UI/Aquarium/AquariumDetailsPanel.prefab";
        private static readonly Color PanelColor = new(0.055f, 0.105f, 0.16f, 0.9f);
        private static readonly Color AccentColor = new(0.27f, 0.78f, 0.76f, 1f);

        [MenuItem("Acuaria/Setup Aquarium HUD")]
        public static void Configure()
        {
            EnsureFolders();
            var definition = CreateDefinition();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var safeArea = FindSceneObject("SafeArea").transform;
            var existing = safeArea.Find("AquariumHUDSystem");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);

            var system = new GameObject("AquariumHUDSystem", typeof(RectTransform), typeof(AquariumInhabitantProvider),
                typeof(AquariumHUDController));
            system.transform.SetParent(safeArea, false);
            Stretch((RectTransform)system.transform);
            var provider = system.GetComponent<AquariumInhabitantProvider>();
            var spawner = Object.FindAnyObjectByType<FishSpawner2D>();
            provider.Configure(spawner);

            var compact = CreateCompactHud(system.transform, out var infoButton, out var name, out var volume,
                out var temperature, out var fishCount, out var status, out var badge);
            var details = CreateDetailsPanel(system.transform);
            var feeding = FindSceneComponent<FeedingUIController>();
            var controller = system.GetComponent<AquariumHUDController>();
            controller.Configure(definition, provider, feeding, details, compact, infoButton, name, volume,
                temperature, fishCount, status, badge);
            compact.SetActive(false);

            PrefabUtility.SaveAsPrefabAsset(compact, HudPrefabPath);
            PrefabUtility.SaveAsPrefabAsset(details.gameObject, DetailsPrefabPath);

            var room = Object.FindAnyObjectByType<RoomViewController>();
            var serialized = new SerializedObject(room);
            serialized.FindProperty("aquariumHud").objectReferenceValue = controller;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Aquarium HUD configured.");
        }

        public static void ConfigureFromCommandLine() => Configure();

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(Root + "/Data/Aquariums");
            Directory.CreateDirectory(Root + "/Prefabs/UI/Aquarium");
        }

        private static AquariumDefinition CreateDefinition()
        {
            var definition = AssetDatabase.LoadAssetAtPath<AquariumDefinition>(DefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<AquariumDefinition>();
                AssetDatabase.CreateAsset(definition, DefinitionPath);
            }
            definition.Configure("starter-aquarium", "Acuario Inicial", 50f, new Vector2(24f, 26f), 25f, 3,
                "Un primer ecosistema para observar y aprender.",
                "El tamaño del acuario importa: un mayor volumen suele ofrecer más estabilidad, " +
                "pero cada especie tiene necesidades distintas.", AccentColor);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static GameObject CreateCompactHud(Transform parent, out Button infoButton, out Text name,
            out Text volume, out Text temperature, out Text fishCount, out Text status, out Image badge)
        {
            var root = new GameObject("AquariumHUD", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Stretch((RectTransform)root.transform);

            var left = Panel(root.transform, "IdentityBlock", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(164f, -24f), new Vector2(450f, 124f));
            name = Label(left.transform, "Name", "Acuario Inicial", 28, TextAnchor.MiddleLeft,
                new Vector2(22f, -14f), new Vector2(-22f, -58f));
            volume = Label(left.transform, "Volume", "◆ 50 L", 25, TextAnchor.MiddleLeft,
                new Vector2(22f, -66f), new Vector2(-22f, -14f));

            var right = Panel(root.transform, "StatusBlock", Vector2.one, Vector2.one,
                new Vector2(-24f, -24f), new Vector2(560f, 124f));
            temperature = Label(right.transform, "Temperature", "▲ 25 °C", 25, TextAnchor.MiddleLeft,
                new Vector2(22f, -14f), new Vector2(-310f, -58f));
            fishCount = Label(right.transform, "FishCount", "● 3 peces", 25, TextAnchor.MiddleLeft,
                new Vector2(22f, -66f), new Vector2(-310f, -14f));
            var badgeRoot = new GameObject("StatusBadge", typeof(RectTransform), typeof(Image));
            badgeRoot.transform.SetParent(right.transform, false);
            var badgeRect = (RectTransform)badgeRoot.transform;
            badgeRect.anchorMin = new Vector2(0.47f, 0.18f);
            badgeRect.anchorMax = new Vector2(0.78f, 0.82f);
            badgeRect.offsetMin = badgeRect.offsetMax = Vector2.zero;
            badge = badgeRoot.GetComponent<Image>();
            badge.color = new Color(0.3f, 0.86f, 0.63f);
            status = Label(badgeRoot.transform, "Status", "Excelente", 20, TextAnchor.MiddleCenter,
                new Vector2(10f, 4f), new Vector2(-10f, -4f));
            infoButton = Button(right.transform, "DetailsButton", "i  Detalles", new Vector2(1f, 0.5f),
                new Vector2(-18f, 0f), new Vector2(132f, 76f));
            return root;
        }

        private static AquariumDetailsPanel CreateDetailsPanel(Transform parent)
        {
            var root = new GameObject("AquariumDetailsPanel", typeof(RectTransform), typeof(Image),
                typeof(CanvasGroup), typeof(AquariumDetailsPanel));
            root.transform.SetParent(parent, false);
            Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color = new Color(0.015f, 0.03f, 0.055f, 0.78f);

            var card = Panel(root.transform, "Card", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1040f, 700f));
            card.GetComponent<Image>().color = new Color(0.045f, 0.09f, 0.14f, 0.98f);
            var title = TopLabel(card.transform, "Title", "Acuario Inicial", 34, 42f, 28f, 210f, 64f);
            var close = Button(card.transform, "CloseButton", "Cerrar", Vector2.one,
                new Vector2(-28f, -28f), new Vector2(150f, 72f));

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask),
                typeof(ScrollRect));
            viewport.transform.SetParent(card.transform, false);
            var viewportRect = (RectTransform)viewport.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(42f, 34f);
            viewportRect.offsetMax = new Vector2(-42f, -112f);
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 760f);
            var scroll = viewport.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var summary = TopLabel(content.transform, "Summary", "", 25, 18f, 12f, 510f, 350f);
            var status = TopLabel(content.transform, "Status", "", 26, 520f, 12f, 18f, 140f);
            TopLabel(content.transform, "InhabitantsHeading", "Habitantes", 28, 520f, 168f, 18f, 48f);
            var inhabitants = TopLabel(content.transform, "Inhabitants", "", 24, 520f, 222f, 18f, 190f);
            TopLabel(content.transform, "EducationHeading", "Consejo educativo", 28, 18f, 432f, 18f, 52f);
            var education = TopLabel(content.transform, "Education", "", 25, 18f, 492f, 18f, 160f);

            var details = root.GetComponent<AquariumDetailsPanel>();
            details.Configure(root.GetComponent<CanvasGroup>(), (RectTransform)card.transform, close, title,
                summary, inhabitants, status, education);
            return details;
        }

        private static GameObject Panel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 position, Vector2 size)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMax;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            root.GetComponent<Image>().color = PanelColor;
            return root;
        }

        private static Text Label(Transform parent, string name, string value, int size, TextAnchor alignment,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Text));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var label = root.GetComponent<Text>();
            label.text = value;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size;
            label.alignment = alignment;
            label.color = new Color(0.92f, 0.96f, 0.96f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private static Text TopLabel(Transform parent, string name, string value, int size, float left, float top,
            float right, float height)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Text));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -top - height);
            rect.offsetMax = new Vector2(-right, -top);
            var label = root.GetComponent<Text>();
            label.text = value;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size;
            label.alignment = TextAnchor.UpperLeft;
            label.color = new Color(0.92f, 0.96f, 0.96f);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private static Button Button(Transform parent, string name, string value, Vector2 anchor,
            Vector2 position, Vector2 size)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            var rect = (RectTransform)root.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            root.GetComponent<Image>().color = new Color(0.16f, 0.53f, 0.57f, 1f);
            Label(root.transform, "Label", value, 21, TextAnchor.MiddleCenter, new Vector2(6f, 4f),
                new Vector2(-6f, -4f));
            return root.GetComponent<Button>();
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
                if (objects[index].name == objectName && objects[index].scene.IsValid()) return objects[index];
            return null;
        }

        private static T FindSceneComponent<T>() where T : Component
        {
            var components = Resources.FindObjectsOfTypeAll<T>();
            for (var index = 0; index < components.Length; index++)
                if (components[index].gameObject.scene.IsValid()) return components[index];
            return null;
        }
    }
}
