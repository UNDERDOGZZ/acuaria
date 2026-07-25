using Acuaria.UI.Aquarium;
using UnityEngine;
namespace Acuaria.UI.FishWelfare
{
    public sealed class FishWelfareDebugController:MonoBehaviour
    {
        [SerializeField] FishWelfareController controller;[SerializeField] AquariumHUDController hud;
        public void Configure(FishWelfareController value,AquariumHUDController hudController){controller=value;hud=hudController;}
        [ContextMenu("Welfare/Temperature Low")] public void TemperatureLow(){hud?.SetTemperature(19);controller?.Evaluate(.1f);}
        [ContextMenu("Welfare/Temperature Normal")] public void TemperatureNormal(){hud?.SetTemperature(25);controller?.Evaluate(.1f);}
        [ContextMenu("Welfare/Reevaluate")] public void Reevaluate()=>controller?.Evaluate(.1f);
    }
}
