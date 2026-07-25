namespace Acuaria.Aquarium
{
    public readonly struct AquariumStatusResult
    {
        public AquariumStatusResult(AquariumStatus status, string message, int severity)
        {
            Status = status;
            Message = message;
            Severity = severity;
        }

        public AquariumStatus Status { get; }
        public string Message { get; }
        public int Severity { get; }
    }
}
