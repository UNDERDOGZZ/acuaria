using System;
using UnityEngine;
namespace Acuaria.Progression
{
    public readonly struct PlayerLevel
    {
        public readonly int Number;public readonly string Title;public readonly int CurrentXp;public readonly int NextLevelXp;
        public PlayerLevel(int number,string title,int current,int next){Number=number;Title=title;CurrentXp=current;NextLevelXp=next;}
    }
    [Serializable] public sealed class PlayerExperience
    {
        public int TotalXp{get;private set;}public event Action<int,int> ExperienceGained;public event Action<PlayerLevel> LevelReached;
        public PlayerLevel Level=>CalculateLevel(TotalXp);
        public int Add(int amount){if(amount<=0)return 0;var before=Level.Number;TotalXp=Math.Min(int.MaxValue,TotalXp+amount);ExperienceGained?.Invoke(amount,TotalXp);if(Level.Number>before)LevelReached?.Invoke(Level);return amount;}
        public static PlayerLevel CalculateLevel(int xp){xp=Math.Max(0,xp);var level=1;var required=100;var consumed=0;while(xp>=consumed+required&&level<10000){consumed+=required;level++;required=100+(level-1)*50;}
            var title=level switch{1=>"Principiante",2=>"Aprendiz",3=>"Cuidador",4=>"Acuarista",_=>"Experto"};return new PlayerLevel(level,title,xp-consumed,required);}
    }
    [Serializable] public sealed class PlayerStatistics
    {
        public int MealsGiven{get;private set;}public int WaterChanges{get;private set;}public int FilterCleanings{get;private set;}
        public double SimulatedHours{get;private set;}public int XpEarned{get;private set;}public double ExcellentWaterHours{get;private set;}
        public double ExcellentWelfareHours{get;private set;}public int WastedFood{get;private set;}
        public void RecordMeal()=>MealsGiven++;public void RecordWaterChange()=>WaterChanges++;public void RecordFilterCleaning()=>FilterCleanings++;
        public void RecordWaste()=>WastedFood++;public void AddXp(int value)=>XpEarned=Math.Max(0,XpEarned+Math.Max(0,value));
        public void AddSimulation(float hours,bool excellentWater,bool excellentWelfare){if(!float.IsFinite(hours)||hours<=0)return;SimulatedHours+=hours;if(excellentWater)ExcellentWaterHours+=hours;if(excellentWelfare)ExcellentWelfareHours+=hours;}
    }
    [Serializable] public sealed class PlayerProgression
    {
        public PlayerExperience Experience{get;}=new();public PlayerStatistics Statistics{get;}=new();
        public event Action Changed;
        public int GrantXp(int value){var granted=Experience.Add(value);Statistics.AddXp(granted);if(granted>0)Changed?.Invoke();return granted;}
        public void NotifyChanged()=>Changed?.Invoke();
    }
}
