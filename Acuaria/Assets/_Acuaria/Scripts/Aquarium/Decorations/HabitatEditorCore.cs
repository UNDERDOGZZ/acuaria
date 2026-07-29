using System;
using System.Collections.Generic;
using UnityEngine;

namespace Acuaria.Aquarium.Decorations
{
    public enum HabitatEditorPhase { Closed, Opening, Browsing, Selected, Dragging, Validating, Confirming, Cancelling, Closing }
    public enum DecorationPlacementValidity { Valid, OutsideBounds, InvalidCategoryZone, ExcessiveOverlap, MissingDefinition, InvalidScale, Blocked, Unknown }

    [CreateAssetMenu(menuName="Acuaria/Habitat/Editor Settings",fileName="StarterHabitatEditorSettings")]
    public sealed class HabitatEditorDefinition : ScriptableObject
    {
        [SerializeField] string editorId="starter-habitat-editor";
        [SerializeField] bool gridEnabled=true,snapToGrid=true,snapToBottom=true,rotationEnabled=true,blockCriticalOverlap=true;
        [SerializeField] float gridSize=.05f,selectionDistance=.12f,dragThreshold=8,horizontalMargin=.025f,verticalMargin=.025f;
        [SerializeField,Range(0,1)] float selectionOpacity=.75f,maximumOverlap=.55f;
        [SerializeField] float selectionScale=1.08f,rotationStep=15,touchSensitivity=1;
        [SerializeField] int maximumUndoSteps=20;
        public string EditorId=>editorId; public bool GridEnabled=>gridEnabled; public bool SnapToGrid=>snapToGrid;
        public bool SnapToBottom=>snapToBottom; public bool RotationEnabled=>rotationEnabled;
        public bool BlockCriticalOverlap=>blockCriticalOverlap; public float GridSize=>SafePositive(gridSize,.05f);
        public float SelectionDistance=>SafePositive(selectionDistance,.12f); public float DragThreshold=>SafeNonNegative(dragThreshold);
        public float HorizontalMargin=>Mathf.Clamp(SafeNonNegative(horizontalMargin),0,.45f);
        public float VerticalMargin=>Mathf.Clamp(SafeNonNegative(verticalMargin),0,.45f);
        public float SelectionOpacity=>Mathf.Clamp01(Safe(selectionOpacity,.75f));
        public float SelectionScale=>SafePositive(selectionScale,1.08f); public float RotationStep=>SafePositive(rotationStep,15);
        public int MaximumUndoSteps=>Mathf.Max(0,maximumUndoSteps); public float MaximumOverlap=>Mathf.Clamp01(Safe(maximumOverlap,.55f));
        public float TouchSensitivity=>SafePositive(touchSensitivity,1);
        public bool IsValid=>!string.IsNullOrWhiteSpace(editorId)&&gridSize>0&&selectionDistance>0&&dragThreshold>=0&&
            rotationStep>0&&maximumUndoSteps>=0&&touchSensitivity>0&&AllFinite();
        public void Configure(string id,float grid,float overlap,int undo=20)
        {editorId=id;gridSize=grid;maximumOverlap=overlap;maximumUndoSteps=undo;}
        bool AllFinite()=>float.IsFinite(gridSize)&&float.IsFinite(selectionDistance)&&float.IsFinite(dragThreshold)&&
            float.IsFinite(horizontalMargin)&&float.IsFinite(verticalMargin)&&float.IsFinite(selectionOpacity)&&
            float.IsFinite(selectionScale)&&float.IsFinite(rotationStep)&&float.IsFinite(maximumOverlap)&&float.IsFinite(touchSensitivity);
        static float Safe(float v,float fallback)=>float.IsFinite(v)?v:fallback;
        static float SafePositive(float v,float fallback)=>Mathf.Max(.0001f,Safe(v,fallback));
        static float SafeNonNegative(float v)=>Mathf.Max(0,Safe(v,0));
    }

    public sealed class HabitatEditorState
    {
        public bool IsActive {get;internal set;} public HabitatEditorPhase Phase {get;internal set;}=HabitatEditorPhase.Closed;
        public string SelectedInstanceId {get;internal set;} public DecorationPlacementValidity Validity {get;internal set;}=DecorationPlacementValidity.Unknown;
        public bool HasPendingChanges {get;internal set;} public int ChangeCount {get;internal set;} public bool CanUndo {get;internal set;}
        public string LastMessage {get;internal set;} public bool IsInitialized {get;internal set;}
        public void ClearSelection(){SelectedInstanceId=null;Phase=IsActive?HabitatEditorPhase.Browsing:HabitatEditorPhase.Closed;}
    }

    public sealed class HabitatLayoutSnapshot
    {
        readonly List<DecorationPlacementData> placements=new();
        public IReadOnlyList<DecorationPlacementData> Placements=>placements;
        public HabitatLayoutSnapshot(IEnumerable<DecorationPlacementData> source){Copy(source,placements);}
        public List<DecorationPlacementData> CreateWorkingCopy(){var result=new List<DecorationPlacementData>();Copy(placements,result);return result;}
        public bool Matches(IReadOnlyList<DecorationPlacementData> other)
        {if(other==null||other.Count!=placements.Count)return false;for(var i=0;i<placements.Count;i++)if(!Same(placements[i],other[i]))return false;return true;}
        static bool Same(DecorationPlacementData a,DecorationPlacementData b)=>a!=null&&b!=null&&a.InstanceId==b.InstanceId&&
            a.Definition==b.Definition&&Vector2.Distance(a.NormalizedPosition,b.NormalizedPosition)<.0001f&&
            Vector2.Distance(a.LocalScale,b.LocalScale)<.0001f&&Mathf.Abs(Mathf.DeltaAngle(a.LocalRotation,b.LocalRotation))<.001f&&a.FlipX==b.FlipX;
        static void Copy(IEnumerable<DecorationPlacementData> source,List<DecorationPlacementData> target)
        {if(source==null)return;foreach(var p in source)if(p!=null)target.Add(p.Clone());}
    }

    public readonly struct DecorationFootprint
    {
        // Placement scale is expressed in aquarium world units while positions are
        // normalized to the decoration area. Convert the starter tank's world-space
        // footprint (7.35 x 3.2 units) before validating bounds and overlap.
        static readonly Vector2 WorldToNormalizedSize=new(1f/7.35f,1f/3.2f);
        public readonly Rect Bounds;
        public DecorationFootprint(Rect bounds){Bounds=bounds;}
        public static DecorationFootprint From(DecorationPlacementData p)
        {if(p==null||p.Definition==null)return new DecorationFootprint(default);
         var s=Vector2.Scale(Vector2.Scale(p.Definition.Scale,p.LocalScale),WorldToNormalizedSize);
         return new DecorationFootprint(new Rect(p.NormalizedPosition-s*.5f,s));}
    }
    public readonly struct DecorationPlacementValidationResult
    {
        public readonly DecorationPlacementValidity Validity; public readonly Vector2 CorrectedPosition; public readonly float OverlapRatio; public readonly string Message;
        public bool IsValid=>Validity==DecorationPlacementValidity.Valid;
        public DecorationPlacementValidationResult(DecorationPlacementValidity v,Vector2 p,float overlap,string message){Validity=v;CorrectedPosition=p;OverlapRatio=overlap;Message=message;}
    }
    public static class DecorationOverlapEvaluator
    {
        public static float Ratio(DecorationFootprint a,DecorationFootprint b)
        {var intersection=Rect.MinMaxRect(Mathf.Max(a.Bounds.xMin,b.Bounds.xMin),Mathf.Max(a.Bounds.yMin,b.Bounds.yMin),
          Mathf.Min(a.Bounds.xMax,b.Bounds.xMax),Mathf.Min(a.Bounds.yMax,b.Bounds.yMax));
         if(intersection.width<=0||intersection.height<=0)return 0;var minArea=Mathf.Min(a.Bounds.width*a.Bounds.height,b.Bounds.width*b.Bounds.height);
         return minArea<=0?0:Mathf.Clamp01(intersection.width*intersection.height/minArea);}
    }
    public sealed class DecorationPlacementValidator
    {
        readonly HabitatEditorDefinition settings;
        public DecorationPlacementValidator(HabitatEditorDefinition value){settings=value;}
        public DecorationPlacementValidationResult Validate(DecorationPlacementData candidate,IReadOnlyList<DecorationPlacementData> all)
        {
            if(candidate?.Definition==null)return Result(DecorationPlacementValidity.MissingDefinition,Vector2.zero,0,"Falta la definición.");
            if(candidate.LocalScale.x<=0||candidate.LocalScale.y<=0)return Result(DecorationPlacementValidity.InvalidScale,candidate.NormalizedPosition,0,"Escala inválida.");
            var pos=candidate.NormalizedPosition;var marginX=settings?.HorizontalMargin??.025f;var marginY=settings?.VerticalMargin??.025f;
            if(settings!=null&&settings.SnapToGrid&&settings.GridEnabled){var g=settings.GridSize;pos=new Vector2(Mathf.Round(pos.x/g)*g,Mathf.Round(pos.y/g)*g);}
            if(settings==null||settings.SnapToBottom)if(IsBottom(candidate.Definition.Category))pos.y=BottomFor(candidate);
            if(pos.x<marginX||pos.x>1-marginX||pos.y<marginY||pos.y>1-marginY)
                return Result(DecorationPlacementValidity.OutsideBounds,pos,0,"Fuera del área decorable.");
            var adjusted=candidate.WithPosition(pos);var footprint=DecorationFootprint.From(adjusted);var max=0f;
            if(all!=null)for(var i=0;i<all.Count;i++){var other=all[i];if(other==null||other.InstanceId==candidate.InstanceId)continue;
                var otherPosition=other.NormalizedPosition;
                if(settings==null||settings.SnapToBottom)if(IsBottom(other.Definition.Category))otherPosition.y=BottomFor(other);
                max=Mathf.Max(max,DecorationOverlapEvaluator.Ratio(footprint,DecorationFootprint.From(other.WithPosition(otherPosition))));}
            if(settings!=null&&settings.BlockCriticalOverlap&&max>settings.MaximumOverlap)
                return Result(DecorationPlacementValidity.ExcessiveOverlap,pos,max,"Superposición excesiva.");
            return Result(DecorationPlacementValidity.Valid,pos,max,max>.25f?"Solapamiento leve permitido.":"Posición válida.");
        }
        static bool IsBottom(DecorationCategory c)=>c==DecorationCategory.Plant||c==DecorationCategory.Rock||c==DecorationCategory.Wood||c==DecorationCategory.Cave;
        static float BottomFor(DecorationPlacementData p)
        {
            var footprint=DecorationFootprint.From(p);
            return Mathf.Clamp(.025f+footprint.Bounds.height*.5f,.025f,.6f);
        }
        static DecorationPlacementValidationResult Result(DecorationPlacementValidity v,Vector2 p,float o,string m)=>new(v,p,o,m);
    }

    public sealed class DecorationSelectionModel
    {
        public string SelectedId {get;private set;}
        public bool Select(string id,IReadOnlyList<DecorationPlacementData> values)
        {if(string.IsNullOrWhiteSpace(id)||values==null)return false;for(var i=0;i<values.Count;i++)if(values[i]?.InstanceId==id){SelectedId=id;return true;}return false;}
        public void Clear()=>SelectedId=null;
    }
}
