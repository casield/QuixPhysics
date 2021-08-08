using System.Numerics;

namespace QuixPhysics
{
    public interface IGamemode
    {
        string name { get; set; }
        User winner { get; set; }
        int gameTime { get; set; }

        bool started { get; set; }

        void OnJoin(User user);
        void Start();
        void Finish();
        void Pause();
        void Update();
        Vector3 GetStartPoint(User user);

    }
    public abstract class Gamemode : IGamemode
    {
        internal Simulator simulator;
        internal Room room;

        public string name { get; set; }
        public User winner { get; set; }
        public bool started { get; set; }
        public int gameTime { get; set; }

        public Gamemode(Simulator simulator, Room room)
        {
            this.simulator = simulator;
            this.room = room;
        }

        public virtual void Finish()
        {

        }

        public virtual void OnJoin(User user)
        {

        }

        public virtual void Pause()
        {

        }

        public virtual void Start()
        {

        }

        public virtual void Update()
        {

        }

        public virtual Vector3 GetStartPoint(User user)
        {
            if (room.map != null)
            {
                var index = 0;
                var x =float.Parse(room.map.startPositions[index].AsBsonDocument["x"].ToString());
                var y =float.Parse(room.map.startPositions[index].AsBsonDocument["y"].ToString());
                var z =float.Parse(room.map.startPositions[index].AsBsonDocument["z"].ToString());
                var point = new Vector3(x,y,z);
                return point;
            }
            else
            {
                QuixConsole.Log("GetStartPoint");
                return new Vector3(0, 1000, 0);
            }
        }
    }
}