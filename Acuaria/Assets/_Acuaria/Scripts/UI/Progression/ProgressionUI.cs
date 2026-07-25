using System;
using UnityEngine;
using UnityEngine.UI;
namespace Acuaria.UI.Progression
{
    public sealed class ProgressionUI:MonoBehaviour
    {
        [SerializeField]CanvasGroup group;[SerializeField]Button closeButton;[SerializeField]Text header,missions,codex,achievements,statistics,notification;
        public event Action Closed;public bool IsOpen=>gameObject.activeSelf;
        public void Configure(CanvasGroup canvas,Button close,Text title,Text missionText,Text codexText,Text achievementText,Text stats,Text toast)
        {group=canvas;closeButton=close;header=title;missions=missionText;codex=codexText;achievements=achievementText;statistics=stats;notification=toast;}
        void OnEnable()=>closeButton?.onClick.AddListener(Close);void OnDisable()=>closeButton?.onClick.RemoveListener(Close);
        public void Show()=>gameObject.SetActive(true);public void Close(){gameObject.SetActive(false);Closed?.Invoke();}
        public void Render(string title,string missionText,string codexText,string achievementText,string stats)
        {if(header!=null)header.text=title;if(missions!=null)missions.text=missionText;if(codex!=null)codex.text=codexText;if(achievements!=null)achievements.text=achievementText;if(statistics!=null)statistics.text=stats;}
        public void Notify(string text){if(notification==null)return;notification.text=text;notification.gameObject.SetActive(true);}
        public void HideNotification(){if(notification!=null)notification.gameObject.SetActive(false);}
    }
}
