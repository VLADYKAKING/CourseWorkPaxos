using System;
using System.Collections.Generic;

namespace CourseWorkPaxos
{
    public class AcceptorState : State
    {
        //initial received1aMsgId is -1
        public int participateMaxId = -1;
        public int received1aMsgId = -1;
        public bool hasSended2bMsg = false;
        public int sended2bMsgId = -1;
        public int sended2bMsgValue = -1;
        public Msg accept2aMsg = null;
        public AcceptorState(RM rm) : base(rm, -1)
        {
        }

        public void conflict2bMsgValue(int oldvalue, int newvalue)
        {
            if (oldvalue == newvalue)
            {
                return;
            }
            else
            {
                List<RM> rms = rm.parent.GetAllRM();
                int cnt = 0;
                List<RM> cntrm = new List<RM>();
                foreach (RM r in rms)
                {
                    if (r.state is AcceptorState)
                    {
                        AcceptorState s = r.state as AcceptorState;
                        if (s.sended2bMsgValue == oldvalue)
                        {
                            cnt++;
                            cntrm.Add(r);
                        }
                        if (cnt > rm.parent.GetAllAcceptor().Count / 2)
                        {
                            Logger.Log("error!conflict value");
                        }
                    }
                }

                return;
            }
        }

        public State Step1a(Msg msg)
        {
            if (msg.msgId >= participateMaxId)
            {
                Msg sendMsg = new Msg(rm.connector.localPort, rm.curEra, Msg.TypeEnum.step1bFree, msg.msgId, -1);
                sendMsg.max1a = received1aMsgId;
                if (hasSended2bMsg)
                {
                    sendMsg.type = Msg.TypeEnum.step1bForced;
                    sendMsg.max2b = sended2bMsgId;
                    sendMsg.data = sended2bMsgValue;
                }
                rm.Send(sendMsg, msg.sourcePort);
                received1aMsgId = Math.Max(received1aMsgId, msg.msgId);
                participateMaxId = Math.Max(participateMaxId, msg.msgId);
            }
            else
            {
                WillnotReplyMsg(msg);
            }
            return this;
        }

        public State Step2a(Msg msg)
        {
            if (msg.msgId >= participateMaxId)
            {
                Msg sendMsg = new Msg(rm.connector.localPort, rm.curEra, Msg.TypeEnum.step2b, msg.msgId, -1);
                sendMsg.max1a = received1aMsgId;
                if (hasSended2bMsg)
                {
                    conflict2bMsgValue(sended2bMsgValue, msg.data);
                    sendMsg.max2b = sended2bMsgId;
                    //sendMsg.data = sended2bMsgValue;
                }
                rm.Send(sendMsg, msg.sourcePort);
                accept2aMsg = msg;
                participateMaxId = Math.Max(participateMaxId, msg.msgId);
                hasSended2bMsg = true;
                sended2bMsgId = Math.Max(sended2bMsgId, msg.msgId);
                sended2bMsgValue = msg.data;
            }
            else
            {
                WillnotReplyMsg(msg);
            }
            return this;
        }

        public State Step3(Msg msg)
        {
            if (msg.era < rm.curEra)
            {
                return this;
            }
            rm.curEra++;
            return new AcceptorState(rm);
        }

        public State EraHasValue(Msg msg)
        {
            if (msg.era < rm.curEra)
            {
                return this;
            }
            rm.curEra++;
            return new AcceptorState(rm);
        }

        override
        public State Execute(Msg msg)
        {
            if (msg == null)
            {
                //simulate corrupt
                if (Util.RandCorrupt(rm))
                {
                    CorruptState state = new CorruptState(rm, this, Util.RandCorruptTime() + Paxos.GetTime());
                    return state;
                }
            }
            else
            {
                if (ProcessDiffEraMsg(msg))
                {
                    return this;
                }
                State res = this;
                switch (msg.type)
                {
                    case Msg.TypeEnum.eraHasValue:
                        res = EraHasValue(msg);
                        break;
                    case Msg.TypeEnum.getEraValue:
                        res = GetEraValue(msg);
                        break;
                    //case Msg.typeEnum.get1aMax:
                    //    res = get1aMax(msg);
                    //    break;
                    case Msg.TypeEnum.step1a:
                        res = Step1a(msg);
                        break;
                    case Msg.TypeEnum.step2a:
                        res = Step2a(msg);
                        break;
                    case Msg.TypeEnum.step3:
                        res = Step3(msg);
                        break;
                    default:
                        res = DebugProcessRemainMsg(msg);
                        break;
                }
                return res;
            }
            return this;
        }
    }

}
