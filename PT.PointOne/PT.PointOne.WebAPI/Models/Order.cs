﻿using System;

namespace PT.PointOne.WebAPI.Models
{
    public enum TapStatus { Waiting, Pour, Pouring, Poured, Error };

    public class Order
    {
        public string ProductId;
        public string RequestId;
        public string OrderId;
        public DateTime Created;
        public DateTime? Poured;
        public OrderStatus Status;
        public TapStatus TapStatus; 
        public bool Paid;
        public string UserId;
        public double Price;
    }
}