using Anthropic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyDemoAPI.Data.Models;
using MyDemoAPI.Services;

namespace MyDemoAPI.Controllers
{

    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ConversationController : ControllerBase
    {
        private readonly IConversationService _service;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="service"></param>
        public ConversationController(IConversationService service) {
            _service = service;
        }

        /// <summary>
        /// Get a list of conversations
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<Conversation>> Get() {
            return await _service.GetAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> Get(string id) {
            var entry = await _service.GetAsync(id);
            if (entry is null)
            {
                return NotFound();
            }
            return Ok(entry);
        }

        [HttpPost]
        public async Task<IActionResult> Post(Conversation data)
        {
            Console.WriteLine("---Post---");
            Console.WriteLine(data);
            data.Messages.Add(new MessageInfo() {
                Message = new Message() {
                    Role = MessageRole.User,
                    Content = "This is a default message",
                }
            });
            await _service.CreateAsync(data);
            return CreatedAtAction(nameof(Get), new { id = data.Id }, data);
        }

        /// <summary>
        /// Add 'Message' to 'Conversation' with provided id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost("{id:length(24)}")]
        public async Task<IActionResult> AddMessage(string id, [FromBody] Message message) {
            await _service.AddMessageAsync(id, message);
            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var entry = await _service.GetAsync(id);

            if (entry is null)
            {
                return NotFound();
            }

            await _service.RemoveAsync(id);
            return NoContent();
        }
    }

}
