using System.Collections;
using System.Text;
using Acuaria.Fish.Welfare;
using Acuaria.Food;
using Acuaria.Progression;
using Acuaria.Simulation.Filtration;
using Acuaria.Simulation.Water;
using Acuaria.UI.Aquarium;
using Acuaria.UI.FishWelfare;
using Acuaria.UI.Maintenance;
using Acuaria.UI.WaterChemistry;
using UnityEngine;
using UnityEngine.UI;
namespace Acuaria.UI.Progression
{
    public sealed class AquaristJournalController:MonoBehaviour
    {
        [SerializeField]MissionDefinition[] missionDefinitions;[SerializeField]CodexEntry[] codexEntries;[SerializeField]AchievementDefinition[] achievementDefinitions;
        [SerializeField]AquariumFoodController food;[SerializeField]AquariumSimulationController simulation;[SerializeField]AquariumMaintenanceController maintenance;
        [SerializeField]FishWelfareController welfare;[SerializeField]AquariumHUDController hud;[SerializeField]ProgressionUI panel;[SerializeField]Button openButton;
        [SerializeField]FeedingUIController feedingUi;
        [SerializeField]Button fishCatalogButton;[SerializeField]Acuaria.UI.FishCatalogPanel fishCatalogPanel;
        readonly PlayerProgression player=new();MissionController missions;CodexController codex;AchievementController achievements;StatisticsController statistics;
        int lastExcellentWaterHour,lastExcellentWelfareHour;bool focused;
        Coroutine notificationRoutine;
        public PlayerProgression Player=>player;public MissionController Missions=>missions;public CodexController Codex=>codex;public AchievementController Achievements=>achievements;
        public void Configure(MissionDefinition[] missionList,CodexEntry[] entries,AchievementDefinition[] achievementList,AquariumFoodController foodController,
            AquariumSimulationController chemistry,AquariumMaintenanceController maintenanceController,FishWelfareController welfareController,AquariumHUDController hudController,
            ProgressionUI progressionPanel,Button open,FeedingUIController feeding)
        {missionDefinitions=missionList;codexEntries=entries;achievementDefinitions=achievementList;food=foodController;simulation=chemistry;maintenance=maintenanceController;welfare=welfareController;hud=hudController;panel=progressionPanel;openButton=open;feedingUi=feeding;}
        void Awake(){missions=new MissionController(player,missionDefinitions);codex=new CodexController(codexEntries);achievements=new AchievementController(player,achievementDefinitions);statistics=new StatisticsController(player.Statistics);}
        void OnEnable(){openButton?.onClick.AddListener(Open);if(panel!=null)panel.Closed+=Close;if(food!=null){food.FoodConsumed+=OnFed;food.FoodExpired+=OnWaste;}
            if(maintenance!=null){maintenance.WaterChangeCompleted+=OnWaterChanged;maintenance.FilterMaintenanceCompleted+=OnFilter;}
            if(simulation!=null)simulation.ChemistryChanged+=OnChemistry;if(welfare!=null)welfare.AquariumWelfareChanged+=OnWelfare;if(hud!=null)hud.DetailsOpened+=OnDetails;
            missions.MissionCompleted+=OnMissionCompleted;codex.EntryUnlocked+=entry=>Notify($"Nuevo concepto: {entry.Title}");achievements.AchievementUnlocked+=a=>Notify($"Logro: {a.Title}");
            player.Experience.ExperienceGained+=OnXp;player.Experience.LevelReached+=level=>Notify($"Nivel {level.Number}: {level.Title}");
            fishCatalogButton?.onClick.AddListener(OpenFishCatalog);Process(ProgressionEventType.Welcome);Unlock("welcome");Refresh();}
        void OnDisable(){openButton?.onClick.RemoveListener(Open);if(panel!=null)panel.Closed-=Close;if(food!=null){food.FoodConsumed-=OnFed;food.FoodExpired-=OnWaste;}
            if(maintenance!=null){maintenance.WaterChangeCompleted-=OnWaterChanged;maintenance.FilterMaintenanceCompleted-=OnFilter;}
            if(simulation!=null)simulation.ChemistryChanged-=OnChemistry;if(welfare!=null)welfare.AquariumWelfareChanged-=OnWelfare;if(hud!=null)hud.DetailsOpened-=OnDetails;
            fishCatalogButton?.onClick.RemoveListener(OpenFishCatalog);}
        public void SetFishCatalog(Button button,Acuaria.UI.FishCatalogPanel catalog){fishCatalogButton=button;fishCatalogPanel=catalog;}
        void OpenFishCatalog()=>fishCatalogPanel?.Open();
        public void SetAquariumFocused(bool value){focused=value;if(openButton!=null)openButton.gameObject.SetActive(value);if(!value)panel?.Close();}
        public void Open(){if(!focused||maintenance!=null&&maintenance.IsActive)return;feedingUi?.CancelFeedingMode();hud?.SetInteractionEnabled(false);panel?.Show();Refresh();}
        void Close()=>hud?.SetInteractionEnabled(true);
        void OnFed(string id){statistics.Process(ProgressionEventType.FishFed);Process(ProgressionEventType.FishFed);Unlock("feeding-basics");}
        void OnWaste(string id){statistics.Process(ProgressionEventType.FoodWasted);Process(ProgressionEventType.FoodWasted);}
        void OnWaterChanged(int percent){statistics.Process(ProgressionEventType.WaterChanged);Process(ProgressionEventType.WaterChanged);Unlock("water-change");}
        void OnFilter(FilterMaintenanceType type){statistics.Process(ProgressionEventType.FilterMaintained);Process(ProgressionEventType.FilterMaintained);Unlock("filter");Unlock("bacteria");}
        void OnDetails(){Process(ProgressionEventType.DetailsObserved);Unlock("ammonia");Unlock("nitrite");Unlock("nitrate");}
        void OnChemistry(WaterChemistryState state){var cycle=AquariumCycleEvaluator.Evaluate(state,simulation.Definition);var quality=WaterQualityEvaluator.Evaluate(state,simulation.Definition,cycle);
            var hours=Mathf.Max(0,state.LastSimulationStep/3600f);player.Statistics.AddSimulation(hours,quality.Status==WaterQualityStatus.Excellent,welfare!=null&&welfare.Current.Status==FishWelfareStatus.Excellent);
            var whole=(int)player.Statistics.ExcellentWaterHours;if(whole>lastExcellentWaterHour){Process(ProgressionEventType.ExcellentWaterHour,whole-lastExcellentWaterHour);lastExcellentWaterHour=whole;}Refresh();}
        void OnWelfare(AquariumWelfareResult result){var whole=(int)player.Statistics.ExcellentWelfareHours;if(whole>lastExcellentWelfareHour){Process(ProgressionEventType.GoodWelfareHour,whole-lastExcellentWelfareHour);lastExcellentWelfareHour=whole;}if(result.Status==FishWelfareStatus.Excellent)Unlock("schooling");Refresh();}
        void Process(ProgressionEventType type,int amount=1){missions.Process(type,amount);achievements.Process(type,amount);player.NotifyChanged();Refresh();}
        void Unlock(string id){if(codex.Unlock(id)){missions.Process(ProgressionEventType.ConceptLearned);achievements.Process(ProgressionEventType.ConceptLearned);player.NotifyChanged();Refresh();}}
        void OnMissionCompleted(MissionDefinition mission)=>Notify($"Misión completada: {mission.Title}");
        void OnXp(int amount,int total)=>Notify($"+{amount} XP");
        void Notify(string value){panel?.Notify(value);if(notificationRoutine!=null)StopCoroutine(notificationRoutine);notificationRoutine=StartCoroutine(HideNotification());}
        IEnumerator HideNotification(){yield return new WaitForSecondsRealtime(2.5f);panel?.HideNotification();notificationRoutine=null;}
        void Refresh(){if(panel==null||missions==null)return;var level=player.Experience.Level;panel.Render($"Diario del Acuarista\nNivel {level.Number} · {level.Title} · XP {level.CurrentXp}/{level.NextLevelXp}",
            MissionText(),CodexText(),AchievementText(),StatisticsText());}
        string MissionText(){var b=new StringBuilder("OBJETIVOS\n");for(var i=0;i<missions.Definitions.Count;i++){var d=missions.Definitions[i];var s=missions.States[i];b.AppendLine($"{(s.Status==MissionStatus.Claimed?"✓":"•")} {d.Title}  {s.Progress}/{d.Target}  +{d.RewardXp} XP");}return b.ToString();}
        string CodexText(){var b=new StringBuilder("LIBRO DEL ACUARIO\n");for(var i=0;i<codex.Entries.Count;i++)b.AppendLine(codex.States[i].IsUnlocked?$"✓ {codex.Entries[i].Title} — {codex.Entries[i].Summary}":$"🔒 Concepto por descubrir");return b.ToString();}
        string AchievementText(){var b=new StringBuilder("LOGROS\n");for(var i=0;i<achievements.Definitions.Count;i++)b.AppendLine($"{(achievements.States[i].IsUnlocked?"✓":"•")} {achievements.Definitions[i].Title}");return b.ToString();}
        string StatisticsText(){var s=player.Statistics;return $"ESTADÍSTICAS\nComidas: {s.MealsGiven} · Cambios de agua: {s.WaterChanges} · Filtro: {s.FilterCleanings}\nHoras simuladas: {s.SimulatedHours:0.0} · Comida desperdiciada: {s.WastedFood}\nXP obtenida: {s.XpEarned}";}
    }
}
