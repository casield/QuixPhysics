namespace QuixPhysics{
    /// <summary>
    /// This HelperItem makes the Helper find dropped gems
    /// </summary>
    public class HI_GemFraction : HelperItem
    {
        public HI_GemFraction(Helper helper) : base(helper)
        {
        }

        public override void Activate()
        {
        }

        public override void Instantiate( Room room)
        {
            throw new System.NotImplementedException();
        }

        public override bool OnLastPolygon()
        {
            return true;
        }

        public override void OnTrailActive()
        {
            
        }

        public override void OnTrailInactive()
        {
            
        }

        public override void Update()
        {
            //Look for gems around if cant see any wonder around looking for more
            //After 3 tries end use
        }
    }
}