namespace QuixPhysics{
    public class Floor:PhyObject {
        public Floor(){

        }
        public float GetTop(float YHalfSize){
            BoxState box = (BoxState)state;

            return box.position.Y+(box.halfSize.Y/2)+(YHalfSize/2);
        }
    }
}