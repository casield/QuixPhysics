using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using OVars;

namespace QuixPhysics
{

    public struct RoomInfo
    {
        public Vector3 position;
        public string roomId;
        public int maxPlayers;
    }
    public class Room
    {

        public RoomInfo props;
        internal Simulator simulator;
        public Gamemode gamemode;

        public Dictionary<string, User> users = new Dictionary<string, User>();
        internal MapMongo map;

        public PhyObjectFactory factory;
        public OVarManager oVarManager;
        public ConnectionState connectionState;

        public Room(Simulator simulator, RoomInfo info, ConnectionState connectionState)
        {
            this.props = info;
            this.simulator = simulator;
            factory = new PhyObjectFactory(this);
            oVarManager = new OVarManager(this);
            this.connectionState = connectionState;
            //simulator.createObjects(this);
            SetGameMode(new Arena(simulator, this));


        }

        public void SetGameMode(Gamemode gamemode)
        {
            this.gamemode = gamemode;
            this.gamemode.Init();
        }
        public void AddUser(User user)
        {
            users.Add(user.sessionId, user);
        }

        public PhyObject Create(ObjectState state)
        {
            return factory.Create(state, this);
        }

        public void Dispose()
        {
            QuixConsole.Log("Room disposed");
            factory.Dispose();
            connectionState.Dispose();
            simulator.roomManager.RoomLeave(this);
        }
    }
}