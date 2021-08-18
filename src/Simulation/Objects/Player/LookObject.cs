using System;
using System.Numerics;
using BepuPhysics;

namespace QuixPhysics
{
    public class LookObject : PhyObject
    {
        private Player2 player;
        public PhyObject watching;

        private float yMessage = 0;
        private float yAdded = 0;

        private float acceleration = 0;
        private float velocity = .02f;

        private bool released = true;
        /// <summary>
        /// How much up can LookObject go.
        /// </summary>
        private float maxYAdded = 380;

        float distance = 300;
        public override void BeforeLoad(ObjectState state)
        {
            base.BeforeLoad(state);
            ((BoxState)state).halfSize = new Vector3(10, 10, 10);
        }

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {

            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            simulator.collidableMaterials[bodyHandle.staticHandle].collidable = false;

            //  staticReference = GetStaticReference();
        }

        public void SetPlayer(Player2 player2)
        {
            this.player = player2;
            watching = player2;


        }

        public void ChangeWatching(PhyObject ob)
        {

            watching.OnDelete -= OnWatchinDeleted;
            if (watching == ob)
            {
                watching = player;
                SendObjectMessage("player");

            }
            else
            {
                watching = ob;

                SendObjectMessage("target");


            }
            watching.OnDelete += OnWatchinDeleted;

        }

        private void OnWatchinDeleted(PhyObject obj)
        {
            ChangeWatching(player);
        }
        float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }
        public void SetLookPosition(Vector3 position)
        {
            if (watching is Player2)
            {
                var newpos = GetAroundPosition();
                SetPosition(Vector3.Lerp(newpos, GetPosition(), .9f));
            }
            else
            {
                SetPosition(Vector3.Lerp(position, GetPosition(), .9f));
            }

        }
        public Vector3 GetAroundPosition()
        {
            float sep = 0;
            var newPos = player.GetPosition();
            var x = -(float)Math.Cos(player.rotationController + sep);
            var y = -(float)Math.Sin(player.rotationController + sep);

            newPos.X += (x * distance);
            newPos.Z += y * distance;

            if (yMessage != 0)
            {
                acceleration += velocity * yMessage;

                //  acceleration = 
            }
            else
            {

                acceleration *= .9f;
            }
            yAdded += acceleration;

            yAdded = Math.Clamp(yAdded, -(maxYAdded), maxYAdded * .5f);
            newPos.Y -= yAdded;


            return newPos;
        }
        public void AddY(float y)
        {
            yMessage = y;
        }

        public void Lock()
        {
            released = false;
        }
        public void Release()
        {
            released = true;
        }

        internal void Update()
        {
            SetLookPosition(watching.GetPosition());
        }
    }
}