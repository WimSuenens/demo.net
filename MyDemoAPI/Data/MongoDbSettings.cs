using Anthropic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using MyDemoAPI.Data.Models;

namespace MyDemoAPI.Data;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string CollectionName { get; set; } = null!;
}

public static class MongoDbExtension {
    public static void SetupMongoDbContext(this IServiceCollection services, IConfiguration configuration) {
        
        services.Configure<MongoDbSettings>(configuration.GetSection("MongoDbSettings"));

        // https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/serialization/class-mapping/
        // https://stackoverflow.com/questions/66980035/get-id-of-an-inserted-document-in-mongodb-when-using-bsonclassmap
        // https://mongodb.github.io/mongo-csharp-driver/2.12/reference/bson/mapping/#id-generators
        // https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/serialization/class-mapping/#manually-creating-a-class-map
        // TODO:  https://stackoverflow.com/questions/13838931/how-can-i-tell-the-mongodb-c-sharp-driver-to-store-all-guids-in-string-format

        BsonSerializer.RegisterSerializer(new EnumSerializer<MessageRole>(BsonType.String));

        BsonSerializer.RegisterSerializer(typeof(OneOf<string?, IList<Block>>), new OneOfBsonSerializer());

        BsonClassMap.RegisterClassMap<Message>(map => {

            // map.AutoMap();

            // map.MapIdField(x => x.Id)
            //     .SetSerializer(new StringSerializer(BsonType.ObjectId))
            //     .SetIdGenerator(StringObjectIdGenerator.Instance)

            map
                .MapProperty(x => x.Role)
                .SetElementName("role");

            map
                .MapProperty(x => x.Content)
                .SetElementName("content");

            map
                .MapProperty(x => x.Model)
                .SetElementName("model")
                .SetIgnoreIfNull(true);

            // map
            //     .MapProperty(x => x.StopReason)
            //     .SetElementName("stop_reason");

            map
                .MapProperty(x => x.StopSequence)
                .SetElementName("stop_sequence")
                .SetIgnoreIfNull(true);

            map
                .MapProperty(x => x.Type)
                .SetElementName("type")
                .SetIgnoreIfNull(true);

            map.SetIgnoreExtraElements(true);

        });

        BsonClassMap.RegisterClassMap<MessageInfo>(map => {

            // map.MapIdField(x => x.Id)
            //     .SetSerializer(new StringSerializer(BsonType.ObjectId))
            //     .SetIdGenerator(StringObjectIdGenerator.Instance);

            map
                .MapProperty(x => x.Id)
                .SetElementName("_id")
                .SetDefaultValue(Guid.NewGuid())
                .SetSerializer(new GuidSerializer(BsonType.String));


            map
                .MapProperty(x => x.Message)
                .SetElementName("message");

            map
                .MapProperty(x => x.Created)
                .SetElementName("created")
                .SetDefaultValue(DateTime.Now)
                .SetSerializer(new DateTimeSerializer(BsonType.DateTime))
                .SetIsRequired(true);
        
        });

        BsonClassMap.RegisterClassMap<Conversation>(map => {
            
            // map.AutoMap();

            map.MapIdField(x => x.Id)
                .SetSerializer(new StringSerializer(BsonType.ObjectId))
                .SetIdGenerator(StringObjectIdGenerator.Instance);

            // map
            //     .MapProperty(x => x.Created)
            //     .SetElementName("created")
            //     .SetDefaultValue(DateTime.UtcNow)
            //     .SetSerializer(new DateTimeSerializer(BsonType.DateTime));

            map
                .MapProperty(x => x.Title)
                .SetElementName("title")
                .SetIgnoreIfNull(true);

            map
                .MapProperty(x => x.Messages)
                .SetElementName("messages");

            // ignores extra elements present in the database, but not on the object. 'false' will throw an exception
            map.SetIgnoreExtraElements(true);

        });


    }
}

// https://stackoverflow.com/questions/48581535/mongodb-driver-unable-cast-custom-serializer-to-ibsonserializer-during-filter-by
public class OneOfBsonSerializer : IBsonSerializer<OneOf<string?, IList<Block>>>
{
    public Type ValueType => typeof(OneOf<string?, IList<Block>>);

    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
    {
        if (value.GetType() == ValueType) Serialize(context, args, (OneOf<string?, IList<Block>>) value);
        // if (value is OneOf<string?, IList<Block>> valueObject) Serialize(context, args, valueObject);
    }
    public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, OneOf<string?, IList<Block>> value)
    {
        if (value.Object is string str) BsonSerializer.Serialize(context.Writer, str.ToString());
        if (value.Object is IList<Block> list) BsonSerializer.Serialize(context.Writer, list);
        // if (value.IsValue1) BsonSerializer.Serialize<string>(context.Writer, value.Value1.ToString());
        // if (value.IsValue2) BsonSerializer.Serialize<IList<Block>>(context.Writer, value.Value2);
    }
    object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) => (OneOf<string?, IList<Block>>) Deserialize(context, args);
    public OneOf<string?, IList<Block>> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        return (context.Reader.CurrentBsonType is BsonType.String)
            ? BsonSerializer.Deserialize<string>(context.Reader)
            : BsonSerializer.Deserialize<List<Block>>(context.Reader);
    }

}

