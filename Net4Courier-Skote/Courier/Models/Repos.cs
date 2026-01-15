//using CourierMVC.CrystalReport;
//using CourierMVC.EntityFramework;
//using CourierMVC.Models;
//using CrystalDecisions.CrystalReports.Engine;
//using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using CMSV2.Models;
using CMSV2.DAL;
namespace CMSV2.Models
{
    public class Repos
    {
        private Entities1 db = new Entities1();

        public S_Audit GetAudit()
        {
            string[] arr = System.Web.HttpContext.Current.Request.FilePath.ToLower().Split('/');
            System.Web.HttpContext.Current.Session["FN"] = arr[1].ToUpper();
            try
            {
                string Token = Encoding.ASCII.GetString(MachineKey.Unprotect(SessionDataModel.GetCookie()));
                int[] nums = Array.ConvertAll(Token.Split(','), int.Parse);
                int i = nums[0], i1 = nums[1], i2 = nums[2];
                if (i < 0)
                {
                    return new S_Audit
                    {
                        S_AuditID = -1,
                        S_DeviceID = "",
                        S_EmpId = i1,
                        S_LocationsActive = "",
                        S_LoginDateTime = DateTime.Now,
                        S_LogoutDateTime = null,
                        S_UserType = i2,
                        S_ZoneActive = ""
                    };
                }
                return db.S_Audit.Where(x => x.S_AuditID == i && x.S_EmpId == i1 && x.S_UserType == i2 && x.S_LogoutDateTime == null).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public S_Audit GetAudit(bool v)
        {
            try
            {
                string Token = Encoding.ASCII.GetString(MachineKey.Unprotect(SessionDataModel.GetCookie()));
                int[] nums = Array.ConvertAll(Token.Split(','), int.Parse);
                int i = nums[0], i1 = nums[1], i2 = nums[2];
                if (i < 0)
                {
                    return new S_Audit
                    {
                        S_AuditID = -1,
                        S_DeviceID = "",
                        S_EmpId = i1,
                        S_LocationsActive = "",
                        S_LoginDateTime = DateTime.Now,
                        S_LogoutDateTime = null,
                        S_UserType = i2,
                        S_ZoneActive = ""
                    };
                }
                return db.S_Audit.Where(x => x.S_AuditID == i && x.S_EmpId == i1 && x.S_UserType == i2 && x.S_LogoutDateTime == null).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public CustAudit GetCustAudit()
        {
            try
            {
                CustAudit audit = null;
                string Token = Encoding.ASCII.GetString(MachineKey.Unprotect(SessionDataModel.GetCookie()));
                int[] nums = Array.ConvertAll(Token.Split(','), int.Parse);
                int i = nums[0], i1 = nums[1];
                var usr = db.S_CustomerLogin.Where(x => x.ID == i).FirstOrDefault();
                var cust = db.S_CustomerMaster.Where(x => x.CustomerID == i1).FirstOrDefault();
                if (usr != null && cust != null)
                {
                    audit = new CustAudit
                    {
                        Customer = cust,
                        User = usr
                    };
                }
                return audit;
            }
            catch
            {
                return null;
            }
        }
        //Authorizes By checking if Audit exists or not and also checks weather user has access to that function or not.
        //Takes String[] as param where array has controller and function in arr[1] and arr[2] respectively.
        public AuthHelp Authenticate()
        {
            string[] arr = System.Web.HttpContext.Current.Request.FilePath.ToLower().Split('/');
            AuthHelp response;
            S_Audit audit = GetAudit(true);
            if (audit != null)
            {
                AuthReq request = new AuthReq
                {
                    Controller = arr[1],
                    FType = arr[2] == "index" ? 1 : (arr[2] == "create" ? 2 : (arr[2] == "edit" ? 3 : (arr[2] == "details" ? 1 : 4))),
                    UserType = (int)audit.S_UserType
                };
                AuthResp resp = Auth(request);
                if (resp.Protocol)
                {
                    response = new AuthHelp
                    {
                        audit = audit,
                        Status = true,
                        Controller = "",
                        Function = "",
                        Permissions = resp.Permissions
                    };
                }
                else
                {
                    response = new AuthHelp
                    {
                        audit = audit,
                        Status = false,
                        Controller = "Error",
                        Function = "PageNotFound"
                    };
                }
            }
            else
            {
                response = new AuthHelp
                {
                    audit = audit,
                    Status = false,
                    Controller = "Auth",
                    Function = "Login"
                };
            }
            return response;
        }

        private S_Access GetAccess(AccessReq req)
        {
            //Access access = db.S_Access.FirstOrDefault(x => x.UserType == req.UserType && x.FunctionName == req.FunctionName);
            //return access;
            return new S_Access
            {
                Display = true,
                Creation = true,
                Updation = true,
                Deletion = true,
            };
        }

        public string GetImage(byte[] arr)
        {
            return arr != null ? String.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(arr)) : null;
        }

        public string GetCountry(int? id)
        {
            return db.S_CountryMaster.Where(x => x.CountryID == id).FirstOrDefault()?.CountryName;
        }
        public string GetCity(int? id)
        {
            return db.S_CityMaster.Where(x => x.CityID == id).FirstOrDefault()?.City;
        }
        public string GetLocation(int? id)
        {
            return db.S_LocationMaster.Where(x => x.LocationID == id).FirstOrDefault()?.Location;
        }
        public string GetCurrency(int? id)
        {
            return db.S_CurrencyMaster.Where(x => x.CurrencyID == id).FirstOrDefault()?.CurrencyName;
        }
        public string GetUsertype(int? id)
        {
            return db.S_UserTypes.Where(x => x.ID == id).FirstOrDefault()?.UserType;
        }
        //Checks weather user has access to Repective requests.
        //UserTypes
        //1: Display 2: Create 3: Update 4: Delete
        private AuthResp Auth(AuthReq req)
        {
            S_Access access = db.S_Access.FirstOrDefault(x => x.UserType == req.UserType && x.Link == req.Controller);
            bool type;
            System.Web.HttpContext.Current.Session["FN"] = access == null ? req.Controller.ToUpper() : access.FunctionName?.ToUpper();
            if (req.FType == 1)
            {
                type = (bool)(access.Display == null ? false : access.Display);
            }
            else if (req.FType == 2)
            {
                type = (bool)(access.Creation == null ? false : access.Creation);
            }
            else if (req.FType == 3)
            {
                type = (bool)(access.Updation == null ? false : access.Updation);
            }
            else
            {
                type = (bool)(access.Deletion == null ? false : access.Deletion);
            }
            return new AuthResp
            {
                Permissions = access,
                Protocol = type
            };
        }
        public SelectList GetMenuTypes()
        {

            List<AccessReq> list = new List<AccessReq>
            {
                new AccessReq
                {
                    FunctionName = "No Group",
                    UserType = 0
                },
                new AccessReq
                {
                    FunctionName = "Master",
                    UserType = 1
                },
                new AccessReq
                {
                    FunctionName = "Setup",
                    UserType = 2
                }


            };
            return new SelectList(list, "UserType", "FunctionName", 1);
        }
        public SelectList GetCountries()
        {
            return new SelectList(db.S_CountryMaster.OrderBy(x => x.CountryName), "CountryID", "CountryName");
        }
        public SelectList GetForwardingAgents()
        {
            return new SelectList(db.S_ForwardingAgentMaster.OrderBy(x => x.AgentName), "ID", "AgentName");
        }
        public SelectList GetCompanyAgents()
        {
            return new SelectList(db.S_ForwardingAgentMaster.Where(d=>d.IsForwardingAgent==false).OrderBy(x => x.AgentName), "ID", "AgentName");
        }
        public SelectList GetCities()
        {
            return new SelectList(db.S_CityMaster.OrderBy(x => x.City), "CityID", "City");
        }
        public SelectList GetCustomers()
        {
            return new SelectList(db.S_CustomerMaster.OrderBy(x => x.CustomerName), "CustomerID", "CustomerName");
        }
        public SelectList UserTypes()
        {
            return new SelectList(db.S_UserTypes, "ID", "UserType");
        }
        public SelectList GetEmployees()
        {
            return new SelectList(db.S_EmployeeMaster.OrderBy(x => x.EmployeeName), "EmployeeID", "EmployeeName");
        }
        public SelectList GetCurrency()
        {
            return new SelectList(db.S_CurrencyMaster.OrderBy(x => x.CurrencyName), "CurrencyID", "CurrencyName");
        }
        public SelectList GetLocations()
        {
            return new SelectList(db.S_LocationMaster.OrderBy(x => x.Location), "LocationID", "Location");
        }
        public SelectList GetVehicleTypes()
        {
            return new SelectList(db.S_VehicleTypes, "ID", "VehicleType");
        }
        public SelectList GetStatus()
        {
            return new SelectList(db.S_ShipmentStatusMaster, "ID", "Name");
        }
        public SelectList GetCities(int id)
        {
            return new SelectList(db.S_CityMaster.Where(c => c.CountryID == id).OrderBy(x => x.City), "CityID", "City");
        }
        public SelectList GetLocations(int id)
        {
            return new SelectList(db.S_LocationMaster.Where(l => l.CityID == id).OrderBy(x => x.Location), "LocationID", "Location");
        }

        public IEnumerable<SelectListItem> GetShipmentType(string selectedVal = null)
        {
            return new List<SelectListItem>
            {
                new SelectListItem{Text = "Transhipment", Value = "Transhipment", Selected = selectedVal == "Transhipment"},
                new SelectListItem{Text = "Import", Value = "Import", Selected = selectedVal == "Import"},
            };
        }
        //public Stream GetAirWayBill(AirwayBillData data)
        //{
        //    S_AcCompany company = db.S_AcCompany.FirstOrDefault();
        //    CrystalDecisions.Shared.DiskFileDestinationOptions CrDiskFileDestinationOptions = new DiskFileDestinationOptions();
        //    var CrysRPT = new CrystalDecisions.CrystalReports.Engine.ReportClass();
        //    CrysRPT = new AirwayBill();
        //    TextObject txtDate = (TextObject)CrysRPT.ReportDefinition.Sections["Section1"].ReportObjects["DateC"];
        //    TextObject txtTime = (TextObject)CrysRPT.ReportDefinition.Sections["Section1"].ReportObjects["TimeC"];
        //    TextObject txtTop = (TextObject)CrysRPT.ReportDefinition.Sections["Section2"].ReportObjects["txtTop"];
        //    TextObject txtAWBNO = (TextObject)CrysRPT.ReportDefinition.Sections["Section2"].ReportObjects["txtAWBNO"];
        //    TextObject txtConsinee = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtConsinee"];
        //    TextObject txtDAddress = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtDAddress"];
        //    TextObject txtDCity = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtDCity"];
        //    TextObject txtDState = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtDState"];
        //    TextObject txtDPin = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtDPin"];
        //    TextObject txtDPhone = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtDPhone"];
        //    TextObject txtConsinor = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtConsinor"];
        //    TextObject txtOAddress = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtOAddress"];
        //    TextObject txtOCity = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtOCity"];
        //    TextObject txtOState = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtOState"];
        //    TextObject txtOPin = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtOPin"];
        //    TextObject txtOPhone = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["txtOPhone"];
        //    TextObject tcHead = (TextObject)CrysRPT.ReportDefinition.Sections["Section1"].ReportObjects["THead"];
        //    TextObject tcPara1 = (TextObject)CrysRPT.ReportDefinition.Sections["Section2"].ReportObjects["Para1"];
        //    TextObject tcPara2 = (TextObject)CrysRPT.ReportDefinition.Sections["Section2"].ReportObjects["Para2"];
        //    TextObject tcSHead = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["ContentHeading"];
        //    TextObject tc1 = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["P1"];
        //    TextObject tc2 = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["P2"];
        //    TextObject tc3 = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["P3"];
        //    TextObject tc4 = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["P4"];
        //    TextObject tc5 = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["P5"];
        //    TextObject tc6 = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["P6"];
        //    TextObject tc7 = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["P7"];
        //    TextObject tc8 = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["P8"];
        //    TextObject tc9 = (TextObject)CrysRPT.ReportDefinition.Sections["Section3"].ReportObjects["P9"];
        //    CrysRPT.DataDefinition.FormulaFields["Barcode"].Text = "BarcodeC39ASCII('" + data.AWBNo.Trim() + "')";
        //    txtDate.Text = data.DateC;
        //    txtTime.Text = data.TimeC;
        //    txtTop.Text = company.AcCompany;
        //    txtAWBNO.Text = data.AWBNo;
        //    txtConsinee.Text = data.ConsigneeName == null ? "" : data.ConsigneeName.ToUpper();
        //    txtDAddress.Text = data.ConsigneeAddress == null ? "" : data.ConsigneeAddress.ToUpper();
        //    txtDCity.Text = data.ConsigneeCity == null ? "" : data.ConsigneeCity.ToUpper();
        //    txtDState.Text = data.ConsigneeState == null ? "" : data.ConsigneeState.ToUpper();
        //    txtDPin.Text = data.ConsigneePin == null ? "" : data.ConsigneePin;
        //    txtDPhone.Text = data.ConsigneePhone == null ? "" : data.ConsigneePhone;
        //    txtConsinor.Text = data.ConsignorName == null ? "" : data.ConsignorName.ToUpper();
        //    txtOAddress.Text = data.ConsignorAddress == null ? "" : data.ConsignorAddress.ToUpper();
        //    txtOCity.Text = data.ConsignorCity == null ? "" : data.ConsignorCity.ToUpper();
        //    txtOState.Text = data.ConsignorState == null ? "" : data.ConsignorState.ToUpper();
        //    txtOPin.Text = data.ConsignorPin == null ? "" : data.ConsignorPin;
        //    txtOPhone.Text = data.ConsignorPhone == null ? "" : data.ConsignorPhone;
        //    tcHead.Text = company.Title == null ? "" : company.Title;
        //    tcPara1.Text = company.Para1 == null ? "" : company.Para1;
        //    tcPara2.Text = company.Para2 == null ? "" : company.Para2;
        //    tcSHead.Text = company.ConditionHeading == null ? "" : company.ConditionHeading;
        //    tc1.Text = company.Condition1 == null ? "" : company.Condition1;
        //    tc2.Text = company.Condition2 == null ? "" : company.Condition2;
        //    tc3.Text = company.Condition3 == null ? "" : company.Condition3;
        //    tc4.Text = company.Condition4 == null ? "" : company.Condition4;
        //    tc5.Text = company.Condition5 == null ? "" : company.Condition5;
        //    tc6.Text = company.Condition6 == null ? "" : company.Condition6;
        //    tc7.Text = company.Condition7 == null ? "" : company.Condition7;
        //    tc8.Text = company.Condition8 == null ? "" : company.Condition8;
        //    tc9.Text = company.Condition9 == null ? "" : company.Condition9;
        //    Stream stream = CrysRPT.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //    stream.Seek(0, System.IO.SeekOrigin.Begin);
        //    return stream;
        //}
        //public AirwayBillData GetAWBillData(int id)
        //{
        //    return db.S_CustomerEnquiry.Where(e => e.S_EnquiryID == id)
        //        .Join(db.S_CustomerMaster, p => p.S_CustomerID, c => c.CustomerID, (p, c) => new
        //        {
        //            EnquiryID = p.S_EnquiryID,
        //            ConsigneeAddress = p.S_ConsigneeAddress,
        //            ConsigneeCity = p.S_ConsigneeCityID,
        //            ConsigneeName = p.S_ConsigneeName,
        //            ConsigneePhone = p.S_ConsigneePhoneNumber,
        //            ConsigneeLocation = p.S_ConsigneeLocationID,
        //            ConsignorName = c.CustomerName,
        //            ConsignorAddress = c.Address1 + " " + c.Address2 + " " + c.Address3,
        //            ConsignorCity = c.CityID,
        //            ConsignorLocation = c.LocationID,
        //            ConsignorPhone = c.Phone,
        //            DT = p.S_EnquiryDate
        //        })
        //        .Join(db.S_Inscan, d => d.EnquiryID, i => i.S_EnquiryID, (d, i) => new { AWBNo = i.S_AWBNO, d.ConsigneeAddress, d.ConsigneeCity, d.ConsigneeLocation, d.ConsigneeName, d.ConsigneePhone, d.ConsignorAddress, d.ConsignorCity, d.ConsignorLocation, d.ConsignorName, d.ConsignorPhone, d.EnquiryID, d.DT })
        //        .Join(db.S_LocationMaster, d => d.ConsignorLocation, l => l.LocationID, (d, l) => new { ConsignorState = l.Location, ConsignorPin = l.Pincode, d.ConsigneeAddress, d.ConsigneeCity, d.ConsigneeLocation, d.ConsigneeName, d.ConsigneePhone, d.ConsignorAddress, d.ConsignorCity, d.ConsignorName, d.ConsignorPhone, d.AWBNo, d.DT })
        //        .Join(db.S_LocationMaster, d => d.ConsigneeLocation, l => l.LocationID, (d, l) => new { ConsigneeState = l.Location, ConsigneePin = l.Pincode, d.ConsigneeAddress, d.ConsigneeCity, d.ConsigneeName, d.ConsigneePhone, d.ConsignorAddress, d.ConsignorCity, d.ConsignorName, d.ConsignorPhone, d.AWBNo, d.ConsignorState, d.ConsignorPin, d.DT })
        //        .Join(db.S_CityMaster, d => d.ConsignorCity, c => c.CityID, (d, c) => new { ConsignorCity = c.City, d.ConsigneeAddress, d.ConsigneeCity, d.ConsigneeName, d.ConsigneePhone, d.ConsignorAddress, d.ConsignorName, d.ConsignorPhone, d.AWBNo, d.ConsignorState, d.ConsignorPin, d.ConsigneeState, d.ConsigneePin, d.DT })
        //        .Join(db.S_CityMaster, d => d.ConsigneeCity, c => c.CityID, (d, c) => new AirwayBillData { ConsigneeCity = c.City, ConsigneeAddress = d.ConsigneeAddress, ConsignorCity = d.ConsignorCity, ConsigneeName = d.ConsigneeName, ConsigneePhone = d.ConsigneePhone, ConsignorAddress = d.ConsignorAddress, ConsignorName = d.ConsignorName, ConsignorPhone = d.ConsignorPhone, AWBNo = d.AWBNo, ConsignorState = d.ConsignorState, ConsignorPin = d.ConsignorPin, ConsigneeState = d.ConsigneeState, ConsigneePin = d.ConsigneePin, DT = d.DT })
        //        .FirstOrDefault();
        //}


        public ImportShipment GetImportShipment(int id)
        {
            //return db.S_ImportShipment.Where(x => x.ID == id).Include(s => s.S_CityMaster)
            //        .Include(s => s.S_CityMaster1).Include(s => s.S_CityMaster2).Include(s => s.S_CountryMaster)
            //        .Include(s => s.S_CountryMaster1).Include(s => s.S_ForwardingAgentLogin).Include(s => s.S_ForwardingAgentLogin1)
            //        .Include(s => s.S_ForwardingAgentMaster).Include(s => s.S_ImportShipmentDetails)
            //        .FirstOrDefault();


            return db.S_ImportShipment.Where(x => x.ID == id).Select(x => new ImportShipment
            {
                ManifestNumber = x.ManifestNumber,
                ConsignorAddress = x.ConsignorAddress,
                OriginCountry = x.OrginCountry,
                OriginCity = x.OriginCity,
                ConsigneeAddress = x.ConsigneeAddress,
                DestinationCountry = x.DestinationCountry,
                DestinationCity = x.DestinationCity,
                Date = x.Date,
                FlightNo = x.FlightNo,
                MAWB = x.MAWB,
                CD = x.CD,
                Bags = x.Bags,
                RunNo = x.RunNo,
                Type = x.Type,
                TotalAWB = x.TotalAWB,
                ImportShipmentDetails = x.S_ImportShipmentDetails.Select(s => new ImportShipmentDetail
                {
                    HAWB = s.HAWB,
                    AWB = s.AWB,
                    PCS = s.PCS,
                    Weight = s.Weight,
                    Contents = s.Contents,
                    Shipper = s.Shipper,
                    Value = s.Value,
                    Reciver = s.Reciver,
                    DestinationCountryID = s.DestinationCountry,
                    DestinationCityID = s.DestinationCity,
                    BagNo = s.BagNo,
                    CurrencyID = s.S_CurrencyMaster.Symbol
                }).ToList(),
            }).FirstOrDefault();
        }
    }
}