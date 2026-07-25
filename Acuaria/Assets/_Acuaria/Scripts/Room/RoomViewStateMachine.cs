namespace Acuaria.Room
{
    public enum RoomViewState
    {
        RoomOverview,
        FocusingAquarium,
        AquariumFocused,
        ReturningToRoom
    }

    public sealed class RoomViewStateMachine
    {
        public RoomViewState State { get; private set; } = RoomViewState.RoomOverview;
        public bool IsTransitioning => State is RoomViewState.FocusingAquarium or RoomViewState.ReturningToRoom;

        public bool TryBeginFocus() => TryTransition(RoomViewState.RoomOverview, RoomViewState.FocusingAquarium);
        public bool TryCompleteFocus() => TryTransition(RoomViewState.FocusingAquarium, RoomViewState.AquariumFocused);
        public bool TryBeginReturn() => TryTransition(RoomViewState.AquariumFocused, RoomViewState.ReturningToRoom);
        public bool TryCompleteReturn() => TryTransition(RoomViewState.ReturningToRoom, RoomViewState.RoomOverview);

        private bool TryTransition(RoomViewState expected, RoomViewState next)
        {
            if (State != expected)
            {
                return false;
            }

            State = next;
            return true;
        }
    }
}
