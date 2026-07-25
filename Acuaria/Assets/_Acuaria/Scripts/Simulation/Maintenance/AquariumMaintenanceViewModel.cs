using Acuaria.Simulation.Filtration;
using Acuaria.Simulation.Water;
namespace Acuaria.Simulation.Maintenance
{
    public sealed class AquariumMaintenanceViewModel
    {
        public WaterChangeOption[] Options { get; }
        public string Preview { get; }
        public string FilterSummary { get; }
        public string Cooldown { get; }
        public AquariumMaintenanceViewModel(AquariumMaintenanceDefinition definition,WaterChemistryState state,
            int selected,FilterRuntimeState filter,AquariumMaintenanceState maintenance)
        {
            var percentages=definition?.AllowedPercentages??System.Array.Empty<int>();Options=new WaterChangeOption[percentages.Length];
            for(var i=0;i<percentages.Length;i++)Options[i]=new WaterChangeOption(percentages[i]);
            var result=new WaterChangeModel().Calculate(state,definition,selected);
            Preview=result.IsValid?$"Cambio de agua: {selected}%{(selected==definition.RecommendedPercentage?"  • RECOMENDADO":"")}\n\n"+
                $"Amoníaco: {state.AmmoniaMgPerLiter:0.00} → {result.Ammonia:0.00} mg/L\n"+
                $"Nitritos: {state.NitriteMgPerLiter:0.00} → {result.Nitrite:0.00} mg/L\n"+
                $"Nitratos: {state.NitrateMgPerLiter:0.0} → {result.Nitrate:0.0} mg/L\n"+
                $"Residuos: reducción aproximada de {(1f-result.Waste/System.Math.Max(0.0001f,state.OrganicWaste))*100f:0}%":
                "No se puede calcular la previsualización.";
            FilterSummary=filter==null?"Filtro no disponible":$"Filtro: {filter.Status}\nEficiencia: {filter.CurrentEfficiency*100f:0}%\nSuciedad: {filter.DirtLevel*100f:0}%\n"+
                (filter.MaintenanceRecommended?"Mantenimiento recomendado":"Funcionamiento normal");
            Cooldown=maintenance!=null&&maintenance.CooldownRemaining>0f?$"Disponible en {maintenance.CooldownRemaining:0.0} s":"Disponible";
        }
    }
}
