using System;
using System.Numerics;
using BepuPhysics;

namespace QuixPhysics
{
    public class LookObject : PhyObject
    {
        private Player2 player2;
        public PhyObject watching;

        private float yAdded = 0;
        private static float YAlways = 50;

        private bool released = true;
        /// <summary>
        /// How much up can LookObject go.
        /// </summary>
        private float maxYAdded = 60;
        /// <summary>
        /// How much low can LookObject go.
        /// </summary>
        private float minYAdded = -25;

        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {

            base.Load(bodyHandle, connectionState, simulator, state, guid, room);

            staticReference = GetStaticReference();
        }

        public void SetPlayer(Player2 player2)
        {
            this.player2 = player2;
            watching = player2;


        }

        public void ChangeWatching(PhyObject ob)
        {

            watching.OnDelete -= OnWatchinDeleted;
            if (watching == ob)
            {
                watching = player2;
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
            ChangeWatching(player2);
        }
        public void SetLookPosition(Vector3 position)
        {
            // var newpos = GetPosition();
            position.Y += yAdded + YAlways;
            SetPosition(position);
        }
        public void AddY(float y)
        {
            yAdded += y;
            yAdded = Math.Clamp(yAdded, minYAdded, maxYAdded);
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
            /*if (watching.state.mass != 0)
            {
                SetLookPosition(watching.GetBodyReference().Pose.Position);
            }
            else
            {
                SetLookPosition(watching.GetStaticReference().Pose.Position);
            }*/
            SetLookPosition(watching.GetPosition());
            if (released)
            {
                if (yAdded != 0)
                {
                    yAdded *= .95f;
                }

            }
        }
    }
}