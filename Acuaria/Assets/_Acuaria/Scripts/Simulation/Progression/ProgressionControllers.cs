using System;
using System.Collections.Generic;
namespace Acuaria.Progression
{
    public sealed class MissionController
    {
        readonly MissionEvaluator evaluator=new();readonly List<MissionDefinition> definitions=new();readonly List<MissionState> states=new();
        readonly PlayerProgression progression;public IReadOnlyList<MissionDefinition> Definitions=>definitions;public IReadOnlyList<MissionState> States=>states;
        public event Action<MissionDefinition> MissionCompleted;
        public MissionController(PlayerProgression player,IReadOnlyList<MissionDefinition> source){progression=player;if(source==null)return;for(var i=0;i<source.Count;i++){if(source[i]==null||!source[i].IsValid)continue;definitions.Add(source[i]);var state=new MissionState();state.Initialize(source[i]);states.Add(state);}}
        public void Process(ProgressionEventType eventType,int amount=1){for(var i=0;i<definitions.Count;i++){if(!evaluator.Apply(definitions[i],states[i],eventType,amount))continue;
            progression.GrantXp(definitions[i].RewardXp);states[i].Claim();MissionCompleted?.Invoke(definitions[i]);}}
    }
    public sealed class CodexController
    {
        readonly List<CodexEntry> entries=new();readonly List<CodexEntryState> states=new();public IReadOnlyList<CodexEntry> Entries=>entries;public IReadOnlyList<CodexEntryState> States=>states;
        public event Action<CodexEntry> EntryUnlocked;
        public CodexController(IReadOnlyList<CodexEntry> source){if(source==null)return;for(var i=0;i<source.Count;i++){if(source[i]==null||!source[i].IsValid)continue;entries.Add(source[i]);states.Add(new CodexEntryState());}}
        public bool Unlock(string id){for(var i=0;i<entries.Count;i++)if(entries[i].EntryId==id&&states[i].Unlock(id)){EntryUnlocked?.Invoke(entries[i]);return true;}return false;}
    }
    public sealed class AchievementController
    {
        readonly List<AchievementDefinition> definitions=new();readonly List<AchievementState> states=new();readonly PlayerProgression progression;
        public IReadOnlyList<AchievementDefinition> Definitions=>definitions;public IReadOnlyList<AchievementState> States=>states;
        public event Action<AchievementDefinition> AchievementUnlocked;
        public AchievementController(PlayerProgression player,IReadOnlyList<AchievementDefinition> source){progression=player;if(source==null)return;for(var i=0;i<source.Count;i++){if(source[i]==null||!source[i].IsValid)continue;definitions.Add(source[i]);var state=new AchievementState();state.Initialize(source[i].AchievementId);states.Add(state);}}
        public void Process(ProgressionEventType eventType,int amount=1){for(var i=0;i<definitions.Count;i++){var definition=definitions[i];if(definition.Condition!=eventType||!states[i].Add(amount,definition.Target))continue;progression.GrantXp(definition.RewardXp);AchievementUnlocked?.Invoke(definition);}}
    }
    public sealed class StatisticsController
    {
        public PlayerStatistics Statistics{get;}public StatisticsController(PlayerStatistics value)=>Statistics=value??throw new ArgumentNullException(nameof(value));
        public void Process(ProgressionEventType type,float amount=1){switch(type){case ProgressionEventType.FishFed:Statistics.RecordMeal();break;case ProgressionEventType.WaterChanged:Statistics.RecordWaterChange();break;
            case ProgressionEventType.FilterMaintained:Statistics.RecordFilterCleaning();break;case ProgressionEventType.FoodWasted:Statistics.RecordWaste();break;}}
    }
}
