using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public class DataBase
    {
        public IMongoDatabase database;
        private IMongoCollection<MapMongo> maps;


        public DataBase()
        {
            QuixConsole.WriteLine("Init Database");
            MongoClient dbClient = new MongoClient("mongodb://localhost/golf");

            database = dbClient.GetDatabase("golf");
            maps = database.GetCollection<MapMongo>("maps");


            //GetMap("arena");
        }


        public async Task<MapMongo> GetMap(string mapname)
        {
            var filter = new BsonDocument("name", mapname);
            filter.ToJson();

            var find = await maps.FindAsync(filter);
            return await find.FirstAsync<MapMongo>();
        }
    }

    [BsonIgnoreExtraElements]
    public class MapMongo
    {

        public List<BsonDocument> objects { set; get; }
        public BsonArray startPositions { set; get; }
        public string name;
    }
}