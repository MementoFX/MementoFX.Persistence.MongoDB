using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Memento.Domain;
using Memento.Persistence.MongoDB.Tests.Events;

namespace Memento.Persistence.MongoDB.Tests.Model
{
    public class CurrentAccount : Aggregate
    {
        public decimal Balance { get; private set; }

        public void ApplyEvent(AccountOpenedEvent @event)
        {
            this.Id = @event.CurrentAccountId;
            this.Balance = @event.Balance;
        }

        public void ApplyEvent(WithdrawalEvent @event)
        {
            this.Balance -= @event.Amount;
        }
    }
}
