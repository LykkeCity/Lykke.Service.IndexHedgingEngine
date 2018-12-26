using System;

namespace Lykke.Service.IndexHedgingEngine.Domain.Exceptions
{
    public class ExchangeException : Exception
    {
        public ExchangeException(string message)
            : base(message)
        {
        }

        public ExchangeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
