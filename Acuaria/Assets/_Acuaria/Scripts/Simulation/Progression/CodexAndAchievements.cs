using System;
using UnityEngine;
namespace Acuaria.Progression
{
    public enum CodexCategory { Water, Filtration, Feeding, Fish, Compatibility, Temperature, Maintenance }
    [CreateAssetMenu(menuName="Acuaria/Progression/Codex Entry")] public sealed class CodexEntry:ScriptableObject
    {
        [SerializeField]string entryId,title,summary;[TextArea,SerializeField]string explanation,tip;[SerializeField]CodexCategory category;
        [SerializeField]Sprite placeholderImage;public string EntryId=>entryId;public string Title=>title;public string Summary=>summary;
        public string Explanation=>explanation;public string Tip=>tip;public CodexCategory Category=>category;public Sprite PlaceholderImage=>placeholderImage;
        public bool IsValid=>!string.IsNullOrWhiteSpace(entryId)&&!string.IsNullOrWhiteSpace(title);
        public void Configure(string id,string label,string shortText,string body,string advice,CodexCategory group)
        {entryId=id;title=label;summary=shortText;explanation=body;tip=advice;category=group;}
    }
    [Serializable] public sealed class CodexEntryState
    {public string EntryId{get;private set;}public bool IsUnlocked{get;private set;}public bool Unlock(string id){if(string.IsNullOrWhiteSpace(id)||IsUnlocked)return false;EntryId=id;IsUnlocked=true;return true;}
     public void Restore(string id,bool unlocked){EntryId=id;IsUnlocked=unlocked;}}
    [CreateAssetMenu(menuName="Acuaria/Progression/Achievement")] public sealed class AchievementDefinition:ScriptableObject
    {
        [SerializeField]string achievementId,title,description;[SerializeField]ProgressionEventType condition;[SerializeField,Min(1)]int target=1;[SerializeField,Min(0)]int rewardXp=15;
        public string AchievementId=>achievementId;public string Title=>title;public string Description=>description;public ProgressionEventType Condition=>condition;
        public int Target=>Mathf.Max(1,target);public int RewardXp=>Mathf.Max(0,rewardXp);public bool IsValid=>!string.IsNullOrWhiteSpace(achievementId)&&!string.IsNullOrWhiteSpace(title);
        public void Configure(string id,string label,string text,ProgressionEventType eventType,int goal,int xp){achievementId=id;title=label;description=text;condition=eventType;target=Mathf.Max(1,goal);rewardXp=Mathf.Max(0,xp);}
    }
    [Serializable] public sealed class AchievementState
    {public string AchievementId{get;private set;}public int Progress{get;private set;}public bool IsUnlocked{get;private set;}
     public void Initialize(string id)=>AchievementId=id;public bool Add(int value,int target){if(IsUnlocked||value<=0)return false;Progress=Math.Min(Math.Max(1,target),Progress+value);if(Progress>=target){IsUnlocked=true;return true;}return false;}
     public void Restore(int progress,bool unlocked,int target){Progress=Math.Clamp(progress,0,Math.Max(1,target));IsUnlocked=unlocked;}}
}
