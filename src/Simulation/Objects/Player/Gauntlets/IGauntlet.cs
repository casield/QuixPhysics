using System.Numerics;

namespace QuixPhysics{
    public interface IGauntlet{

        void AddPlayer(Player2 player);
        void Activate(bool active);
        void Swipe(double degree,Vector3 direction);
        void Init();
    }
}