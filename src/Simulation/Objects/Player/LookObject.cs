using System;
using System.Numerics;
using BepuPhysics;

namespace QuixPhysics
{
    public class LookObject : PhyObject
    {
        private Player2 player2;
        public StaticReference staticReference;

        public PhyObject watching;

        private float yAdded = 0;
        private bool released = true;

        private float YAlways = 50;

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

        private void SetPosition(Vector3 position)
        {
            staticReference.Pose.Position = position;
            staticReference.Pose.Position.Y = staticReference.Pose.Position.Y + yAdded + YAlways;
            needUpdate = true;
        }

        public void AddY(float y)
        {
            yAdded += y;
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
            if (watching.state.mass != 0)
            {
                SetPosition(watching.GetReference().Pose.Position);
            }
            else
            {
                SetPosition(watching.GetStaticReference().Pose.Position);
            }
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