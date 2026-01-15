using Net4Courier.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Net4Courier.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        Entities1 db = new Entities1();
        public ActionResult Index()
        {
            string jsonString = @"
        {
            'shipments': [{
                'shipmentTrackingNumber': '7367529691',
                'status': 'Success',
                'description': 'Lighting Products',
                'shipperDetails': {
                    'name': 'MACKWELL ELECTRONICS LIMITED',
                    'serviceArea': [{
                        'code': 'BHX',
                        'description': 'Birmingham-UNITED KINGDOM'
                    }]
                },
                'receiverDetails': {
                    'name': 'ARCHILUM LIGHTING',
                    'serviceArea': [{
                        'code': 'SHJ',
                        'description': 'Sharjah-UNITED ARAB EMIRATES'
                    }]
                },
                'totalWeight': '26.000',
                'shipperReferences': [{
                    'value': '7367529691GB20240611112730821',
                    'typeCode': 'SP'
                }],
                'events': [{
                    'date': '2024-06-13',
                    'time': '10:41:16',
                    'typeCode': 'RR',
                    'description': 'RESPONSE RECEIVED',
                    'serviceArea': [{
                        'code': 'DXB',
                        'description': 'Dubai-UNITED ARAB EMIRATES'
                    }]
                }, {
                    'date': '2024-06-13',
                    'time': '12:31:03',
                    'typeCode': 'RR',
                    'description': 'RESPONSE RECEIVED',
                    'serviceArea': [{
                        'code': 'DXB',
                        'description': 'Dubai-UNITED ARAB EMIRATES'
                    }]
                }, {
                    'date': '2024-06-13',
                    'time': '21:29:14',
                    'typeCode': 'RR',
                    'description': 'RESPONSE RECEIVED',
                    'serviceArea': [{
                        'code': 'DXB',
                        'description': 'Dubai-UNITED ARAB EMIRATES'
                    }]
                }, {
                    'date': '2024-06-12',
                    'time': '12:22:29',
                    'typeCode': 'PU',
                    'description': 'SHIPMENT PICKUP',
                    'serviceArea': [{
                        'code': 'BHX',
                        'description': 'Birmingham-UNITED KINGDOM'
                    }]
                }, {
                    'date': '2024-06-14',
                    'time': '09:34:37',
                    'typeCode': 'OH',
                    'description': 'ON HOLD',
                    'serviceArea': [{
                        'code': 'SHJ',
                        'description': 'Sharjah-UNITED ARAB EMIRATES'
                    }]
                }, {
                    'date': '2024-06-14',
                    'time': '09:34:44',
                    'typeCode': 'OH',
                    'description': 'ON HOLD',
                    'serviceArea': [{
                        'code': 'SHJ',
                        'description': 'Sharjah-UNITED ARAB EMIRATES'
                    }]
                }, {
                    'date': '2024-06-14',
                    'time': '09:34:40',
                    'typeCode': 'OH',
                    'description': 'ON HOLD',
                    'serviceArea': [{
                        'code': 'SHJ',
                        'description': 'Sharjah-UNITED ARAB EMIRATES'
                    }]
                }],
                'numberOfPieces': 3,
                'estimatedDeliveryDate': '2024-06-14'
            }]
        }";


            ShipmentsResponse response = JsonConvert.DeserializeObject<ShipmentsResponse>(jsonString);
            string AWBNos = "";
            foreach (var awbitem in response.shipments)
            {

                SaveAWB(awbitem);

            }
            string ss = AWBNos;
            Console.WriteLine($"Shipment Tracking Number: {response.shipments[0].shipmentTrackingNumber}");
            Console.WriteLine($"Status: {response.shipments[0].status}");
            Console.WriteLine($"Description: {response.shipments[0].description}");
            return View();
        }


        public string SaveAWB(Shipment shipment)
        {
            try
            {
                var awb = db.InScanMasters.Where(cc => cc.AWBNo == shipment.shipmentTrackingNumber).FirstOrDefault();
                var branchcountryname = "United Arab Emirates";
                if (awb == null)
                {
                    awb = new InScanMaster();
                    awb.AWBNo = shipment.shipmentTrackingNumber;
                    awb.TransactionDate = DateTime.Now.AddHours(4);
                    awb.Consignor = shipment.ShipperDetails.Name;
                    var consignorarea = shipment.ShipperDetails.ServiceArea[0].Description;
                    var _ConsignorCityName = consignorarea.ToString().Split('-');
                    if (_ConsignorCityName != null)
                    {
                        awb.ConsignorCityName = _ConsignorCityName[0];
                        awb.ConsignorCountryName = _ConsignorCityName[1];
                    }

                    awb.Consignee = shipment.ReceiverDetails.Name;
                    var consigneearea = shipment.ReceiverDetails.ServiceArea[0].Description;
                    var _ConsigneeCityName = consigneearea.ToString().Split('-');
                    if (_ConsigneeCityName != null)
                    {
                        awb.ConsigneeCityName = _ConsigneeCityName[0];
                        awb.ConsigneeCountryName = _ConsigneeCityName[1];
                    }
                    decimal _weight = 0;
                    _weight = decimal.Parse(shipment.TotalWeight);
                    awb.Weight = _weight;
                    awb.Pieces = shipment.NumberOfPieces.ToString();
                    if (_weight > 30)
                    {
                        awb.ParcelTypeId = 4;
                    }
                    else if (_weight <= 30)
                    {
                        awb.ParcelTypeId = 3;
                    }

                    if (awb.ConsignorCountryName.ToLower() != branchcountryname && (awb.ConsigneeCountryName.ToLower() == branchcountryname.ToLower()))
                    {
                        awb.MovementID = 3;

                    }
                    else if (awb.ConsignorCountryName.ToLower() == branchcountryname && (awb.ConsigneeCountryName.ToLower() != branchcountryname))
                    {
                        awb.MovementID = 2;
                    }
                    else if (awb.ConsignorCountryName.ToLower() == branchcountryname && (awb.ConsigneeCountryName.ToLower() == branchcountryname))
                    {
                        awb.MovementID = 1;
                    }
                    else if (awb.ConsignorCountryName.ToLower() != branchcountryname && (awb.ConsigneeCountryName.ToLower() != branchcountryname))
                    {
                        awb.MovementID = 4;
                    }
                    awb.PaymentModeId = 3;
                    if (awb.MovementID == 2)
                        awb.CustomerID = SaveCustomer(awb.Consignor, awb.ConsignorCityName, awb.ConsignorCountryName);
                    else if (awb.MovementID == 3)
                        awb.CustomerID = SaveCustomer(awb.Consignee, awb.ConsigneeCityName, awb.ConsigneeCountryName);
                    else
                    {
                        awb.CustomerID = 1;

                    }
                    awb.ProductTypeID = 11;

                    awb.EntrySource = 3;
                    awb.StatusTypeId = 2;
                    awb.CourierStatusID = 4;
                    awb.AcCompanyID = 1;
                    awb.BranchID = 2;
                    awb.DepotID = 4;
                    awb.CurrencyID = 1;
                    awb.CreatedBy = -1;
                    awb.CreatedDate = DateTime.Now.AddHours(4);

                    awb.AcCompanyID = -1;
                    awb.CreatedDate = DateTime.Now.AddHours(4);
                    db.InScanMasters.Add(awb);
                    db.SaveChanges();

                 //   logger.Info($"* AWB Created " + shipment.shipmentTrackingNumber);

                    AWBTrackStatu _status = new AWBTrackStatu();
                    foreach (var awbstatus in shipment.Events)
                    {
                        string status = awbstatus.Description;
                        DateTime statustime = Convert.ToDateTime(awbstatus.Date + " " + awbstatus.Time);
                    //    logger.Info($"* AWB Tracking date  " + statustime);
                        _status = db.AWBTrackStatus.Where(cc => cc.AWBNo == shipment.shipmentTrackingNumber && cc.CourierStatus == status).FirstOrDefault();
                        if (_status == null)
                        {
                            _status = new AWBTrackStatu();
                            _status.InScanId = awb.InScanID;
                            _status.AWBNo = awb.AWBNo;
                            _status.EmpID = -1;
                            _status.UserId = -1;
                            if (awbstatus.Description.Contains("DELIVERY"))
                            {
                                _status.StatusTypeId = 4;
                                _status.CourierStatusId = 13;
                                _status.CourierStatus = "Shipment Delivered";
                                _status.ShipmentStatus = "Delivered";
                            }
                            else
                            {
                                _status.StatusTypeId = 99;
                                _status.CourierStatusId = 99;
                                _status.CourierStatus = awbstatus.Description;
                                _status.ShipmentStatus = "DELIVERY AGENT TRACKING STATUS";
                            }
                            if (awbstatus.ServiceArea != null)
                            {
                                _status.Remarks = awbstatus.ServiceArea[0].Description;
                            }
                            _status.EntryDate = statustime;
                            db.AWBTrackStatus.Add(_status);
                            db.SaveChanges();

                           // logger.Info($"* AWB Tracking Add " + shipment.shipmentTrackingNumber);
                        }


                    }



                    return "OK";

                }
                else
                {
                  
                    AWBTrackStatu _status = new AWBTrackStatu();
                    foreach (var awbstatus in shipment.Events)
                    {
                        string status = awbstatus.Description;
                        DateTime statustime = Convert.ToDateTime(awbstatus.Date + " " + awbstatus.Time);

                        _status = db.AWBTrackStatus.Where(cc => cc.AWBNo == shipment.shipmentTrackingNumber && cc.CourierStatus == status).FirstOrDefault();
                        if (_status == null)
                        {
                            _status = new AWBTrackStatu();
                            _status.InScanId = awb.InScanID;
                            _status.AWBNo = awb.AWBNo;
                            _status.EmpID = -1;
                            _status.UserId = -1;
                            if (awbstatus.Description.Contains("DELIVERY"))
                            {
                                _status.StatusTypeId = 4;
                                _status.CourierStatusId = 13;
                                _status.CourierStatus = "Shipment Delivered";
                                _status.ShipmentStatus = "Delivered";
                            }
                            else
                            {
                                _status.StatusTypeId = 99;
                                _status.CourierStatusId = 99;
                                _status.CourierStatus = awbstatus.Description;
                                _status.ShipmentStatus = "DELIVERY AGENT TRACKING STATUS";
                            }
                            if (awbstatus.ServiceArea != null)
                            {
                                _status.Remarks = awbstatus.ServiceArea[0].Description;
                            }
                            _status.EntryDate = statustime;
                            db.AWBTrackStatus.Add(_status);
                            db.SaveChanges();

                           // logger.Info($"* AWB Tracking Add " + shipment.shipmentTrackingNumber);
                        }


                    }
                    return "OK";
                }
            }
            catch (Exception ex)
            {
               // logger.Info($"* Save AWB Error :" + ex.Message);
                return "Failed";
            }


        }

        public int SaveCustomer(string CustomerName, string CityName, string CountryName)
        {
            try
            {

                var cust = (from c in db.CustomerMasters where c.CustomerName == CustomerName select c).FirstOrDefault();  //--c.CustomerType == "CR"
                int uid = -1;
                int accompanyid = 1;
                int branchid = 2;
                int depotid = 4;
                if (cust == null)
                {
                    CustomerMaster obj = new CustomerMaster();


                    obj.AcCompanyID = accompanyid;

                    obj.CustomerCode = ""; // _dao.GetMaxCustomerCode(branchid); // c.CustomerCode;
                    obj.CustomerName = CustomerName;//  v.Consignor;
                    obj.CustomerType = "CR"; //Cash customer

                    obj.ContactPerson = "";
                    obj.Address1 = "";
                    obj.Address2 = "";
                    obj.Address3 = "";
                    obj.Phone = "";
                    obj.CountryName = CountryName;
                    obj.CityName = CityName;
                    obj.LocationName = "";
                    obj.UserID = -1;
                    obj.statusCommission = false;
                    obj.Referal = "";
                    obj.StatusActive = true;
                    obj.StatusTaxable = false;
                    obj.CreditLimit = 0;
                    obj.Email = "";
                    obj.BranchID = branchid;
                    obj.CurrencyID = 1;
                    // Convert.ToInt32(Session["UserID"].ToString());
                    obj.CreatedBy = uid;
                    obj.CreatedDate = DateTime.Now.AddHours(4);
                    obj.ModifiedDate = DateTime.Now.AddHours(4);
                    obj.ModifiedBy = uid;
                    obj.DepotID = depotid;
                    obj.Mobile = "";
                    db.CustomerMasters.Add(obj);
                    db.SaveChanges();
                    ReSaveCustomerCode();
                    return obj.CustomerID;
                }
                else
                {
                    return cust.CustomerID;
                }
            }
            catch (Exception ex)
            {
               // logger.Info($"* Save Customer  :" + ex.Message);
                return -1;
            }
        }

        public void ReSaveCustomerCode()
        {
            //SP_InsertJournalEntryForRecPay
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = new SqlConnection(GetConnectionString());
                cmd.CommandText = "SP_ReSaveCustomerCode";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
               // logger.Info($"* Save Customer Code :" + ex.Message);

            }
        }

        public string GetConnectionString()
        {

            return System.Configuration.ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
        }
    }
}