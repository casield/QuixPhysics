using System;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public class ObjectState
    {
        
        public Vector3 position{get;set;}
        public Quaternion quaternion {get;set;}
        public string uID {get;set;}
        public string mesh {get;set;}
        public string type {get;set;}
        public float mass {get;set;}
        public bool instantiate {get;set;}
        public string owner {get;set;}
        

        internal string getJson(){
            return JsonConvert.SerializeObject(this);
        }
    }

    public class UserState{
        public string sessionId;
        public int gems;
    }
    public class BoxState:ObjectState
    {
        public Vector3 halfSize {get;set;}
    }
}