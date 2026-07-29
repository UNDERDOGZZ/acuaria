using System;
using UnityEngine;
namespace Acuaria.Progression
{
    public enum MissionType { Tutorial, DailySimulated, Educational, Progression }
    public enum MissionStatus { Locked, Active, Completed, Claimed }
    public enum ProgressionEventType { Welcome, FishFed, DetailsObserved, WaterChanged, ExcellentWaterHour, FilterMaintained, GoodWelfareHour, ConceptLearned, FoodWasted,
        PlantLearned, HidingPlaceLearned, RockObserved, WoodObserved, NaturalHabitatCreated, HidingPlaceAdded, PlantedAquariumCreated }
    [CreateAssetMenu(menuName="Acuaria/Progression/Mission")] public sealed class MissionDefinition:ScriptableObject
    {
        [SerializeField]string missionId,title,description;[SerializeField]MissionType type;[SerializeField]ProgressionEventType condition;
        [SerializeField,Min(1)]int target=1;[SerializeField,Min(0)]int rewardXp=25;[SerializeField]string conceptToUnlock;
        public string MissionId=>missionId;public string Title=>title;public string Description=>description;public MissionType Type=>type;
        public ProgressionEventType Condition=>condition;public int Target=>Mathf.Max(1,target);public int RewardXp=>Mathf.Max(0,rewardXp);public string ConceptToUnlock=>conceptToUnlock;
        public bool IsValid=>!string.IsNullOrWhiteSpace(missionId)&&!string.IsNullOrWhiteSpace(title)&&Target>0;
        public void Configure(string id,string label,string text,MissionType missionType,ProgressionEventType eventType,int goal,int xp,string concept=null)
        {missionId=id;title=label;description=text;type=missionType;condition=eventType;target=Mathf.Max(1,goal);rewardXp=Mathf.Max(0,xp);conceptToUnlock=concept;}
    }
    [Serializable] public sealed class MissionState
    {
        public string MissionId{get;private set;}public int Progress{get;private set;}public MissionStatus Status{get;private set;}
        public bool IsInitialized{get;private set;}public void Initialize(MissionDefinition definition,bool active=true)
        {if(definition==null||!definition.IsValid)throw new ArgumentException();MissionId=definition.MissionId;Progress=0;Status=active?MissionStatus.Active:MissionStatus.Locked;IsInitialized=true;}
        public bool AddProgress(int amount,int target){if(Status!=MissionStatus.Active||amount<=0)return false;Progress=Math.Min(Math.Max(1,target),Progress+amount);if(Progress>=target){Status=MissionStatus.Completed;return true;}return false;}
        public void Claim(){if(Status==MissionStatus.Completed)Status=MissionStatus.Claimed;}
        public void Restore(int progress,MissionStatus status,int target){Progress=Math.Clamp(progress,0,Math.Max(1,target));Status=status;}
    }
    public sealed class MissionEvaluator
    {
        public bool Apply(MissionDefinition definition,MissionState state,ProgressionEventType eventType,int amount=1)=>
            definition!=null&&state!=null&&definition.Condition==eventType&&state.AddProgress(amount,definition.Target);
    }
}
