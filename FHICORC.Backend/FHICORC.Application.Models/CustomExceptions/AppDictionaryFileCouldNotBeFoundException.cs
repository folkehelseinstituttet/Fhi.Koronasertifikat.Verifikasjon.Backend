using System;

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

        protected AppDictionaryFileCouldNotBeFoundException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
        }
    }
}
