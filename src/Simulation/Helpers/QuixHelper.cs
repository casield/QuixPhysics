using System.Text;

namespace QuixPhysics
{

    public class Console
    {
        public static void Log(params object[] array)
        {
            StringBuilder builder = new StringBuilder();
            for (int a = 0; a < array.Length; a++)
            {
                if (a > 0)
                {
                    System.Console.Write(" ");
                    System.Console.ForegroundColor = System.ConsoleColor.Red;

                }
                System.Console.Write(array[a]);
                builder.Append(array[a]);




            }
            System.Console.WriteLine("");
            System.Console.ResetColor();
        }
        public static void WriteLine(object obj)
        {
            System.Console.WriteLine(obj);
        }
    }

}