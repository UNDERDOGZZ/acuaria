using System.IO;
using Acuaria.Aquarium;
using Acuaria.Fish;
using Acuaria.Food;
using Acuaria.Room;
using Acuaria.Simulation.Water;
using Acuaria.Simulation.Maintenance;
using Acuaria.Simulation.Filtration;
using Acuaria.UI.Aquarium;
using Acuaria.UI.Maintenance;
using Acuaria.UI.WaterChemistry;
using TMPro;
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
        private const string ChemistryDefinitionPath = Root + "/Data/WaterChemistry/StarterAquariumChemistry.asset";
        private const string MaintenanceDefinitionPath = Root + "/Data/Maintenance/StarterMaintenance.asset";
        private const string FilterDefinitionPath = Root + "/Data/Filters/StarterInternalFilter.asset";
        private const string MaintenancePrefabPath = Root + "/Prefabs/UI/Maintenance/AquariumMaintenancePanel.prefab";
        private static readonly Color PanelColor = new(0.055f, 0.105f, 0.16f, 0.9f);
        private static readonly Color AccentColor = new(0.27f, 0.78f, 0.76f, 1f);

        [MenuItem("Acuaria/Setup Aquarium HUD")]
        public static void Configure()
        {
            EnsureFolders();
            var definition = CreateDefinition();
            var chemistryDefinition = CreateChemistryDefinition();
            var maintenanceDefinition = CreateMaintenanceDefinition();
            var filterDefinition = CreateFilterDefinition();
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var safeArea = FindSceneObject("SafeArea").transform;
            var back = FindSceneObject("BackButton").GetComponent<Button>();
            var feed = FindSceneObject("FeedButton").GetComponent<Button>();
            var existing = safeArea.Find("AquariumHUDSystem");
            if (existing != null)
            {
                back.transform.SetParent(safeArea, true);
                Object.DestroyImmediate(existing.gameObject);
            }

            var system = new GameObject("AquariumHUDSystem", typeof(RectTransform), typeof(AquariumInhabitantProvider),
                typeof(AquariumHUDController), typeof(AquariumSimulationController),
                typeof(WaterChemistryDebugController));
            system.transform.SetParent(safeArea, false);
            Stretch((RectTransform)system.transform);
            var provider = system.GetComponent<AquariumInhabitantProvider>();
            var spawner = Object.FindAnyObjectByType<FishSpawner2D>();
            provider.Configure(spawner);

            var compact = CreateCompactHud(system.transform, back, out var responsive, out var infoButton,
                out var name, out var volume, out var temperature, out var fishCount, out var status, out var badge);
            var details = CreateDetailsPanel(system.transform);
            var feeding = FindSceneComponent<FeedingUIController>();
            var controller = system.GetComponent<AquariumHUDController>();
            controller.Configure(definition, provider, feeding, details, responsive, compact, back, feed, infoButton,
                name, volume, temperature, fishCount, status, badge);
            var simulation = system.GetComponent<AquariumSimulationController>();
            simulation.Configure(chemistryDefinition, provider, Object.FindAnyObjectByType<AquariumFoodController>(),
                controller, definition.NominalVolumeLitres);
            system.GetComponent<WaterChemistryDebugController>().Configure(simulation);
            var maintenanceButton = Button(safeArea, "MaintenanceButton", "Mantenimiento",
                new Vector2(0f, 0f), new Vector2(28f, 28f), new Vector2(210f, 72f));
            maintenanceButton.GetComponent<RectTransform>().pivot = Vector2.zero;
            var maintenancePanel = CreateMaintenancePanel(system.transform, out var visualController);
            var maintenanceController = system.AddComponent<AquariumMaintenanceController>();
            var maintenanceDebug = system.AddComponent<MaintenanceDebugController>();
            maintenanceController.Configure(maintenanceDefinition, filterDefinition, simulation, controller, feeding,
                maintenancePanel, visualController, maintenanceButton, back);
            maintenanceDebug.Configure(maintenanceController);
            controller.SetMaintenanceController(maintenanceController);
            PrefabUtility.SaveAsPrefabAsset(maintenancePanel.gameObject, MaintenancePrefabPath);
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

        [MenuItem("Acuaria/Import TMP Essential Resources")]
        public static void ImportTmpEssentialResources()
        {
            const string settingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
            if (AssetDatabase.LoadAssetAtPath<TMP_Settings>(settingsPath) != null)
            {
                Debug.Log("TextMeshPro essential resources are already available.");
                return;
            }

            var packagePath = Path.Combine(EditorApplication.applicationContentsPath, "Resources",
                "PackageManager", "BuiltInPackages", "com.unity.ugui", "Package Resources",
                "TMP Essential Resources.unitypackage");
            AssetDatabase.ImportPackage(packagePath, false);
            AssetDatabase.Refresh();
            Debug.Log("TextMeshPro essential resources imported.");
        }

        public static void ConfigureFromCommandLine() => Configure();

        private static void EnsureFolders()
        {
            Directory.CreateDirectory(Root + "/Data/Aquariums");
            Directory.CreateDirectory(Root + "/Data/WaterChemistry");
            Directory.CreateDirectory(Root + "/Prefabs/UI/Aquarium");
            Directory.CreateDirectory(Root + "/Data/Maintenance");
            Directory.CreateDirectory(Root + "/Data/Filters");
            Directory.CreateDirectory(Root + "/Prefabs/UI/Maintenance");
        }

        private static AquariumMaintenanceDefinition CreateMaintenanceDefinition()
        {
            var asset=AssetDatabase.LoadAssetAtPath<AquariumMaintenanceDefinition>(MaintenanceDefinitionPath);
            if(asset==null){asset=ScriptableObject.CreateInstance<AquariumMaintenanceDefinition>();AssetDatabase.CreateAsset(asset,MaintenanceDefinitionPath);}
            asset.Configure("starter-maintenance",new[]{10,25,40,50},25,4f,0.8f,new Vector3(1.2f,1.2f,0.8f),new Vector2(25f,20f));
            EditorUtility.SetDirty(asset);return asset;
        }

        private static FilterDefinition CreateFilterDefinition()
        {
            var asset=AssetDatabase.LoadAssetAtPath<FilterDefinition>(FilterDefinitionPath);
            if(asset==null){asset=ScriptableObject.CreateInstance<FilterDefinition>();AssetDatabase.CreateAsset(asset,FilterDefinitionPath);}
            asset.Configure("starter-internal-filter","Filtro interno inicial",new Vector2(30f,70f),0.85f,0.35f,0.002f,0.7f,168f,0.08f,
                FilterType.Internal,"Enjuaga suavemente el material biológico. Una limpieza profunda elimina bacterias beneficiosas.");
            EditorUtility.SetDirty(asset);return asset;
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

        private static WaterChemistryDefinition CreateChemistryDefinition()
        {
            var definition = AssetDatabase.LoadAssetAtPath<WaterChemistryDefinition>(ChemistryDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<WaterChemistryDefinition>();
                AssetDatabase.CreateAsset(definition, ChemistryDefinitionPath);
            }
            definition.Configure("starter-chemistry", 50f, new Vector3(0f, 0f, 7f),
                new Vector2(0.55f, 0.55f), new Vector2(0.1f, 0.08f),
                new Vector2(0.012f, 0.001f), new Vector3(0.08f, 0.12f, 0.01f),
                new Vector2(200f, 1000f), new Vector2(1f, 60f), 5, 0.001f,
                WaterQualityThresholds.Default);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static GameObject CreateCompactHud(Transform parent, Button backButton,
            out AquariumHUDResponsiveLayout responsive, out Button infoButton, out TMP_Text name,
            out TMP_Text volume, out TMP_Text temperature, out TMP_Text fishCount, out TMP_Text status,
            out Image badge)
        {
            var root = new GameObject("AquariumHUD", typeof(RectTransform), typeof(AquariumHUDResponsiveLayout));
            root.transform.SetParent(parent, false);
            Stretch((RectTransform)root.transform);

            var topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
            topBar.transform.SetParent(root.transform, false);
            var topRect = (RectTransform)topBar.transform;
            topRect.anchorMin = new Vector2(0f, 1f);
            topRect.anchorMax = Vector2.one;
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.offsetMin = new Vector2(16f, -128f);
            topRect.offsetMax = new Vector2(-16f, -16f);
            topBar.GetComponent<Image>().color = new Color(0.045f, 0.09f, 0.14f, 0.84f);
            var wideRow = new GameObject("WideRow", typeof(RectTransform));
            wideRow.transform.SetParent(topBar.transform, false);
            Stretch((RectTransform)wideRow.transform);
            var wide = wideRow.AddComponent<HorizontalLayoutGroup>();
            ConfigureHorizontal(wide, 14f, new RectOffset(16, 16, 12, 12));
            var compactStack = new GameObject("CompactStack", typeof(RectTransform));
            compactStack.transform.SetParent(topBar.transform, false);
            Stretch((RectTransform)compactStack.transform);
            var compact = compactStack.AddComponent<VerticalLayoutGroup>();
            compact.padding = new RectOffset(16, 16, 12, 12);
            compact.spacing = 10f;
            compact.childAlignment = TextAnchor.MiddleCenter;
            compact.childControlWidth = true;
            compact.childControlHeight = false;
            compact.childForceExpandWidth = true;
            compact.childForceExpandHeight = false;
            compactStack.SetActive(false);

            var primary = LayoutContainer(compactStack.transform, "CompactPrimaryRow", true);
            primary.gameObject.AddComponent<LayoutElement>().preferredHeight = 78f;
            primary.gameObject.SetActive(false);

            var backRect = (RectTransform)backButton.transform;
            backRect.SetParent(wideRow.transform, false);
            backRect.localScale = Vector3.one;
            var backLayout = GetOrAddLayoutElement(backButton.gameObject);
            backLayout.minWidth = 76f;
            backLayout.preferredWidth = 76f;
            backLayout.minHeight = 72f;
            backLayout.preferredHeight = 72f;
            backButton.GetComponent<Image>().color = new Color(0.16f, 0.28f, 0.38f, 0.94f);

            var identity = LayoutContainer(wideRow.transform, "AquariumIdentityGroup", false);
            var identityLayout = identity.gameObject.AddComponent<LayoutElement>();
            identityLayout.minWidth = 270f;
            identityLayout.preferredWidth = 370f;
            identityLayout.minHeight = 78f;
            var identityVertical = identity.gameObject.AddComponent<VerticalLayoutGroup>();
            identityVertical.padding = new RectOffset(14, 14, 8, 8);
            identityVertical.spacing = 2f;
            identityVertical.childAlignment = TextAnchor.MiddleLeft;
            identityVertical.childControlWidth = true;
            identityVertical.childControlHeight = false;
            identityVertical.childForceExpandWidth = true;
            identityVertical.childForceExpandHeight = false;
            name = TmpLabel(identity, "AquariumNameText", "Acuario Inicial", 29f, 24f, 34f,
                TextAlignmentOptions.MidlineLeft);
            name.GetComponent<LayoutElement>().preferredHeight = 38f;
            volume = TmpLabel(identity, "VolumeText", "50 L", 22f, 20f, 25f, TextAlignmentOptions.MidlineLeft);
            volume.GetComponent<LayoutElement>().preferredHeight = 28f;

            var spacer = new GameObject("FlexibleSpacer", typeof(RectTransform), typeof(LayoutElement));
            spacer.transform.SetParent(wideRow.transform, false);
            spacer.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var stats = LayoutContainer(wideRow.transform, "AquariumStatsGroup", true);
            var statsLayout = stats.gameObject.AddComponent<LayoutElement>();
            statsLayout.minWidth = 690f;
            statsLayout.preferredWidth = 760f;
            statsLayout.minHeight = 78f;
            statsLayout.flexibleWidth = 0f;

            var temperatureItem = StatItem(stats, "TemperatureItem", 158f);
            TmpIcon(temperatureItem.transform, "TemperatureIcon", "▲");
            temperature = TmpLabel(temperatureItem.transform, "TemperatureText", "25 °C", 24f, 21f, 27f,
                TextAlignmentOptions.Center);
            var fishItem = StatItem(stats, "FishCountItem", 170f);
            TmpIcon(fishItem.transform, "FishIcon", "●");
            fishCount = TmpLabel(fishItem.transform, "FishCountText", "3 peces", 24f, 21f, 27f,
                TextAlignmentOptions.Center);
            var badgeRoot = StatItem(stats, "StatusBadge", 166f);
            badge = badgeRoot.GetComponent<Image>();
            badge.color = new Color(0.3f, 0.86f, 0.63f);
            status = TmpLabel(badgeRoot.transform, "StatusText", "Estable", 22f, 19f, 25f,
                TextAlignmentOptions.Center);
            infoButton = TmpButton(stats, "DetailsButton", "i  Detalles", 164f);

            responsive = root.GetComponent<AquariumHUDResponsiveLayout>();
            responsive.Configure((RectTransform)root.transform, topRect, (RectTransform)wideRow.transform,
                (RectTransform)compactStack.transform, primary, identity,
                (RectTransform)spacer.transform, stats, backRect, (RectTransform)infoButton.transform, 2100f);
            return root;
        }

        private static RectTransform LayoutContainer(Transform parent, string name, bool horizontal)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = new Color(0.07f, 0.14f, 0.2f, 0.72f);
            if (horizontal)
            {
                var layout = root.AddComponent<HorizontalLayoutGroup>();
                ConfigureHorizontal(layout, 10f, new RectOffset(8, 8, 6, 6));
            }
            return (RectTransform)root.transform;
        }

        private static GameObject StatItem(Transform parent, string name, float width)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = new Color(0.09f, 0.19f, 0.25f, 0.9f);
            var element = root.GetComponent<LayoutElement>();
            element.minWidth = width;
            element.preferredWidth = width;
            element.minHeight = 64f;
            element.preferredHeight = 64f;
            ConfigureHorizontal(root.AddComponent<HorizontalLayoutGroup>(), 5f, new RectOffset(10, 10, 6, 6));
            return root;
        }

        private static void ConfigureHorizontal(HorizontalLayoutGroup layout, float spacing, RectOffset padding)
        {
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        private static LayoutElement GetOrAddLayoutElement(GameObject target)
        {
            var element = target.GetComponent<LayoutElement>();
            return element != null ? element : target.AddComponent<LayoutElement>();
        }

        private static TMP_Text TmpLabel(Transform parent, string name, string value, float size, float minimum,
            float maximum, TextAlignmentOptions alignment)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            var text = root.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = size;
            text.fontSizeMin = minimum;
            text.fontSizeMax = maximum;
            text.enableAutoSizing = true;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.alignment = alignment;
            text.color = new Color(0.92f, 0.96f, 0.96f);
            var element = root.GetComponent<LayoutElement>();
            element.minWidth = name.Contains("Icon") ? 26f : 88f;
            element.preferredWidth = name.Contains("Icon") ? 26f : 110f;
            element.flexibleWidth = name.Contains("Text") ? 1f : 0f;
            return text;
        }

        private static void TmpIcon(Transform parent, string name, string glyph)
        {
            var icon = TmpLabel(parent, name, glyph, 22f, 20f, 24f, TextAlignmentOptions.Center);
            icon.color = AccentColor;
        }

        private static Button TmpButton(Transform parent, string name, string value, float width)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button),
                typeof(LayoutElement));
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = new Color(0.16f, 0.53f, 0.57f, 1f);
            var layout = root.GetComponent<LayoutElement>();
            layout.minWidth = width;
            layout.preferredWidth = width;
            layout.minHeight = 64f;
            layout.preferredHeight = 64f;
            var label = TmpLabel(root.transform, "DetailsButtonText", value, 22f, 20f, 24f,
                TextAlignmentOptions.Center);
            var rect = (RectTransform)label.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(10f, 6f);
            rect.offsetMax = new Vector2(-10f, -6f);
            return root.GetComponent<Button>();
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
            contentRect.sizeDelta = new Vector2(0f, 1120f);
            var scroll = viewport.GetComponent<ScrollRect>();
            scroll.viewport = viewportRect;
            scroll.content = contentRect;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            var summary = TopLabel(content.transform, "Summary", "", 25, 18f, 12f, 510f, 350f);
            var status = TopLabel(content.transform, "Status", "", 26, 520f, 12f, 18f, 140f);
            TopLabel(content.transform, "InhabitantsHeading", "Habitantes", 28, 520f, 168f, 18f, 48f);
            var inhabitants = TopLabel(content.transform, "Inhabitants", "", 24, 520f, 222f, 18f, 190f);
            TopLabel(content.transform, "WaterChemistryHeading", "Calidad del agua", 28, 18f, 432f, 18f, 52f);
            var chemistry = TopLabel(content.transform, "WaterChemistry", "", 24, 18f, 490f, 18f, 300f);
            TopLabel(content.transform, "EducationHeading", "Consejo educativo", 28, 18f, 810f, 18f, 52f);
            var education = TopLabel(content.transform, "Education", "", 25, 18f, 870f, 18f, 160f);

            var details = root.GetComponent<AquariumDetailsPanel>();
            details.Configure(root.GetComponent<CanvasGroup>(), (RectTransform)card.transform, close, title,
                summary, inhabitants, status, education, chemistry);
            return details;
        }

        private static AquariumMaintenancePanel CreateMaintenancePanel(Transform parent,
            out WaterChangeVisualController visualController)
        {
            var root=new GameObject("AquariumMaintenancePanel",typeof(RectTransform),typeof(Image),typeof(CanvasGroup),
                typeof(AquariumMaintenancePanel),typeof(WaterChangeVisualController));
            root.transform.SetParent(parent,false);Stretch((RectTransform)root.transform);
            root.GetComponent<Image>().color=new Color(0.015f,0.03f,0.055f,0.86f);
            var card=Panel(root.transform,"MaintenanceCard",new Vector2(0.5f,0.5f),new Vector2(0.5f,0.5f),Vector2.zero,new Vector2(980f,680f));
            card.GetComponent<Image>().color=new Color(0.045f,0.09f,0.14f,0.99f);
            TopLabel(card.transform,"Title","Mantenimiento del acuario",34,38f,28f,180f,56f);
            var close=Button(card.transform,"CloseButton","Cerrar",Vector2.one,new Vector2(-28f,-28f),new Vector2(140f,64f));
            var percentages=new Button[4];
            var values=new[]{10,25,40,50};
            for(var i=0;i<values.Length;i++)
                percentages[i]=Button(card.transform,$"WaterChange{values[i]}Button",values[i]==25?"25% • Recomendado":$"{values[i]}%",
                    new Vector2(0f,1f),new Vector2(42f+i*205f,-112f),new Vector2(185f,62f));
            for(var i=0;i<percentages.Length;i++)percentages[i].GetComponent<RectTransform>().pivot=new Vector2(0f,1f);
            var preview=TopLabel(card.transform,"Preview","",23,42f,200f,500f,300f);
            var filter=TopLabel(card.transform,"FilterSummary","",23,520f,200f,38f,240f);
            var status=TopLabel(card.transform,"MaintenanceStatus","Disponible",22,42f,515f,38f,42f);
            var confirm=Button(card.transform,"ConfirmButton","Confirmar",new Vector2(0f,0f),new Vector2(42f,32f),new Vector2(190f,68f));
            var cancel=Button(card.transform,"CancelButton","Cancelar",new Vector2(0f,0f),new Vector2(250f,32f),new Vector2(170f,68f));
            var gentle=Button(card.transform,"GentleRinseButton","Enjuague suave",new Vector2(1f,0f),new Vector2(-250f,32f),new Vector2(210f,68f));
            var deep=Button(card.transform,"DeepCleanButton","Limpieza profunda",new Vector2(1f,0f),new Vector2(-28f,32f),new Vector2(210f,68f));
            confirm.GetComponent<RectTransform>().pivot=cancel.GetComponent<RectTransform>().pivot=Vector2.zero;
            gentle.GetComponent<RectTransform>().pivot=deep.GetComponent<RectTransform>().pivot=new Vector2(1f,0f);
            var overlayObject=new GameObject("WaterChangeOverlay",typeof(RectTransform),typeof(Image));
            overlayObject.transform.SetParent(root.transform,false);Stretch((RectTransform)overlayObject.transform);
            var overlay=overlayObject.GetComponent<Image>();overlay.color=Color.clear;overlay.raycastTarget=false;
            var panel=root.GetComponent<AquariumMaintenancePanel>();
            panel.Configure(root.GetComponent<CanvasGroup>(),percentages,confirm,cancel,gentle,deep,close,preview,filter,status);
            visualController=root.GetComponent<WaterChangeVisualController>();visualController.Configure(overlay,null);
            root.SetActive(false);return panel;
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
