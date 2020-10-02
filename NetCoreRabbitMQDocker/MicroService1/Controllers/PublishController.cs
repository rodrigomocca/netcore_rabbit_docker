using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Events;
using System;
using System.Threading.Tasks;

namespace MicroService1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PublishController : ControllerBase
    {
        private readonly IRabbitManager _manager;

        public PublishController(IRabbitManager manager)
        {
            _manager = manager;
        }

        [HttpPost("{message}")]
        public async Task<IActionResult> Post(string message)
        {
            try
            {
                var @event = new Event(Guid.NewGuid().ToString(), message);
                await _manager.Publish(@event);
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}