using System;
using System.Runtime.Serialization;

namespace CitieZ.Util
{
    internal class WrongArraySizeException : Exception
    {
        public WrongArraySizeException()
        {
        }

        public WrongArraySizeException(string message) : base(message)
        {
        }

        public WrongArraySizeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WrongArraySizeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}