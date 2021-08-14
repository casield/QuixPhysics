using System;
using System.Diagnostics;
using System.Text;

namespace QuixPhysics
{

    public class QuixConsole
    {
        private static bool isActive = true;
        private static Random _random = new Random();
        private static bool showReference = false;
        public static void Log(params object[] array)
        {
            if (isActive)
            {

                if (showReference)
                {
                    StackTrace stackTrace = new StackTrace();
                    System.Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("[" + stackTrace.GetFrame(1).GetMethod().Name + "] ");
                }
                System.Console.ForegroundColor = ConsoleColor.White;
                for (int a = 0; a < array.Length; a++)
                {


                    if (a > 0)
                    {
                        System.Console.ForegroundColor = GetConsoleColor(a);
                        System.Console.Write(" ");


                    }
                    if(array[a]==null){
                         System.Console.Write("null");
                    }else{
                         System.Console.Write(array[a].ToString());
                    }
                   

                }
                System.Console.WriteLine("");
                System.Console.ResetColor();
            }
        }
        public static void WriteLine(object obj)
        {
            if (isActive)
            {
                System.Console.WriteLine(obj);
            }

        }
        private static ConsoleColor GetRandomConsoleColor()
        {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor));
            return GetConsoleColor(_random.Next(consoleColors.Length));
        }
        private static ConsoleColor GetConsoleColor(int index)
        {
            var consoleColors = Enum.GetValues(typeof(ConsoleColor));
            if (index >= consoleColors.Length)
            {
                index = 0;
            }
            var color = (ConsoleColor)consoleColors.GetValue(index);
            return color.ToString().Contains("Black") ? ConsoleColor.DarkRed : color;
        }

    }


}