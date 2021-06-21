using System;
using System.Runtime.Serialization;

namespace FHICORC.Integrations.DGCGateway
{
    [Serializable]
    public class GeneralDgcgFaultException : Exception
    {
        public GeneralDgcgFaultException()
        {

        }
        public GeneralDgcgFaultException(string message)
            : base(message)
        {
        }

        public GeneralDgcgFaultException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected GeneralDgcgFaultException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
}
