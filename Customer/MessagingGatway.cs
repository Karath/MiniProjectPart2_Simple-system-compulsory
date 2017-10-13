using EasyNetQ;
using Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Customer
{
    class MessagingGatway
    {
        IBus bus = RabbitHutch.CreateBus("host=localhost");
        OrderReplyMessage reply = null;
        int timeout = 10000;

        public MessagingGatway(int customerId)
        {
            // Listen to reply messages from the Retailer
            bus.Subscribe<OrderReplyMessage>("customer" + customerId,
            HandleOrderEvent, x => x.WithTopic(customerId.ToString()));
        }

        public OrderReplyMessage PlaceOrder(CustomerOrderRequestMessage request)
        {
            bool gotReply;

            // Send an order request message to the Retailer
            bus.Send<CustomerOrderRequestMessage>("retailerQueue", request);
            lock (this)
            {
                // Block the thread until a reply is received from the Retailer
                gotReply = Monitor.Wait(this, timeout);
            }

            if (gotReply)
                return reply;
            else
                throw new Exception("Timeout. The requested product is out of stock!");
        }

        private void HandleOrderEvent(OrderReplyMessage message)
        {
            reply = new OrderReplyMessage
            {
                //CustomerId = message.CustomerId,
                WarehouseId = message.WarehouseId,
                OrderId = message.OrderId,
                ItemsInStock = message.ItemsInStock,
                ShippingCharge = message.ShippingCharge,
                DaysForDelivery = message.DaysForDelivery
            };

            lock (this)
            {
                // Wake up the blocked thread which sent the order request message 
                // so that it can return the reply message to the application.
                Monitor.Pulse(this);
            }
        }

    }
}