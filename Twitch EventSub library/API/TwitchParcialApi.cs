using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Twitch_EventSub_library.API.Models;

namespace Twitch_EventSub_library.API
{
    public class TwitchParcialApi
    {
        private readonly string _baseUrl = "https://api.twitch.tv/helix/eventsub/subscriptions";
        private readonly ILogger<TwitchParcialApi> _logger;

        public TwitchParcialApi(ILogger<TwitchParcialApi> Logger)
        {
            _logger = Logger;
        }
        /// <summary>
        /// Function sends filled CreateSubscriptionRequest to twitch for processing
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="request"></param>
        /// <returns>True on success, false on failure</returns>
        /// <exception cref="InvalidAccessTokenException"></exception>
        /// <exception cref="Exception">This state means that accessToken is not set up properly for given request</exception>
        public async Task<bool> SubscribeAsync(string? clientId, string? accessToken, CreateSubscriptionRequest request)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Client-Id", clientId);
                try
                {
                    string requestBody = JsonConvert.SerializeObject(request);
                    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(_baseUrl, content);
                    return response.StatusCode switch
                    {
                        HttpStatusCode.Accepted => true,
                        HttpStatusCode.Unauthorized => throw new InvalidAccessTokenException("Subscribe failed due" + await response.Content.ReadAsStreamAsync() + response.ReasonPhrase),
                        HttpStatusCode.Forbidden => throw new Exception("Invalid Scopes"),
                        _ => false
                    };
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogInformation($"Error: {ex.Message}");
                    return false;
                }
            }
        }
        /// <summary>
        /// Function unsubscribes subs
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="subscriptionId"></param>
        /// <returns>True on success, false on failure</returns>
        /// <exception cref="InvalidAccessTokenException"></exception>
        public async Task<bool> UnSubscribeAsync(string? clientId, string? accessToken, string subscriptionId)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Client-Id", clientId);
                try
                {
                    var url = $"{_baseUrl}?id={subscriptionId}";
                    var response = await httpClient.DeleteAsync(url);
                    return response.StatusCode switch
                    {
                        HttpStatusCode.NoContent => true,
                        HttpStatusCode.Unauthorized => throw new InvalidAccessTokenException("Unsubscribe failed due" + await response.Content.ReadAsStringAsync() + response.ReasonPhrase),
                        _ => false
                    };
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogInformation($"Error: {ex.Message}");
                    return false;
                }
            }
        }
        /// <summary>
        /// Gets a list of subs
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="statusSelector"></param>
        /// <param name="after"></param>
        /// <returns cref="GetSubscriptionsResponse"> Provides segment of subscriptions, content MAY BE NULL</returns>
        /// <exception cref="InvalidAccessTokenException"></exception>
        private async Task<GetSubscriptionsResponse?> GetSubscriptionsAsync(string? clientId, string? accessToken, StatusProvider.SubscriptionStatus statusSelector, string? after = null)
        {
            var status = StatusProvider.GetStatusString(statusSelector);

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                httpClient.DefaultRequestHeaders.Add("Client-Id", clientId);

                try
                {
                    var queryBuilder = new StringBuilder(_baseUrl);

                    if (!string.IsNullOrEmpty(status))
                        queryBuilder.Append($"?status={WebUtility.UrlEncode(status)}");

                    if (!string.IsNullOrEmpty(after))
                        queryBuilder.Append($"&after={WebUtility.UrlEncode(after)}");

                    var response = await httpClient.GetAsync(queryBuilder.ToString());
                    return response.StatusCode switch
                    {
                        HttpStatusCode.OK => JsonConvert.DeserializeObject<GetSubscriptionsResponse>(await response.Content.ReadAsStringAsync()),
                        HttpStatusCode.Unauthorized => throw new InvalidAccessTokenException(
                            "GetSubscriptions failed due" + await response.Content.ReadAsStringAsync() + response.ReasonPhrase),
                        _ => default // probably invalid parameter or user ID
                    };
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogInformation($"Error: {ex.Message}");
                    return default;
                }
            }
        }
        /// <summary>
        /// Provides entire list of subscriptions, handles pagination.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="accessToken"></param>
        /// <param name="statusSelector"></param>
        /// <returns> list of subscriptions</returns>
        /// <exception cref="InvalidAccessTokenException">May provide exception from GetSubscriptionsAsync</exception>
        public async Task<List<GetSubscriptionsResponse>> GetAllSubscriptionsAsync(string? clientId, string? accessToken, StatusProvider.SubscriptionStatus statusSelector = StatusProvider.SubscriptionStatus.Enabled)
        {
            var allSubscriptions = new List<GetSubscriptionsResponse>();
            string? afterCursor = null;
            int totalPossibleIterations = Int32.MaxValue;

            for (int i = 0; i < totalPossibleIterations; i++)
            {
                var response = await GetSubscriptionsAsync(clientId, accessToken, statusSelector, afterCursor);
                if (response != null)
                {
                    allSubscriptions.Add(response);
                    if (afterCursor == null)
                    {
                        totalPossibleIterations = response.Total;
                    }
                    afterCursor = response.Cursor;
                }
                else
                {
                    _logger.LogInformation("Response returned null cause of invalid userId or filter parameter");
                    break;
                }

                if (string.IsNullOrEmpty(afterCursor))
                {
                    break;
                }
            }
            if (allSubscriptions.Count == 0)
            {
                _logger.LogInformation("List of subscriptions returned EMPTY!");
            }

            return allSubscriptions;
        }

    }
}
