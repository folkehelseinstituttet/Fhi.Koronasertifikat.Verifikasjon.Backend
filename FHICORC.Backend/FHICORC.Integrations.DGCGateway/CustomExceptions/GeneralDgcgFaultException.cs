using System;

namespace FHICORC.Integrations.DGCGateway
{
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
    }
}
