using System;
using System.Numerics;

namespace QuixPhysics
{
    public class TownBullet : PhyObject
    {
        float velocity = 5000;
        int damage = 1;
        bool hasDamage = false;


        public static SphereState Build()
        {
            return new SphereState()
            {
                radius = 5,
                instantiate = true,
                mass = 20,
                type = "TownBullet"
            };
        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            room.factory.OnContactListeners.Add(guid, this);
        }

        public override void OnContact<TManifold>(PhyObject obj, TManifold manifold)
        {
            if (!hasDamage && obj is Player2)
            {
                Player2 player = (Player2)obj;

                player.actionsManager.TakeDamage(damage);
                hasDamage = true;

            }

            DestroyOnTime(100);
        }
        public void ShootToObject(PhyObject obj)
        {
            var direction = (Vector3.Normalize(obj.GetPosition() - GetPosition()));
            ShootToDirection(direction);
        }
        public void ShootToDirection(Vector3 direction)
        {
            bodyReference.ApplyLinearImpulse((direction * velocity));
        }
    }
    public class Town : Item
    {
        private User user;

        private int maxShoots = 3;
        private int shoots = 0;

        public Town(User user)
        {
            this.user = user;
        }

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            AddWorker(new PhyInterval(5000, simulator)).Completed += Check;
        }

        private void Check()
        {
            foreach (var user in room.users)
            {
                var distance = Vector3.Distance(user.Value.player.GetPosition(), GetPosition());
                if (distance < 500)
                {
                    StartShoot(user.Value.player, distance);
                }
            }
        }

        private void StartShoot(Player2 player, float distance)
        {
                TownBullet bullet = (TownBullet)room.factory.Create(TownBullet.Build(), room);
                bullet.SetPosition(GetPosition() + new Vector3(0, GetHeight() + 10, 0));
                bullet.ShootToObject(player);
                shoots++;
                if(shoots==maxShoots){
                    shoots = 0;
                    return;
                }

                AddWorker(new PhyTimeOut(300,simulator,true)).Completed+=()=>{
                    StartShoot(player,distance);
                } ;  
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