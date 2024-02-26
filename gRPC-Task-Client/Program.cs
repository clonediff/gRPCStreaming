using gRPC_Task;
using Grpc.Net.Client;

using var channel = GrpcChannel.ForAddress("http://localhost:5270");
var client = new OpenMeteo.OpenMeteoClient(channel);
using var res = client.SendForecastStream(new ForecastRequest());
while (await res.ResponseStream.MoveNext(CancellationToken.None))
{
    var cur = res.ResponseStream.Current;
    var date = cur.Date.ToDateTime();
    date = date.ToLocalTime();
    Console.WriteLine($"{date}: {cur.Temperature}\u00b0C");
}
Console.ReadKey();
