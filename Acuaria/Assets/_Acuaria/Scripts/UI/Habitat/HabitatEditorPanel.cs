using Acuaria.Aquarium.Decorations;
using Acuaria.Food;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.UI.Habitat
{
    public sealed class HabitatEditorPanel:MonoBehaviour
    {
        [SerializeField] HabitatEditorController editor;[SerializeField] DecorationRegistry registry;[SerializeField] CanvasGroup group;
        [SerializeField] FeedingUIController feeding;
        [SerializeField] Text instruction,status;[SerializeField] Button confirm,cancel,undo,flip,rotate,remove;
        public void Configure(HabitatEditorController controller,DecorationRegistry source,FeedingUIController feedingController,CanvasGroup canvas,Text help,Text state,
            Button ok,Button discard,Button back,Button mirror,Button turn,Button delete)
        {editor=controller;registry=source;feeding=feedingController;group=canvas;instruction=help;status=state;confirm=ok;cancel=discard;undo=back;flip=mirror;rotate=turn;remove=delete;}
        void Start(){if(editor==null||!editor.State.IsActive){SetCanvas(false);gameObject.SetActive(false);}}
        void OnEnable(){if(editor!=null)editor.Changed+=Refresh;confirm?.onClick.AddListener(Confirm);cancel?.onClick.AddListener(Cancel);undo?.onClick.AddListener(Undo);flip?.onClick.AddListener(Flip);rotate?.onClick.AddListener(Rotate);remove?.onClick.AddListener(Remove);Refresh();}
        void OnDisable(){if(editor!=null)editor.Changed-=Refresh;confirm?.onClick.RemoveListener(Confirm);cancel?.onClick.RemoveListener(Cancel);undo?.onClick.RemoveListener(Undo);flip?.onClick.RemoveListener(Flip);rotate?.onClick.RemoveListener(Rotate);remove?.onClick.RemoveListener(Remove);}
        public void Open(AquariumHabitatController source)
        {
            feeding?.CancelFeedingMode();feeding?.SetInteractionEnabled(false);gameObject.SetActive(true);SetCanvas(true);
            if(editor==null||!editor.ConfigureSource(source)||!editor.Open()){feeding?.SetInteractionEnabled(true);SetCanvas(false);gameObject.SetActive(false);}
        }
        public void AddById(string id){var item=registry?.FindById(id);if(item!=null)editor?.Add(item);}
        void Confirm(){if(editor?.Confirm()==true)CloseVisual();}void Cancel(){editor?.Cancel();CloseVisual();}void Undo()=>editor?.Undo();void Flip()=>editor?.FlipSelected();void Rotate()=>editor?.RotateSelected();void Remove()=>editor?.RemoveSelected();
        void CloseVisual(){feeding?.SetInteractionEnabled(true);SetCanvas(false);gameObject.SetActive(false);}
        void OnDestroy(){feeding?.SetInteractionEnabled(true);}
        void Refresh(){if(editor==null)return;if(instruction!=null)instruction.text=editor.State.SelectedInstanceId==null?"Toca una decoración. Arrástrala para moverla.":"Arrastra, gira, voltea o quita la selección.";if(status!=null)status.text=$"{editor.State.LastMessage}\nCambios: {editor.State.ChangeCount}";if(confirm!=null)confirm.interactable=editor.State.HasPendingChanges;if(undo!=null)undo.interactable=editor.State.CanUndo;var selected=editor.Selected()!=null;if(flip!=null)flip.interactable=selected;if(rotate!=null)rotate.interactable=selected;if(remove!=null)remove.interactable=selected;}
        void SetCanvas(bool visible){if(group==null)return;group.alpha=visible?1:0;group.interactable=visible;group.blocksRaycasts=visible;}
    }
    public sealed class AvailableDecorationProvider
    {readonly DecorationRegistry registry;public AvailableDecorationProvider(DecorationRegistry source){registry=source;}public System.Collections.Generic.IReadOnlyList<DecorationDefinition> GetAvailable()=>registry?.Decorations;}
}
