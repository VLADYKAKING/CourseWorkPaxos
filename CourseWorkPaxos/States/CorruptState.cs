namespace CourseWorkPaxos
{
    public class CorruptState : State
    {
        private State revertState;
        private long reverseTime;
        public override State Execute(Msg msg)
        {
            if (reverseTime <= Paxos.GetTime())
            {
                return revertState;
            }
            return this;
        }
        public CorruptState(RM rm, State storedState, long reverseTime) : base(rm, -1)
        {
            Logger.Debug(rm.GetType().Name + " " + rm.id + " corrupt");
            revertState = storedState;
            this.reverseTime = reverseTime;
        }
    }

}
