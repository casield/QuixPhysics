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

        private StringBuilder builder;

        public ObjectState(){
            builder = new StringBuilder();
        }
        
        internal string getJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
        internal string getPosRot(){
            double[] b = new double[]{position.X,position.Y,position.Z,quaternion.X,quaternion.Y,quaternion.Z,quaternion.W};
            return b.ToString();
        }


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