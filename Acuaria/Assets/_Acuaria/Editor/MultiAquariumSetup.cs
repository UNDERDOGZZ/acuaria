using System.Linq;
using Acuaria.Aquarium;
using Acuaria.Aquarium.Decorations;
using Acuaria.Aquarium.MultiAquarium;
using Acuaria.Room;
using Acuaria.UI.Aquarium;
using Acuaria.UI.Maintenance;
using Acuaria.UI.Progression;
using Acuaria.UI.WaterChemistry;
using Acuaria.Fish;
using Acuaria.UI.FishWelfare;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Acuaria.Editor
{
    public static class MultiAquariumSetup
    {
        const string ScenePath="Assets/_Acuaria/Scenes/Room.unity";
        const string CarouselAssetPath="Assets/_Acuaria/Data/Aquariums/AquariumCarousel.asset";
        const string SecondAquariumPath="Assets/_Acuaria/Data/Aquariums/Aquarium02.asset";
        const string ThirdAquariumPath="Assets/_Acuaria/Data/Aquariums/Aquarium03.asset";
        [MenuItem("Acuaria/Sprint 13/Configure Multi Aquarium")]
        public static void Configure()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var definition=AssetDatabase.LoadAssetAtPath<AquariumDefinition>("Assets/_Acuaria/Data/Aquariums/StarterAquarium.asset");
            if(definition==null){Debug.LogError("Sprint 13: StarterAquarium asset missing.");return;}
            var secondDefinition=LoadOrCreateAquarium(SecondAquariumPath,"aquarium-definition-02","Acuario 2",80f,27f,5,
                new Color(.16f,.62f,.72f),definition);
            var thirdDefinition=LoadOrCreateAquarium(ThirdAquariumPath,"aquarium-definition-03","Acuario 3",35f,23f,2,
                new Color(.35f,.45f,.72f),definition);
            var aquariumDefinitions=new[]{definition,secondDefinition,thirdDefinition};
            var roots=scene.GetRootGameObjects();
            var root=(roots.FirstOrDefault(x=>x.name=="Root"||x.name=="RoomSceneRoot")??roots.FirstOrDefault())?.transform;
            if(root==null)root=new GameObject("MultiAquariumSceneRoot").transform;
            var safe=roots.SelectMany(x=>x.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(x=>x.name=="SafeArea"||x.name=="UIRoot");
            if(safe==null)safe=roots.SelectMany(x=>x.GetComponentsInChildren<Canvas>(true)).FirstOrDefault()?.transform;
            if(safe==null){Debug.LogError($"Sprint 13: no Canvas/SafeArea found among {roots.Length} roots.");return;}
            var carouselDefinition=AssetDatabase.LoadAssetAtPath<AquariumCarouselDefinition>(CarouselAssetPath);
            if(carouselDefinition==null)
            {
                carouselDefinition=ScriptableObject.CreateInstance<AquariumCarouselDefinition>();
                carouselDefinition.Configure(12f,.16f,.48f);
                AssetDatabase.CreateAsset(carouselDefinition,CarouselAssetPath);
            }

            var carousel=roots.SelectMany(x=>x.GetComponentsInChildren<Transform>(true)).FirstOrDefault(x=>x.name=="AquariumCarouselRoot");
            var slotViews=carousel!=null?carousel.GetComponentsInChildren<AquariumSlotView>(true)
                .OrderBy(x=>x.SlotId).ToArray():System.Array.Empty<AquariumSlotView>();
            var original=slotViews.Select(x=>x.GetComponentInChildren<AquariumFocusTarget>(true)).FirstOrDefault();
            var bindings=new AquariumViewBinding[slotViews.Length];
            if(original!=null)
            {
                for(var i=0;i<slotViews.Length;i++)
                {
                    var content=slotViews[i].transform.Find("Content");
                    var display=content!=null?content.GetComponentInChildren<AquariumFocusTarget>(true):null;
                    if(display==null)
                    {
                        display=Object.Instantiate(original,content,false);
                        display.name=$"AquariumRoomDisplay_{i+1:00}";
                    }
                    slotViews[i].transform.localScale=Vector3.one;
                    slotViews[i].transform.localPosition=AquariumCarouselLayout.Position(i,carouselDefinition.Spacing);
                    display.transform.localPosition=Vector3.zero;
                    display.transform.localRotation=Quaternion.identity;
                    display.transform.localScale=original.transform.localScale;
                    var focusTarget=display.GetComponent<AquariumFocusTarget>();
                    var focusPoint=display.transform.Find("FocusPoint")??display.transform;
                    focusTarget?.Configure(slotViews[i].SlotId,focusPoint,original.OrthographicSize);
                    var binding=display.GetComponent<AquariumViewBinding>()??display.gameObject.AddComponent<AquariumViewBinding>();
                    binding.Configure(slotViews[i].SlotId,display.transform,focusPoint,display.GetComponentInChildren<FishSpawner2D>(true));
                    bindings[i]=binding;
                }
            }
            var environment=roots.SelectMany(x=>x.GetComponentsInChildren<Transform>(true)).FirstOrDefault(x=>x.name=="Environment");
            if(environment!=null)
            {
                foreach(var backdropName in new[]{"Background","Wall","Floor","FloorTrim"})
                {
                    var backdrop=environment.Find(backdropName);
                    if(backdrop==null)continue;
                    var expander=backdrop.GetComponent<AquariumCarouselBackdrop>()??backdrop.gameObject.AddComponent<AquariumCarouselBackdrop>();
                    expander.Expand(slotViews.Length,carouselDefinition.Spacing);
                }
            }

            var old=Find("MultiAquariumRuntime");if(old!=null)Object.DestroyImmediate(old.gameObject);
            var runtime=new GameObject("MultiAquariumRuntime",typeof(AquariumManager),typeof(AquariumContextBinder),
                typeof(MultiAquariumRoomController),typeof(AquariumSwipeNavigationController),
                typeof(AquariumCameraCarouselController),typeof(AquariumNavigationCoordinator));
            runtime.transform.SetParent(root,false);
            var manager=runtime.GetComponent<AquariumManager>();
            var hud=Object.FindFirstObjectByType<AquariumHUDController>(FindObjectsInactive.Include);
            var simulation=Object.FindFirstObjectByType<AquariumSimulationController>(FindObjectsInactive.Include);
            var maintenance=Object.FindFirstObjectByType<AquariumMaintenanceController>(FindObjectsInactive.Include);
            var journal=Object.FindFirstObjectByType<AquaristJournalController>(FindObjectsInactive.Include);
            var habitat=Object.FindFirstObjectByType<AquariumHabitatController>(FindObjectsInactive.Include);
            var binder=runtime.GetComponent<AquariumContextBinder>();
            binder.Configure(manager,definition,hud,simulation,maintenance,journal,habitat);
            binder.SetFishSpawner(bindings.Length>0?bindings[0]?.FishSpawner:null);
            binder.SetAquariumSpawners(bindings.Select(x=>x?.FishSpawner).ToArray());
            binder.SetWelfareController(Object.FindFirstObjectByType<FishWelfareController>(FindObjectsInactive.Include));
            binder.SetManageFishPresentation(false);
            var cameraController=runtime.GetComponent<AquariumCameraCarouselController>();
            cameraController.Configure(Object.FindFirstObjectByType<Camera>(),carouselDefinition);
            var navigation=runtime.GetComponent<AquariumNavigationCoordinator>();
            navigation.Configure(manager,cameraController,bindings);
            var roomView=Object.FindFirstObjectByType<RoomViewController>(FindObjectsInactive.Include);
            for(var i=0;i<bindings.Length;i++)
            {
                var interactable=bindings[i]?.GetComponent<AquariumInteractable>();
                var selectable=bindings[i]?.GetComponent<AquariumCarouselSelectable>()??
                    bindings[i]?.gameObject.AddComponent<AquariumCarouselSelectable>();
                selectable?.Configure(interactable,navigation,roomView,i);
            }
            runtime.GetComponent<AquariumSwipeNavigationController>().Configure(manager,navigation);

            var oldPanel=safe.Find("AquariumSlotsPanel");if(oldPanel!=null)Object.DestroyImmediate(oldPanel.gameObject);
            var panel=new GameObject("AquariumSlotsPanel",typeof(RectTransform),typeof(Image));
            panel.transform.SetParent(safe,false);var rect=panel.GetComponent<RectTransform>();
            rect.anchorMin=new Vector2(.3f,.01f);rect.anchorMax=new Vector2(.7f,.105f);rect.offsetMin=rect.offsetMax=Vector2.zero;
            panel.GetComponent<Image>().color=new Color(.015f,.05f,.075f,.94f);
            var buttons=new Button[3];var labels=new Text[3];
            for(var i=0;i<3;i++)
            {
                var go=new GameObject($"AquariumSlot_{i+1:00}",typeof(RectTransform),typeof(Image),typeof(Button));
                go.transform.SetParent(panel.transform,false);var r=go.GetComponent<RectTransform>();
                r.anchorMin=new Vector2(i/3f+.01f,.12f);r.anchorMax=new Vector2((i+1)/3f-.01f,.88f);r.offsetMin=r.offsetMax=Vector2.zero;
                go.GetComponent<Image>().color=i==0?new Color(.12f,.55f,.62f):new Color(.08f,.3f,.36f);
                var textGo=new GameObject("Label",typeof(RectTransform),typeof(Text));textGo.transform.SetParent(go.transform,false);
                var tr=textGo.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=new Vector2(8,4);tr.offsetMax=new Vector2(-8,-4);
                var text=textGo.GetComponent<Text>();text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=15;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;
                buttons[i]=go.GetComponent<Button>();labels[i]=text;
            }
            var roomController=runtime.GetComponent<MultiAquariumRoomController>();
            roomController.Configure(manager,definition,buttons,labels);
            roomController.SetDefinitions(aquariumDefinitions);
            roomController.SetNavigation(navigation);
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("Sprint 13 multi-aquarium runtime and three slots configured.");
        }
        public static void ConfigureFromCommandLine(){Configure();EditorApplication.Exit(0);}
        public static void DumpHierarchyFromCommandLine()
        {
            var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
            var aquariumArea=scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(x=>x.name=="AquariumArea");
            if(aquariumArea!=null)Dump(aquariumArea,0);
            EditorApplication.Exit(0);
        }
        static void Dump(Transform value,int depth)
        {
            Debug.Log($"CAROUSEL_HIERARCHY {new string(' ',depth*2)}{value.name} [{string.Join(",",value.GetComponents<Component>().Select(x=>x.GetType().Name))}]");
            if(depth>=6)return;
            foreach(Transform child in value)Dump(child,depth+1);
        }
        static Transform Find(string name)
        {
            foreach(var item in Resources.FindObjectsOfTypeAll<Transform>())
                if(item!=null&&item.gameObject.scene.IsValid()&&item.name==name)return item;
            return null;
        }
        static AquariumDefinition LoadOrCreateAquarium(string path,string id,string label,float litres,float temperature,
            int capacity,Color color,AquariumDefinition template)
        {
            var value=AssetDatabase.LoadAssetAtPath<AquariumDefinition>(path);
            if(value==null){value=ScriptableObject.CreateInstance<AquariumDefinition>();AssetDatabase.CreateAsset(value,path);}
            value.Configure(id,label,litres,new Vector2(temperature-1f,temperature+1f),temperature,capacity,
                $"Acuario independiente de {litres:0} litros.","Cada acuario conserva parámetros propios.",color);
            if(template!=null)
            {
                value.ConfigureDecorations(template.InstalledDecorations.ToArray());
                value.ConfigureDecorationPlacements(template.DecorationPlacements.ToArray());
            }
            EditorUtility.SetDirty(value);
            return value;
        }
    }
}
