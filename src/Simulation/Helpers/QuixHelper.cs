using System;
using System.Text;

namespace QuixPhysics
{

    public class QuixConsole
    {
        private static bool isActive = true;
        private static Random _random = new Random();
        public static void Log(params object[] array)
        {
            if(isActive){

            
   
            for (int a = 0; a < array.Length; a++)
            {
                if (a > 0)
                {
                    System.Console.Write(" ");
                    System.Console.ForegroundColor = GetConsoleColor(a);

                }
                System.Console.Write(array[a]);
                
            }
            System.Console.WriteLine("");
            System.Console.ResetColor();
            }
        }
        public static void WriteLine(object obj)
        {
            if(isActive){
                System.Console.WriteLine(obj);
            }
            
        }
        private static ConsoleColor GetRandomConsoleColor()
        {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor));
            return GetConsoleColor(_random.Next(consoleColors.Length));
        }
        private static ConsoleColor GetConsoleColor(int index){
              var consoleColors = Enum.GetValues(typeof(ConsoleColor));
              if(index >= consoleColors.Length){
                  index = 0;
              }
            var color = (ConsoleColor)consoleColors.GetValue(index);
            return color.ToString().Contains("Black")?ConsoleColor.DarkRed:color;
        }

    }


}