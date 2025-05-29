namespace CourseWorkPaxos
{
    public class Proposer : RM
    {
        public int proposeValue = -1;
        public Proposer()
        {
            state = new ProposerWorkState(this, -1);
        }
    }

}
