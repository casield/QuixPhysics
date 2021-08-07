using System;
using System.Numerics;
using BepuPhysics;

namespace QuixPhysics
{
    public class LookTarget : PhyObject
    {
        private PhyInterval timer;
        StaticReference reference;
        float rotation = 0;
        float distance = 152;
        float velocity = 0.1f;
        StaticHandle handle;

        StaticDescription description;

        Vector3 position = new Vector3();

        int destroyTick = 0;
        public override void Load(Handle bodyHandle, ConnectionState connectionState, Simulator simulator, ObjectState state, Guid guid, Room room)
        {
            base.Load(bodyHandle, connectionState, simulator, state, guid, room);
            timer = new PhyInterval(1, simulator);
            reference = GetStaticReference();

            position = reference.Pose.Position;
            timer.Tick += Update;
            handle = bodyHandle.staticHandle;

            QuixConsole.Log("Target",((BoxState)state).halfSize);
            simulator.Simulation.Statics.GetDescription(handle,out description);
        }

        private void Update()
        {
            if (reference.Exists)
            {
                var x = MathF.Cos(rotation) * distance;
                var y = MathF.Sin(rotation) * distance;

                description.Pose.Position.X = position.X + x;
                description.Pose.Position.Z = position.Z + y;
                description.Pose.Position.Y = position.Y + y;

            

                simulator.Simulation.Statics.ApplyDescription(handle,description);

                rotation += .01f * velocity;

                needUpdate = true;

            }

        }

        public override void Destroy()
        {
            base.Destroy();
            timer.Destroy();
        }

        internal override void OnObjectMessage(string data, string clientId, string roomId)
        {
            base.OnObjectMessage(data, clientId, roomId);
            QuixConsole.Log("LookTarget", data, clientId, roomId);
            Vector3 pos = GetStaticReference().Pose.Position;
            room.users[clientId].player.lookObject.ChangeWatching(this);
        }
    }
}