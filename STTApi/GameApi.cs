using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace STTFleet.STTApi
{
   public class GameApi
   {
        private string _accessToken;
        private STTApiConfiguration _configuration;

        public GameApi(STTApiConfiguration apiConfiguration)
        {
            _configuration = apiConfiguration;
        }

        public async Task Login()
        {
            string userName = _configuration.Username;
            string password = _configuration.Password;

            var body = new
            {
            username = userName,
            password = password,
            client_id = _configuration.ClientId,
            grant_type = "password"
            };

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("username", userName),
                new KeyValuePair<string, string>("client_id", _configuration.ClientId),
                new KeyValuePair<string, string>("grant_type", "password"),
            });

            HttpClient client = new HttpClient();
            var response = await client.PostAsync(_configuration.UrlPlatform + "oauth2/token", content);
            if (response.StatusCode != HttpStatusCode.OK) throw new System.Exception(response.ReasonPhrase);

            var responseMessage = await response.Content.ReadAsStringAsync();
            dynamic jObject = JObject.Parse(responseMessage);
            _accessToken = jObject.access_token;
        }

        public async Task<string> GetPlayerData(string accessToken)
        {
            string fleetId = _configuration.FleetId;
            var data = await _ExecuteGetRequest("player", accessToken);
            return data;
        }

        public async Task<string> GetFleetData(string accessToken)
        {
            var data = await _ExecuteGetRequest("fleet/" + _configuration.FleetId, accessToken);

            return data;
        }

        public async Task<FleetMemberInfo> GetFleetMemberInfo()
        {
            string fleetId = _configuration.FleetId;
            var data = await _ExecutePostRequest("fleet/complete_member_info", new[] { $"guild_id={fleetId}" });
            return JsonConvert.DeserializeObject<FleetMemberInfo>(data);
        }

        private async Task<string> _ExecuteGetRequest(string url, params string[] queryParams)
        {
            HttpClient client = new HttpClient();

            var queryString = string.Join("&", queryParams.Union(new[] { $"client_api={_configuration.ClientApiVersion}", $"access_token={_accessToken}" }));
            var response = await client.GetAsync(_configuration.UrlServer + url + "?" + queryString);
            var responseMessage = await response.Content.ReadAsStringAsync();
            return responseMessage;
        }

        private async Task<string> _ExecutePostRequest(string url, params string[] bodyParams)
        {
            HttpClient client = new HttpClient();
            var body = string.Join("&", bodyParams.Union(new[] { $"client_api={_configuration.ClientApiVersion}", $"access_token={_accessToken}" }));
            var response = await client.PostAsync(_configuration.UrlServer + url, new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"));
            var responseMessage = await response.Content.ReadAsStringAsync();
            return responseMessage;
        }
   }
}