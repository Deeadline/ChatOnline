using ChatOnline.Interface;
using ChatOnline.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System;

namespace ChatOnline.Controllers
{
    [Produces("application/json")]
    [Route("api/Message")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IHubContext<ChatHub, ITypedHubClient> _hubContext;
        public MessageController(IHubContext<ChatHub, ITypedHubClient> hubContext) => _hubContext = hubContext;

        [HttpGet]
        public IActionResult Get()
        {
            var data = JsonConvert.SerializeObject(new Message { Type = "Success", Payload = "Hello World" });
            _hubContext.Clients.All.BroadcastMessage(new Message { Type = "Success", Payload = "Hello World" });
            return Ok(new { Message = "Request completed" });
        }

        [HttpPost]
        public IActionResult Post([FromBody] Message message)
        {
            string retMessage;
            try
            {
                _hubContext.Clients.All.BroadcastMessage(message);
                return Ok(new { Message = "Success" });
            }
            catch (Exception e)
            {
                retMessage = e.ToString();
            }

            return Content(retMessage);
        }
    }
}