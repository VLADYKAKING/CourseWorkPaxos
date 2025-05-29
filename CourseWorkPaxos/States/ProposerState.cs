namespace CourseWorkPaxos
{
    public class ProposerState : State
    {
        public ProposerState(RM rm, int sendedMsgId) : base(rm, sendedMsgId)
        {
        }

        public State EraHasValue(Msg msg)
        {
            if (msg.era < rm.curEra)
            {
                return this;
            }
            rm.curEra++;
            (rm as Proposer).proposeValue = -1;
            return new ProposerWorkState(rm, -1);

        }
        public bool ProcessDiffMsgId(Msg msg)
        {
            //sometime ，other proposer will send step3 message to it
            if (msg.type == Msg.TypeEnum.step3 || msg.type == Msg.TypeEnum.eraHasValue)
            {
                return false;
            }
            if (msg.msgId > sendedMsgId)
            {
                Logger.Error("error,reviced bigger msg id");
                return true;
            }
            else if (msg.msgId < sendedMsgId)
            {
                Logger.Debug("get pre msgId");
                return true;
            }
            return false;
        }
    }

}
