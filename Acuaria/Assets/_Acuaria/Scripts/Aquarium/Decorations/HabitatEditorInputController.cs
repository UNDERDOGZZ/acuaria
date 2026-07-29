using UnityEngine;
using UnityEngine.InputSystem;

namespace Acuaria.Aquarium.Decorations
{
    public sealed class HabitatEditorInputController : MonoBehaviour
    {
        [SerializeField] HabitatEditorController editor;[SerializeField] Camera worldCamera;[SerializeField] DecorationSelectionVisual selectionVisual;
        Vector2 pointerDown,dragStartPosition,lastPreviewPosition;bool pointerHeld,dragging;
        public void Configure(HabitatEditorController controller,Camera camera,DecorationSelectionVisual visual){editor=controller;worldCamera=camera;selectionVisual=visual;}
        void Update()
        {
            if(editor==null||!editor.State.IsActive||worldCamera==null)return;
            if(TryPointerDown(out var down)){if(IsOverEditorControls(down))return;pointerHeld=true;dragging=false;pointerDown=down;SelectAt(down);}
            if(pointerHeld&&TryPointerPosition(out var current))
            {
                if(!dragging&&Vector2.Distance(current,pointerDown)>=8)dragging=editor.Selected()!=null;
                if(dragging)
                {
                    var screenSize=new Vector2(Mathf.Max(1,Screen.width),Mathf.Max(1,Screen.height));
                    var normalized=pointerDown==current
                        ? dragStartPosition
                        : dragStartPosition+new Vector2((current.x-pointerDown.x)/screenSize.x,(current.y-pointerDown.y)/screenSize.y);
                    var result=editor.PreviewMove(normalized);
                    selectionVisual?.SetValidity(result.IsValid);
                    lastPreviewPosition=ClampPreview(result.CorrectedPosition);
                    ApplyPreview(lastPreviewPosition);
                }
            }
            if(pointerHeld&&TryPointerUp(out var up))
            {
                if(dragging)
                {
                    if(!editor.MoveSelected(lastPreviewPosition))
                        editor.RestoreWorkingPreview();
                }
                pointerHeld=false;dragging=false;RefreshVisual();
            }
        }
        void SelectAt(Vector2 screen)
        {
            DecorationView best=null;var bestDistance=float.PositiveInfinity;var bestContainsPointer=false;
            foreach(var pair in editor.Spawner.Views){var view=pair.Value;if(view?.Renderer==null)continue;
                var screenBounds=ScreenBounds(view.Renderer);
                var distance=Vector2.Distance(screen,screenBounds.center);
                var containsPointer=screenBounds.Contains(screen);
                if(best==null||containsPointer&&!bestContainsPointer||
                  containsPointer==bestContainsPointer&&distance<bestDistance)
                {best=view;bestDistance=distance;bestContainsPointer=containsPointer;}}
            if(best==null){editor.ClearSelection();selectionVisual?.Hide();return;}editor.Select(best.InstanceId);
            dragStartPosition=editor.Selected()?.NormalizedPosition??editor.Area.WorldToNormalized(best.transform.position);
            lastPreviewPosition=dragStartPosition;
            selectionVisual?.Show(best);
        }
        Rect ScreenBounds(Renderer renderer)
        {
            var bounds=renderer.bounds;
            var min=worldCamera.WorldToScreenPoint(new Vector3(bounds.min.x,bounds.min.y,bounds.center.z));
            var max=worldCamera.WorldToScreenPoint(new Vector3(bounds.max.x,bounds.max.y,bounds.center.z));
            const float minimumHitSize=24f;
            var width=Mathf.Max(minimumHitSize,Mathf.Abs(max.x-min.x));
            var height=Mathf.Max(minimumHitSize,Mathf.Abs(max.y-min.y));
            return new Rect((min.x+max.x-width)*.5f,(min.y+max.y-height)*.5f,width,height);
        }
        void ApplyPreview(Vector2 normalized){var p=editor.Selected();if(p==null)return;var view=editor.Spawner.Views.TryGetValue(p.InstanceId,out var found)?found:null;if(view!=null)view.transform.position=editor.Area.NormalizedToWorld(normalized);}
        static Vector2 ClampPreview(Vector2 value)=>new(Mathf.Clamp01(value.x),Mathf.Clamp01(value.y));
        void RefreshVisual(){var p=editor.Selected();if(p!=null&&editor.Spawner.Views.TryGetValue(p.InstanceId,out var view))selectionVisual?.Show(view);}
        bool IsOverEditorControls(Vector2 screen)
        {
            var panel=transform as RectTransform;
            if(panel==null)return false;
            var canvas=panel.GetComponentInParent<Canvas>();
            var eventCamera=canvas!=null&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
            return RectTransformUtility.RectangleContainsScreenPoint(panel,screen,eventCamera);
        }
        static bool TryPointerDown(out Vector2 p)
        {
            var touch=Touchscreen.current?.primaryTouch;
            if(touch!=null&&touch.press.wasPressedThisFrame){p=touch.position.ReadValue();return true;}
            var mouse=Mouse.current;p=mouse?.position.ReadValue()??default;
            return mouse!=null&&mouse.leftButton.wasPressedThisFrame;
        }
        static bool TryPointerPosition(out Vector2 p)
        {
            var touch=Touchscreen.current?.primaryTouch;
            if(touch!=null&&touch.press.isPressed){p=touch.position.ReadValue();return true;}
            var mouse=Mouse.current;p=mouse?.position.ReadValue()??default;
            return mouse!=null&&mouse.leftButton.isPressed;
        }
        static bool TryPointerUp(out Vector2 p)
        {
            var touch=Touchscreen.current?.primaryTouch;
            if(touch!=null&&touch.press.wasReleasedThisFrame){p=touch.position.ReadValue();return true;}
            var mouse=Mouse.current;p=mouse?.position.ReadValue()??default;
            return mouse!=null&&mouse.leftButton.wasReleasedThisFrame;
        }
    }

}
