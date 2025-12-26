namespace SagaOrchestrator.Commonalities
{
    public class SagaOrchestration
    {
        private readonly ILogger<SagaOrchestration> _logger;

        public SagaOrchestration(ILogger<SagaOrchestration> logger)
        {
            _logger = logger;
        }

        public async Task<SagaStep> ExecuteSagaStep(string stepName, Func<Task<(bool Success, string Message)>> execute, Func<Task<bool>> compensate)
        {
            try
            {
                _logger.LogInformation("Executing step: {StepName}", stepName);

                var result = await execute();

                if (!result.Success)
                {
                    throw new Exception($"Step {stepName} failed: {result.Message}");
                }

                _logger.LogInformation("Step {StepName} executed successfully", stepName);

                return new SagaStep
                {
                    Name = stepName,
                    Compensate = compensate,
                    Executed = true,
                    ExecutedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Step {StepName} execution failed", stepName);
                throw;
            }
        }

        public async Task CompensateSaga(List<SagaStep> executedSteps, string transactionId)
        {
            var executedStepsCount = executedSteps.Count(step => step.Executed);

            if (executedStepsCount == 0)
            {
                _logger.LogInformation("No steps to compensate for transaction {TransactionId}", transactionId);
                return;
            }

            _logger.LogWarning("Starting compensation for {ExecutedStepsCount} steps in transaction {TransactionId}",
                executedStepsCount, transactionId);

            // Execute compensation in reverse order
            foreach (var step in executedSteps.Where(s => s.Executed).Reverse())
            {
                try
                {
                    _logger.LogInformation("Executing compensation for step: {StepName}", step.Name);

                    var compensationSuccess = await step.Compensate();

                    if (compensationSuccess)
                    {
                        _logger.LogInformation("Compensation executed successfully for step {StepName}", step.Name);
                    }
                    else
                    {
                        _logger.LogWarning("Compensation returned failure for step {StepName}", step.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Compensation execution failed for step {StepName}", step.Name);
                    // Continue with other compensations even if one fails
                }
            }

            _logger.LogInformation("Compensation completed for transaction {TransactionId}", transactionId);
        }
    }
}