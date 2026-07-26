using System;
using System.Collections.Generic;
using Acuaria.Fish;
using Acuaria.Fish.Care;
using Acuaria.Fish.Compatibility;
using Acuaria.UI;
using Acuaria.UI.Progression;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.Editor
{
    public static class FishSpeciesContentSetup
    {
        const string Root="Assets/_Acuaria/Data/FishSpecies/Freshwater";
        const string PopulationRoot="Assets/_Acuaria/Data/FishSpecies/PopulationPresets";
        [MenuItem("Acuaria/Setup/Sprint 10 Species Content")]
        public static void Configure()
        {
            EnsureFolder("Assets/_Acuaria/Data/FishSpecies","Freshwater");EnsureFolder("Assets/_Acuaria/Data/FishSpecies","PopulationPresets");
            var prefab=AssetDatabase.LoadAssetAtPath<FishView>("Assets/_Acuaria/Prefabs/Fish/Fish2D.prefab");
            var species=new[]{
                Create("fish.guppy","Guppy","Poecilia reticulata","Poeciliidae","Norte de Sudamérica",new Vector2(2.8f,6),new Vector2(2,3),
                    new Vector2(22,28),40,60,5,FishActivityLevel.High,SwimmingLevel.Upper,FishSocialType.Group,5,Color.cyan,
                    "Vivíparo activo que agradece vegetación y compañía de su especie.","Evita mezclar sin revisar temperatura, población y proporción social.",prefab),
                Create("fish.neon_tetra","Tetra neón","Paracheirodon innesi","Characidae","Cuenca amazónica",new Vector2(2.5f,4),new Vector2(3,5),
                    new Vector2(21,26),50,60,3.5f,FishActivityLevel.Moderate,SwimmingLevel.Middle,FishSocialType.Schooling,6,new Color(.15f,.65f,1),
                    "Pequeño pez de cardumen que se muestra más seguro en grupo.","Mantén un cardumen y agua estable; su pequeño tamaño no elimina la necesidad de espacio horizontal.",prefab),
                Create("fish.peppered_corydoras","Corydora pimienta","Corydoras paleatus","Callichthyidae","Sur de Sudamérica",new Vector2(5,7.5f),new Vector2(5,10),
                    new Vector2(18,25),60,75,6.5f,FishActivityLevel.Moderate,SwimmingLevel.Lower,FishSocialType.Group,6,new Color(.55f,.5f,.42f),
                    "Bagre social de fondo que explora el sustrato con sus barbillones.","Usa sustrato suave y no lo consideres un sustituto del mantenimiento.",prefab),
                Create("fish.betta_splendens","Betta","Betta splendens","Osphronemidae","Sudeste asiático",new Vector2(5,7),new Vector2(2,5),
                    new Vector2(24,28),20,20,6.5f,FishActivityLevel.Low,SwimmingLevel.Upper,FishSocialType.Solitary,1,new Color(.8f,.2f,.35f),
                    "Pez laberíntido territorial; los individuos y variedades pueden responder de forma distinta.","No alojes dos machos juntos; evalúa cualquier comunidad caso por caso.",prefab),
                Create("fish.platy","Platy","Xiphophorus maculatus","Poeciliidae","México y Centroamérica",new Vector2(4,6),new Vector2(3,5),
                    new Vector2(20,25),50,60,5,FishActivityLevel.Moderate,SwimmingLevel.Middle,FishSocialType.Group,5,new Color(1,.55f,.15f),
                    "Vivíparo robusto y activo que utiliza la zona media.","Planifica la población: su reproducción no se simula en este sprint.",prefab)};
            var registry=LoadOrCreate<FishSpeciesRegistry>("Assets/_Acuaria/Data/FishSpecies/FishSpeciesRegistry.asset");
            registry.Configure(species);EditorUtility.SetDirty(registry);
            CreatePopulation("starter-real-species","Población inicial educativa",PopulationValidationStatus.Reviewed,
                new AquariumPopulationEntry(species[3],1,1701,"Betta"),new AquariumPopulationEntry(species[2],2,2701,"Corydora"));
            CreateDebugPresets(species);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            LinkStarterPopulation();ConfigureCatalogUI();
        }
        public static void ConfigureFromCommandLine(){Configure();}

        static FishSpeciesDefinition Create(string id,string common,string scientific,string family,string origin,Vector2 length,
            Vector2 lifespan,Vector2 temperature,float individualVolume,float groupVolume,float adultSize,FishActivityLevel activity,
            SwimmingLevel zone,FishSocialType socialType,int group,Color color,string summary,string warning,FishView prefab)
        {
            var path=$"{Root}/{id.Replace('.','-')}.asset";var asset=LoadOrCreate<FishSpeciesDefinition>(path);
            asset.Configure(id,common,new Vector2(.45f,.8f),new Vector2(.75f,1),new Vector2(2.5f,5),zone==SwimmingLevel.Upper?.45f:zone==SwimmingLevel.Lower?-.55f:0,color,zone);
            var care=new FishCareRequirements();care.Configure(temperature,individualVolume,groupVolume,adultSize,activity,zone,
                FishWaterSensitivity.Moderate,false,true,Vector3.one,2,FishDietType.Omnivore);
            care.ConfigureExtended(FishCareDifficulty.Beginner,60,FishEnvironmentLevel.Low,FishEnvironmentLevel.Moderate,.55f,.55f,true,warning);
            var social=new FishSocialRequirements();social.Configure(socialType,group,Mathf.Max(group,10),socialType==FishSocialType.Solitary,
                socialType==FishSocialType.Solitary?FishTerritoriality.Territorial:FishTerritoriality.Peaceful,true,
                socialType==FishSocialType.Schooling,false,true,socialType!=FishSocialType.Solitary);
            social.ConfigureEducation(group,"Comportamiento intraespecífico sujeto a sexo y espacio.","Información educativa; sin sexo runtime.",summary);
            var compatibility=new FishCompatibilityProfile();compatibility.Configure(new Vector2(2,12),true,true,false,true,true,
                socialType==FishSocialType.Solitary,false,null,socialType==FishSocialType.Solitary?new[]{"fish.betta_splendens"}:null);
            asset.ConfigureCare(care,social,compatibility);
            var biology=new FishBiologicalProfile();biology.Configure(scientific,family,origin,length,lifespan,activity,zone,summary);
            var education=new FishEducationalProfile();education.Configure(summary,summary,warning,"Dieta omnívora variada en porciones pequeñas.",
                socialType==FishSocialType.Solitary?"Se mantiene de forma solitaria.":$"Se recomienda un grupo de al menos {group}.",
                "La compatibilidad depende del conjunto completo de necesidades.","Compara la ficha con el acuario actual.",warning,$"species.{id}");
            var visual=new FishVisualDefinition();visual.Configure(prefab,new Vector2(.75f,1),new Vector2(.45f,.8f),color);
            asset.ConfigureContent(biology,education,visual,SpeciesDataValidationStatus.Reviewed,Sources(scientific),
                "Perfil educativo aproximado basado en fuentes documentadas; requiere revisión acuarista.",1,"freshwater","real-species","placeholder-art");
            EditorUtility.SetDirty(asset);return asset;
        }
        static SpeciesSourceReference[] Sources(string scientific)
        {
            var fishBase=new SpeciesSourceReference();fishBase.Configure($"fishbase.{scientific.Replace(' ','.').ToLowerInvariant()}",
                $"{scientific} summary page","FishBase",SpeciesSourceType.Scientific,"2026-07-26",
                $"https://www.fishbase.se/summary/{scientific.Replace(' ','-')}","Taxonomía, distribución, tamaño y temperatura.","scientificName","family","origin","adultLength","temperature");
            return new[]{fishBase};
        }
        static void CreateDebugPresets(FishSpeciesDefinition[] s)
        {CreatePopulation("debug-guppy","Debug Guppy",PopulationValidationStatus.Debug,new AquariumPopulationEntry(s[0],5,100));
         CreatePopulation("debug-neon-tetra","Debug Tetra neón",PopulationValidationStatus.Debug,new AquariumPopulationEntry(s[1],6,200));
         CreatePopulation("debug-corydoras","Debug Corydora",PopulationValidationStatus.Debug,new AquariumPopulationEntry(s[2],6,300));
         CreatePopulation("debug-betta","Debug Betta",PopulationValidationStatus.Debug,new AquariumPopulationEntry(s[3],1,400));
         CreatePopulation("debug-incompatible","Debug incompatible",PopulationValidationStatus.Debug,new AquariumPopulationEntry(s[3],2,500));}
        static AquariumPopulationDefinition CreatePopulation(string id,string label,PopulationValidationStatus status,params AquariumPopulationEntry[] entries)
        {var asset=LoadOrCreate<AquariumPopulationDefinition>($"{PopulationRoot}/{id}.asset");asset.Configure(id,label,status,entries);EditorUtility.SetDirty(asset);return asset;}
        static T LoadOrCreate<T>(string path)where T:ScriptableObject
        {var asset=AssetDatabase.LoadAssetAtPath<T>(path);if(asset!=null)return asset;asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);return asset;}
        static void EnsureFolder(string parent,string name){if(!AssetDatabase.IsValidFolder($"{parent}/{name}"))AssetDatabase.CreateFolder(parent,name);}
        static void LinkStarterPopulation()
        {
            var scene=EditorSceneManager.OpenScene("Assets/_Acuaria/Scenes/Room.unity",OpenSceneMode.Single);
            var spawner=UnityEngine.Object.FindAnyObjectByType<FishSpawner2D>();if(spawner==null)return;
            var serialized=new SerializedObject(spawner);serialized.FindProperty("population").objectReferenceValue=
                AssetDatabase.LoadAssetAtPath<AquariumPopulationDefinition>($"{PopulationRoot}/starter-real-species.asset");
            serialized.ApplyModifiedPropertiesWithoutUndo();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }

        [MenuItem("Acuaria/Setup/Sprint 10 Catalog UI")]
        public static void ConfigureCatalogUI()
        {
            if(EditorSceneManager.GetActiveScene().path!="Assets/_Acuaria/Scenes/Room.unity")
                EditorSceneManager.OpenScene("Assets/_Acuaria/Scenes/Room.unity",OpenSceneMode.Single);
            var journal=FindSceneComponent<AquaristJournalController>();
            var progression=FindSceneComponent<ProgressionUI>();
            if(journal==null||progression==null){Debug.LogError($"Catalog setup missing scene references. Journal={journal!=null}, Progression={progression!=null}");return;}
            var journalRoot=progression.gameObject.transform;var old=journalRoot.Find("FishCatalogPanel");if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var oldButton=journalRoot.Find("FishCatalogButton");if(oldButton!=null)UnityEngine.Object.DestroyImmediate(oldButton.gameObject);
            var registry=AssetDatabase.LoadAssetAtPath<FishSpeciesRegistry>("Assets/_Acuaria/Data/FishSpecies/FishSpeciesRegistry.asset");
            var catalogButton=CreateButton(journalRoot,"FishCatalogButton","Catálogo de peces",
                new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,24),new Vector2(250,58));
            catalogButton.GetComponent<RectTransform>().pivot=new Vector2(.5f,0);
            var root=new GameObject("FishCatalogPanel",typeof(RectTransform),typeof(Image),typeof(CanvasGroup),typeof(FishCatalogController),typeof(FishCatalogPanel));
            root.transform.SetParent(journalRoot,false);Stretch((RectTransform)root.transform);root.GetComponent<Image>().color=new Color(.015f,.03f,.055f,.97f);
            var group=root.GetComponent<CanvasGroup>();var controller=root.GetComponent<FishCatalogController>();controller.Configure(registry,"fish.betta_splendens","fish.peppered_corydoras");
            var title=CreateText(root.transform,"CatalogTitle","Catálogo de peces",28,TextAnchor.MiddleLeft,new Color(.92f,.97f,1,1));
            SetRect(title.rectTransform,new Vector2(.04f,.87f),new Vector2(.58f,.97f),Vector2.zero,Vector2.zero);
            var close=CreateButton(root.transform,"CatalogClose","Volver al Diario",new Vector2(.78f,.88f),new Vector2(.96f,.96f),Vector2.zero,Vector2.zero);
            var filterLabel=CreateText(root.transform,"FilterLabel","Zona: Todas",19,TextAnchor.MiddleCenter,Color.white);
            var filter=CreateButton(root.transform,"FilterButton","Zona: Todas",new Vector2(.04f,.77f),new Vector2(.27f,.85f),Vector2.zero,Vector2.zero,filterLabel);
            var sortLabel=CreateText(root.transform,"SortLabel","Orden: Catálogo",19,TextAnchor.MiddleCenter,Color.white);
            var sort=CreateButton(root.transform,"SortButton","Orden: Catálogo",new Vector2(.29f,.77f),new Vector2(.54f,.85f),Vector2.zero,Vector2.zero,sortLabel);
            var empty=CreateText(root.transform,"EmptyLabel","No hay especies con este filtro.",20,TextAnchor.MiddleCenter,new Color(.8f,.85f,.9f,1));
            SetRect(empty.rectTransform,new Vector2(.05f,.35f),new Vector2(.55f,.55f),Vector2.zero,Vector2.zero);empty.gameObject.SetActive(false);
            var buttons=new Button[5];var labels=new Text[5];
            for(var i=0;i<5;i++){var top=.73f-i*.125f;labels[i]=CreateText(root.transform,$"SpeciesLabel{i}","Especie",20,TextAnchor.MiddleLeft,Color.white);
             buttons[i]=CreateButton(root.transform,$"SpeciesButton{i}","Especie",new Vector2(.04f,top-.1f),new Vector2(.54f,top),Vector2.zero,Vector2.zero,labels[i]);
             labels[i].rectTransform.offsetMin=new Vector2(16,4);labels[i].rectTransform.offsetMax=new Vector2(-8,-4);}
            var info=CreateText(root.transform,"CatalogHelp","Toca una especie para estudiar su ficha.\nEl catálogo informa; no compra ni modifica la población.",18,TextAnchor.UpperLeft,new Color(.7f,.83f,.88f,1));
            SetRect(info.rectTransform,new Vector2(.59f,.66f),new Vector2(.96f,.82f),Vector2.zero,Vector2.zero);
            var detail=CreateDetailPanel(root.transform);
            var panel=root.GetComponent<FishCatalogPanel>();panel.Configure(group,controller,close,filter,filterLabel,sort,sortLabel,empty,buttons,labels,detail);
            journal.SetFishCatalog(catalogButton,panel);root.SetActive(false);
            PrefabUtility.SaveAsPrefabAsset(journalRoot.gameObject,"Assets/_Acuaria/Prefabs/UI/Progression/AquaristJournal.prefab");
            EditorSceneManager.MarkSceneDirty(journalRoot.gameObject.scene);EditorSceneManager.SaveScene(journalRoot.gameObject.scene);
            AssetDatabase.SaveAssets();Debug.Log("Sprint 10 fish catalog UI configured.");
        }
        static FishSpeciesDetailPanel CreateDetailPanel(Transform parent)
        {
            var root=new GameObject("FishSpeciesDetailPanel",typeof(RectTransform),typeof(Image),typeof(CanvasGroup),typeof(FishSpeciesDetailPanel));
            root.transform.SetParent(parent,false);SetRect((RectTransform)root.transform,new Vector2(.56f,.08f),new Vector2(.97f,.75f),Vector2.zero,Vector2.zero);
            root.GetComponent<Image>().color=new Color(.04f,.09f,.14f,.99f);var title=CreateText(root.transform,"DetailTitle","Ficha",25,TextAnchor.UpperLeft,Color.white);
            SetRect(title.rectTransform,new Vector2(.05f,.76f),new Vector2(.72f,.96f),Vector2.zero,Vector2.zero);
            var close=CreateButton(root.transform,"DetailClose","Cerrar ficha",new Vector2(.72f,.84f),new Vector2(.96f,.96f),Vector2.zero,Vector2.zero);
            var body=CreateText(root.transform,"DetailBody","",18,TextAnchor.UpperLeft,new Color(.9f,.95f,.97f,1));
            SetRect(body.rectTransform,new Vector2(.05f,.21f),new Vector2(.95f,.75f),Vector2.zero,Vector2.zero);
            var suitability=CreateText(root.transform,"Suitability","",17,TextAnchor.UpperLeft,new Color(.48f,.9f,.78f,1));
            SetRect(suitability.rectTransform,new Vector2(.05f,.04f),new Vector2(.95f,.19f),Vector2.zero,Vector2.zero);
            var panel=root.GetComponent<FishSpeciesDetailPanel>();panel.Configure(root.GetComponent<CanvasGroup>(),close,title,body,suitability);root.SetActive(false);return panel;
        }
        static T FindSceneComponent<T>()where T:Component
        {var values=Resources.FindObjectsOfTypeAll<T>();for(var i=0;i<values.Length;i++)if(values[i].gameObject.scene.IsValid()&&!EditorUtility.IsPersistent(values[i]))return values[i];return null;}
        static Text CreateText(Transform parent,string name,string value,int size,TextAnchor alignment,Color color)
        {var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));go.transform.SetParent(parent,false);var text=go.GetComponent<Text>();
         text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.text=value;text.fontSize=size;text.alignment=alignment;text.color=color;text.raycastTarget=false;
         text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;return text;}
        static Button CreateButton(Transform parent,string name,string label,Vector2 min,Vector2 max,Vector2 position,Vector2 size,Text supplied=null)
        {var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);
         SetRect((RectTransform)go.transform,min,max,position,size);var image=go.GetComponent<Image>();image.color=new Color(.13f,.48f,.54f,1);var button=go.GetComponent<Button>();button.targetGraphic=image;
         var text=supplied??CreateText(go.transform,"Label",label,19,TextAnchor.MiddleCenter,Color.white);if(supplied!=null)supplied.transform.SetParent(go.transform,false);
         Stretch(text.rectTransform);text.text=label;return button;}
        static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;}
        static void SetRect(RectTransform rect,Vector2 min,Vector2 max,Vector2 position,Vector2 size)
        {rect.anchorMin=min;rect.anchorMax=max;rect.anchoredPosition=position;rect.sizeDelta=size;}
    }
}
