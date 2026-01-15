using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{

    public class ServiceArea
    {
        public string Code { get; set; }
        public string Description { get; set; }
    }

    public class ShipperDetails
    {
        public string Name { get; set; }
        public List<ServiceArea> ServiceArea { get; set; }
    }

    public class ReceiverDetails
    {
        public string Name { get; set; }
        public List<ServiceArea> ServiceArea { get; set; }
    }

    public class ShipperReference
    {
        public string value { get; set; }
        public string typeCode { get; set; }
    }

    public class Event
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string TypeCode { get; set; }
        public string Description { get; set; }
        public List<ServiceArea> ServiceArea { get; set; }
    }

    public class Shipment
    {
        public string shipmentTrackingNumber { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public ShipperDetails ShipperDetails { get; set; }
        public ReceiverDetails ReceiverDetails { get; set; }
        public string TotalWeight { get; set; }
        public List<ShipperReference> ShipperReferences { get; set; }
        public List<Event> Events { get; set; }
        public int NumberOfPieces { get; set; }
        public string EstimatedDeliveryDate { get; set; }
    }

    public class ShipmentsResponse
    {
        public List<Shipment> shipments { get; set; }
    }
}