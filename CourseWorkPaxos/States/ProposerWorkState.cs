using System.Collections.Generic;

namespace CourseWorkPaxos
{
    public class ProposerWorkState : ProposerState
    {
        public ProposerWorkState(RM rm, int sendedMsgId) : base(rm, sendedMsgId)
        {
        }

        private State TryStart1a()
        {
            if (Util.RandPropose(rm) == false)
            {
                return this;
            }
            int nextMsgId = (sendedMsgId / 10 + 1) * 10 + rm.id;
            Msg sendMsg = new Msg(rm.connector.localPort, rm.curEra, Msg.TypeEnum.step1a, nextMsgId, -1);
            List<Acceptor> acceptors = rm.parent.GetAllAcceptor();
            foreach (Acceptor i in acceptors)
            {
                rm.Send(sendMsg, i.connector.localPort);
            }
            ProposerWait1bState res = new ProposerWait1bState(rm, nextMsgId);
            return res;
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
                if (createTime + rm.parent.GetWaitWorkMaxTime() < Paxos.GetTime())
                {
                    return new ProposerWorkState(rm, sendedMsgId);
                }
                return TryStart1a();
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
                    case Msg.TypeEnum.getEraValue:
                        res = GetUnreasonableMsg(msg);
                        break;
                    case Msg.TypeEnum.eraHasValue:
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
