using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using MongoLayer.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoLayer.Infrastructure
{
    internal static class Database
    {
        public static string ConnectString { get; set; }   // For unit tests
        public static MongoClient Client { get; private set; }
        private static IMongoDatabase _db = null;
        public static IMongoDatabase Db
        {
            get
            {
                _makeDb();
                return _db;
            }
        }

        private static void _makeDb()
        {
            if (_db != null)
                return;  // Already good, though this shouldn't happen

            MongoClient cli;
            IMongoDatabase dbi;
            doOpen(out cli, out dbi);

            Client = cli;
            _db = dbi;
        }

        internal static object _bsonSerial_Object = new object();
        internal static bool _bsonSerial_Registered = false;

        internal static void doOpen(out MongoClient cli, out IMongoDatabase dbi, string constr = null)
        {
            var conn = Environment.GetEnvironmentVariable("MONGO_URI");
            if (conn == null) conn = constr;
            if (!String.IsNullOrWhiteSpace(ConnectString) && conn == null)
                conn = ConnectString;

            BsonDefaults.GuidRepresentation = GuidRepresentation.Standard;

            lock (Database._bsonSerial_Object)
            {
                if (!_bsonSerial_Registered)
                {
                    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
                    BsonSerializer.RegisterSerializer(
                      typeof(decimal),
                       new DecimalSerializer(BsonType.Double,
                       new RepresentationConverter(
                         true, // allow overflow, return decimal.MinValue or decimal.MaxValue instead
                         true //allow truncation
                        ))
                     );
                }
                _bsonSerial_Registered = true;
            }

            //BsonClassMap.RegisterClassMap<InventoryObjects>();

            var mongoUrl = MongoUrl.Create(conn);

            cli = new MongoClient(conn);
            dbi = cli.GetDatabase(mongoUrl.DatabaseName);

            Console.WriteLine($"[Open Mongo Database: {mongoUrl.DatabaseName}]");
        }


        public static IMongoCollection<Avatars> Avatars(this IMongoDatabase db)
        {
            return db.GetCollection<Avatars>("Avatars");
        }
        public static IMongoCollection<Devices> Devices(this IMongoDatabase db)
        {
            return db.GetCollection<Devices>("Devices");
        }
        public static IMongoCollection<Emissaries> Emissaries(this IMongoDatabase db)
        {
            return db.GetCollection<Emissaries>("Emissaries");
        }
        public static IMongoCollection<Manufacturers> Manufacturers(this IMongoDatabase db)
        {
            return db.GetCollection<Manufacturers>("Manufacturers");
        }
        public static IMongoCollection<Scenes> Scenes(this IMongoDatabase db)
        {
            return db.GetCollection<Scenes>("Scenes");
        }
        public static IMongoCollection<Worlds> Worlds(this IMongoDatabase db)
        {
            return db.GetCollection<Worlds>("Worlds");
        }
    }
}
