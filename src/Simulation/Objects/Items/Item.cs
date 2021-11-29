using System.Numerics;
using Newtonsoft.Json.Linq;

namespace QuixPhysics
{
    public interface IItem
    {

        void Drop(Room room, Vector3 position);
        void OnCollect(User user);

    }

    public interface ItemMessage<T>
    {
        User user { get; set; }
        T data { get; set; }
    }
    public class IM_MovePosition : ItemMessage<Vector3>
    {
        public User user { get; set; }
        public Vector3 data { get; set; }

        public IM_MovePosition(User user, Vector3 position)
        {
            this.user = user;
            this.data = position;
        }
    }

    public delegate void ItemDroppedEvent(Item item);

    public abstract class Item : PhyObject, IItem
    {
        public bool canChangePosition = true;
        public bool isInstantiated = false;
        public virtual void OnCollect(User user)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// This method is use to drop and register in the room onitemdropped event.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="position"></param>
        public virtual void Drop(Room room, Vector3 position)
        {
            Instantiate(room, position);
            isInstantiated = true;
            room.gamemode.OnItemDropped(this);
        }
        public abstract void Instantiate(Room room, Vector3 position);

        internal override void OnObjectMessage(string data, string clientId, string roomId)
        {
            base.OnObjectMessage(data, clientId, roomId);
            JObject par = JObject.Parse(data);
            if (((string)par["type"]) == "move")
            {
                var user = room.users[clientId];
                Vector3 pos = JObject.Parse(((string)par["data"])).ToObject<Vector3>();
                OnItemMessage<Vector3>(new IM_MovePosition(user, pos));
            }

        }
        /// <summary>
        /// Gets the price of this item
        /// </summary>
        /// <returns></returns>
        public virtual int GetPrice(){
            return 0;
        }
        internal virtual void OnItemMessage<T>(ItemMessage<T> message)
        {

        }
        public override void Destroy()
        {
            base.Destroy();
            isInstantiated = false;
        }
    }
}