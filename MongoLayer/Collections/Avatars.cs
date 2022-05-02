using MongoDB.Bson.Serialization.Attributes;
using MongoLayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoLayer.Collections
{
    internal class Avatars
    {
        [BsonRequired] public Guid _id { get; set; }
        [BsonRequired] public Guid ManufacturerId { get; set; }
        [BsonRequired] public I18N Name { get; set; }
    }
}
