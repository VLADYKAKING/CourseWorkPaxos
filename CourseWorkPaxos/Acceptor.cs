namespace CourseWorkPaxos
{
    public class Acceptor : RM
    {
        public Acceptor()
        {
            state = new AcceptorState(this);
        }
    }

}
