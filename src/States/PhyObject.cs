using System;
using System.Numerics;
using BepuPhysics;

namespace QuixPhysics
{
    public class PhyObject
    {
        public string uID;
        BodyDescription description;

        public PhyObject(BodyDescription description)
        {
            this.description = description;
          

            uID = createUID();
            Console.WriteLine(uID);
        }

        public string createUID()
        {
            var bytes = new byte[5];
            var random = new Random();
            random.NextBytes(bytes);
            var idStr = (Math.Floor((double)(random.Next() * 25)) + 10).ToString() + "_";
            // add a timestamp in milliseconds (base 36 again) as the base
            idStr += (Math.Floor((double)(random.Next() * 25)) + 10).ToString();

            return idStr;
        }
    }
}