using System.Numerics;

namespace QuixPhysics{
    public class GemShelter : Item
    {
        public override void Instantiate(Room room, Vector3 position)
        {
            var state = new BoxState(){
                instantiate=true,
                position=position,
                halfSize=new Vector3(100),
                type="Quixtam",
                mesh="Objects/Quixtam/Quixtam"
            };
            room.factory.Create(state,room);
        }
    }
}