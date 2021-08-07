namespace QuixPhysics
{
    public interface IItem {
        
        void OnDrop();
        void OnCollect(User user);

    }

    public class Item : PhyObject, IItem
    {

        public virtual void OnCollect(User user)
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnDrop()
        {
            throw new System.NotImplementedException();
        }
    }
}