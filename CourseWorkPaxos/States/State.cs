using System.Collections.Generic;

namespace CourseWorkPaxos
{
    public class State
    {
        public RM rm;
        public int sendedMsgId;
        public long createTime;

        public State(RM rm, int sendedMsgId)
        {
            this.sendedMsgId = sendedMsgId;
            createTime = Paxos.GetTime();
            this.rm = rm;
        }

        public State GetUnreasonableMsg(Msg msg)
        {
            Logger.Debug("unreasonable message");
            return this;
        }

        public virtual State Execute(Msg msg)
        {
            return this;
        }

        public void WillnotReplyMsg(Msg msg)
        {
            Logger.Debug("do not reply id:" + (rm.parent.PortToRM(msg.sourcePort)).id + " with " + msg);
        }

        protected virtual bool ProcessDiffEraMsg(Msg msg)
        {
            if (rm.curEra != msg.era)
            {
                if (rm.curEra < msg.era)
                {
                    Msg sendMsg = new Msg(rm.connector.localPort, rm.curEra, Msg.TypeEnum.getEraValue, sendedMsgId, -1);
                    rm.Send(sendMsg, msg.sourcePort);
                }
                else if (rm.curEra > msg.era)
                {
                    //prevent infinite loop
                    if (msg.type == Msg.TypeEnum.eraHasValue)
                    {
                        return true;
                    }
                    Msg sendMsg = new Msg(rm.connector.localPort, msg.era, Msg.TypeEnum.eraHasValue, sendedMsgId, rm.parent.GetEraValue(msg.era));
                    rm.Send(sendMsg, msg.sourcePort);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public State GetEraValue(Msg msg)
        {
            if (msg.era >= rm.curEra)
            {
                Logger.Log("error rm " + rm.ToString() + " didn't has ear data:" + msg.ToString());
            }
            else
            {
                Msg sendMsg = new Msg(rm.connector.localPort, msg.era, Msg.TypeEnum.eraHasValue, sendedMsgId, rm.parent.GetEraValue(msg.era));
                rm.Send(sendMsg, msg.sourcePort);
            }
            return this;
        }

        protected int GetDistinctSource(List<Msg> msg)
        {
            SortedSet<int> filter = new SortedSet<int>();
            foreach (Msg it in msg)
            {
                filter.Add(it.sourcePort);
            }
            return filter.Count;
        }

        protected int GetDistinctSource(List<Msg> msg, List<Msg> msg2)
        {
            SortedSet<int> filter = new SortedSet<int>();
            foreach (Msg it in msg)
            {
                filter.Add(it.sourcePort);
            }
            foreach (Msg it in msg2)
            {
                filter.Add(it.sourcePort);
            }
            return filter.Count;
        }

        protected State DebugProcessRemainMsg(Msg msg)
        {
            RM sourceRm = rm.parent.PortToRM(msg.sourcePort);
            Logger.Debug("some error happen," + rm.ToString() + " can't process msg:" + msg.ToString());
            Logger.Debug("source rm is: " + sourceRm.ToString());
            return this;
        }
    }

}
