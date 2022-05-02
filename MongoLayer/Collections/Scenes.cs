using MongoDB.Bson.Serialization.Attributes;
using MongoLayer.Infrastructure;

namespace MongoLayer.Collections
{
    public class Scenes
    {
        [BsonRequired] public Guid _id { get; set; }
        [BsonRequired] public Guid WorldId { get; set; }
        [BsonRequired] public Guid ManufacturerId { get; set; }
        [BsonRequired] public I18N Description { get; set; }
    }
}