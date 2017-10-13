using System;
using EasyNetQ;
using Messages;
using System.Threading;
using System.Collections.Concurrent;

namespace Retailer
{
    public class Retailer
    {
        private string replyQueueName = "replyQueueForRetailerOrderRequestMessage";
        private IBus bus = null;
        private int orderId = 0;


        private ConcurrentDictionary<int, RetailerOrderRequestMessage> outstandingOrderRequests =
            new ConcurrentDictionary<int, RetailerOrderRequestMessage>();


        private ConcurrentDictionary<int, int> customerIdsForOutstandingOrderRequests =
        new ConcurrentDictionary<int, int>();
        public void Start()
        {
            using (bus = RabbitHutch.CreateBus("host=localhost"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Retailer started. Waiting for customer requests\n");
                Console.ResetColor();

                // Listen for order request messages from customers
                bus.Receive<CustomerOrderRequestMessages>("retailerQueue", message => HandleOrderRequest(message));

                // Listen for order reply messages from warehouses
                bus.Receive<OrderReplyMessage>(replyQueueName, message => HandleOrderReplyMessage(message));

                lock (this)
                {
                    // Block this thread so that the Retailer program will not exit.
                    Monitor.Wait(this);
                }
            }
        }

        private void HandleOrderRequest(CustomerOrderRequestMessages request)
        {
            int customerId = request.CustomerId;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Request received from customer " + customerId);
            Console.WriteLine("Trying to send the request to a local warehouse.");
            Console.ResetColor();

            CustomerOrderRequestMessage filteredMessage =
               MessageTransformers.FilterCustomerOrderRequestMessage(request);

            OrderRequestMessageToLocalWarehouse requestMessage =
                MessageTransformers.EnrichOrderRequestMessage(filteredMessage, replyQueueName, ++orderId);

            outstandingOrderRequests.TryAdd(orderId, requestMessage);
            customerIdsForOutstandingOrderRequests.TryAdd(orderId, customerId);
            //OrderRequestMessageToLocalWarehouse requestMessage = new OrderRequestMessageToLocalWarehouse
            //{
            //    ProductId = request.ProductId,
            //    CustomerId = request.CustomerId,
            //    Country = request.Country,
            //    OrderId = ++orderId,
            //    ReplyTo = replyQueueName

            //};

            // Uses Topic Based Routing to send the request to a local warehouse. The topic
            // is requestMessage.Country.
            bus.Publish<OrderRequestMessageToLocalWarehouse>(requestMessage, requestMessage.Country);
        }

        private void HandleOrderReplyMessage(OrderReplyMessage message)
        {
            int customerId;
            Console.WriteLine("Reply received");
            RetailerOrderRequestMessage requestMessage = new RetailerOrderRequestMessage();

            if (message.ItemsInStock > 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Reply received from warehouse" + message.WarehouseId);
                Console.WriteLine("Order Id: " + message.OrderId);
                Console.WriteLine("Items in stock: " + message.ItemsInStock);
                Console.WriteLine("Days for delivery: " + message.DaysForDelivery);
                Console.WriteLine("Shipping charge: " + message.ShippingCharge);
                Console.ResetColor();

                customerIdsForOutstandingOrderRequests.TryGetValue(message.OrderId, out customerId);
                // Uses Topic Based Routing to send the reply to a customer.
                // The topic ís the customerId from the reply message.
                bus.Publish<OrderReplyMessage>(message, customerId.ToString());
            }
            else if (message.DaysForDelivery == 2)
            {
                // Publish the message again to all warehouses, if the reply
                // was from a local warehouse (DaysForDelivery = 2) with no
                // items in stock.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Publishing to all warehouses");
                Console.ResetColor();
                //RetailerOrderRequestMessage m = message as RetailerOrderRequestMessage;
                OrderBroadcastRequestMessage broadcastRequestMessage = new OrderBroadcastRequestMessage
                {
                    OrderId = message.OrderId,
                    ProductId = message.ProductId,
                    ReplyTo = replyQueueName,
                    Country = ""
                };

                bus.Publish<OrderBroadcastRequestMessage>(broadcastRequestMessage);
            }
        }

    }
}
