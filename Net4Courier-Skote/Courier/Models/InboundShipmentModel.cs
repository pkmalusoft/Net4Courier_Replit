using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class InboundShipmentModel :InboundShipment
    {
        public int SNo { get; set; }
        public string Currency { get; set; }
        public string LastModifiedByName { get; set; }
        public string CreatedByName { get; set; }
        public string PaymentMode { get; set; }
        public string ParcelType { get; set; }
        public string MovementType { get; set; }
        public string ProductType { get; set; }
        public string StatusType { get; set; }
        public string CourierStatus { get; set; }
        public string Destination { get; set; }
        public string CustomerName { get; set; }
        public string BranchLocation { get; set; }
        public string BranchCountry { get; set; }
        public int DefaultCurrencyId { get; set; }
        public string BranchCity { get; set; }
        public int DefaultSurchargePercent { get; set; }
        public int[] SelectedValues { get; set; }
        public string VoucherNo { get; set; }
        public string FAgentName { get; set; }
        public bool AWBValidStatus { get; set; }
        public string AwbValidStatusclass { get; set; }
        public string InvoiceStatus { get; set; }

        public string TaxInvoiceStatus { get; set; }
        public string AWBStatus { get; set; }

    }
    public class ImportFixationSource
    {
        public string SourceValue { get; set; }
    }
    public class InboundAWBSearch
    {
        public int StatusID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int MovementTypeID { get; set; }
        public int PaymentModeId { get; set; }
        public string ConsignorConsignee { get; set; }
        public string Origin { get; set; }

        public string Destination { get; set; }
        public string AWBNo { get; set; }
        public string BatchNo { get; set; }
        public DateTime BatchDate { get; set; }
        public List<InboundShipmentModel> Details { get; set; }
    }
    public class InboundAWBBatchModel : InboundShipmentModel
    {
        public List<InboundShipmentModel> Details {get;set;}
        public int ID { get; set; }
        public string BatchNumber { get; set; }
        public DateTime BatchDate { get; set; }
        public int TotalAWB { get; set; }
        public string Status { get; set; }
        public string OriginAirportCity { get; set; }
        public string DestinationAirportCity { get; set; }
        public DateTime FlightDate { get; set; }
        public string FlightNo { get; set; }
        
        public int Bags { get; set; }
        public string RunNo { get; set; }
        public int ParcelNo { get; set; }
        public decimal TotalWeight { get; set; }
        
    }

    public class ExportAWBList
    {
        public int InScanId { get; set; }
        public string AWB { get; set; }

        public string Shipper { get; set; }

        public string Receiver { get; set; }

        public string Bags { get; set; }

        public decimal? Weight { get; set; }
        public int Pieces { get; set; }

        public string Contents { get; set; }
        public decimal Value { get; set; }
        public string DestinationCountry { get; set; }
        public string DestinationCity { get; set; }

        //
        public int ShipmentID { get; set; }
        public string AWBNo { get; set; }
        public System.DateTime AWBDate { get; set; }

        public string Consignor { get; set; }


        public string ConsignorCountryName { get; set; }
        public string ConsignorCityName { get; set; }

        public string Consignee { get; set; }


        public string ConsigneeCountryName { get; set; }
        public string ConsigneeCityName { get; set; }

        public string CargoDescription { get; set; }
        public string BagNo { get; set; }
        public string MAWB { get; set; }

    }
}