namespace OVars
{
    public class OVar
    {
        public OVarManager oVarManager;
        public object value;
        public object defaultValue;
        public string name;
        public OVar(string name,object _defaultValue,OVarManager _oVarManager){
            oVarManager= _oVarManager;
            value = _defaultValue;
            defaultValue = _defaultValue;
            SetName(name);
            oVarManager.AddToDictionary(this);
        }
        public void SetName(string name){
            this.name = name;
        }
        public void Update(object newValue){
            value = newValue;
            oVarManager.UpdateInGolf(this);
        }
    }
}