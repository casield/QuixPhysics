using System.Diagnostics;
using System.Numerics;

namespace QuixPhysics
{
    public class UserLoader:ArenaAddon
    {
        public UserLoader(Simulator simulator, Arena arena) : base(simulator, arena)
        {
        }

        public void LoadItems(User user)
        {
            //Create Helper

            Entity entity = (Entity)room.factory.Create(new SphereState(){radius=30,//halfSize=new Vector3(100),
            instantiate=true,
            mass=30,
            owner=user.sessionId
            ,type="Helper",
            },room);
            
            entity.Init();
            //Load saved Items
        }
        public override void OnStart()
        {

              foreach (var item in arena.room.users)
            {
                LoadItems(item.Value);
            }
        }
        public override void OnMapsLoaded()
        {
            base.OnMapsLoaded();
          
            
        }

    }
}