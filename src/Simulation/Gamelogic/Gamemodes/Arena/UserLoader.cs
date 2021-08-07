using System.Numerics;

namespace QuixPhysics
{
    public class UserLoader
    {
        Simulator simulator;
        private Arena arena;

        public UserLoader(Simulator simulator, Arena arena)
        {
            this.simulator = simulator;
            this.arena = arena;
        }

        public void OnEnter(User user)
        {
            //Create Gematorium

            CreateGematorium(user);

            //Load saved Items
        }

        private void CreateGematorium(User user)
        {
            var pos = arena.GetStartPoint(user);
          
            var gematorium = simulator.Create(Gematorium.Build(pos, Quaternion.Identity, user), arena.room);
            gematorium.needUpdate=true;

              QuixConsole.Log("Gematorium",gematorium.state.position);
           /* var gematorium2 = simulator.Create(
                  new BoxState()
                  {
                      position = gematorium.state.position,
                      quaternion = gematorium.state.quaternion,
                      halfSize = ((BoxState)gematorium.state).halfSize,
                      instantiate = true,
                      mass = 0,
            

                      type = "QuixBox"
                  }, arena.room
        );*/
        }
    }
}