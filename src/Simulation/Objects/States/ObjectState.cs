using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    [BsonIgnoreExtraElements]
    public class ObjectState
    {

        public Vector3 position { get; set; }
        public Quaternion quaternion { get; set; }
        public string uID { get; set; }

        public bool isMesh {get;set;} 
        public string mesh { get; set; }
        public string type { get; set; }
        public float mass { get; set; }
        public bool instantiate { get; set; }
        public string owner { get; set; }
        public PhyBoundBox boundBox {get;set;}

        internal string getJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
    public class PhyBoundBox{
        public Vector3 center;
        public Vector3 extents;
    }
    public class BoxState : ObjectState
    {
        public Vector3 halfSize { get; set; }
    }

    public class SphereState : ObjectState
    {
        public float radius { get; set; }
    }

}