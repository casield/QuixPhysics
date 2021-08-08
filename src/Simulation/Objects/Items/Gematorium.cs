using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;

namespace QuixPhysics
{

    public class FlyinGem
    {
        public GematoriumGem gem;
        public StaticDescription description;
        public float direction;
        public float rotation;
        public float rotationY;
        public Vector3 position;
        public float velocity;

    }
    public class Gematorium : PhyObject
    {

        public List<FlyinGem> gems = new List<FlyinGem>();

        public int initialGems = 3;

        private Random random = new Random();
        private int maxDistanceGems = 70;
        public static float SIZE = 160;

        public Gematorium()
        {

        }
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {


            base.Load(bodyHandle, connectionState, simulator, state, guid, room);

            for (int a = 0; a < initialGems; a++)
            {
                var pos = new Vector3(random.Next(-maxDistanceGems, maxDistanceGems), random.Next(-maxDistanceGems, maxDistanceGems), random.Next(-maxDistanceGems, maxDistanceGems));
                pos = Vector3.Add(state.position, pos);
                pos.Y += ((BoxState)state).halfSize.Y;
                var gem = GematoriumGem.Build(pos, Quaternion.Identity, 0);

                GematoriumGem phygem = (GematoriumGem)simulator.Create(gem, room);
                StaticDescription description;

                float velocity = ((float)(random.Next(-10, 10)) / 3000);
                velocity = velocity != 0 ? velocity : -0.001f;
                // QuixConsole.Log("Velocity",velocity);
                simulator.Simulation.Statics.GetDescription(phygem.bodyHandle.staticHandle, out description);
                FlyinGem flyinGem = new FlyinGem() { direction = 1, gem = phygem, description = description, position = pos, velocity = velocity };


                gems.Add(flyinGem);
                phygem.OnDelete += OnGemDeleted;

            }

            AddWorker(new PhyInterval(1, simulator)).Tick += Update;
        }

        private void OnGemDeleted(PhyObject obj)
        {
            var gem = gems.Find((FlyinGem g) => { return g.gem == obj; });
            gems.Remove(gem);
        }

        private void Update()
        {
            if (!simulator.Disposed)
            {
                foreach (var gem in gems)
                {

                    float distance = maxDistanceGems;
                    var newPos = gem.position;
                    var x = -(float)Math.Cos(gem.rotation);
                    var z = -(float)Math.Sin(gem.rotation);
                    var y = (float)Math.Cos(gem.rotationY);

                    newPos.X += x * distance;
                    newPos.Z += z * distance;
                    newPos.Y += y * distance / 3;
                    gem.description.Pose.Position = newPos;

                    simulator.Simulation.Statics.ApplyDescription(gem.gem.bodyHandle.staticHandle, gem.description);
                    gem.rotation += gem.velocity;
                    gem.rotationY += 0.001f;
                    gem.gem.needUpdate = true;
                }
            }else{
                QuixConsole.Log("Simulator disposed");
            }

        }

        public static BoxState Build(Vector3 position, Quaternion rotation, User owner)
        {
            float size = Gematorium.SIZE;
            BoxState state = new BoxState()
            {
                position = position,
                quaternion = rotation,
                halfSize = new Vector3(size, size, size),
                instantiate = true,
                mass = 0,
                mesh = "Objects/Items/Gematorium",
                owner = owner.sessionId,
                type = "Gematorium"
            };

            return state;
        }

    }
}