namespace Acuaria.Simulation.Time
{
    public enum AquariumModalType { None, Details, Maintenance, Journal, Welfare, Codex }
    public sealed class AquariumInteractionState
    {
        public AquariumModalType OpenModal{get;private set;}
        public bool InteractionBlocked=>OpenModal!=AquariumModalType.None;
        public bool SimulationPaused{get;private set;}
        public bool FishVisualMovementPaused{get;private set;}
        public void Open(AquariumModalType modal){OpenModal=modal;}
        public void Close(AquariumModalType modal){if(OpenModal==modal)OpenModal=AquariumModalType.None;}
        public void SetExplicitSimulationPause(bool paused)=>SimulationPaused=paused;
        public void SetExplicitFishVisualPause(bool paused)=>FishVisualMovementPaused=paused;
    }
}
