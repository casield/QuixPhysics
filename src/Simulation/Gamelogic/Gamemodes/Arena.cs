using System.Collections.Generic;

namespace QuixPhysics
{
    public class Arena : Gamemode
    {
        public List<User> users = new List<User>();
        private int MIN_USERS = 1;
        public Arena(Simulator simulator) : base(simulator)
        {
        }
        public override void OnJoin(User user)
        {
            users.Add(user);
            if(users.Count==MIN_USERS){
                Start();
            }
        }
        public override void Start()
        {
            
        }

    }
}