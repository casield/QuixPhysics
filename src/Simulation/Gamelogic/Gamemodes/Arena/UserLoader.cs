using System.Diagnostics;
using System.Numerics;

namespace QuixPhysics
{
    /// <summary>
    /// 
    /// </summary>
    public class UserLoader : ArenaAddon
    {

        public UserLoader(Simulator simulator, Arena arena) : base(simulator, arena)
        {
        }

        public void LoadUser(User user)
        {
            user.player.SetPositionToStartPoint();
            user.gems.Update(30);
            //Create Helper

            for (int i = 0; i < 1; i++)
            {
                Entity entity = (Entity)room.factory.Create(new SphereState()
                {
                    radius = 30,//halfSize=new Vector3(100),
                    instantiate = true,
                    mass = 30,
                    owner = user.sessionId,
                  //  mesh = "Stadiums/Isla/Helper/Helper"
                    type = "Helper",
                }, room);

                entity.Init();
            }
            //Load saved Items
        }
        public override void OnStart()
        {

           /* foreach (var item in arena.room.users)
            {
                LoadUser(item.Value);
            }*/
        }
        public override void OnMapsLoaded()
        {
            base.OnMapsLoaded();

            
        }

    }
}