using Anthropic;
using ImageMagick;
using Microsoft.AspNetCore.Components.Forms;

namespace MyDemoAPI.Services;

public interface IAnthropicService {
  // Task CreateAsync(ConversationAddForm form);

}

public record FileTest(string FileName, int Page, string Base64);
public class AnthropicService : IAnthropicService
{
  private ImageBlock ConvertStreamToImageBlock(MemoryStream ms, ImageBlockSourceMediaType mediaType) {
    var bytes = ms.ToArray();
    var base64 = Convert.ToBase64String(bytes);
    return new ImageBlock() {
      Type = "image",
      Source = new ImageBlockSource() {
        MediaType = mediaType,
        Type = ImageBlockSourceType.Base64,
        Data = base64,
      }
    };
  }
  private void ConvertImageToBase64(List<ImageBlock> list, IBrowserFile file, ImageBlockSourceMediaType mediaType) {
    using (var ms = new MemoryStream()) {
      file.OpenReadStream(1024 * 1024 * 3).CopyTo(ms);
      list.Add(ConvertStreamToImageBlock(ms, mediaType));
    }
  }

  private void ConvertPDFToImages(List<ImageBlock> list, IBrowserFile file, ImageBlockSourceMediaType mediaType) {
    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
    string fileFullPath = Path.Combine("/tmp", fileName);
    using (var stream = File.Create(fileFullPath)) {
      file
        .OpenReadStream(1024 * 1024 * 3)
        .CopyTo(stream);
    }
    // var settings = new MagickReadSettings
    // {
    //     Density = new Density(300, 300),
    // };
    using (var images = new MagickImageCollection()) {
      images.Read(fileFullPath);
      foreach(var (image, index) in images.Select((x, i) => (x, i))) {
        image.Format = MagickFormat.Jpeg;
        using (var ms = new MemoryStream()) {
          image.Write(ms);
          list.Add(ConvertStreamToImageBlock(ms, mediaType));
        }
      }
    };
    File.Delete(fileFullPath);
  }
  public async Task CreateAsync(ConversationAddForm form)
  {
    // https://www.codeproject.com/Questions/5258080/How-to-convert-PDF-to-image-using-graphicsmagick-i
    var images = form.Files.Aggregate(
      new List<ImageBlock>(),
      (list, file) => {
        ImageBlockSourceMediaType mediaType = file.ContentType switch {
          "image/jpeg" => ImageBlockSourceMediaType.ImageJpeg,
          "image/png" => ImageBlockSourceMediaType.ImagePng,
          "image/webp" => ImageBlockSourceMediaType.ImageWebp,
          "image/gif" => ImageBlockSourceMediaType.ImageGif,
          "application/pdf" => ImageBlockSourceMediaType.ImageJpeg,
          _ => throw new Exception($"{file.ContentType} is not supported...")
        };
        if (file.ContentType == "application/pdf") {
          ConvertPDFToImages(list, file, mediaType);
          return list;
        }
        ConvertImageToBase64(list, file, mediaType);
        return list;
    });
    

    var test = new Message() {
      Role = MessageRole.User,
      Content = new List<Block>() {
        new ImageBlock() {
          Source = new ImageBlockSource() {
            Type = ImageBlockSourceType.Base64,
            Data = "",
            MediaType = ImageBlockSourceMediaType.ImageJpeg
          },
          Type = ""
        }
      }
    };

    var api = new AnthropicApi();
    var response = await api.CreateMessageAsync(new CreateMessageRequest() {
      System = "",
      Model = CreateMessageRequestModel.Claude20,
      Messages = [test],
      MaxTokens = 100,
      Tools = [
        new Tool() {
          Name = "",
          Description = "",
          InputSchema = new ToolInputSchema() {
            
          }
        }
      ]
    });
  }
}
