using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoLayer.Collections
{
    internal class Devices
    {
        [BsonRequired] public Guid _id { get; set; }
    }
}
