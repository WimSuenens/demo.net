using System.ComponentModel.DataAnnotations;
using Anthropic;
using ImageMagick;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MyDemoAPI.Data;
using MyDemoAPI.Data.Models;

namespace MyDemoAPI.Services;

// https://docs.anthropic.com/en/docs/about-claude/models
public class AIModel {
  public string Label { get; set; }
  public string Name { get; set; }

  public AIModel(string label, string name)
  {
    Label = label;
    Name = name;
  }

  public static List<AIModel> GetOptions() => new List<AIModel>() {
    new AIModel("Claude 3 Haiku", "claude-3-haiku-20240307"),  
    new AIModel("Claude 3 Sonnet", "claude-3-sonnet-20240229"),
    new AIModel("Claude 3 Opus	", "claude-3-opus-20240229"),
    new AIModel("Claude 3.5 Sonnet", "claude-3-5-sonnet-20240620"),
  };
    // https://docs.anthropic.com/en/docs/about-claude/models

}


public class ConversationAddForm {
  [Required(ErrorMessage = "You need to select a AI-model!")]
  public string Model { get; set; } = "claude-3-haiku-20240307";

  [Required(AllowEmptyStrings = false, ErrorMessage = "This is a required field!")]
  // [StringLength(8, ErrorMessage = "Name length can't be more than 8.")]
  public string Message { get; set; } = string.Empty;

  public IReadOnlyList<IBrowserFile> Files = new List<IBrowserFile>();
}

public interface IConversationService {
  Task<List<Conversation>> GetAsync();
  Task<Conversation> GetAsync(string id);
  Task<string> CreateAsync(Conversation data);
  Task<Conversation> CreateAsync(ConversationAddForm form);
  Task RemoveAsync(string id);
  Task AddMessageAsync(string id, Message message);
}
public class ConversationService: IConversationService
{
  // private readonly IMongoClient _client;
  private readonly IMongoCollection<Conversation> _collection;

  public ConversationService(IOptions<MongoDbSettings> settings)
  {
    MongoUrl mongoUrl = new MongoUrl(settings.Value.ConnectionString);
    var client = new MongoClient(mongoUrl);
    var database = client.GetDatabase(settings.Value.DatabaseName);
    _collection = database.GetCollection<Conversation>(settings.Value.CollectionName);
  }

  public async Task<List<Conversation>> GetAsync()
  {
    return await _collection
      .Find(new BsonDocument())
      .ToListAsync();
  }

  public async Task<Conversation> GetAsync(string id) {
    return await _collection
      .Find(x => x.Id == id)
      .FirstOrDefaultAsync();
  }
  public async Task<string> CreateAsync(Conversation data) {
    // var database = _client.GetDatabase("AI");
    // var coll = database.GetCollection<Conversation>("Conversations");
    await _collection.InsertOneAsync(data);
    return data.Id!;
  }

  private ImageBlock ConvertStreamToImageBlock(byte[] bytes, ImageBlockSourceMediaType mediaType) {
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
  private async Task<Block> ConvertImageToBase64(IBrowserFile file, ImageBlockSourceMediaType mediaType) {
    using (var ms = new MemoryStream()) {
      await file.OpenReadStream(1024 * 1024 * 3).CopyToAsync(ms);
      var bytes = ms.ToArray();
      return ConvertStreamToImageBlock(bytes, mediaType);
    }
  }

  private async Task<List<Block>> ConvertPDFToImages(IBrowserFile file) {
    var list = new List<Block>();
    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
    string fileFullPath = Path.Combine("/tmp", fileName);
    using (var stream = File.Create(fileFullPath)) {
      await file
        .OpenReadStream(1024 * 1024 * 3)
        .CopyToAsync(stream);
    }
    var settings = new MagickReadSettings
    {
        Density = new Density(300, 300),
    };
    using (var images = new MagickImageCollection()) {
      images.Read(fileFullPath, settings);
      images
        .Select((x, i) => (x, i))
        .Aggregate(
          list,
          (list, next) => {
            var (image, index) = next;
            image.Format = MagickFormat.Jpeg;
            using (var ms = new MemoryStream()) {
              image.Write(ms);
              var bytes = ms.ToArray();
              list.Add(ConvertStreamToImageBlock(bytes, ImageBlockSourceMediaType.ImageJpeg));
            }
            return list;
          }
        );
    };
    File.Delete(fileFullPath);
    return list;
  }

  public async Task<Conversation> CreateAsync(ConversationAddForm form) {
    // https://www.codeproject.com/Questions/5258080/How-to-convert-PDF-to-image-using-graphicsmagick-i
    var images = new List<Block>();
    foreach (var file in form.Files) {
      if (file.ContentType == "application/pdf") {
        images.AddRange(await ConvertPDFToImages(file));
      } else {
        ImageBlockSourceMediaType mediaType = file.ContentType switch {
          "image/jpeg" => ImageBlockSourceMediaType.ImageJpeg,
          "image/png" => ImageBlockSourceMediaType.ImagePng,
          "image/webp" => ImageBlockSourceMediaType.ImageWebp,
          "image/gif" => ImageBlockSourceMediaType.ImageGif,
          // "application/pdf" => ImageBlockSourceMediaType.ImageJpeg,
          _ => throw new Exception($"{file.ContentType} is not supported...")
        };
        images.Add(await ConvertImageToBase64(file, mediaType));
      }
    }
    // var images = form.Files.Aggregate(
    //   new List<Block>(),
    //   (list, file) => {
    //     return list;
    // });
    var content = new List<Block>();
    content.AddRange(images);
    content.Add(new TextBlock() { Type = "text", Text = form.Message });
    var messages = new List<MessageInfo>
    {
        new MessageInfo() {
          Message = new Message() { Role = MessageRole.User, Content = content }
        }
    };
    var conversation = new Conversation() {
      Title = "TODO - title generation",
      Messages = messages
    };
    var newId = await CreateAsync(conversation);
    return conversation;
  }

  public async Task RemoveAsync(string id)
  {
    await _collection.DeleteOneAsync(x => x.Id == id);
  }

  public async Task AddMessageAsync(string id, Message message)
  {
    var entry = await GetAsync(id);
    if (entry is null) return;
    entry.Messages.Add(new MessageInfo() {
      Message = message
    });
    await _collection.ReplaceOneAsync(x => x.Id == id, entry);
  }
}
