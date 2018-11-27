using System;

namespace Lykke.Service.IndexHedgingEngine.Domain.Exceptions
{
    public class BalanceOperationException : Exception
    {
        public BalanceOperationException(string message, int code)
            : base(message)
        {
            Code = code;
        }

        public int Code { get; }
    }
}
