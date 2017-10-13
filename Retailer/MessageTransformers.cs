using Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Retailer
{
    class MessageTransformers
    {
        public static CustomerOrderRequestMessage FilterCustomerOrderRequestMessage(CustomerOrderRequestMessage message)
        {
            return new CustomerOrderRequestMessage
            {
                ProductId = message.ProductId,
                Country = message.Country
            };
        }

        public static OrderRequestMessageToLocalWarehouse EnrichOrderRequestMessage(
           CustomerOrderRequestMessage message, string replyTo, int orderId)
        {
            return new OrderRequestMessageToLocalWarehouse
            {
                ProductId = message.ProductId,
                Country = message.Country,
                OrderId = orderId,
                ReplyTo = replyTo
            };
        }
    }
}
