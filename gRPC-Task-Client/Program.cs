using gRPC_Task;
using Grpc.Core;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5270");

var secretClient = new Secret.SecretClient(channel);
try
{
    var _ = await secretClient.GetSecretAsync(new SecretDto());
}
catch (RpcException e)
{
    Console.WriteLine(e.StatusCode);
}

var loginClient = new Jwt.JwtClient(channel);
var jwt = await loginClient.GetJwtAsync(new LoginDto {Username = "clonediff", Password = "P@ssw0rd!"});
Console.WriteLine($"jwt: {jwt}");

var metadata = new Metadata();
metadata.Add("Authorization", $"Bearer {jwt.Result}");
var secretResult = await secretClient.GetSecretAsync(new SecretDto(), metadata);
Console.WriteLine($"Secret string: {secretResult.Result}");

var forecastClient = new OpenMeteo.OpenMeteoClient(channel);
using var forecasts = forecastClient.SendForecastStream(new ForecastRequest());
while (await forecasts.ResponseStream.MoveNext(CancellationToken.None))
{
    var cur = forecasts.ResponseStream.Current;
    var date = cur.Date.ToDateTime();
    date = date.ToLocalTime();
    Console.WriteLine($"{date}: {cur.Temperature}\u00b0C");
}
Console.ReadKey();
