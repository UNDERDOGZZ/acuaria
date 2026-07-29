using System;
using System.Collections.Generic;
using UnityEngine;

namespace Acuaria.Aquarium.Decorations
{
    public sealed class HabitatEditorController : MonoBehaviour
    {
        [SerializeField] HabitatEditorDefinition definition;
        [SerializeField] AquariumHabitatController habitat;
        [SerializeField] DecorationSpawner2D spawner;
        [SerializeField] AquariumDecorationArea2D area;
        readonly HabitatEditorState state=new(); readonly DecorationSelectionModel selection=new();
        HabitatLayoutSnapshot snapshot;List<DecorationPlacementData> working=new();HabitatEditHistory history;DecorationPlacementValidator validator;int idSequence;
        public HabitatEditorState State=>state;public IReadOnlyList<DecorationPlacementData> WorkingPlacements=>working;
        public AquariumDecorationArea2D Area=>area;public DecorationSpawner2D Spawner=>spawner;
        public event Action Changed;public event Action<bool> SessionChanged;public event Action Confirmed,Cancelled;
        public void Configure(HabitatEditorDefinition settings,AquariumHabitatController source,DecorationSpawner2D visualSpawner,
            AquariumDecorationArea2D decorationArea)
        {definition=settings;habitat=source;spawner=visualSpawner;area=decorationArea;Initialize();}
        public bool ConfigureSource(AquariumHabitatController source)
        {
            if(state.IsActive||source==null||source.Spawner==null||source.Spawner.Area==null)return false;
            habitat=source;spawner=source.Spawner;area=source.Spawner.Area;return true;
        }
        void Initialize(){history=new HabitatEditHistory(definition!=null?definition.MaximumUndoSteps:20);validator=new DecorationPlacementValidator(definition);state.IsInitialized=true;}
        public bool Open()
        {
            if(state.IsActive||habitat==null||spawner==null||area==null)return false;if(!state.IsInitialized)Initialize();
            snapshot=new HabitatLayoutSnapshot(habitat.Placements);
            working=snapshot.CreateWorkingCopy();history.Clear();selection.Clear();state.IsActive=true;state.Phase=HabitatEditorPhase.Browsing;
            state.Validity=DecorationPlacementValidity.Unknown;RefreshState("Toca una decoración para seleccionarla.");spawner.SynchronizeInstalledDecorations(working);SessionChanged?.Invoke(true);return true;
        }
        public bool Select(string id){if(!state.IsActive||!selection.Select(id,working))return false;state.SelectedInstanceId=id;state.Phase=HabitatEditorPhase.Selected;RefreshState("Arrastra para mover.");return true;}
        public void ClearSelection(){selection.Clear();state.ClearSelection();RefreshState("Selecciona una decoración.");}
        public DecorationPlacementData Selected(){var i=Find(state.SelectedInstanceId);return i>=0?working[i]:null;}
        public DecorationPlacementValidationResult PreviewMove(Vector2 normalized)
        {
            var current=Selected();if(current==null)return new(DecorationPlacementValidity.Blocked,normalized,0,"No hay selección.");
            var result=validator.Validate(current.WithPosition(normalized),working);state.Validity=result.Validity;state.Phase=HabitatEditorPhase.Dragging;state.LastMessage=result.Message;Changed?.Invoke();return result;
        }
        public bool MoveSelected(Vector2 normalized)
        {var current=Selected();if(current==null)return false;var result=validator.Validate(current.WithPosition(normalized),working);state.Validity=result.Validity;
         if(!result.IsValid){state.Phase=HabitatEditorPhase.Selected;RefreshState(result.Message);return false;}
         return Execute(new ReplacePlacementCommand(current,current.WithPosition(result.CorrectedPosition)),result.Message);}
        public void RestoreWorkingPreview()
        {
            if(state.IsActive)spawner?.SynchronizeInstalledDecorations(working);
        }
        public bool RotateSelected(){var p=Selected();if(p==null||definition==null||!definition.RotationEnabled)return false;return Execute(new ReplacePlacementCommand(p,p.WithRotation(p.LocalRotation+definition.RotationStep)),"Rotación aplicada.");}
        public bool FlipSelected(){var p=Selected();return p!=null&&Execute(new ReplacePlacementCommand(p,p.WithFlip(!p.FlipX)),"Orientación invertida.");}
        public bool RemoveSelected(){var p=Selected();if(p==null)return false;var ok=Execute(new RemoveDecorationCommand(p),"Decoración retirada provisionalmente.");if(ok)ClearSelection();return ok;}
        public bool Add(DecorationDefinition item)
        {
            if(!state.IsActive||item==null)return false;var id=$"edit-{item.DecorationId}-{++idSequence}";
            var candidate=new DecorationPlacementData(id,item,new Vector2(.5f,.25f),Vector2.one);
            var result=validator.Validate(candidate,working);if(!result.IsValid){RefreshState(result.Message);return false;}
            candidate=candidate.WithPosition(result.CorrectedPosition);if(!Execute(new AddDecorationCommand(candidate),"Decoración añadida provisionalmente."))return false;Select(id);return true;
        }
        public bool Undo(){if(!state.IsActive||!history.Undo(working))return false;spawner.SynchronizeInstalledDecorations(working);RefreshState("Cambio deshecho.");return true;}
        public bool Confirm()
        {
            if(!state.IsActive)return false;for(var i=0;i<working.Count;i++)if(!validator.Validate(working[i],working).IsValid){RefreshState("Corrige las posiciones inválidas.");return false;}
            state.Phase=HabitatEditorPhase.Confirming;habitat.ApplyPlacements(working);CloseSession();Confirmed?.Invoke();return true;
        }
        public void Cancel(){if(!state.IsActive)return;state.Phase=HabitatEditorPhase.Cancelling;working=snapshot?.CreateWorkingCopy()??new();spawner.SynchronizeInstalledDecorations(working);CloseSession();Cancelled?.Invoke();}
        void CloseSession(){history.Clear();selection.Clear();state.IsActive=false;state.Phase=HabitatEditorPhase.Closed;state.SelectedInstanceId=null;state.HasPendingChanges=false;state.CanUndo=false;SessionChanged?.Invoke(false);Changed?.Invoke();}
        bool Execute(IHabitatEditCommand command,string message){if(!history.Execute(command,working))return false;spawner.SynchronizeInstalledDecorations(working);state.Phase=HabitatEditorPhase.Selected;RefreshState(message);return true;}
        void RefreshState(string message){state.HasPendingChanges=snapshot!=null&&!snapshot.Matches(working);state.ChangeCount=history?.Count??0;state.CanUndo=history?.CanUndo==true;state.LastMessage=message;Changed?.Invoke();}
        int Find(string id){if(string.IsNullOrWhiteSpace(id))return-1;for(var i=0;i<working.Count;i++)if(working[i]?.InstanceId==id)return i;return-1;}
        void OnDisable(){if(state.IsActive)Cancel();}
    }
}
