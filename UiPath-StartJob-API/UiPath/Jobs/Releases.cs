using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace UiPath_StartJob_API.UiPath.Jobs
{
    public class Releases
    {
        public async Task<Tuple<bool, string>> GetReleaseKey(string BearerToken,string ProcessName )
        {
            bool status = false;
            string result = string.Empty;
            try
            {
                IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .Build();

                string OrganizationUnitId = configuration["UiPathProcess:OrganizationUnitId"];
                string getFilterReleaseURL = configuration["UiPathProcess:ReleaseKeyRoute"].Replace("TOKEN1",ProcessName);


                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", BearerToken);
                client.DefaultRequestHeaders.Add("X-UIPATH-OrganizationUnitId", OrganizationUnitId);

                HttpResponseMessage response = await client.GetAsync(getFilterReleaseURL);

                string responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    status = true;

                    JsonDocument document = JsonDocument.Parse(responseBody);
                    JsonElement root = document.RootElement;
                    JsonElement value = root.GetProperty("value")[0];
                    string key = value.GetProperty("Key").GetString();

                    result = key!;
                }
                else
                {
                    throw new Exception($"Cant get release key!, {responseBody}");
                }

            }
            catch (Exception ex)
            {
                status = false;
                result = ex.Message;
            }
            return new Tuple<bool, string>(status, result!);
        }

    }
}
