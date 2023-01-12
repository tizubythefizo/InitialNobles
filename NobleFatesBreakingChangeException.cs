using System;
using System.Runtime.Serialization;

namespace InitialNobles
{
    [Serializable]
    public class NobleFatesBreakingChangeException : Exception
    {
        public NobleFatesBreakingChangeException()
        {
        }

        public NobleFatesBreakingChangeException(string message) : base(message)
        {
        }

        public NobleFatesBreakingChangeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NobleFatesBreakingChangeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}