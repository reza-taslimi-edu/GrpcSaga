namespace SagaOrchestrator.Commonalities
{
    public class SagaStep
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public Func<Task<bool>>? Compensate { get; set; }
        public bool Executed { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}