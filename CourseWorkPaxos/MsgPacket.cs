using System;

namespace CourseWorkPaxos
{
    public class MsgPacket : IComparable
    {

        private long uid;
        public long time;
        public int targetPort;
        public Msg msg;
        public MsgPacket(Msg msg, long time, bool depreciated, int targetPort)
        {
            lock (seedlock)
            {
                this.msg = msg;
                this.time = time;
                this.targetPort = targetPort;
                uid = seed++;
            }
        }

        private static object seedlock = new object();
        private static long seed = 0;

        public int CompareTo(object y)
        {
            if (y is MsgPacket)
            {
                MsgPacket cur = (y as MsgPacket);
                return time == cur.time ?
                    (uid < cur.uid ? -1 : 1) : (time < cur.time ? -1 : 1);
            }
            return 1;
        }
    }

}
