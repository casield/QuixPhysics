using System;
using System.Numerics;

namespace QuixPhysics
{
    public class GematoriumGem : Item
    {
        public GematoriumGem()
        {

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            room.factory.OnContactListeners.Add(guid, this);
        }
        public override void OnContact<TManifold>(PhyObject obj, TManifold manifold)
        {

            if (obj is GolfBall2)
            {
                Gem gem  = new Gem();
                gem.Drop(room,GetPosition());
               
                var user = room.users[obj.state.owner];
                user.gems.Update(((int)user.gems.value) + 1);
                Destroy();
            }
        }
        public static ObjectState Build(Vector3 position, Quaternion rotation, float mass)
        {
            return new SphereState() { position = position, quaternion = rotation, mass = mass, type = "GematoriumGem", mesh = "Objects/Items/Gem/Gem_prefab", instantiate = true, radius = 10 };
        }

        internal override void OnObjectMessage(string data, string clientId, string roomId)
        {
           // base.OnObjectMessage(data, clientId, roomId);
            var player = room.users[clientId].player;
            player.lookObject.ChangeWatching(this);

        }

        public override void Instantiate(Room room,Vector3 position)
        {
            throw new NotImplementedException();
        }
    }
}