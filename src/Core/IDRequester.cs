using System;
using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WBBasket.Core;

public class IDRequest
{
    public readonly uint _maxBasketInstances;

    private readonly HttpClient _client = new HttpClient();

    /// <summary>
    /// Creates request that can be activated to acquire unique
    /// product's ID using <paramref name="vendorCode"/>.
    /// </summary>
    /// <remarks>
    /// Products in WB divided into volumes and using manual lookup I found out
    /// that WB stores each N'th volumes on separate service instances
    /// I think this value is inconsistent and may change from day to day and
    /// it would be cool to make another object that tries to figure out the N value
    /// by sending to WB couple of requests with different "vol" value,
    /// BUT this is too much for test task :) so I decided to sequentally access each instance to success the request.
    /// </remarks>
    public IDRequest(string vendorCode, uint maxBasketInstances = 16)
    {
        VendorCode = vendorCode;
        _maxBasketInstances = maxBasketInstances;

        ValidateBasketInstances();
        ValidateVendorCode();

        PossibleURIs = BuildPossibleURIs();
    }

    /// <summary>
    /// Possible URIs that will be made to WB's service
    /// to get the ID for specified product
    /// </summary>
    public Uri[] PossibleURIs { get; }

    /// <summary>
    /// Contains last acquired id of a WB product
    /// </summary>
    /// <returns>
    /// Null if there was no successfull request
    /// </returns>
    public string AcquiredID { get; private set; }

    public string VendorCode { get; }

    /// <summary>
    /// Acquires unique product code using Wildberries product's card vendor code
    /// </summary>
    /// <remarks>
    /// Returns cached value without making the actual
    /// request if value already was acquired previously
    /// </remarks>
    /// <returns>
    /// Unique WB product's ID
    /// </returns>
    public async Task<string> Execute()
    {
        if (AcquiredID is not null)
            return AcquiredID;

        string result = null;
        foreach (var uri in PossibleURIs)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            HttpResponseMessage response = null;

            response = await _client.SendAsync(request);

            if ((int)response.StatusCode >= 500)
                throw new InvalidOperationException($"Internal server error. URI: {uri}");
            if (response.StatusCode != HttpStatusCode.OK)
                continue;

            var receivedContent = await response.Content.ReadAsStringAsync();

            try
            {
                result = JsonObject.Parse(receivedContent)["data"]["chrt_ids"][0].ToJsonString();
                AcquiredID = result;
            }
            catch
            {
                throw new InvalidOperationException("Error during parsing. The response has arrived in unexpected format");
            }

            return result;
        }

        if (result is null)
            throw new InvalidOperationException($"Could not find the product's ID with vendor code {VendorCode}");
        return result;
    }

    private Uri[] BuildPossibleURIs()
    {
        var part = VendorCode[0..(VendorCode.Length - 3)];
        var vol = VendorCode[0..(part.Length - 2)];

        var result = new Uri[_maxBasketInstances];
        var uriBuilder = new UriBuilder("https", "");
        uriBuilder.Path = $"vol{vol}/part{part}/{VendorCode}/info/ru/card.json";
        for (int i = 0; i < result.Length; i++)
        {
            uriBuilder.Host = $"basket-{(i + 1).ToString("00")}.wbbasket.ru";
            result[i] = uriBuilder.Uri;
        }

        return result;
    }

    private void ValidateVendorCode()
    {
        if (VendorCode is null) throw new InvalidOperationException();

        if (VendorCode.Length < 6 || VendorCode.Length > 12)
            throw new InvalidOperationException("Vendor code must be in between of 6 and 12 symbols length inclusive");

        foreach (var ch in VendorCode)
            if (!char.IsDigit(ch)) throw new InvalidOperationException("Vendor code must contain digits only");
    }

    private void ValidateBasketInstances()
    {
        if (_maxBasketInstances < 1 || _maxBasketInstances > 99)
            throw new InvalidOperationException("Only value from 1 to 99 is allowed to be specified as number of instances");
    }
}
