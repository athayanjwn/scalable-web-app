using System.Diagnostics;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

public class ScaleDemo
{
    [Function("scale-demo")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "scale-demo")] HttpRequestData req)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        int ms = int.TryParse(query["workMs"], out var v) ? Math.Clamp(v, 0, 5000) : 200;

        var sw = Stopwatch.StartNew();

        // Beban CPU agar autoscale lebih mungkin terpicu saat load tinggi
        double x = 0;
        while (sw.ElapsedMilliseconds < ms)
        {
            for (int i = 0; i < 50_000; i++) x += Math.Sqrt(i + 123.456);
        }
        sw.Stop();

        string instanceId =
            Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")
            ?? Environment.GetEnvironmentVariable("COMPUTERNAME")
            ?? "unknown";

        var res = req.CreateResponse(System.Net.HttpStatusCode.OK);
        await res.WriteAsJsonAsync(new
        {
            message = "ok",
            workMs = ms,
            elapsedMs = sw.ElapsedMilliseconds,
            instanceId,
            utc = DateTime.UtcNow
        });
        return res;
    }
}
