using System;
using UnityEngine;
using UnityEngine.UI;
namespace Acuaria.UI.Maintenance
{
    public sealed class AquariumMaintenancePanel:MonoBehaviour
    {
        [SerializeField] CanvasGroup group;[SerializeField] Button[] percentageButtons;[SerializeField] Button confirmButton;
        [SerializeField] Button cancelButton;[SerializeField] Button gentleButton;[SerializeField] Button deepButton;
        [SerializeField] Button closeButton;[SerializeField] Text preview;[SerializeField] Text filter;[SerializeField] Text status;
        public event Action<int> PercentageSelected;public event Action Confirmed,Cancelled,GentleRinseRequested,DeepCleanRequested;
        public bool IsOpen=>gameObject.activeSelf;
        public void Configure(CanvasGroup canvas,Button[] percentages,Button confirm,Button cancel,Button gentle,Button deep,
            Button close,Text previewText,Text filterText,Text statusText)
        {group=canvas;percentageButtons=percentages;confirmButton=confirm;cancelButton=cancel;gentleButton=gentle;deepButton=deep;closeButton=close;preview=previewText;filter=filterText;status=statusText;}
        private void OnEnable(){for(var i=0;i<(percentageButtons?.Length??0);i++){var index=i;percentageButtons[i].onClick.AddListener(()=>PercentageSelected?.Invoke(index));}
            confirmButton?.onClick.AddListener(OnConfirm);cancelButton?.onClick.AddListener(OnCancel);closeButton?.onClick.AddListener(OnCancel);
            gentleButton?.onClick.AddListener(OnGentle);deepButton?.onClick.AddListener(OnDeep);}
        private void OnDisable(){for(var i=0;i<(percentageButtons?.Length??0);i++)percentageButtons[i].onClick.RemoveAllListeners();
            confirmButton?.onClick.RemoveListener(OnConfirm);cancelButton?.onClick.RemoveListener(OnCancel);closeButton?.onClick.RemoveListener(OnCancel);
            gentleButton?.onClick.RemoveListener(OnGentle);deepButton?.onClick.RemoveListener(OnDeep);}
        public void Show(){gameObject.SetActive(true);SetBusy(false);}
        public void Close(){gameObject.SetActive(false);}
        public void Render(string previewValue,string filterValue,string statusValue){if(preview!=null)preview.text=previewValue;if(filter!=null)filter.text=filterValue;if(status!=null)status.text=statusValue;}
        public void SetBusy(bool busy){if(group!=null)group.blocksRaycasts=!busy;if(confirmButton!=null)confirmButton.interactable=!busy;if(cancelButton!=null)cancelButton.interactable=!busy;}
        void OnConfirm()=>Confirmed?.Invoke();void OnCancel()=>Cancelled?.Invoke();void OnGentle()=>GentleRinseRequested?.Invoke();void OnDeep()=>DeepCleanRequested?.Invoke();
    }
}
