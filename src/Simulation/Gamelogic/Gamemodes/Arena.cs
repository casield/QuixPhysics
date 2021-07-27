using System.Collections.Generic;
using System.Numerics;

namespace QuixPhysics
{
    public class Arena : Gamemode
    {
        public List<User> users = new List<User>();
        private int MIN_USERS = 1;
        private Room room;

        public Arena(Simulator simulator, Room room) : base(simulator)
        {
            this.room = room;
        }
        public override void OnJoin(User user)
        {
            users.Add(user);
            if (users.Count == MIN_USERS)
            {
                Start();
            }
        }

        public override Vector3 GetStartPoint(User user)
        {
            if (room.map != null)
            {

                var index = users.IndexOf(user);
                if (room.map.startPositions.Count > index)
                {
                    var point = new Vector3((float)room.map.startPositions[index].AsBsonDocument["x"].AsDouble,
                        (float)room.map.startPositions[index].AsBsonDocument["y"].AsDouble,
                             (float)room.map.startPositions[index].AsBsonDocument["z"].AsDouble);
                    return point;
                }
                else
                {
                     var point = new Vector3((float)room.map.startPositions[0].AsBsonDocument["x"].AsDouble,
                        (float)room.map.startPositions[0].AsBsonDocument["y"].AsDouble,
                             (float)room.map.startPositions[0].AsBsonDocument["z"].AsDouble);
                    return point;
                }

            }
            else
            {
                QuixConsole.Log("GetStartPoint");
                return base.GetStartPoint(user);
            }

        }
        public override void Start()
        {
            GenerateMapCommand command = new GenerateMapCommand(simulator);

            command.GenerateMap("arena", room);
            QuixConsole.Log("Start");
            simulator.createObjects(room);
            foreach (var item in users)
            {
                item.player.SetPositionToStartPoint();
            }
        }

    }
}