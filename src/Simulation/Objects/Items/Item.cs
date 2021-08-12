namespace QuixPhysics
{
    public interface IItem {
        
        void OnDrop( Room room);
        void OnCollect(User user);

    }

    public delegate void ItemDroppedEvent(Item item);

    public abstract class Item : PhyObject, IItem
    {

        public virtual void OnCollect(User user)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnDrop(Room room)
        {
            Instantiate( room);
            room.gamemode.OnItemDropped(this);
        }
        public abstract void Instantiate(Room room);
    }
}