using MongoDB.Bson.Serialization.Attributes;
using MongoLayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoLayer.Collections
{
    internal class Worlds
    {
        [BsonRequired] public Guid _id { get; set; }
        [BsonRequired] public Guid ManufacturerId { get; set; }
        [BsonRequired] public I18N Description { get; set; }
    }
}
