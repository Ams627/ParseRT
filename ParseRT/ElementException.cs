using System;
using System.Runtime.Serialization;

namespace ParseRT
{
    [Serializable]
    internal class ElementException : Exception
    {
        public ElementException()
        {
        }

        public ElementException(string message) : base(message)
        {
        }

        public ElementException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ElementException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}