using System.Diagnostics;
using System.Numerics;

namespace QuixPhysics
{
    public class UserLoader:ArenaHelper
    {
        public UserLoader(Simulator simulator, Arena arena) : base(simulator, arena)
        {
        }

        public void LoadItems(User user)
        {
            //Create Gematorium

            //arena.navObjects.Add(CreateGematorium(user));
            CreateGematorium(user);

            //Load saved Items
        }

        private Gematorium CreateGematorium(User user)
        {
            var pos = arena.GetStartPoint(user);
            pos.Y = floor.GetTop(Gematorium.SIZE);
            pos.X += 300;
            var gembox = Gematorium.Build(pos, Quaternion.Identity, user);
            var gematorium = room.Create(gembox);

            QuixConsole.Log("Gematorium",gematorium.state.position);
            return (Gematorium)gematorium;
        }

    }
}