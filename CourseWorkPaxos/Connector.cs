using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CourseWorkPaxos
{
    public class Connector
    {
        static private ConcurrentPriorityQueue<long, MsgPacket> msgs = new ConcurrentPriorityQueue<long, MsgPacket>();

        public bool Send(Msg msg, int port)
        {
            if (!Util.RandDepreciate())
            {
                MsgPacket packet = new MsgPacket(msg, Paxos.GetTime() + Util.RandSendDelayTime(), false, port);
                var item = new KeyValuePair<long, MsgPacket>(packet.time, packet);
                msgs.Enqueue(item);
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool isStartConsume = false;
        public static void StartConsumeThread()
        {
            if (isStartConsume)
            {
                return;
            }
            isStartConsume = true;
            ThreadStart job = new ThreadStart(Consume);
            Thread thread = new Thread(job);
            thread.Start();
        }

        public static void StartConsumeThread(int nThead)
        {
            if (isStartConsume)
            {
                return;
            }
            isStartConsume = true;
            for (int i = 0; i < nThead; i++)
            {
                ThreadStart job = new ThreadStart(Consume);
                Thread thread = new Thread(job);
                thread.Start();
            }
        }

        public static bool ConsumeCurrent()
        {
            bool isConsumeSomeTask = false;
            for (; ; )
            {
                KeyValuePair<long, MsgPacket> res = new KeyValuePair<long, MsgPacket>();
                if (msgs.TryDequeue(out res))
                {
                    MsgPacket msg = res.Value;
                    if (msg.time > Paxos.GetTime())
                    {
                        msgs.Enqueue(res);
                        return isConsumeSomeTask;
                    }
                    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    IPAddress broadcast = IPAddress.Parse("127.0.0.1");
                    IPEndPoint ep = new IPEndPoint(broadcast, msg.targetPort);
                    s.SendTo(Util.Searial(msg.msg), ep);
                    s.Close();
                    isConsumeSomeTask = true;
                }
                else
                {
                    return isConsumeSomeTask;
                }
            }
        }

        private static void Consume()
        {
            for (; ; )
            {
                KeyValuePair<long, MsgPacket> res = new KeyValuePair<long, MsgPacket>();
                if (msgs.TryDequeue(out res))
                {
                    MsgPacket msg = res.Value;
                    if (msg.time > Paxos.GetTime())
                    {
                        msgs.Enqueue(res);
                        continue;
                    }
                    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    IPAddress broadcast = IPAddress.Parse("127.0.0.1");
                    IPEndPoint ep = new IPEndPoint(broadcast, msg.targetPort);
                    s.SendTo(Util.Searial(msg.msg), ep);
                }

            }
        }

        private IPEndPoint groupEP;
        private UdpClient listener;
        public int localPort;
        public Connector()
        {
            listener = new UdpClient(0);
            localPort = ((IPEndPoint)listener.Client.LocalEndPoint).Port;
            groupEP = new IPEndPoint(IPAddress.Any, localPort);
        }

        public List<Msg> GetMsgs()
        {
            List<Msg> msgs = new List<Msg>();
            while (listener.Available > 0)
            {
                msgs.Add(Util.Desearial<Msg>(listener.Receive(ref groupEP)));
            }
            return msgs;
        }
    }

}
