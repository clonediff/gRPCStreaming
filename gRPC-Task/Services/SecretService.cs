using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace gRPC_Task.Services;

[Authorize]
public class SecretService : Secret.SecretBase
{
    public override Task<SecretResponse> GetSecret(SecretDto request, ServerCallContext context)
    {
        return Task.FromResult(new SecretResponse{Result = "Some really secret string"});
    }
}
