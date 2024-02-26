using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace gRPC_Task.Services;

public class OpenMeteoService : OpenMeteo.OpenMeteoBase
{
    private static readonly DateTime StartDate = new DateTime(2023, 9, 21, 0, 0, 0);
    private const string RequestURI = "https://api.open-meteo.com/v1/forecast?latitude=55.7887&longitude=49.1221&hourly=temperature_2m&timezone=Europe%2FMoscow&start_hour={0}&end_hour={0}";
    private readonly IHttpClientFactory _httpClientFactory;

    public OpenMeteoService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override Task<ForecastResponse> SendForecast(ForecastRequest request, ServerCallContext context)
    {
        var client = _httpClientFactory.CreateClient();
        return GetForecastAsync(client, StartDate, CancellationToken.None);
    }

    public override async Task SendForecastStream(ForecastRequest request, IServerStreamWriter<ForecastResponse> responseStream, ServerCallContext context)
    {
        var client = _httpClientFactory.CreateClient();
        var cur = StartDate;
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var forecastResponse = await GetForecastAsync(client, cur, context.CancellationToken);
            await responseStream.WriteAsync(forecastResponse, context.CancellationToken);
    
            await Task.Delay(1000, context.CancellationToken);
            cur = cur.AddHours(2);
        }

        Console.WriteLine("Stream cancelled");
    }

    async Task<ForecastResponse> GetForecastAsync(HttpClient client, DateTime date, CancellationToken cancellationToken)
    {
        var response = await client.GetAsync(string.Format(RequestURI, date.ToString("yyyy-MM-ddTHH:mm")), cancellationToken);
        var jsonValue = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var hourly = jsonValue.GetProperty("hourly");
        var time = hourly.GetProperty("time").EnumerateArray();
        time.MoveNext();
        var temperatures = hourly.GetProperty("temperature_2m").EnumerateArray();
        temperatures.MoveNext();
        var forecastResponse = new ForecastResponse
        {
            Date = time.Current.GetDateTime().ToUniversalTime().ToTimestamp(),
            Temperature = temperatures.Current.GetDouble()
        };
        return forecastResponse;
    }
}
