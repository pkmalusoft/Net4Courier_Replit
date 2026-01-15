using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public class ImportShipmentFormModel : ImportShipment
    {
        public ImportShipmentFormModel()
            {
            Shipments = new List<ImportShipmentDetail>();
            }
        public string ConsignorAddress {get;set;}
        public string ConsigneeAddress { get; set; }
        public string Route { get; set; }
        public List<ImportShipmentDetail> Shipments { get; set; }
    }
    public class ImportShipmentDetailVM :ImportShipmentDetail
    {
        //public string HAWB { get; set; }
        //public string AWB { get; set; }
        //public int PCS { get; set; }
        //public decimal Weight { get; set; }
        //public string Contents { get; set; }
        //public string Shipper { get; set; }
        //public decimal Value { get; set; }
        //public string Reciver { get; set; }
        //public string DestinationCountryID { get; set; }
        //public string DestinationCityID { get; set; }
        //public string BagNo { get; set; }
        //public int CurrencyID { get; set; }
        public string CurrencyName{ get; set; }
        public int Sno { get; set; }
    }
    public class ImportShipmentVM:ImportShipment
    {
        public string AgentName { get; set; }        
        public string ConsignorAddress { get; set; }
        public string OriginCountry { get; set; }
        public string OriginCity { get; set; }
        public string ConsigneeAddress { get; set; }
        public string DestinationCountry { get; set; }
        public string DestinationCity { get; set; }
       
        
        public string CourierStatus { get; set; }
       
        public string AWBNumbers { get; set; }
       
        public int ExportShipmentID { get; set; }
        public List<ImportShipmentDetailVM> Details { get; set; }
    }

    public class ImportShipmentSearch
    {
        public string AWBNo { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<ImportShipmentVM> Details { get; set; }
    }
    public class ExportShipmentFormModel : ExportShipment
    { 
        public ExportShipmentFormModel()
        {
            Shipments = new List<ExportShipmentDetail>();
        }
        public string ConsignorAddress { get; set; }
        public string ConsignorAddress2 { get; set; }
        public string ConsignorName { get; set; }
        public string ConsignorCityName { get; set; }
        public string ConsignorCountryName { get; set; }
        
        public string ConsigneeAddress { get; set; }
        public string ConsigneeAddress2 { get; set; }
        public string ConsigneeName { get; set; }
        public string ConsigneeCountryname { get; set; }
        public string ConsigneeCityName { get; set; }
        
        public string CreatedByName { get; set; }
        public string AgentName { get; set; }
        public string FAgentName { get; set; }
        public string ShipmentType { get; set; }
        public string AWBNumbers { get; set; }
        public decimal MAWBWeight { get; set; }
        public List<ExportShipmentDetail> Shipments { get; set; }

        public List<ExportShipmentDetailVM> ShipmentsVM { get; set; }
    }
    public class ExportShipmentDetailVM : ExportShipmentDetail
    {
        public string PaymentMode { get; set; }
        public string FwdAgentName {get;set;}
        public string CurrencyName { get; set; }
        public string CurrenySymbol { get; set; }
        public string ConsignorPhone { get; set; }
        public string ConsigneePhone { get; set; }
        public string ConsignorAddress1 { get; set; }
        public string ConsignorAddress2 { get; set; }
        public string ConsigneeAddress1 { get; set; }
        public string ConsigneeAddress2 { get; set; }
        public decimal CustomsValue { get; set; }
        public string Currency { get; set; }
         public bool AWBChecked { get; set; }
        public decimal? AWBOtherCharge { get; set; }
        public decimal? AWBCourierCharge { get; set; }
        
    }
    public class ManifestCSV
    {
        public string AWBNo { get; set; }
        public string AgentName { get; set; }
        public string AgentTrackingNo { get; set; }
        public string Rate { get; set; }
        public string VerifiedWeight { get; set; }
        public string BagNo { get; set; }
    }
    public class ExportShipmentVM :ExportShipment
    {
        //public string ManifestNumber { get; set; }
        public string ConsignorAddress { get; set; }
        public string OriginCountry { get; set; }
        public string OriginCity { get; set; }
        public string ConsigneeAddress { get; set; }
        public string DestinationCountry { get; set; }
        public string DestinationCity { get; set; }
        public System.DateTime Date { get; set; }
        public string AirportOfShipment { get; set; }
        public string FlightNo { get; set; }
        public string MAWB { get; set; }
        public string CD { get; set; }
        public Nullable<int> Bags { get; set; }
        public string RunNo { get; set; }
        public string Type { get; set; }
        public int TotalAWB { get; set; }
        public List<ExportShipmentDetail> ExportShipmentdetails { get; set; }
    }
    public class ExportShipmentSearch
    {
        public string AWBNo { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public List<ExportShipmentFormModel> Details { get; set; }
    }
    public class DatePicker
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Create { get; set; }

        public int? StatusId { get; set; }
        public int? AgentId { get; set; }
        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }
        public string MovementId { get; set; }
        public int[] SelectedValues { get; set; }

        public int paymentId { get; set; }
        public int AcHeadId { get; set; }

    }

    public class MCDatePicker
    {
        public string SearchOption { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }        
        public string AWBNo { get; set; }
        public int InScanID { get; set; }
        public DateTime? FromDate1 { get; set; }
        public DateTime? ToDate1 { get; set; }
                public string Shipper { get; set; }
        public string ShipperAddress { get; set; }



    }
    public class InvoiceAllParam
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceType { get; set; }
        public string MovementId { get; set; }
        public string CustomerIDS { get; set; }
        public int[] SelectedValues { get; set; }

        public List<CustomerInvoicePendingModel> Details { get; set; }
    }
    public class AccountsReportParam
    {
        public int  AcHeadId { get; set; }        
        public string AcHeadName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool CurrentPeriod { get; set; }
        public int AcTypeId { get; set; }
        public int AcGroupId { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }
        public string VoucherTypeId { get; set; }
        public int[] SelectedValues { get; set; }
        public bool Exceptional { get; set; }
        public string[] SelectedValues1 { get; set; }
        public decimal BankAmount { get; set; }
        //public List<ReportLog> ReportLogs { get; set; }

    }
    public class AccountsReportParam1
    {
        public int AcHeadId { get; set; }
        public string AcHeadName { get; set; }
        public bool isPeriod { get; set; }
        public DateTime AsOnDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Period { get; set; }
        public int AcTypeId { get; set; }
        public int AcGroupId { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }
        public string VoucherTypeId { get; set; }
        public int[] SelectedValues { get; set; }

    }
    public class LabelPrintingParam
    {
        public int LabelStartNo { get; set; }
        public int LabelQty { get; set; }
        public int Increment { get; set; }
        public int InScanId { get; set; }

        public string ConsignmentNo { get; set; }
        public string Output { get; set; }

        public string ReportFileName { get; set; }
    }
    public class CustomerLedgerReportParam
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public DateTime AsonDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }                
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }     
        public string CustomerType { get; set; }
        //public List<ReportLog> ReportLogs { get; set; }

    }

    public class CustomerInvoiceReportParam
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public bool DateWise { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string FromNo{ get; set; }
        public string ToNo { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }
        public string CustomerType { get; set; }

    }
    public class SupplierLedgerReportParam
    {
        public int SupplierTypeId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime AsonDate { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }

    }
    public class AWBReportParam
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Create { get; set; }

        public int? PaymentModeId { get; set; }
        public int? ParcelTypeId { get; set; }                
        public string MovementId { get; set; }
        public int[] SelectedValues { get; set; }
        public string Output { get; set; }
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }
        public string SortBy { get; set; }
    }

    public class TaxReportParam
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Create { get; set; }

        public string TransactionType{ get; set; }        
        public string Output { get; set; }
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }
        public string SortBy { get; set; }
    }
    public class ExportManifestReportParam
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public bool Create { get; set; }

        public int FAgentID{ get; set; }

        public int ManifestId { get; set; }
        public string Output { get; set; }
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }
        public string SortBy { get; set; }
    }
    public class CODReportParam
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? ParcelTypeId { get; set; }
        public string MovementId { get; set; }
        public int[] SelectedValues { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }
        public string SortBy { get; set; }
    }
    public class SalesReportParam
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int EmployeeID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }
        public string MovementId { get; set; }
        public int PaymentModeId { get; set; }
        public string CustomerType { get; set; }
        public int[] SelectedValues { get; set; }

    }
    public class SalesRegisterSummaryParam
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int EmployeeID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }

    }

    public class SalesRegisterCountryParam
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CountryName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }

    }

    public class MaterialCostLedgerParam
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Shipper { get; set; }
        public string Receiver { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }

        public bool Pending { get; set; }

    }

    public class AcInvoiceOpeningParam
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        public int SupplierTypeId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }
        public string CustomerType { get; set; }

    }

    public class DateParam
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string SearchConsignor { get; set; }

        public int CustomerId { get; set; }
        public int DeliveredBy { get; set; }
        public string SearchCity { get; set; }
        //public string SearchText { get; set; }
        public string ReportFileName { get; set; }
    }
    public class CustomerAWBPrintParam
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int AWBCount { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
    }
    public class InvoicePendingDateParam
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string CustomerType { get; set; }
    }

    public class DRSCashFlowReportParam
    {
        public DateTime AsonDate { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName
        {
            get; set;
        }
    }

    public class PDCRegister
    {
        public DateTime AsonDate { get; set; }
        public int AcHeadID { get; set; }

        public bool AllBank { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName
        {
            get; set;
        }
    }

    public class AWBListSearch
    {
        public int StatusID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int MovementTypeID { get; set; }
        public int PaymentModeId { get; set; }
        public string Customer { get; set; }
        public int CustomerId { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string MovementID { get; set; }
        public string AWBNo { get; set; }   
        public int[] SelectedValues { get; set; }
        public List<QuickAWBVM> Details { get; set; }
    }

    public class AWBOtherChargeReportParam
    {
        
        public string AWBNo { get; set; }
        public string InvoiceNo { get; set; }
        public int OtherChargeID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Output { get; set; } //printer ,pdf,word,excel
        public string ReportType { get; set; } //sumary details
        public string ReportFileName { get; set; }
        public string Filters { get; set; }
        

    }
}