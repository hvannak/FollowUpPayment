using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using FollowUpPayment;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.AR;
using PX.Objects.SO;
using PX.Reports;
using PX.Reports.Data;
using PX.SM;
using static PX.SM.EMailAccount;

namespace PX.Objects.SO
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public class SOOrderEntry_Extension : PXGraphExtension<PX.Objects.SO.SOOrderEntry>
  {
        [InjectDependency]
        protected IReportLoaderService ReportLoader { get; private set; }

        [InjectDependency]
        protected internal PX.Reports.IReportDataBinder ReportDataBinder { get; private set; }

        #region Event Handlers
        public delegate IEnumerable ReleaseFromHoldDelegate(PXAdapter adapter);
    [PXOverride]
    public IEnumerable ReleaseFromHold(PXAdapter adapter, ReleaseFromHoldDelegate baseMethod)
    {
            try {
                if (PXAccess.GetCompanyName() == "MD" || PXAccess.GetCompanyName() == "AG")
                {
                    if (Base.Document.Current.OrderType != "CM" && Base.Document.Current.OrderType != "DM")
                    {
                        SOOrderShipment sOOrderShipment = SelectFrom<SOOrderShipment>.Where<SOOrderShipment.orderNbr.IsEqual<@P.AsString>.And<SOOrderShipment.orderType.IsEqual<@P.AsString>>>.View.Select(Base, Base.Document.Current.RefNbr, Base.Document.Current.OrderType);
                        if(sOOrderShipment == null)
                        {
                            //Send By SMS
                            //PX.Objects.CR.Contact contactDefault = SelectFrom<PX.Objects.CR.Contact>.InnerJoin<Customer>.On<PX.Objects.CR.Contact.contactID.IsEqual<Customer.defContactID>>.Where<Customer.bAccountID.IsEqual<@P.AsInt>>.View.Select(Base, Base.Document.Current.CustomerID).TopFirst;
                            //if (contactDefault != null)
                            //{
                            //    if (!contactDefault.Phone1.StartsWith("+") && !string.IsNullOrEmpty(contactDefault.Phone1))
                            //    {
                            //        //get setting
                            //        ARSetup aRSetup = SelectFrom<ARSetup>.View.Select(Base).TopFirst;
                            //        ARSetupExt aRSetupExt = aRSetup.GetExtension<ARSetupExt>();
                            //        string tmp = "";
                            //        string totalOrder = string.Format("{0:N2}", Base.Document.Current.OrderTotal);
                            //        string textmsg = $"Your Order Nbr: {Base.Document.Current.RefNbr}, Total Order: {totalOrder} + {Base.Document.Current.CuryID}";
                            //        string phoneNumber = Helper.ValidatePhone(contactDefault.Phone1);
                            //        //string urlSms = "https://client.mekongsms.com/api/sendsms.aspx?username=agri_master@mekongnet&pass=96e79218965eb72c92a549dd5a330112&sender=Medivet&smstext=" + textmsg + "&isflash=0&gsm=" + phoneNumber;
                            //        string urlSms = aRSetupExt.UsrSmsurl + "?username=" + aRSetupExt.UsrSmsuser + "&pass=" + aRSetupExt.UsrSmspass + "&sender=" + aRSetupExt.UsrSmssender + "&smstext=" + textmsg + "&isflash=0&gsm=" + phoneNumber;
                            //        Uri uri = new Uri(urlSms);
                            //        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
                            //        request.Method = WebRequestMethods.Http.Get;
                            //        request.ContentType = "application/x-www-form-urlencoded";
                            //        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            //        StreamReader reader = new StreamReader(response.GetResponseStream());
                            //        //tmp will be content that was returned by mekongsms
                            //        tmp = reader.ReadToEnd();
                            //        response.Close();
                            //    }
                            //}

                            //Send By Telegram
                            //get chat id from data set
                            PX.Objects.CR.Contact contactPrimary = SelectFrom<PX.Objects.CR.Contact>.InnerJoin<Customer>.On<PX.Objects.CR.Contact.contactID.IsEqual<Customer.primaryContactID>>.Where<Customer.bAccountID.IsEqual<@P.AsInt>>.View.Select(Base, Base.Document.Current.CustomerID).TopFirst;
                            if (contactPrimary != null)
                            {
                                string c_chat_id = contactPrimary.Fax;
                                if (!string.IsNullOrEmpty(c_chat_id))
                                {
                                    StringBuilder sb = new StringBuilder();
                                    //sb.Append($"<strong>Your Order Number: {Base.Document.Current.RefNbr}</strong>\n");
                                    //sb.Append($"<code>Total Quantity: {Base.Document.Current.OrderQty} </code>\n");
                                    //sb.Append($"<code>Total Order: {Base.Document.Current.OrderTotal} {Base.Document.Current.CuryID} </code>\n");
                                    string qtyStr = string.Format("{0:N2}", Base.Document.Current.OrderQty);
                                    string totalOrder = string.Format("{0:N2}", Base.Document.Current.OrderTotal);
                                    sb.Append($"<pre>Your Order Number: {Base.Document.Current.RefNbr} \n Total Quantity: {qtyStr} \n Total Order: {totalOrder} {Base.Document.Current.CuryID} </pre>");
                                    Helper.SendByTelegram(c_chat_id, sb.ToString());
                                }
                            }

                            SOOrderExt rowExt = PXCache<SOOrder>.GetExtension<SOOrderExt>(Base.Document.Current);
                            if (rowExt.UsrDefCompanyTemplate != null)
                            {
                                if (Base.Document.Current.OrderNbr != null)
                                {
                                   PXResultset<CustSalesPeople> custSalesPeople = SelectFrom<CustSalesPeople>.Where<CustSalesPeople.bAccountID.IsEqual<@P.AsInt>>.View.Select(Base, Base.Document.Current.CustomerID);
                                    if (custSalesPeople != null)
                                    {
                                        foreach(var item in custSalesPeople)
                                        {
                                            CustSalesPeople custSalesPeople1 = (CustSalesPeople)item;
                                            SalesPerson salesPerson = SelectFrom<SalesPerson>.Where<SalesPerson.salesPersonID.IsEqual<@P.AsInt>>.View.Select(Base, custSalesPeople1.SalesPersonID).TopFirst;
                                            if (salesPerson != null)
                                            {
                                                SalesPersonExt salesPersonExt = salesPerson.GetExtension<SalesPersonExt>();
                                                //Report Paramenters
                                                Dictionary<String, String> parameters = new Dictionary<String, String>();
                                                parameters["OrderType"] = Base.Document.Current.OrderType;
                                                parameters["RefNbr"] = Base.Document.Current.OrderNbr;
                                                //Report Processing
                                                PXReportSettings settings = new PXReportSettings(rowExt.UsrDefReport);
                                                PX.Reports.Controls.Report _report = ReportLoader.CheckIfNull(nameof(ReportLoader)).LoadReport(rowExt.UsrDefReport, null);
                                                ReportLoader.InitReportParameters(_report, parameters, settings, false);
                                                PX.Reports.Data.ReportNode reportNode = ReportDataBinder.CheckIfNull(nameof(ReportDataBinder)).ProcessReportDataBinding(_report);
                                                //Generation PDF
                                                byte[] data = PX.Reports.Mail.Message.GenerateReport(reportNode, RenderType.FilterPdf).First();

                                                if (!salesPersonExt.UsrBotID.IsNullOrEmpty())
                                                {
                                                    Helper.SendByTelegramWithFile(salesPersonExt.UsrBotID, data, Base.Document.Current.OrderNbr, "Sales Order: " + Base.Document.Current.OrderNbr);
                                                }
                                                if (salesPersonExt.UsrSupervisorID != null)
                                                {
                                                    SalesPerson supervisor = SelectFrom<SalesPerson>.Where<SalesPerson.salesPersonID.IsEqual<@P.AsInt>>.View.Select(Base, salesPersonExt.UsrSupervisorID).TopFirst;
                                                    if(supervisor != null)
                                                    {
                                                        SalesPersonExt supervisorExt = supervisor.GetExtension<SalesPersonExt>();
                                                        if (!supervisorExt.UsrBotID.IsNullOrEmpty())
                                                        {
                                                            Helper.SendByTelegramWithFile(supervisorExt.UsrBotID, data, Base.Document.Current.OrderNbr, "Sales Order: " + Base.Document.Current.OrderNbr);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        
                                  }
                                }
                            }

                        }                       
                    }
                }
                return baseMethod(adapter);
            } catch(PXException e)
            {
                throw e;
            }
    }

        protected void _(Events.RowUpdated<SOOrder> e)
        {
            var row = (SOOrder)e.Row;
            var oldRow = (SOOrder)e.OldRow;

            if (oldRow.Approved == false && row.Approved == true)
            {
                SOOrderExt rowExt = PXCache<SOOrder>.GetExtension<SOOrderExt>(row);
                if (rowExt.UsrDefCompanyTemplate != null)
                {
                    if (row.OrderNbr != null)
                    {
                        PXResultset<CustSalesPeople> custSalesPeople = SelectFrom<CustSalesPeople>.Where<CustSalesPeople.bAccountID.IsEqual<@P.AsInt>>.View.Select(Base, Base.Document.Current.CustomerID);
                        if (custSalesPeople != null)
                        {
                            foreach(var item in custSalesPeople)
                            {
                                CustSalesPeople custSalesPeople1 = (CustSalesPeople)item;
                                SalesPerson salesPerson = SelectFrom<SalesPerson>.Where<SalesPerson.salesPersonID.IsEqual<@P.AsInt>>.View.Select(Base, custSalesPeople1.SalesPersonID).TopFirst;
                                if (salesPerson != null)
                                {
                                    SalesPersonExt salesPersonExt = salesPerson.GetExtension<SalesPersonExt>();
                                    if (!salesPersonExt.UsrBotID.IsNullOrEmpty())
                                    {
                                        Helper.SendByTelegram(salesPersonExt.UsrBotID, row.OrderNbr + " is approved");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        #endregion
    }
}