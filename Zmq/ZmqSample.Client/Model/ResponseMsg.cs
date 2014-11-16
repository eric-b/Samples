using System;
using System.Linq;

namespace ZmqSample.Client.Model
{
    [Serializable]
    public sealed class ResponseMsg : IMessage
    {
        public string Answer { get; set; }

        public ResponseMsg(string answer)
        {
            Answer = answer;
        }

        public override string ToString()
        {
            return Answer;
        }
    }
}