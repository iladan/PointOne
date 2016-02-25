﻿using PT.PointOne.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using System.Security;
using System.Configuration;
using IOTHubInterface.Models;

namespace PT.PointOne.WebAPI.Controllers
{
    [RoutePrefix("Order")]
    public class OrderController : ApiController
    {
        public static List<Order> orders = new List<Order>();
        public static double OrderCount 
        {
            get
            {
                return orders.Count;
            }            
        }
        private bool Locked
        {
            get
            {
                return orders.Any(k => k.Status == OrderStatus.READY || k.Status == OrderStatus.POURING);
            }
        }

        [HttpPost]
        [Route("New")]
        public OrderStatusResponse New(NewOrderRequest request)
        {
            var x = Request.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(request.OrderId))
                return new OrderStatusResponse { Locked = Locked, RequestId = "", Status = OrderStatus.ERROR, Message = "Order ID missing" };
            
            var RequestId = Guid.NewGuid();
            var order = new Order
            {
                OrderId = request.OrderId,
                RequestId = RequestId.ToString(),
                ProductId = "34",
                Created = DateTime.Now,
                Poured = null,
                Paid = true,
                Price = double.Parse(request.Price), 
                UserId = request.UserId,                
                Status = OrderStatus.QUEUED,
                TapStatus = TapStatus.Waiting,

            };
            SharePointOnline.AddNewOrder(order);
            orders.Add(order);
            return new OrderStatusResponse { Locked = Locked, RequestId = RequestId.ToString(), Status = order.Status, Message = "" };
        } 

        [HttpPost]
        [Route("Pour")]
        public OrderStatusResponse Pour(OrderStatusRequest request)
        {
            try {
                if (DateTime.Now.Subtract(TapController.LastPing).TotalSeconds > 20)
                    return new OrderStatusResponse { Locked = Locked, Message = "Device not active.", RequestId = request.RequestID, Status = OrderStatus.ERROR };

                if (Locked)
                    return new OrderStatusResponse { Locked = Locked, Message = "", RequestId = request.RequestID, Status = OrderStatus.QUEUED }; 

                Guid RequestID;
                if(!Guid.TryParse(request.RequestID, out RequestID))                
                    return new OrderStatusResponse { Locked = Locked, RequestId = request.RequestID, Status = OrderStatus.ERROR, Message = "Invalid request ID" };
                
                var order = orders.Where(o => o.RequestId == RequestID.ToString() && o.Status == OrderStatus.QUEUED).FirstOrDefault();

                if (order == null)
                    return new OrderStatusResponse { Locked = Locked, RequestId = request.RequestID, Status = OrderStatus.ERROR, Message = "No orders for request id" };

                if (!order.Paid)
                    return new OrderStatusResponse { Locked = Locked, RequestId = request.RequestID, Status = OrderStatus.WAITING_FOR_PAYMENT, Message = "" };
                                
                order.TapStatus = TapStatus.Pour;
                order.Status = OrderStatus.READY;

                return new OrderStatusResponse { Locked = Locked, RequestId = request.RequestID, Status = order.Status , Message = "" };

            }
            catch(Exception ex)
            {
                return new OrderStatusResponse { Locked = true, RequestId = request.RequestID, Status = OrderStatus.ERROR, Message = "Exception occured, " + ex.Message };
            }
        }

        [HttpGet]
        [Route("Status/{requestID}")]
        public OrderStatusResponse Status(string requestID)
        {
            if (DateTime.Now.Subtract(TapController.LastPing).TotalSeconds > 20)
                return new OrderStatusResponse { Locked = Locked, Message = "Device not active.", RequestId = requestID, Status = OrderStatus.ERROR };

            var order = orders.Where(o => o.RequestId == requestID).FirstOrDefault();
            if (order == null)
                return new OrderStatusResponse { Locked = Locked, Message = "No order for request id", RequestId = requestID, Status = OrderStatus.ERROR };

            return new OrderStatusResponse { Locked = Locked, Message = "", RequestId = requestID, Status = order.Status, TapStatus = order.TapStatus };
        }
    }
}
