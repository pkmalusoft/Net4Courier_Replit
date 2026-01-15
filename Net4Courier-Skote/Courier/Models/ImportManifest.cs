using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class ImportManifestVM : ImportShipment
    {
        public bool EnableAPI { get; set; }
        public string CompanyCountryName { get; set; }
        public string ManifestDate { get; set; }
        public string FlightDate1 { get; set; }
        public string CustomerName { get; set; }
        public List<ImportManifestItem> Details { get; set; }
        public string AWBNo { get; set; }
        public List<TranshipmentModel> TransDetails { get; set; }
        public List<TranshipmentCountry> TransCountryDetails { get; set; }
        
    }
    public class ImportManifestSearch
    {
        public string AWBNo { get; set; }        
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<ImportManifestVM> Details { get; set; }
        public List<ImportManifestItem> Details1 { get; set; }

    }
    public class TranshipmentAWB
    {
        public string AWBNo { get; set; }
        public int InScanId{ get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        
        
    }

    public class ImportManifestFixation
    {
        public string FieldName { get; set; }
        public string SourceValue { get; set; }
        public string DestinationValue { get; set; }
        public string DestinationLocation { get; set; }
        public string DestinationCountry { get; set; }
        public string DestinationCity { get; set; }
        public bool AllItem { get; set; }
        public string FilterItem { get; set; }
    }
    public class ImportManifestItem
    {
        public int ShipmentDetailID { get; set; }
        public int Sno { get; set; }
        public string Bag { get; set; }
        public string AWBNo { get; set; }
        public string AWBDate { get; set; }
        public string AWBDate1 { get; set; }
        public string Rfnc { get; set; }
        public string Shipper { get; set; }
        public string ShipperPhone { get; set; }
        public string Receiver { get; set; }
        public string ReceiverContact{ get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverPhone { get; set; }
        public string DestinationLocation { get; set; }
        public string DestinationCity { get; set; }
        public string DestinationCountry { get; set; }
        public string ImportType { get; set; }
        public string Content { get; set; }
        public int Pcs { get; set; }
        public string Weight {get;set;}
        public string Value { get; set; }
        public string COD { get; set; }
        public string Currency { get; set; }
        public int CurrencyID { get; set; }
        public string Remark { get; set; }        
        public string lastStatusRmk {get;set;}
        public string route { get; set; }
        public string MAWB { get; set; }
        public string status { get; set; }
        public string groupCode { get; set; }

        public List<AWBTrackStatusVM> awbtrackdetails = new List<AWBTrackStatusVM>();

    }
}