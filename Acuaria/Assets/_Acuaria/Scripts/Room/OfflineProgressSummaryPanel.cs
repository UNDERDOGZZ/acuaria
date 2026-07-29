using System.Text;
using Acuaria.Offline;
using UnityEngine;
using UnityEngine.UI;

namespace Acuaria.Room
{
    public sealed class OfflineProgressSummaryPanel:MonoBehaviour
    {
        [SerializeField] SaveCoordinator coordinator;[SerializeField] CanvasGroup group;
        [SerializeField] Text title,body;[SerializeField] Button closeButton;
        public void Configure(SaveCoordinator source,CanvasGroup canvasGroup,Text titleLabel,Text bodyLabel,Button dismissButton)
        {coordinator=source;group=canvasGroup;title=titleLabel;body=bodyLabel;closeButton=dismissButton;}
        void Start()
        {
            Hide();
            if(coordinator==null)coordinator=FindAnyObjectByType<SaveCoordinator>();
            if(coordinator!=null){coordinator.OfflineProgressApplied+=Show;if(coordinator.LastOfflineReport!=null)Show(coordinator.LastOfflineReport);}
            closeButton?.onClick.AddListener(Hide);
        }
        void OnDestroy(){if(coordinator!=null)coordinator.OfflineProgressApplied-=Show;closeButton?.onClick.RemoveListener(Hide);}
        public void Show(OfflineSimulationReport report)
        {
            if(report?.Applied!=true||!report.Relevant)return;
            if(title!=null)title.text="Mientras no estabas";
            if(body!=null)
            {
                var builder=new StringBuilder();
                builder.AppendLine($"Tiempo simulado: {report.Time.Effective.TotalHours:0.#} h");
                if(report.Time.WasCapped)builder.AppendLine("El intervalo alcanzó el máximo seguro.");
                foreach(var item in report.Events)builder.AppendLine($"• {item.Message}");
                body.text=builder.ToString();
            }
            SetVisible(true);
        }
        public void Hide()=>SetVisible(false);
        void SetVisible(bool visible)
        {
            if(group==null)return;group.alpha=visible?1:0;group.interactable=visible;group.blocksRaycasts=visible;
        }
    }
}
