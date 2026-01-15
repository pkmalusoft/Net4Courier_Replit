using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using System.Data;

namespace Net4Courier.Models
{
    
    public class EditCommanFu
    {

        public int EditRecpayDetails(int recpayid, decimal amount, int cueencyid, string remark)
        {
            using (Entities1 Context1 = new Entities1())
            {
                var obj = (from t in Context1.RecPayDetails where t.RecPayID == recpayid && t.InvoiceID == 0 select t).FirstOrDefault();
                //var obj = Context1.RecPayDetails.Find(recpayid,0);
                RecPayDetail recpdetail = new RecPayDetail();
                recpdetail.RecPayDetailID = obj.RecPayDetailID;
                recpdetail.Amount = -(amount);
                recpdetail.CurrencyID = cueencyid;
                //recpd.InvDate = item.InvoiceDate.Value;
                recpdetail.RecPayID = obj.RecPayID;
                recpdetail.Remarks = remark;
                recpdetail.InvoiceID = 0;
                recpdetail.StatusInvoice = obj.StatusInvoice;
                recpdetail.InvDate = obj.InvDate;
                recpdetail.InvNo = obj.InvNo;
                recpdetail.Lock = false;
                Context1.Entry(obj).CurrentValues.SetValues(recpdetail);
               // Context1.Entry(recpdetail).State = EntityState.Modified;
                Context1.SaveChanges();
                return 1;
            }
        }

        public int EditAcJDetails(int acjournalId,decimal amount)
        {
            using (Entities1 Context1 = new Entities1())
            {
                var query = (from t in Context1.AcJournalDetails where t.AcJournalID == acjournalId select t).ToList();
                foreach (var itme1 in query)
                {
                    AcJournalDetail acjD = new AcJournalDetail();
                    acjD.AcJournalDetailID = itme1.AcJournalDetailID;
                    acjD.AcJournalID = itme1.AcJournalID;
                    //acjD.AcHeadID = itme1.AcHeadID;
                    acjD.AcHeadID = itme1.AcHeadID;
                    if (itme1.Amount < 0)
                    {
                        acjD.Amount = -(amount);
                    }
                    else
                    {
                        acjD.Amount = (amount);
                    }
                    acjD.BranchID = itme1.BranchID;
                    acjD.ID = itme1.ID;
                    Context1.Entry(itme1).CurrentValues.SetValues(acjD);
                    Context1.SaveChanges();
                }


                return 1;
            }
        }

             public int EditRecpayDetailsCustR(int recpayid, decimal amount)
        {
            using (Entities1 Context1 = new Entities1())
            {
                var obj = (from t in Context1.RecPayDetails where t.RecPayID == recpayid && t.InvoiceID == 0 select t).FirstOrDefault();
                //var obj = Context1.RecPayDetails.Find(recpayid,0);
                if (obj != null)
                {
                    RecPayDetail recpdetail = new RecPayDetail();
                    recpdetail.RecPayDetailID = obj.RecPayDetailID;
                    recpdetail.Amount = -(amount);
                    // recpdetail.CurrencyID = cueencyid;
                    //recpd.InvDate = item.InvoiceDate.Value;
                    recpdetail.RecPayID = obj.RecPayID;
                    // recpdetail.Remarks = remark;
                    recpdetail.InvoiceID = 0;
                    recpdetail.StatusInvoice = obj.StatusInvoice;
                    recpdetail.InvDate = obj.InvDate;
                    recpdetail.InvNo = obj.InvNo;
                    recpdetail.Lock = false;
                    Context1.Entry(obj).CurrentValues.SetValues(recpdetail);
                    // Context1.Entry(recpdetail).State = EntityState.Modified;
                    Context1.SaveChanges();
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
        }
    }
