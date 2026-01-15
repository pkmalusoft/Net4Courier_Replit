using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Net4Courier.Models;
using Net4Courier.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Net4Courier.Models
{
    public static class SessionDataModel
    {
        public static void SetCurrency(CurrencyMaster currency)
        {
            System.Web.HttpContext.Current.Session["Currency"] = currency;
        }
        public static CurrencyMaster GetCurrency()
        {
            return (CurrencyMaster)System.Web.HttpContext.Current.Session["Currency"];
        }
        public static void ClearTableVariable()
        {
            System.Web.HttpContext.Current.Session["TableFilter"] = null;
        }
        public static void SetTableVariable(DatePicker datePicker)
        {
            System.Web.HttpContext.Current.Session["TableFilter"] = datePicker;
        }

        public static DatePicker GetTableVariable1()
        {
            return (DatePicker)System.Web.HttpContext.Current.Session["TableFilter1"];
        }
        public static void SetTableVariable1(DatePicker datePicker)
        {
            System.Web.HttpContext.Current.Session["TableFilter1"] = datePicker;
        }

        public static MCDatePicker GetMCTableVariable1()
        {
            return (MCDatePicker)System.Web.HttpContext.Current.Session["MCTableFilter1"];
        }
        public static void SetMCTableVariable1(MCDatePicker datePicker)
        {
            System.Web.HttpContext.Current.Session["MCTableFilter1"] = datePicker;
        }

        public static DatePicker GetTableVariable()
        {
            return (DatePicker)System.Web.HttpContext.Current.Session["TableFilter"];
        }
        public static AccountsReportParam GetAccountsLedgerParam()
        {
            return (AccountsReportParam)System.Web.HttpContext.Current.Session["AccountsLedgerParam"];
        }
        public static void SetAccountsLedgerParam(AccountsReportParam reportparam)
        {
            System.Web.HttpContext.Current.Session["AccountsLedgerParam"] = reportparam;
        }

        public static AccountsReportParam GetAccountsParam()
        {
            return (AccountsReportParam)System.Web.HttpContext.Current.Session["AccountsParam"];
        }
        public static void  SetAccountsParam(AccountsReportParam reportparam)
        {
            System.Web.HttpContext.Current.Session["AccountsParam"] = reportparam;            
        }
        public static CustomerLedgerReportParam GetCustomerLedgerReportParam()
        {
            return (CustomerLedgerReportParam)System.Web.HttpContext.Current.Session["CustomerLedgerReportParam"];
        }
        public static AccountsReportParam1 GetAccountsParam1()
        {
            return (AccountsReportParam1)System.Web.HttpContext.Current.Session["AccountsParam1"];
        }
        public static void SetAccountsParam1(AccountsReportParam1 reportparam)
        {
            System.Web.HttpContext.Current.Session["AccountsParam1"] = reportparam;
        }

        public static AccountsReportParam GetAccountsParam2()
        {
            return (AccountsReportParam)System.Web.HttpContext.Current.Session["AccountsParam2"];
        }
        public static void SetAccountsParam2(AccountsReportParam reportparam)
        {
            System.Web.HttpContext.Current.Session["AccountsParam2"] = reportparam;
        }
        public static void SetCustomerLedgerParam(CustomerLedgerReportParam reportparam)
        {
            System.Web.HttpContext.Current.Session["CustomerLedgerReportParam"] = reportparam;
        }
        public static void SetSupplierLedgerParam(SupplierLedgerReportParam reportparam)
        {
            System.Web.HttpContext.Current.Session["SupplierLedgerReportParam"] = reportparam;
        }

        public static SupplierLedgerReportParam GetSupplierLedgerReportParam()
        {
            return (SupplierLedgerReportParam)System.Web.HttpContext.Current.Session["SupplierLedgerReportParam"];
        }
        public static AWBReportParam GetAWBReportParam()
        {
            return (AWBReportParam)System.Web.HttpContext.Current.Session["AWBReportParam"];
        }
        public static void SetAWBReportParam(AWBReportParam reportparam)
        {
            System.Web.HttpContext.Current.Session["AWBReportParam"] = reportparam;
        }
        public static TaxReportParam GetTaxReportParam()
        {
            return (TaxReportParam)System.Web.HttpContext.Current.Session["TaxReportParam"];
        }
        public static void SetTaxReportParam(TaxReportParam reportparam)
        {
            System.Web.HttpContext.Current.Session["TaxReportParam"] = reportparam;
        }

        public static LabelPrintingParam GetLabelPrintParam()
        {
            return (LabelPrintingParam)System.Web.HttpContext.Current.Session["LabelPrintingParam"];

        }
        public static void SetCookie(byte[] arr)
        {
            System.Web.HttpContext.Current.Session["sessionVariable"] = arr;
        }
        public static byte[] GetCookie()
        {
            return (byte[])System.Web.HttpContext.Current.Session["sessionVariable"];
        }
        public static void SetEmpName(string Name)
        {
            System.Web.HttpContext.Current.Session["EN"] = Name;
        }
        public static string GetEmpName()
        {
            return (string)System.Web.HttpContext.Current.Session["EN"];
        }
        //public static void SetSideMenu(SideMenus menus)
        //{
        //    System.Web.HttpContext.Current.Session["SideMenu"] = menus;
        //}
        //public static SideMenus GetSideMenu()
        //{
        //    return (SideMenus)System.Web.HttpContext.Current.Session["SideMenu"];
        //}
        public static void ResetSideMenu()
        {
            System.Web.HttpContext.Current.Session["SideMenu"] = null;
        }
        public static void ResetCookies()
        {
            System.Web.HttpContext.Current.Session["SideMenu"] = null;
            System.Web.HttpContext.Current.Session["sessionVariable"] = null;
            System.Web.HttpContext.Current.Session["EN"] = null;
        }
        public static void SetCustomerLogin(byte[] login)
        {
            System.Web.HttpContext.Current.Session["sessionVariable"] = login;
        }
        public static byte[] GetCustomerLogin()
        {
            return (byte[])System.Web.HttpContext.Current.Session["sessionVariable"];
        }
        public static void SetProfileImage(string photo)
        {
            System.Web.HttpContext.Current.Session["profilePic"] = photo;
        }
        public static string GetProfileImage()
        {
            return System.Web.HttpContext.Current.Session["profilePic"]?.ToString();
        }

        public static OpeningInvoiceSearch GetOpeningInvoiceSearch()
        {
            return (OpeningInvoiceSearch)System.Web.HttpContext.Current.Session["OpeningInvoiceSearch"];
        }
        public static void SetOpeningInvoiceSearch(OpeningInvoiceSearch reportparam)
        {
            System.Web.HttpContext.Current.Session["OpeningInvoiceSearch"] = reportparam;
        }
    }
}