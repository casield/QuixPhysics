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

        private float accelerationY = 0;
        private float velocity = .3f;

        private bool released = true;
        /// <summary>
        /// How much up can LookObject go.
        /// </summary>
        private float maxYAdded = 150;

        float distance = 300;
        public override void BeforeLoad(ObjectState state)
        {
            base.BeforeLoad(state);
            //   ((BoxState)state).halfSize = new Vector3(10, 10, 10);
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
        public void SetLookPosition(Vector3 position)
        {
            if (watching is Player2)
            {
                var newpos = GetAroundPosition();
                SetPosition(Vector3.Lerp(newpos, GetPosition(), .1f));
            }
            else
            {
                SetPosition(Vector3.Lerp(position, GetPosition(), .9f));
            }

        }
        public Vector3 GetAroundPosition()
        {

            var newPos = player.GetPosition();
            var x = -(float)Math.Cos(player.rotationController);
            var y = -(float)Math.Sin(player.rotationController);

            newPos.X += (x * distance);
            newPos.Z += y * distance;

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