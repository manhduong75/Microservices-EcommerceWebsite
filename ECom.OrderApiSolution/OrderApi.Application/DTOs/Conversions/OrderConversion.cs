using OrderApi.Domain.Entities;

namespace OrderApi.Application.DTOs.Conversions
{
    public static class OrderConversion
    {
        public static Order ToEntity(OrderDTO order) => new ()
        {
            Id = order.Id,
            ProductId = order.ProductId,
            ClientId = order.ClientId,
            OrderedDate = order.OrderedDate,
            PurchaseQuantity = order.PurchaseQuantity
        };

        public static (OrderDTO?, IEnumerable<OrderDTO>?) FromEntity(Order? order, IEnumerable<Order>? orders)
        {
            // Return single

            if (order is not null || orders is null)
            {
                var singleOrder = new OrderDTO
                    (order!.Id,
                    order.ClientId!,
                    order.ProductId,
                    order.PurchaseQuantity,
                    order.OrderedDate);
                return (singleOrder, null);
            }

            // Return list
            if (orders is not null || order is null)
            {
                var _orders = orders!.Select(o =>
                    new OrderDTO(
                    o!.Id,
                    o.ClientId!,
                    o.ProductId,
                    o.PurchaseQuantity,
                    o.OrderedDate));

                return (null, _orders);
            }

            return (null, null);
        }
    }
}
