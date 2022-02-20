using System.Numerics;

namespace QuixPhysics
{
    public class Town : Item
    {
        private User user;

        public Town(User user)
        {
            this.user = user;
        }

        public override void Instantiate(Room room, Vector3 position)
        {
            room.factory.Create(BuildPart(position, Quaternion.Identity, user), room, this);
        }

        public static BoxState BuildPart(Vector3 position, Quaternion rotation, User owner)
        {
            float size = 30;
            BoxState state = new BoxState()
            {
                position = position,
                quaternion = rotation,
                halfSize = new Vector3(size, size, size),
                instantiate = true,
                mass = 150,
                owner = owner.sessionId,
                type = "Town_Part"
            };

            return state;
        }

        public override int GetPrice()
        {
            return 10;
        }
    }
}