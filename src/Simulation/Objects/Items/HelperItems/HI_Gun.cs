using System.Numerics;

namespace QuixPhysics{
    /// <summary>
    /// Prototype gun.
    /// It's activated if any enemy entity is near the vision of this Helper.
    /// It throws a bomb and then desactivate.
    /// </summary>
    public class HI_Gun : HelperItem
    {
        public HI_Gun(Helper helper) : base(helper)
        {
        }

        public override void Activate()
        {
            throw new System.NotImplementedException();
        }

        public override void Instantiate(Room room, Vector3 position)
        {
            throw new System.NotImplementedException();
        }

        public override void OnFall()
        {
            throw new System.NotImplementedException();
        }

        public override bool OnLastPolygon()
        {
            throw new System.NotImplementedException();
        }

        public override void OnStuck()
        {
            throw new System.NotImplementedException();
        }

        public override void OnTrailActive()
        {
            throw new System.NotImplementedException();
        }

        public override void OnTrailInactive()
        {
            throw new System.NotImplementedException();
        }

        public override bool ShouldActivate()
        {
            throw new System.NotImplementedException();
        }
    }
}