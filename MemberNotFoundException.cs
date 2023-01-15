// Credit to Bruno Zell @ stack overflow for giving an example on how to use extensions for quickly grabbing private members via reflection.
// https://stackoverflow.com/questions/95910/find-a-private-field-with-reflection

using System;
using System.Runtime.Serialization;

namespace InitialNobles
{
    [Serializable]
    internal class MemberNotFoundException : Exception
    {
        public MemberNotFoundException()
        {
        }

        public MemberNotFoundException(string message) : base(message)
        {
        }

        public MemberNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MemberNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}