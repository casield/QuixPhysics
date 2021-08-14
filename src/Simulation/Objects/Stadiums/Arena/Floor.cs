using System;
using System.Collections.Generic;
using System.Numerics;

namespace QuixPhysics
{
    public class Floor : PhyObject
    {
        List<PhyObject> montains = new List<PhyObject>();
        public Vector3 size;
        private Random random;
        Arena arena;
        public Floor()
        {
            random = new Random();
        }

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            size = ((BoxState)state).halfSize * 2;

            arena = (Arena)room.gamemode;

            CreateMountains();
        }

        public void CreateMountains()
        {
            var montainsToCreate = 30;
            for (int i = 0; i < montainsToCreate; i++)
            {
               var mont = RandomMontainInArea(state.position,10000);
               arena.navObjects.Add(mont);
            }
        }
        public PhyObject RandomMontainInArea(Vector3 position, int radius)
        {
            var y = GetTop(100);//change
            var randpos = new Vector3(RandomMinMax(radius), y, RandomMinMax(radius));
            randpos+=position;

           var randomQuat = Quaternion.Multiply(new Quaternion(00.707107f,0,0,-0.707107f),new Quaternion(new Vector3(0,0,1),RandomMinMax(MathF.PI)));
         // var randomQuat = new Quaternion(0.707107f,0,0,-0.707107f);

            var minSize = 30;
            var maxSize = 200;
            var randSize = new Vector3(random.Next(minSize,maxSize),random.Next(minSize,maxSize),random.Next(minSize,maxSize));

            var p = room.Create(new BoxState()
            {
                position = randpos,
                instantiate = true,
                type="QuixBox",
                mesh = "Stadiums/Isla/Montana1",
                isMesh = true,
                quaternion = Quaternion.Normalize(randomQuat),
                mass = 0,
                halfSize = randSize
            });
            return p;
        }
        private float RandomMinMax(float val)
        {
            var r = (float)(random.NextDouble() * random.Next(-(int)val,(int)val));
            QuixConsole.Log("random",r);
            return r;
        }
        public float GetTop(float YHalfSize)
        {
            BoxState box = (BoxState)state;

            return box.position.Y + (box.halfSize.Y / 2) + (YHalfSize / 2);
        }
    }
}