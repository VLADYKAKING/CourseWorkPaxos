using System;

namespace CourseWorkPaxos
{
    [Serializable]
    public class Msg
    {
        public int era;
        public int msgId;
        public TypeEnum type;
        public int data;
        public int max1a;//receive max 1a;
        public int max2b;//send max 2b;
        public int sourcePort;

        public enum TypeEnum
        {
            step1a,
            step1bFree,
            step1bForced,
            step2a,
            step2b,
            step3,
            eraHasValue,
            getEraValue
        }


        public Msg(int sourcePort, int era, TypeEnum type, int msgId, int data)
        {
            this.sourcePort = sourcePort;
            this.msgId = msgId;
            this.data = data;
            this.era = era;
            this.type = type;
        }

        override
        public string ToString()
        {
            return string.Format("Msg: {0} {1} {2} {3}", era, type.ToString(), msgId, data);
        }
    }
}
