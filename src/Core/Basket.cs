using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WBBasket.Core;

public class Basket
{
    private readonly HttpClient _client = new HttpClient();

    private const string _DeviceID = "site_d200f29db67f4a03a34b4523c517ad9b";

    private readonly string _apiKey;

    public Basket(string apiKey)
    {
        _apiKey = apiKey;
    }

    /// <summary>
    /// Adds specified product to basket using <paramref name="idRequest"/> to
    /// obtain unique product's ID
    /// </summary>
    public async Task AddAsync(IDRequest idRequest)
    {
        var productId = await idRequest.Execute();

        var request = BuildRequest(idRequest.VendorCode, productId);

        var response = await _client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new InvalidOperationException("Can't authorize. Probably API key is wrong");
        if ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600)
            throw new InvalidOperationException("Internal server error on the side of Wildberries");
        if (response.StatusCode != HttpStatusCode.OK)
            throw new InvalidOperationException("Operation not completed. Response code: " + (int)response.StatusCode);
    }

    private HttpRequestMessage BuildRequest(string vendorCode, string productID)
    {
        var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString();

        var uriBuilder = new UriBuilder("https", "cart-storage-api.wildberries.ru");
        uriBuilder.Path = "api/basket/sync";
        uriBuilder.Query = $"?ts={timestamp}&device_id={_DeviceID}";

        var result = new HttpRequestMessage(HttpMethod.Post, uriBuilder.Uri);

        var content = new {
            chrt_id = int.Parse(productID),
            quantity = 1,
            cod_1s = int.Parse(vendorCode),
            op_type = 1 // 1 is for adding to basket
        };

        result.Content = JsonContent.Create(new object[] { content });

        result.Headers.Host = uriBuilder.Host;
        result.Headers.UserAgent.Add(new ProductInfoHeaderValue(new ProductHeaderValue("HackerPC")));

        result.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        result.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
        result.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("br"));
        result.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("zstd"));

        result.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
        result.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("en", 0.9));
        result.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("ru-RU", 0.8));
        result.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("ru", 0.7));

        var api = _apiKey.Replace("Bearer ", "");
        result.Headers.Authorization = new AuthenticationHeaderValue("Bearer", api);

        result.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
        result.Content.Headers.ContentLength = result.Content.ReadAsStringAsync().Result.Length;

        return result;
    }
}
