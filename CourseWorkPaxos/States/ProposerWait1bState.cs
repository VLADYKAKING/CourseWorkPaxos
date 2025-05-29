using System;
using System.Collections.Generic;

namespace CourseWorkPaxos
{
    public class ProposerWait1bState : ProposerState
    {
        List<Msg> freemsg = new List<Msg>();
        List<Msg> forcedmsg = new List<Msg>();
        public ProposerWait1bState(RM rm, int sendedMsgId) : base(rm, sendedMsgId)
        {
        }

        private State Step1b(Msg msg)
        {
            if (msg.type == Msg.TypeEnum.step1bForced)
            {
                forcedmsg.Add(msg);
            }
            else if (msg.type == Msg.TypeEnum.step1bFree)
            {
                freemsg.Add(msg);
            }
            else
            {
                Logger.Log("error no step1b message");
            }

            if (GetDistinctSource(freemsg, forcedmsg) > rm.parent.GetAllAcceptor().Count / 2)
            {
                rm.preReceiveMsgForce = forcedmsg;
                rm.preReceiveMsgFree = freemsg;
                if (forcedmsg.Count == 0)
                {
                    //random set value
                    if ((rm as Proposer).proposeValue == -1)
                    {
                        (rm as Proposer).proposeValue = Util.RandRgb();
                    }
                    Msg sendMsg = new Msg(rm.connector.localPort, rm.curEra, Msg.TypeEnum.step2a, sendedMsgId, (rm as Proposer).proposeValue);
                    List<Acceptor> acceptors = rm.parent.GetAllAcceptor();
                    foreach (Acceptor i in acceptors)
                    {
                        rm.Send(sendMsg, i.connector.localPort);
                    }
                    ProposerWait2bState res = new ProposerWait2bState(rm, sendedMsgId, (rm as Proposer).proposeValue);
                    return res;
                }
                else
                {
                    int u = 0;
                    foreach (Msg m in forcedmsg)
                    {
                        u = Math.Max(u, m.max2b);
                    }
                    int v = -1;
                    foreach (Msg m in forcedmsg)
                    {
                        if (u == m.max2b)
                        {
                            if (v == -1 || v == m.data)
                            {
                                v = m.data;
                            }
                            else
                            {
                                Logger.Log("error, step1bForced has mutil value" + rm.ToString() + " msg:" + msg.ToString());
                            }
                        }
                    }
                    //send receive value
                    Msg sendMsg = new Msg(rm.connector.localPort, rm.curEra, Msg.TypeEnum.step2a, sendedMsgId, v);
                    List<Acceptor> acceptors = rm.parent.GetAllAcceptor();
                    foreach (Acceptor i in acceptors)
                    {
                        rm.Send(sendMsg, i.connector.localPort);
                    }
                    ProposerWait2bState res = new ProposerWait2bState(rm, sendedMsgId, v);
                    return res;
                }
            }
            return this;
        }

        public override State Execute(Msg msg)
        {
            if (msg == null)
            {
                //simulate corrupt
                if (Util.RandCorrupt(rm))
                {
                    CorruptState state = new CorruptState(rm, this, Util.RandCorruptTime() + Paxos.GetTime());
                    return state;
                }
                if (createTime + rm.parent.GetWait1bMaxTime() < Paxos.GetTime())
                {
                    return new ProposerWorkState(rm, sendedMsgId);
                }
                return this;
            }
            else
            {
                if (ProcessDiffEraMsg(msg) || ProcessDiffMsgId(msg))
                {
                    return this;
                }
                State res = this;
                switch (msg.type)
                {
                    case Msg.TypeEnum.step1bFree:
                    case Msg.TypeEnum.step1bForced:
                        res = Step1b(msg);
                        break;
                    case Msg.TypeEnum.eraHasValue:
                        res = EraHasValue(msg);
                        break;
                    case Msg.TypeEnum.getEraValue:
                        res = GetUnreasonableMsg(msg);
                        break;
                    case Msg.TypeEnum.step3:
                        res = EraHasValue(msg);
                        break;
                    default:
                        res = DebugProcessRemainMsg(msg);
                        break;
                }
                return res;
            }
        }
    }

}
