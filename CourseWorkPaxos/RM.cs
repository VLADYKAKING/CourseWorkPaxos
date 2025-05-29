using System.Collections.Generic;

namespace CourseWorkPaxos
{
    public class RM
    {
        public int id;
        //use to draw ui
        public int x, y;
        public Paxos parent;
        public int curEra = 0;
        //rm's color in the ui
        public byte[] rgb = new byte[3];
        //store the rm communicated mostly recently time,value is use to demonstrate the gap time
        public Dictionary<RM, long> connectedRms = new Dictionary<RM, long>();
        public Connector connector = new Connector();
        public List<Msg> receiveMsgHistory = new List<Msg>();
        public List<Msg> senedMsgHistory = new List<Msg>();
        public State state;
        public List<Msg> preReceiveMsgForce;
        public List<Msg> preReceiveMsgFree;

        public void Send(Msg msg, int port)
        {
            if (Util.debug)
            {
                RM source = parent.PortToRM(msg.sourcePort);
                RM target = parent.PortToRM(port);
                if (source is Acceptor && target is Acceptor)
                {
                    Logger.Log("source and target both are acceptor");
                }
                else if (source.id == target.id)
                {
                    Logger.Log("source and target are the same");
                }
            }
            if (connector.Send(msg, port))
            {
                senedMsgHistory.Insert(0, msg);
            }
        }

        public virtual void Act()
        {
            List<Msg> msgs = connector.GetMsgs();
            foreach (Msg msg in msgs)
            {
                connectedRms[parent.PortToRM(msg.sourcePort)] = Paxos.GetTime();
                receiveMsgHistory.Insert(0, msg);
                state = state.Execute(msg);
            }
            state = state.Execute(null);
        }
    }

}
