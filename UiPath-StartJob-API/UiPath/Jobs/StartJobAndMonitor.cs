using System.Text.Json;
using System.Text;
using UiPath_StartJob_API.Models;

namespace UiPath_StartJob_API.UiPath.Jobs
{
    public class StartJobAndMonitor
    {
        public async Task<Tuple<bool, string>> StartAndMonitorJob(string BearerToken,string ProcessReleaseKey,TicketRequest inputArgs)
        {
            bool status = false;
            string result = string.Empty;
            try
            {
                IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .Build();

                Int32 robotIds = Int32.Parse(configuration["UiPathProcess:RobotIds"].ToString());
                string startJobURL = configuration["UiPathProcess:StartJobRoute"];
                string OrganizationUnitId = configuration["UiPathProcess:OrganizationUnitId"];

                int MonitorCount = Int32.Parse(configuration["UiPathProcess:MonitorCount"].ToString());
                int MonitorInterval = Int32.Parse(configuration["UiPathProcess:MonitorInterval"].ToString());


                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", BearerToken);
                client.DefaultRequestHeaders.Add("X-UIPATH-OrganizationUnitId", OrganizationUnitId);

                var dataBody = new
                {
                    startInfo = new
                    {
                        ReleaseKey = ProcessReleaseKey,
                        RobotIds = new int[] { robotIds },
                        JobsCount = 0,
                        Strategy = "Specific",
                        InputArguments =  JsonSerializer.Serialize<TicketRequest>(inputArgs)
                    }
                };

                string requestBody = JsonSerializer.Serialize(dataBody);
                HttpContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(startJobURL, content);

                string responseBody = await response.Content.ReadAsStringAsync();


                if (response.IsSuccessStatusCode)
                {
                    JsonDocument doc = JsonDocument.Parse(responseBody);
                    JsonElement root = doc.RootElement;
                    JsonElement idElement = root.GetProperty("value")[0].GetProperty("Id");
                    int id = idElement.GetInt32();

                    //Monitor for n times
                    var monitorResults = await MonitorJob(BearerToken, id);
                    while (MonitorCount >= 0)
                    {
                        await Task.Delay(MonitorInterval);

                        monitorResults = await MonitorJob(BearerToken, id);

                        if(monitorResults.Item1 && monitorResults.Item2.Equals("Successful"))
                        {
                            status = true;
                            result = monitorResults.Item3;
                            break;
                        }

                        MonitorCount--;
                    }
                
                    if(!status)
                    {
                        throw new Exception(monitorResults.Item2);
                    }
                }
                else
                {
                    throw new Exception($"Error while start and monitor the job!, {responseBody}");
                }

            }
            catch (Exception ex)
            {
                status = false;
                result = ex.Message;
            }
            return new Tuple<bool, string>(status, result);
        }


        private async Task<Tuple<bool, string,string>> MonitorJob(string BearerToken, int jobId)
        {
            bool status = false;
            string resultStatus = string.Empty;
            string resultOutArgs = string.Empty;
            try
            {
                IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .Build();

                string getJobURL = configuration["UiPathProcess:GetJobRoute"].Replace("TOKEN1",jobId.ToString());


                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", BearerToken);
                //client.DefaultRequestHeaders.Add("Content-Type", "application/json");


                HttpResponseMessage response = await client.GetAsync(getJobURL);

                string responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    //Job started successfully, now monitor it 
                    JsonDocument document = JsonDocument.Parse(responseBody);
                    JsonElement root = document.RootElement;
                    status = true;
                    resultStatus = root.GetProperty("State").GetString();
                    resultOutArgs = root.GetProperty("OutputArguments").GetString();
                }
                else
                {
                    throw new Exception($"Cannot monitor the job!, {responseBody}");
                }

            }
            catch (Exception ex)
            {
                status = false;
                resultStatus = ex.Message;
                resultOutArgs = string.Empty;
            }
            return new Tuple<bool, string,string>(status, resultStatus,resultOutArgs);
        }

    }
}
