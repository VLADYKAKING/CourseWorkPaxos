using System.Collections.Generic;

namespace CourseWorkPaxos
{
    public class ProposerWait2bState : ProposerState
    {
        int sendedValue;
        List<Msg> step2bMsg = new List<Msg>();
        public ProposerWait2bState(RM rm, int sendedMsgId, int sendedValue) : base(rm, sendedMsgId)
        {
            this.sendedValue = sendedValue;
        }
        public State Step2b(Msg msg)
        {
            step2bMsg.Add(msg);
            if (GetDistinctSource(step2bMsg) > rm.parent.GetAllAcceptor().Count / 2)
            {
                //send receive value
                Msg sendMsg = new Msg(rm.connector.localPort, rm.curEra, Msg.TypeEnum.step3, sendedMsgId, sendedValue);
                List<RM> rms = rm.parent.GetAllRM();
                rm.parent.DeterminEraMsg(rm.curEra, sendedValue, rm);
                foreach (RM i in rms)
                {
                    rm.Send(sendMsg, i.connector.localPort);
                }
                rm.curEra++;
                //becareful, new round's msgid is init
                //return new ProposerWorkState(rm, sendedMaxMsgId);
                return new ProposerWorkState(rm, -1);
            }
            else
            {
                return this;
            }
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
                if (createTime + rm.parent.GetWait2bMaxTime() < Paxos.GetTime())
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
                    case Msg.TypeEnum.step2b:
                        res = Step2b(msg);
                        break;
                    case Msg.TypeEnum.eraHasValue:
                        res = EraHasValue(msg);
                        break;
                    case Msg.TypeEnum.step1bForced:
                    case Msg.TypeEnum.step1bFree:
                        //discard the delay msg
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
