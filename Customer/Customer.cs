using System;
using Messages;
using System.Threading;
using EasyNetQ;
using System.Text;

namespace Customer
{
    public class Customer
    {
        //static IBus bus = null;
        private int customerID;
        private int productID;
        private string country;
        //private int timeout = 10000;

        public Customer(int customerID, int productID, string country)
        {
            this.customerID = customerID;
            this.productID = productID;
            this.country = country;
        }

        //public void SendRequest()
        //{
        //    CustomerOrderRequestMessage request = new CustomerOrderRequestMessage
        //    {
        //        CustomerId = customerID,
        //        ProductId = productID,
        //        Country = country
        //    };

        //}

        public void Start()
        {
            SynchronizedWriteLine("Customer running. Waiting for a reply.\n");
            CustomerOrderRequestMessages request = new CustomerOrderRequestMessages
            {
                CustomerId = customerID,
                ProductId = productID,
                Country = country
            };

                MessagingGatway gateway = new MessagingGatway(customerID);

                try
                {
                    OrderReplyMessage message = gateway.PlaceOrder(request);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Order reply received by customer:" + customerID);
                    Console.WriteLine("Warehouse Id: " + message.WarehouseId);
                    Console.WriteLine("Order Id: " + message.OrderId);
                    Console.WriteLine("Items in stock: " + message.ItemsInStock);
                    Console.WriteLine("Shipping charge: " + message.ShippingCharge);
                    Console.WriteLine("Days for delivery: " + message.DaysForDelivery);
                    Console.ResetColor();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
        }

        //private void HandleOrderEvent(OrderReplyMessage message)
        //{
        //    StringBuilder reply = new StringBuilder();
        //    reply.Append("Order reply received by customer:" + customerID + "\n");
        //    reply.Append("Warehouse Id: " + message.WarehouseId + "\n");
        //    reply.Append("Order Id: " + message.OrderId + "\n");
        //    reply.Append("Items in stock: " + message.ItemsInStock + "\n");
        //    reply.Append("Shipping charge: " + message.ShippingCharge + "\n");
        //    reply.Append("Days for delivery: " + message.DaysForDelivery + "\n");
        //    SynchronizedWriteLine(reply.ToString());

        //    lock (this)
        //    {
        //        // Wake up the blocked Customer thread
        //        Monitor.Pulse(this);
        //    }
        //}

        private void SynchronizedWriteLine(string s)
        {
            lock (this)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(s);
                Console.ResetColor();
            }
        }

    }
}
