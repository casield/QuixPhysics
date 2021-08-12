namespace QuixPhysics{
    public class Gem : Item
    {
        public override void Instantiate( Room room)
        {
           // room.
           SphereState sp = new SphereState(){instantiate=true,mass=1,radius=10,type="Gem",mesh="Objects/Items/Gem/Gem_prefab"};
           room.factory.Create(sp,room,this);
        }
    }
}