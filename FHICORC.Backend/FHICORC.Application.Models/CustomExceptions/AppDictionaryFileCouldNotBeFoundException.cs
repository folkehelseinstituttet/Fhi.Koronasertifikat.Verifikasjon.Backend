using System;
using System.Runtime.Serialization;

namespace FHICORC.Application.Models.CustomExceptions
{
    [Serializable]
    public class AppDictionaryFileCouldNotBeFoundException : Exception
    {
        public AppDictionaryFileCouldNotBeFoundException()
        {
        }

        public AppDictionaryFileCouldNotBeFoundException(string message)
            : base(message)
        {
        }

        public AppDictionaryFileCouldNotBeFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected AppDictionaryFileCouldNotBeFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
