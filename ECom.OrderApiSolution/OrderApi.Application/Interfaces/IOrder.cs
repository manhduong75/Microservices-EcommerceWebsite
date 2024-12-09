using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ECom.SharedLibrary.Interface;
using OrderApi.Domain.Entities;

namespace OrderApi.Application.Interfaces
{
    public interface IOrder: IGenericInterface<Order>
    { 
        Task<IEnumerable<Order>> GetOrdersAsync(Expression<Func<Order, bool>> predicate);
    }
}
