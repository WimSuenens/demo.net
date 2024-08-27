using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Anthropic;
using ImageMagick;
using PDFiumSharp;

namespace MyDemoAPI.Controllers
{
    public class AnthropicCreateDto {

    }

    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AnthropicController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> Get() {
            // Settings the density to 300 dpi will create an image with a better quality
            var settings = new MagickReadSettings
            {
                Density = new Density(300, 300)
            };
            using var images = new MagickImageCollection();
            using (var stream = new MemoryStream()) {
                images.Read(stream, settings);
            }
            foreach(var (image, index) in images.Select((x, i) => (x, i))) {
                image.Format = MagickFormat.Jpeg;
                using (var stream = new MemoryStream()) {
                    image.Write(stream);
                    var bytes = stream.ToArray();
                    var base64 = Convert.ToBase64String(bytes);
                }
            }

            await Task.CompletedTask;
            return "Hello Anthropic";
        }

        [HttpPost]
        public async Task<Message> Post(AnthropicCreateDto data) {
            var client = new AnthropicApi("TODO");
            var response = await client.CreateMessageAsync(
                model: CreateMessageRequestModel.Claude3Haiku20240307,
                messages: [

                ],
                maxTokens: 250
            );
            var val1 = response.Content.Value1;
            var val2 = response.Content.Value2;
            return response;
        }
    }
}
