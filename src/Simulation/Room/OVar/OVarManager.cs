using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using QuixPhysics;

namespace OVars
{
    public class OVarManager
    {

        private Simulator simulator;
        private Dictionary<string, OVar> oVars = new Dictionary<string, OVar>();
        private static int MAX_OVARS = 10;
        private int added = 0;

        public OVarManager(Simulator _simulator)
        {
            simulator = _simulator;


        }

        internal void OnUpdate(OVarMessage m)
        {
           oVars[m.i].value = m.v;
        }

        internal OVar AddedInGolf(string name, object defaultValue)
        {
            OVar newO = new OVar(name,defaultValue, this);
            AddToDictionary(newO);
            return newO;
        }
        internal void AddToDictionary(OVar ovar){
            if(!oVars.ContainsKey(ovar.name)){
               oVars.Add(ovar.name,ovar); 
            }
            
        }

        internal void UpdateInGolf(OVar oVar)
        {
            JObject j = new JObject();
            //{a="up",i=oVar.name,v=oVar.value}
            j.Add("a","up");
            j.Add("i",oVar.name);
            j.Add("v",new JValue(oVar.value));
            
            simulator.SendMessage("OVar", j, simulator.connectionState.workSocket);
        }
    }
}