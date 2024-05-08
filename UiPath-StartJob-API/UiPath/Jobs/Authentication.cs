using System.Text;
using System.Text.Json;

namespace UiPath_StartJob_API.UiPath.Jobs
{
    public class Authentication
    {
        public async Task<Tuple<bool,string>> GetBearerToken()
        {
            bool status = false;
            string result = string.Empty;
			try
			{
                IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .Build();

                string oAuthString = configuration["UiPathAPI:oAuthString"];
                string oAuthURL = configuration["UiPathAPI:oAuthURL"];


                HttpClient client = new HttpClient();

                string requestBody = oAuthString;
                HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

                HttpResponseMessage response = await client.PostAsync(oAuthURL, content);

                string responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    status = true;

                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseBody);
                    string accessToken = tokenResponse!.access_token;

                    result = $"Bearer {accessToken}";
                }
                else
                {
                    throw new Exception($"Cannot Authorized!, {responseBody}");
                }

            }
            catch (Exception ex)
			{
                status = false;
                result = ex.Message;
			}
            return new Tuple<bool, string>(status, result);
        }
    }

    public class TokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
    }
}
             