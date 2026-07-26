using System;
using System.Collections.Generic;
using Acuaria.Aquarium;
using Acuaria.Aquarium.Decorations;
using Acuaria.Fish;
using Acuaria.Progression;
using Acuaria.UI.FishWelfare;
using Acuaria.UI.Habitat;
using Acuaria.UI.Progression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.Editor
{
    public static class HabitatContentSetup
    {
        const string Root = "Assets/_Acuaria/Data/Decorations";
        [MenuItem("Acuaria/Setup/Sprint 11 Habitat")]
        public static void Configure()
        {
            EnsureFolder("Assets/_Acuaria/Data", "Decorations");
            var species = AssetDatabase.LoadAssetAtPath<FishSpeciesRegistry>("Assets/_Acuaria/Data/FishSpecies/FishSpeciesRegistry.asset");
            FishSpeciesDefinition Fish(string id) => species?.FindById(id);
            var plants=Create("decoration.plant.cluster","Plantas naturales","Vegetación de hojas suaves que aporta cobertura.",DecorationCategory.Plant,
                new HabitatContribution(.35f,1,.08f,.08f,.18f,.3f),"Las plantas ofrecen límites visuales y refugio; no sustituyen el mantenimiento.",Fish("fish.betta_splendens"),Fish("fish.guppy"));
            var wood=Create("decoration.wood.branch","Tronco ramificado","Madera neutra educativa con huecos y cobertura.",DecorationCategory.Wood,
                new HabitatContribution(0,1,.1f,.12f,.12f,.35f),"Los troncos estructuran el territorio. En este sprint no alteran el pH.",Fish("fish.peppered_corydoras"));
            var rock=Create("decoration.rock.group","Grupo de rocas","Rocas estables que forman límites y pequeños refugios.",DecorationCategory.Rock,
                new HabitatContribution(0,.7f,.08f,.05f,.05f,.25f),"Las rocas deben colocarse de forma estable y sin aristas peligrosas.",Fish("fish.peppered_corydoras"));
            var cave=Create("decoration.cave.small","Cueva pequeña","Refugio definido para especies que buscan esconderse.",DecorationCategory.Cave,
                new HabitatContribution(0,2,.09f,.04f,.08f,.3f),"Un escondite permite retirarse del estímulo sin aislar permanentemente al pez.",Fish("fish.betta_splendens"),Fish("fish.peppered_corydoras"));
            var sand=Create("decoration.substrate.sand","Arena suave","Sustrato fino de función informativa.",DecorationCategory.Substrate,
                new HabitatContribution(0,0,0,0,0,.08f),"La arena suave protege los barbillones de peces de fondo; aquí no modifica química.",Fish("fish.peppered_corydoras"));
            var gravel=Create("decoration.substrate.gravel","Grava neutra","Sustrato de grava redondeada.",DecorationCategory.Substrate,
                new HabitatContribution(0,0,0,0,0,.08f),"El tamaño y limpieza del sustrato importan aunque no simulemos su química.");
            var neutral=Create("decoration.artificial.neutral","Decoración neutra","Elemento artificial sin beneficios específicos.",DecorationCategory.Artificial,
                new HabitatContribution(0,0,.05f,0,0,.06f),"Una decoración debe ser segura y no liberar sustancias, aunque sea artificial.");
            var registry=LoadOrCreate<DecorationRegistry>($"{Root}/DecorationRegistry.asset");
            registry.Configure(plants,wood,rock,cave,sand,gravel,neutral);EditorUtility.SetDirty(registry);
            var aquarium=AssetDatabase.LoadAssetAtPath<AquariumDefinition>("Assets/_Acuaria/Data/Aquariums/StarterAquarium.asset");
            aquarium.ConfigureDecorations(sand,plants,plants,wood,rock,cave);
            aquarium.ConfigureDecorationPlacements(
                Placement("starter-substrate",sand,.5f,.11f,7.1f,.16f,DecorationVisualLayer.Substrate),
                Placement("starter-plant-left",plants,.18f,.36f,.42f,1.15f),
                Placement("starter-plant-right",plants,.82f,.36f,.42f,1.05f),
                Placement("starter-wood",wood,.62f,.24f,1.25f,.3f,DecorationVisualLayer.Midground,12),
                Placement("starter-rock",rock,.42f,.22f,.62f,.42f),
                Placement("starter-cave",cave,.72f,.24f,.82f,.5f));EditorUtility.SetDirty(aquarium);
            CreateProgression();
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            ConfigureScene(aquarium,registry);
        }
        public static void ConfigureFromCommandLine()=>Configure();

        static DecorationDefinition Create(string id,string label,string description,DecorationCategory category,HabitatContribution contribution,string education,params FishSpeciesDefinition[] favoured)
        {var asset=LoadOrCreate<DecorationDefinition>($"{Root}/{id.Replace('.','-')}.asset");asset.Configure(id,label,description,category,Vector2.one,.1f,contribution,education,favoured);
         asset.ConfigureVisual(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Acuaria/Art/Prototype/Room/PrototypeWhite.png"));EditorUtility.SetDirty(asset);return asset;}

        static void ConfigureScene(AquariumDefinition aquarium,DecorationRegistry registry)
        {
            var scene=EditorSceneManager.OpenScene("Assets/_Acuaria/Scenes/Room.unity",OpenSceneMode.Single);
            var safeArea=FindTransform("SafeArea")??FindTransform("UIRoot");if(safeArea==null){Debug.LogError("Sprint 11: SafeArea/UIRoot no encontrado.");return;}
            var old=safeArea.Find("HabitatSystem");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var system=new GameObject("HabitatSystem",typeof(RectTransform),typeof(AquariumHabitatController));system.transform.SetParent(safeArea,false);
            var populations=FindTransforms("FishPopulation");if(populations.Count==0){Debug.LogError("Sprint 11 hotfix: FishPopulation no encontrado.");return;}
            var controller=system.GetComponent<AquariumHabitatController>();DecorationSpawner2D primarySpawner=null;Transform primaryRoot=null;
            for(var populationIndex=0;populationIndex<populations.Count;populationIndex++)
            {
                var fishPopulation=populations[populationIndex];var aquariumRoot=fishPopulation.parent;
                var previousDecorations=aquariumRoot.Find("DecorationsRoot");if(previousDecorations!=null)UnityEngine.Object.DestroyImmediate(previousDecorations.gameObject);
                var decorationsRoot=new GameObject("DecorationsRoot",typeof(AquariumDecorationArea2D),typeof(DecorationSpawner2D));
                decorationsRoot.transform.SetParent(aquariumRoot,false);decorationsRoot.transform.localPosition=fishPopulation.localPosition;
                var area=decorationsRoot.GetComponent<AquariumDecorationArea2D>();area.Configure(new Vector2(7.35f,3.2f),Vector2.zero);
                var spawner=decorationsRoot.GetComponent<DecorationSpawner2D>();spawner.Configure(decorationsRoot.transform,area,controller,aquarium);
                if(primarySpawner==null){primarySpawner=spawner;primaryRoot=decorationsRoot.transform;}
            }
            controller.Configure(aquarium,registry,primaryRoot);controller.ConfigureVisuals(primarySpawner);
            var open=Button(safeArea,"HabitatButton","Hábitat",new Vector2(.31f,0),new Vector2(.41f,.075f));
            var panel=new GameObject("HabitatPanel",typeof(RectTransform),typeof(Image),typeof(CanvasGroup),typeof(AquariumHabitatPanel));
            panel.transform.SetParent(safeArea,false);Rect(panel.GetComponent<RectTransform>(),new Vector2(.12f,.1f),new Vector2(.88f,.9f));
            panel.GetComponent<Image>().color=new Color(.02f,.055f,.075f,.98f);
            var title=Text(panel.transform,"Title","HÁBITAT",28,TextAnchor.MiddleLeft);Rect(title.rectTransform,new Vector2(.05f,.86f),new Vector2(.6f,.97f));
            var close=Button(panel.transform,"Close","Cerrar",new Vector2(.78f,.87f),new Vector2(.95f,.96f));
            var summary=Text(panel.transform,"Summary","",23,TextAnchor.UpperLeft);Rect(summary.rectTransform,new Vector2(.06f,.34f),new Vector2(.46f,.82f));
            var reason=Text(panel.transform,"Explanation","",20,TextAnchor.UpperLeft);Rect(reason.rectTransform,new Vector2(.5f,.52f),new Vector2(.94f,.82f));
            var catalog=Text(panel.transform,"Catalog","",18,TextAnchor.UpperLeft);Rect(catalog.rectTransform,new Vector2(.5f,.12f),new Vector2(.94f,.48f));
            var detail=Text(panel.transform,"Detail","Selecciona fichas desde el catálogo educativo.",16,TextAnchor.UpperLeft);Rect(detail.rectTransform,new Vector2(.06f,.08f),new Vector2(.46f,.3f));
            panel.GetComponent<AquariumHabitatPanel>().Configure(controller,panel.GetComponent<CanvasGroup>(),summary,reason,close);
            var catalogPanel=panel.AddComponent<DecorationCatalogPanel>();catalogPanel.Configure(registry,catalog,detail);
            CatalogButton(panel.transform,"PlantCard","Planta",0,"decoration.plant.cluster",catalogPanel);
            CatalogButton(panel.transform,"RockCard","Roca",1,"decoration.rock.group",catalogPanel);
            CatalogButton(panel.transform,"WoodCard","Tronco",2,"decoration.wood.branch",catalogPanel);
            CatalogButton(panel.transform,"CaveCard","Cueva",3,"decoration.cave.small",catalogPanel);
            UnityEventTools.AddPersistentListener(open.onClick,panel.GetComponent<AquariumHabitatPanel>().Open);
            var debugRoot=new GameObject("HabitatDebug",typeof(RectTransform));debugRoot.transform.SetParent(panel.transform,false);Rect(debugRoot.GetComponent<RectTransform>(),new Vector2(.05f,.01f),new Vector2(.95f,.08f));
            DebugButton(debugRoot.transform,"+ Planta",0,controller.AddPlant);DebugButton(debugRoot.transform,"- Planta",1,controller.RemovePlant);
            DebugButton(debugRoot.transform,"+ Roca",2,controller.AddRock);DebugButton(debugRoot.transform,"- Roca",3,controller.RemoveRock);
            DebugButton(debugRoot.transform,"+ Cueva",4,controller.AddCave);DebugButton(debugRoot.transform,"Restablecer",5,controller.ResetHabitat);
            panel.SetActive(false);
            var welfare=FindComponent<FishWelfareController>();if(welfare!=null){var so=new SerializedObject(welfare);so.FindProperty("aquariumDefinition").objectReferenceValue=aquarium;so.FindProperty("habitatController").objectReferenceValue=controller;so.ApplyModifiedPropertiesWithoutUndo();}
            LinkProgressionContent();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            Debug.Log("Sprint 11 habitat and decoration system configured.");
        }

        static void CreateProgression()
        {
            EnsureFolder("Assets/_Acuaria/Data/Progression/Missions","Habitat");EnsureFolder("Assets/_Acuaria/Data/Progression/Achievements","Habitat");
            Mission("know-plants","Conoce las plantas","Consulta por qué las plantas ofrecen cobertura.",ProgressionEventType.PlantLearned,15);
            Mission("learn-hiding","Aprende por qué existen escondites","Estudia la función de los refugios.",ProgressionEventType.HidingPlaceLearned,15);
            Mission("inspect-rock","Consulta una roca","Abre la ficha educativa de una roca.",ProgressionEventType.RockObserved,10);
            Mission("inspect-wood","Consulta un tronco","Abre la ficha educativa de un tronco.",ProgressionEventType.WoodObserved,10);
            Achievement("first-natural-habitat","Primer hábitat natural","Crea tu primer hábitat equilibrado.",ProgressionEventType.NaturalHabitatCreated);
            Achievement("first-hiding-place","Primer escondite","Añade el primer refugio.",ProgressionEventType.HidingPlaceAdded);
            Achievement("first-planted-aquarium","Primer acuario plantado","Añade cobertura vegetal.",ProgressionEventType.PlantedAquariumCreated);
        }
        static void Mission(string id,string title,string text,ProgressionEventType condition,int xp){var a=LoadOrCreate<MissionDefinition>($"Assets/_Acuaria/Data/Progression/Missions/Habitat/{id}.asset");a.Configure(id,title,text,MissionType.Educational,condition,1,xp);EditorUtility.SetDirty(a);}
        static void Achievement(string id,string title,string text,ProgressionEventType condition){var a=LoadOrCreate<AchievementDefinition>($"Assets/_Acuaria/Data/Progression/Achievements/Habitat/{id}.asset");a.Configure(id,title,text,condition,1,20);EditorUtility.SetDirty(a);}
        static DecorationPlacementData Placement(string id,DecorationDefinition definition,float x,float y,float sx,float sy,
            DecorationVisualLayer layer=DecorationVisualLayer.Midground,float rotation=0)=>
            new(id,definition,new Vector2(x,y),new Vector2(sx,sy),rotation,false,0,layer);
        static void DebugButton(Transform parent,string label,int index,UnityEngine.Events.UnityAction action){var b=Button(parent,label.Replace(" ",""),label,new Vector2(index/6f,0),new Vector2((index+1)/6f-.005f,1));UnityEventTools.AddPersistentListener(b.onClick,action);}
        static void CatalogButton(Transform parent,string name,string label,int index,string id,DecorationCatalogPanel catalog)
        {
            var width=.44f/4f;
            var button=Button(parent,name,label,new Vector2(.5f+index*width,.485f),new Vector2(.5f+(index+1)*width-.005f,.535f));
            UnityEventTools.AddStringPersistentListener(button.onClick,catalog.Show,id);
        }
        static void LinkProgressionContent()
        {
            var journal=FindComponent<AquaristJournalController>();if(journal==null)return;
            var serialized=new SerializedObject(journal);
            AppendAssets<MissionDefinition>(serialized.FindProperty("missionDefinitions"),"Assets/_Acuaria/Data/Progression/Missions/Habitat");
            AppendAssets<AchievementDefinition>(serialized.FindProperty("achievementDefinitions"),"Assets/_Acuaria/Data/Progression/Achievements/Habitat");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
        static void AppendAssets<T>(SerializedProperty array,string folder)where T:UnityEngine.Object
        {
            var values=new List<UnityEngine.Object>();for(var i=0;i<array.arraySize;i++){var value=array.GetArrayElementAtIndex(i).objectReferenceValue;if(value!=null)values.Add(value);}
            foreach(var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}",new[]{folder}))
            {var value=AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));if(value!=null&&!values.Contains(value))values.Add(value);}
            array.arraySize=values.Count;for(var i=0;i<values.Count;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=values[i];
        }
        static T LoadOrCreate<T>(string path)where T:ScriptableObject{var a=AssetDatabase.LoadAssetAtPath<T>(path);if(a!=null)return a;a=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(a,path);return a;}
        static void EnsureFolder(string parent,string name){if(!AssetDatabase.IsValidFolder($"{parent}/{name}"))AssetDatabase.CreateFolder(parent,name);}
        static T FindComponent<T>()where T:Component{var all=Resources.FindObjectsOfTypeAll<T>();foreach(var x in all)if(x.gameObject.scene.IsValid()&&!EditorUtility.IsPersistent(x))return x;return null;}
        static Transform FindTransform(string name){foreach(var r in EditorSceneManager.GetActiveScene().GetRootGameObjects()){var all=r.GetComponentsInChildren<Transform>(true);foreach(var t in all)if(t.name==name)return t;}return null;}
        static List<Transform> FindTransforms(string name){var result=new List<Transform>();foreach(var r in EditorSceneManager.GetActiveScene().GetRootGameObjects()){var all=r.GetComponentsInChildren<Transform>(true);foreach(var t in all)if(t.name==name)result.Add(t);}return result;}
        static Text Text(Transform parent,string name,string value,int size,TextAnchor alignment){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.alignment=alignment;t.color=Color.white;t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;t.raycastTarget=false;return t;}
        static Button Button(Transform parent,string name,string label,Vector2 min,Vector2 max){var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);Rect(go.GetComponent<RectTransform>(),min,max);go.GetComponent<Image>().color=new Color(.12f,.48f,.55f,1);var t=Text(go.transform,"Label",label,17,TextAnchor.MiddleCenter);Rect(t.rectTransform,Vector2.zero,Vector2.one);return go.GetComponent<Button>();}
        static void Rect(RectTransform rect,Vector2 min,Vector2 max){rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;}
    }
}
