﻿namespace PT.PointOne.WebAPI.Models
{
    public enum OrderStatus {
        WAITING_FOR_PAYMENT,
        QUEUED,
        READY,
        POURING,
        ERROR,
        COMPLETE
    }

    public class OrderStatusResponse
    {
        public OrderStatus Status;
        public TapStatus TapStatus; 
        public string RequestId;
        public bool Locked;
        public string Message; 
    }
}
