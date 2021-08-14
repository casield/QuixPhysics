using System.Numerics;

namespace QuixPhysics{
    public class Gem : Item
    {
        public override void Instantiate( Room room,Vector3 position)
        {
           // room.
           SphereState sp = new SphereState(){instantiate=true,mass=1,radius=10,type="Gem",mesh="Objects/Items/Gem/Gem_prefab",position =position};
           room.factory.Create(sp,room,this);
           
        }
    }
}