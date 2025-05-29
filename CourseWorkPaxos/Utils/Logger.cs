using System;
using System.Runtime.InteropServices;

namespace CourseWorkPaxos
{
    public class Logger
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        private static bool isInit = false;
        private static readonly object loglock = new object();

        public static void Log(Object obj)
        {
            lock (loglock)
            {
                if (!isInit)
                {
                    isInit = true;
                    AllocConsole();
                    Console.WriteLine("open console successfully");
                }
                string res = obj.ToString();
                Console.WriteLine(res);
            }
        }

        public static void Debug(Object obj)
        {
            if (Util.debug)
            {
                Log(obj);
            }
        }

        public static void Error(Object obj)
        {
            Log(obj);
        }
    }

}
