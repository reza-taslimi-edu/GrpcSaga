using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using protoServices.Grpc;
using Shared.Extensions;

namespace Producer.ProtosServices
{
    public class GrpcCityServiceClient
    {
        private readonly ILogger<GrpcCityServiceClient> _logger;
        private readonly GrpcChannelOptions _channelOptions;
        private readonly string _channelUrl;

        public GrpcCityServiceClient(ILogger<GrpcCityServiceClient> logger,
            IOptions<Settings> settings)
        {
            _logger = logger;

            _channelOptions = ExtensionGrpc.GetDefaultGrpcChannelOptions();

            _channelUrl = settings.Value.SagaUrl;
        }

        private GrpcChannel CreateChannel(string serverUrl)
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
                throw new ArgumentException("Server URL cannot be null or empty", nameof(serverUrl));

            return GrpcChannel.ForAddress(serverUrl, _channelOptions);
        }

        public async Task<CityResponse> AddCityAsync(CityRequest request)
        {
            using var channel = CreateChannel(_channelUrl);

            var client = new CityService.CityServiceClient(channel);

            var result = await client.AddCityAsync(request);

            return result;
        }

        public async Task<CityResponse> UpdateCityAsync(CityRequest request)
        {
            using var channel = CreateChannel(_channelUrl);

            var client = new CityService.CityServiceClient(channel);

            var result = await client.UpdateCityAsync(request);

            return result;
        }

        public async Task<CityResponse> DeleteCityAsync(DeleteRequest request)
        {
            using var channel = CreateChannel(_channelUrl);

            var client = new CityService.CityServiceClient(channel);

            var result = await client.DeleteCityAsync(request);

            return result;
        }
    }
}