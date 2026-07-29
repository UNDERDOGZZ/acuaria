using System.Collections.Generic;

namespace Acuaria.Aquarium.Decorations
{
    public interface IHabitatEditCommand { bool Execute(List<DecorationPlacementData> layout); void Undo(List<DecorationPlacementData> layout); }
    public sealed class ReplacePlacementCommand : IHabitatEditCommand
    {
        readonly DecorationPlacementData before,after; bool executed;
        public ReplacePlacementCommand(DecorationPlacementData oldValue,DecorationPlacementData newValue){before=oldValue?.Clone();after=newValue?.Clone();}
        public bool Execute(List<DecorationPlacementData> layout){if(executed||before==null||after==null)return false;var i=Find(layout,before.InstanceId);if(i<0)return false;layout[i]=after.Clone();executed=true;return true;}
        public void Undo(List<DecorationPlacementData> layout){if(!executed)return;var i=Find(layout,after.InstanceId);if(i>=0)layout[i]=before.Clone();executed=false;}
        internal static int Find(List<DecorationPlacementData> list,string id){for(var i=0;i<list.Count;i++)if(list[i]?.InstanceId==id)return i;return-1;}
    }
    public sealed class AddDecorationCommand:IHabitatEditCommand
    {readonly DecorationPlacementData value;bool executed;public AddDecorationCommand(DecorationPlacementData p){value=p?.Clone();}
     public bool Execute(List<DecorationPlacementData> l){if(executed||value==null||ReplacePlacementCommand.Find(l,value.InstanceId)>=0)return false;l.Add(value.Clone());executed=true;return true;}
     public void Undo(List<DecorationPlacementData> l){if(!executed)return;var i=ReplacePlacementCommand.Find(l,value.InstanceId);if(i>=0)l.RemoveAt(i);executed=false;}}
    public sealed class RemoveDecorationCommand:IHabitatEditCommand
    {readonly DecorationPlacementData value;int index;bool executed;public RemoveDecorationCommand(DecorationPlacementData p){value=p?.Clone();}
     public bool Execute(List<DecorationPlacementData> l){if(executed||value==null)return false;index=ReplacePlacementCommand.Find(l,value.InstanceId);if(index<0)return false;l.RemoveAt(index);executed=true;return true;}
     public void Undo(List<DecorationPlacementData> l){if(!executed)return;l.Insert(System.Math.Min(index,l.Count),value.Clone());executed=false;}}
    public sealed class HabitatEditHistory
    {
        readonly List<IHabitatEditCommand> commands=new();readonly int maximum;
        public bool CanUndo=>commands.Count>0;public int Count=>commands.Count;
        public HabitatEditHistory(int max){maximum=System.Math.Max(0,max);}
        public bool Execute(IHabitatEditCommand command,List<DecorationPlacementData> layout)
        {if(command==null||!command.Execute(layout))return false;if(maximum==0)return true;commands.Add(command);if(commands.Count>maximum)commands.RemoveAt(0);return true;}
        public bool Undo(List<DecorationPlacementData> layout){if(commands.Count==0)return false;var i=commands.Count-1;commands[i].Undo(layout);commands.RemoveAt(i);return true;}
        public void Clear()=>commands.Clear();
    }
}
