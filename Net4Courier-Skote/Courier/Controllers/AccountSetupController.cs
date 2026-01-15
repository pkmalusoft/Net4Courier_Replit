using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Net4Courier.Models;
using Net4Courier.DAL;

namespace Net4Courier.Controllers
{
    [SessionExpire]
    public class AccountSetupController : Controller
    {
        Entities1 db = new Entities1();
        // GET: AccountSetup
        public ActionResult Index()
        {

            List<AccountSetupMasterVM> model = new List<AccountSetupMasterVM>();
            model = AccountsDAO.GetAccountSetupList();
            return View(model);
        }

        public ActionResult Create(int id=0)
        {

            
            AccountSetup v = new AccountSetup();
            AccountSetupMasterVM vm = new AccountSetupMasterVM();
            if (id==0)
            {
                
                vm.ID = 0;
            }
            else
            {
                v = db.AccountSetups.Find(id);
                vm.ID = v.ID;
                if (v.CreditAccountId != null)
                {
                    
                    vm.CreditAccountId = v.CreditAccountId;
                    var head = db.AcHeads.Find(v.CreditAccountId);
                    if (head != null)
                        vm.CreditHead = head.AcHead1;
                }
                else
                {

                }
                if (v.DebitAccountId!=null)
                {
                    vm.DebitAccountId = v.DebitAccountId;
                    var head = db.AcHeads.Find(v.DebitAccountId);
                    if (head != null)
                        vm.DebitHead = head.AcHead1;
                }
                
                vm.PageName = v.PageName;
                vm.TransType = v.TransType;
                vm.SalesType = v.SalesType;
                vm.ParcelType = v.ParcelType;

            }
            
            return View(vm);
        }

        [HttpPost]
        public ActionResult Create(AccountSetupMasterVM model)
        {

            AccountSetup v = new AccountSetup();
            AccountSetupMasterVM vm = new AccountSetupMasterVM();
            if (model.ID == 0)
            {
                v.PageName = model.PageName;
                v.TransType = model.TransType;
                v.SalesType = model.SalesType;
                v.DebitAccountId=model.DebitAccountId;
                v.CreditAccountId = model.CreditAccountId;
                db.AccountSetups.Add(v);
                db.SaveChanges();
                TempData["SuccessMsg"] = "Account Setup Added Succesfully!";
            }
            else
            {
                v = db.AccountSetups.Find(model.ID);
                v.PageName = model.PageName;
                v.TransType = model.TransType;
                v.SalesType = model.SalesType;
                v.DebitAccountId = model.DebitAccountId;
                v.CreditAccountId = model.CreditAccountId;
                db.Entry(v).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["SuccessMsg"] = "Account Setup Updated Succesfully!";
            }

            return RedirectToAction("Index");

        }

        public ActionResult DeleteConfirmed(int id)
        {
            //int k = 0;
            if (id != 0)
            {
                var item = db.AccountSetups.Find(id);
                if (item != null)
                {
                    db.AccountSetups.Remove(item);
                    db.SaveChanges();
                    TempData["SuccessMsg"] = "Account Setup Deleted Succesfully!";
                    

                }
                else
                {
                    TempData["ErrorMsg"] = "Error at delete";
                }
            }

            return RedirectToAction("Index", "AccountSetup");

        }
    }
}