using System;

namespace Lykke.Service.IndexHedgingEngine.Domain.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException()
            : base("Entity not found")
        {
        }
    }
}
