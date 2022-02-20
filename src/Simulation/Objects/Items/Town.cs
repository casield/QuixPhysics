using System;
using System.Numerics;

namespace QuixPhysics
{
    public class TownBullet:PhyObject{
        float velocity = 50;
        public static SphereState Build(){
            return new SphereState(){
                radius=5,
                instantiate=true,
                mass=20,
                type="TownBullet"
            };
        }
        public void ShootToDirection(Vector3 direction,float distance){
            QuixConsole.Log("Distance",distance);
            bodyReference.ApplyLinearImpulse((direction*velocity));
        }
    }
    public class Town : Item
    {
        private User user;

        public Town(User user)
        {
            this.user = user;
        }

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            AddWorker(new PhyInterval(5000,simulator)).Completed+=Check;
        }

        private void Check()
        {
            foreach (var user in room.users)
            {
                var distance = Vector3.Distance(user.Value.player.GetPosition(),GetPosition());
                if(distance < 500){
                    Shoot(user.Value.player,distance);
                }
            }
        }

        private void Shoot(Player2 player,float distance)
        {
           var direction = (Vector3.Normalize(player.GetPosition() - GetPosition()));
           TownBullet bullet = (TownBullet)room.factory.Create(TownBullet.Build(),room);
           bullet.SetPosition(GetPosition()+new Vector3(0,GetHeight()+10,0));
           bullet.ShootToDirection(direction,distance);
        }

        public override void Instantiate(Room room, Vector3 position)
        {
            room.factory.Create(BuildPart(position, Quaternion.Identity, user), room, this);
        }

        public static BoxState BuildPart(Vector3 position, Quaternion rotation, User owner)
        {
            float size = 30;
            BoxState state = new BoxState()
            {
                position = position,
                quaternion = rotation,
                halfSize = new Vector3(size, size, size),
                instantiate = true,
                mass = 150,
                owner = owner.sessionId,
                type = "Town_Part"
            };

            return state;
        }

        public override int GetPrice()
        {
            return 10;
        }
    }
}