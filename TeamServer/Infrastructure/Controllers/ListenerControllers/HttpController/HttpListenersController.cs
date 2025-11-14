using ApiModels.DTOs.HttpDTOs.RequestDTOs;
using Microsoft.AspNetCore.Mvc;
using TeamServer.Application.Services.ListenerServices.HttpListenerService.HttpCore;
using TeamServer.Application.Services.ListenerServices.HttpListenerService.HttpCRUD;
using TeamServer.Domain.Entities.Listeners.HttpListeners;

namespace TeamServer.Infrastructure.Controllers.ListenerControllers.HttpController
{
    [ApiController]
    [Route("[controller]")]
    public class HttpListenersController : ControllerBase
    {
        private readonly IHttpCRUD _httpCRUD;
        private readonly IHttpCore _httpCore;

        public HttpListenersController(IHttpCRUD httpCRUD, IHttpCore httpCore)
        {
            _httpCRUD = httpCRUD;
            _httpCore = httpCore;
        }

        [HttpGet]
        public async Task<IActionResult> GetListeners()
        {
            var listeners = await _httpCRUD.GetAllListenersAsync();
            if (listeners == null || !listeners.Any())
            {
                return Ok(Array.Empty<HttpListenerEntity>());
            }
            
            return Ok(listeners);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetListener(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            var listener = await _httpCRUD.GetListenerAsync(name);
            if (listener == null)
                return NotFound();

            return Ok(listener);
        }

        [HttpPost("Start")]
        public async Task<IActionResult> AddListener([FromBody] StartHttpRequestDTO req)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var allowedPorts = new HashSet<int> { 8080 };

            if (!allowedPorts.Contains(req.BindPort))
                return BadRequest($"The bindport {req.BindPort} is not active on this host");

            var listener = new HttpListenerEntity(req.Name, req.BindPort);

            await _httpCRUD.AddListenerAsync(listener);

            await _httpCore.StartHttpListenerAsync(listener.BindPort);

            return CreatedAtAction(nameof(GetListener), new { name = listener.Name }, listener );
        }

        [HttpPost("Stop")]
        public async Task<IActionResult> RemoveListener([FromBody] StopHttpRequestDTO req)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var listener = await _httpCRUD.GetListenerAsync(req.Name);
            if (listener == null)
                return BadRequest("The listener does not exist");

            await _httpCore.StopHttpListenerAsync();

            var delete = await _httpCRUD.RemoveListenerAsync(listener.Name);
            if (!delete)
                return BadRequest("Failed to delete resource");

            return Ok($"Listener: {listener.Name} deleted");
        }
    }
}
