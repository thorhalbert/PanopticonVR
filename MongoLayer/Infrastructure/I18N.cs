using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoLayer.Infrastructure
{
    public class I18N
    {
        [BsonRequired] public string Invariant { get; set; }
        [BsonExtraElements] public string Values { get; set; }
    }
}
