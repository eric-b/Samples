using System;
using System.Linq;

namespace ZmqSample.Client.Model
{
    [Serializable]
    public sealed class RequestMsg : IMessage
    {
        public RequestMsg(string question)
        {
            Question = question;
        }

        public string Question { get; set; }

        public override string ToString()
        {
            return Question;
        }
    }
}