using inzynierka.Auth.Grpc;

namespace inzynierka.Auth.Grpc.Clients;

public interface IAuthGrpcClient
{
    Task<ValidateTokenResponse> ValidateTokenAsync(string token);
    Task<GetUserInfoResponse> GetUserInfoAsync(string userId);
}

public class AuthGrpcClient : IAuthGrpcClient
{
    private readonly AuthService.AuthServiceClient _client;

    public AuthGrpcClient(AuthService.AuthServiceClient client)
    {
        _client = client;
    }

    public async Task<ValidateTokenResponse> ValidateTokenAsync(string token)
    {
        var request = new ValidateTokenRequest { Token = token };
        return await _client.ValidateTokenAsync(request);
    }

    public async Task<GetUserInfoResponse> GetUserInfoAsync(string userId)
    {
        var request = new GetUserInfoRequest { UserId = userId };
        return await _client.GetUserInfoAsync(request);
    }
}