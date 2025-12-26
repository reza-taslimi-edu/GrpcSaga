using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using protoServices.Grpc;
using SagaOrchestrator.Commonalities;
using Shared.Extensions;

namespace SagaOrchestrator.ProtosServices
{
    public class GrpcCityService : CityService.CityServiceBase
    {
        private readonly ILogger<GrpcCityService> _logger;
        private readonly SagaOrchestration _sagaOrchestration;
        private readonly GrpcChannelOptions _channelOptions;
        private readonly string _channelOneUrl;
        private readonly string _channelTwoUrl;

        public GrpcCityService(ILogger<GrpcCityService> logger,
            IOptions<ConsumerSettings> consumerSettings,
            SagaOrchestration sagaOrchestration)
        {
            _logger = logger;

            _sagaOrchestration = sagaOrchestration;

            _channelOneUrl = consumerSettings.Value.One;
            _channelTwoUrl = consumerSettings.Value.Two;

            _channelOptions = ExtensionGrpc.GetDefaultGrpcChannelOptions();

        }

        private GrpcChannel CreateChannel(string serverUrl)
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
                throw new ArgumentException("Server URL cannot be null or empty", nameof(serverUrl));

            return GrpcChannel.ForAddress(serverUrl, _channelOptions);
        }

        public override async Task<CityResponse> AddCity(CityRequest request, ServerCallContext context)
        {
            var executedSteps = new List<SagaStep>();
            var transactionId = context.GetHttpContext().TraceIdentifier;

            using var channelOne = CreateChannel(_channelOneUrl);
            using var channelTwo = CreateChannel(_channelTwoUrl);

            var clientOne = new CityService.CityServiceClient(channelOne);
            var clientTwo = new CityService.CityServiceClient(channelTwo);

            try
            {
                _logger.LogInformation("Starting Saga for transaction {TransactionId}", transactionId);

                // Step 1: Execute ConsumerOne

                var step1Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerOne_Add",
                    execute: async () =>
                    {
                        var result = await clientOne.AddCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientOne.RollbackToAddCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step1Result);

                // Step 2: Execute ConsumerTwo

                var step2Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerTwo_Add",
                    execute: async () =>
                    {
                        var result = await clientTwo.AddCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientTwo.RollbackToAddCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step2Result);

                _logger.LogInformation("Saga completed successfully for transaction {TransactionId}", transactionId);

                return new CityResponse
                {
                    Success = true,
                    Message = "City added successfully"
                };
            }
            catch (RpcException rpcEx)
            {
                _logger.LogError(rpcEx, "Saga failed for transaction {TransactionId}", transactionId);

                // Execute compensation for all executed steps
                await _sagaOrchestration.CompensateSaga(executedSteps, transactionId);

                return new CityResponse
                {
                    Success = false,
                    Message = $"Operation failed : {rpcEx.Status.Detail}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga failed for transaction {TransactionId}", transactionId);

                // Execute compensation for all executed steps
                await _sagaOrchestration.CompensateSaga(executedSteps, transactionId);

                return new CityResponse
                {
                    Success = false,
                    Message = $"Operation failed : {ex.Message}"
                };
            }
        }

        public override async Task<CityResponse> UpdateCity(CityRequest request, ServerCallContext context)
        {
            var executedSteps = new List<SagaStep>();
            var transactionId = context.GetHttpContext().TraceIdentifier;

            using var channelOne = CreateChannel(_channelOneUrl);
            using var channelTwo = CreateChannel(_channelTwoUrl);

            var clientOne = new CityService.CityServiceClient(channelOne);
            var clientTwo = new CityService.CityServiceClient(channelTwo);

            try
            {
                _logger.LogInformation("Starting Saga for transaction {TransactionId}", transactionId);

                // Step 1: Execute ConsumerOne

                var step1Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerOne_Update",
                    execute: async () =>
                    {
                        var result = await clientOne.UpdateCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientOne.RollbackToUpdateCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step1Result);

                // Step 2: Execute ConsumerTwo

                var step2Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerTwo_Update",
                    execute: async () =>
                    {
                        var result = await clientTwo.UpdateCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientTwo.RollbackToUpdateCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step2Result);

                _logger.LogInformation("Saga completed successfully for transaction {TransactionId}", transactionId);

                return new CityResponse
                {
                    Success = true,
                    Message = "City update successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga failed for transaction {TransactionId}", transactionId);

                // Execute compensation for all executed steps
                await _sagaOrchestration.CompensateSaga(executedSteps, transactionId);

                var mess = ex.Message;

                return new CityResponse
                {
                    Success = false,
                    Message = $"Operation failed: {ex.Message}"
                };
            }
        }

        public override async Task<CityResponse> DeleteCity(DeleteRequest request, ServerCallContext context)
        {
            var executedSteps = new List<SagaStep>();
            var transactionId = context.GetHttpContext().TraceIdentifier;

            using var channelOne = CreateChannel(_channelOneUrl);
            using var channelTwo = CreateChannel(_channelTwoUrl);

            var clientOne = new CityService.CityServiceClient(channelOne);
            var clientTwo = new CityService.CityServiceClient(channelTwo);

            try
            {
                _logger.LogInformation("Starting Saga for transaction {TransactionId}", transactionId);

                // Step 1: Execute ConsumerOne

                var step1Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerOne_Delete",
                    execute: async () =>
                    {
                        var result = await clientOne.DeleteCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientOne.RollbackToDeleteCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step1Result);

                // Step 2: Execute ConsumerTwo

                var step2Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerTwo_Delete",
                    execute: async () =>
                    {
                        var result = await clientTwo.DeleteCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientTwo.RollbackToDeleteCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step2Result);

                _logger.LogInformation("Saga completed successfully for transaction {TransactionId}", transactionId);

                return new CityResponse
                {
                    Success = true,
                    Message = "City delete successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga failed for transaction {TransactionId}", transactionId);

                // Execute compensation for all executed steps
                await _sagaOrchestration.CompensateSaga(executedSteps, transactionId);

                return new CityResponse
                {
                    Success = false,
                    Message = $"Operation failed: {ex.Message}"
                };
            }
        }

        public override async Task<CityResponse> RollbackToAddCity(CityRequest request, ServerCallContext context)
        {
            var executedSteps = new List<SagaStep>();
            var transactionId = context.GetHttpContext().TraceIdentifier;

            using var channelOne = CreateChannel(_channelOneUrl);
            using var channelTwo = CreateChannel(_channelTwoUrl);

            var clientOne = new CityService.CityServiceClient(channelOne);
            var clientTwo = new CityService.CityServiceClient(channelTwo);

            try
            {
                _logger.LogInformation("Starting Saga for transaction {TransactionId}", transactionId);

                // Step 1: Execute ConsumerOne

                var step1Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerOne_Add",
                    execute: async () =>
                    {
                        var result = await clientOne.RollbackToAddCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientOne.RollbackToAddCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step1Result);

                // Step 2: Execute ConsumerTwo

                var step2Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerTwo_Add",
                    execute: async () =>
                    {
                        var result = await clientTwo.RollbackToAddCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientTwo.RollbackToAddCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step2Result);

                _logger.LogInformation("Saga completed successfully for transaction {TransactionId}", transactionId);

                return new CityResponse
                {
                    Success = true,
                    Message = "City add successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga failed for transaction {TransactionId}", transactionId);

                // Execute compensation for all executed steps
                await _sagaOrchestration.CompensateSaga(executedSteps, transactionId);

                return new CityResponse
                {
                    Success = false,
                    Message = $"Operation failed: {ex.Message}"
                };
            }
        }

        public override async Task<CityResponse> RollbackToUpdateCity(CityRequest request, ServerCallContext context)
        {
            var executedSteps = new List<SagaStep>();
            var transactionId = context.GetHttpContext().TraceIdentifier;

            using var channelOne = CreateChannel(_channelOneUrl);
            using var channelTwo = CreateChannel(_channelTwoUrl);

            var clientOne = new CityService.CityServiceClient(channelOne);
            var clientTwo = new CityService.CityServiceClient(channelTwo);

            try
            {
                _logger.LogInformation("Starting Saga for transaction {TransactionId}", transactionId);

                // Step 1: Execute ConsumerOne

                var step1Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerOne_Update",
                    execute: async () =>
                    {
                        var result = await clientOne.RollbackToUpdateCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientOne.RollbackToUpdateCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step1Result);

                // Step 2: Execute ConsumerTwo

                var step2Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerTwo_Update",
                    execute: async () =>
                    {
                        var result = await clientTwo.RollbackToUpdateCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientTwo.RollbackToUpdateCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step2Result);

                _logger.LogInformation("Saga completed successfully for transaction {TransactionId}", transactionId);

                return new CityResponse
                {
                    Success = true,
                    Message = "City update successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga failed for transaction {TransactionId}", transactionId);

                // Execute compensation for all executed steps
                await _sagaOrchestration.CompensateSaga(executedSteps, transactionId);

                return new CityResponse
                {
                    Success = false,
                    Message = $"Operation failed: {ex.Message}"
                };
            }
        }

        public override async Task<CityResponse> RollbackToDeleteCity(DeleteRequest request, ServerCallContext context)
        {
            var executedSteps = new List<SagaStep>();
            var transactionId = context.GetHttpContext().TraceIdentifier;

            using var channelOne = CreateChannel(_channelOneUrl);
            using var channelTwo = CreateChannel(_channelTwoUrl);

            var clientOne = new CityService.CityServiceClient(channelOne);
            var clientTwo = new CityService.CityServiceClient(channelTwo);

            try
            {
                _logger.LogInformation("Starting Saga for transaction {TransactionId}", transactionId);

                // Step 1: Execute ConsumerOne

                var step1Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerOne_Delete",
                    execute: async () =>
                    {
                        var result = await clientOne.RollbackToDeleteCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientOne.RollbackToDeleteCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step1Result);

                // Step 2: Execute ConsumerTwo

                var step2Result = await _sagaOrchestration.ExecuteSagaStep(
                    stepName: "ConsumerTwo_Delete",
                    execute: async () =>
                    {
                        var result = await clientTwo.RollbackToDeleteCityAsync(request);
                        return (result.Success, result.Message);
                    },
                    compensate: async () =>
                    {
                        var result = await clientTwo.RollbackToDeleteCityAsync(request);
                        return result.Success;
                    });

                executedSteps.Add(step2Result);

                _logger.LogInformation("Saga completed successfully for transaction {TransactionId}", transactionId);

                return new CityResponse
                {
                    Success = true,
                    Message = "City delete successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Saga failed for transaction {TransactionId}", transactionId);

                // Execute compensation for all executed steps
                await _sagaOrchestration.CompensateSaga(executedSteps, transactionId);

                return new CityResponse
                {
                    Success = false,
                    Message = $"Operation failed: {ex.Message}"
                };
            }
        }
    }
}