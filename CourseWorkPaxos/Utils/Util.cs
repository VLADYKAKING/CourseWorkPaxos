using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace CourseWorkPaxos
{
    public class Util
    {
        public static Random rnd = new Random(111);

        public static bool debug = false;

        public static long RandSendDelayTime()
        {
            if (debug)
            {
                return 0;
            }
            return rnd.Next(1, 4);
        }

        public static bool RandCorrupt(RM rm)
        {
            if (debug)
            {
                return false;
            }
            return rnd.Next(0, 1000) >= 999;
        }

        public static long RandCorruptTime()
        {
            if (debug)
            {
                return 0;
            }
            return rnd.Next(2, 11);
        }

        public static bool RandDepreciate()
        {
            if (debug)
            {
                return false;
            }
            return rnd.Next(0, 100) > 90;
        }

        public static int RandRgb()
        {
            return rnd.Next(0, 255) * 256 * 256 + rnd.Next(0, 255) * 256 + rnd.Next(0, 255);
        }

        public static bool DisplayUI()
        {
            return true;
        }

        public static bool RandPropose(RM rm)
        {
            if (debug)
            {
                return true;
            }
            return rnd.Next(0, 100) > 88;
        }

        public static T Desearial<T>(byte[] data)
        {
            byte[] bytes = data;
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream(bytes);
            T obj = (T)formatter.Deserialize(stream);
            stream.Close();
            return obj;
        }

        public static byte[] Searial(Object obj)
        {
            byte[] bytes;
            IFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                bytes = stream.ToArray();
            }
            return bytes;
        }

    }

}
