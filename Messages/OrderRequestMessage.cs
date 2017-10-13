namespace Messages
{
    public class CustomerOrderRequestMessage
    {
        public int ProductId { get; set; }
        public string Country { get; set; }
    }

    public class RetailerOrderRequestMessage : CustomerOrderRequestMessage
    {
        public int OrderId { get; set; }
        public string ReplyTo { get; set; }
    }

    public class CustomerOrderRequestMessages : CustomerOrderRequestMessage
    {
        public int CustomerId;
    }


    public class OrderRequestMessageToLocalWarehouse : RetailerOrderRequestMessage
    {
    }

    public class OrderBroadcastRequestMessage : RetailerOrderRequestMessage
    {
    }
}
