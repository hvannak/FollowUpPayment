using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.BQL;
using RestSharp;
using System.Net;
using Newtonsoft.Json;
using static PX.Data.BQL.BqlPlaceholder;
using PX.SM;
using System.Configuration;
using PX.Reports;
using PX.Reports.Data;
using PX.Data.Reports;
using static PX.SM.Warden;
using static PX.Objects.RQ.RQRequestLine.FK;
using PX.Data.Licensing;
using P = PX.Data.BQL.P;
using Sales;
using PX.Objects.CR;

namespace PX.Objects.AR
{
    // Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
    public class CustomerMaint_Extension : PXGraphExtension<PX.Objects.AR.CustomerMaint>
  {
        [InjectDependency]
        protected IReportLoaderService ReportLoader { get; private set; }

        [InjectDependency]
        protected internal PX.Reports.IReportDataBinder ReportDataBinder { get; private set; }
        #region Event Handlers

        public PXAction<PX.Objects.AR.Customer> MyFollowUpPayment;

        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Follow Up Payment")]
        protected void myFollowUpPayment()
        {
            //Report Paramenters
            Dictionary<String, String> parameters = new Dictionary<String, String>();
            parameters["CustomerID"] = Base.BAccount.Current.AcctCD;
            //Report Processing
            PXReportSettings settings = new PXReportSettings("AR631000");
            PX.Reports.Controls.Report _report = ReportLoader.CheckIfNull(nameof(ReportLoader)).LoadReport("AR631000", null);
            ReportLoader.InitReportParameters(_report, parameters, settings, false);
            PX.Reports.Data.ReportNode reportNode = ReportDataBinder.CheckIfNull(nameof(ReportDataBinder)).ProcessReportDataBinding(_report);
            //Generation PDF
            byte[] data = PX.Reports.Mail.Message.GenerateReport(reportNode, RenderType.FilterPdf).First();
            PX.SM.FileInfo file = new PX.SM.FileInfo("AR" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.ToString("HHmmss") + ".pdf", null, data);
            var uploadFileMaintenance = PXGraph.CreateInstance<UploadFileMaintenance>();
            uploadFileMaintenance.SaveFile(file);
            PXNoteAttribute.AttachFile(Base.BAccount.Cache, Base.BAccount.Current, file);
            //Downloading of the report
            //throw new PXRedirectToFileException(file, true);
            //===============================================


            //get chat id from data set
            PX.Objects.CR.Contact contactPrimary = SelectFrom<PX.Objects.CR.Contact>.InnerJoin<Customer>.On<PX.Objects.CR.Contact.contactID.IsEqual<Customer.primaryContactID>> .Where<Customer.bAccountID.IsEqual<@P.AsInt>> .View.Select(Base, Base.BAccount.Current.BAccountID).TopFirst;
            if (contactPrimary != null)
            {
                string c_chat_id = contactPrimary.Fax;
                if(!string.IsNullOrEmpty(c_chat_id))
                    SendByTelegram(c_chat_id, data, file.FullName, "Customer Outstanding Balance Of " + Base.BAccount.Current.AcctName);
            }
            PXResultset<SalesPerson> listSalesPerson = SelectFrom<SalesPerson>.InnerJoin<CustSalesPeople>.On<SalesPerson.salesPersonID.IsEqual<CustSalesPeople.salesPersonID>>.Where<CustSalesPeople.bAccountID.IsEqual<@P.AsInt>>.View.Select(Base, Base.BAccount.Current.BAccountID);
            //var bAccount = SelectFrom<BAccount>.Where<BAccount.bAccountID.IsEqual<@P.AsInt>>.View.Select(Base, gltran.ReferenceID).TopFirst;
            foreach (var item in listSalesPerson)
            {
                SalesPerson salesPerson = (SalesPerson)item;
                var salesPersonExt = salesPerson.GetExtension<SalesPersonExt>();
                string chat_id = salesPersonExt.UsrBotID.ToString();
                if(!string.IsNullOrEmpty(chat_id))
                    SendByTelegram(chat_id, data, file.FullName,"Customer Outstanding Balance Of " + Base.BAccount.Current.AcctName);
                SalesPerson salesPersonSup = SelectFrom<SalesPerson>.Where<SalesPerson.salesPersonID.IsEqual<@P.AsInt>>.View.Select(Base, salesPersonExt.UsrSupervisorID);
                if(salesPersonSup != null)
                {
                    var salesPersonSupExt = salesPersonSup.GetExtension<SalesPersonExt>();
                    string sup_chat_id = salesPersonSupExt.UsrBotID.ToString();
                    if(!string.IsNullOrEmpty(sup_chat_id))
                        SendByTelegram(sup_chat_id, data, file.FullName, "Customer Outstanding Balance Of " + Base.BAccount.Current.AcctName);
                }

            }

            //send cc 
            ARSetup aRSetup = Base.ARSetup.Current;
            ARSetupExt aRSetupExt = aRSetup.GetExtension<ARSetupExt>();
            string telCC = aRSetupExt.UsrTelegramCC.ToString();
            if (telCC.Contains(";"))
            {
                string[] listCC = telCC.Split(';');
                foreach (var cc_chat_id in listCC)
                {
                    if(!string.IsNullOrEmpty(cc_chat_id))
                        SendByTelegram(cc_chat_id, data, file.FullName, "Customer Outstanding Balance Of " + Base.BAccount.Current.AcctName);
                }
            }
            else
            {
                if(!string.IsNullOrEmpty(telCC))
                    SendByTelegram(telCC, data, file.FullName, "Customer Outstanding Balance Of " + Base.BAccount.Current.AcctName);
            }

        }

        private void SendByTelegram(string chat_id, byte[] data, string fileName,string caption)
        {
            var restClient = new RestClient() { Timeout = -1 };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var loginRequest = new RestRequest(new Uri("https://api.telegram.org/bot5252109221:AAEx28rf7PZ6W5l6Ii0FLTWM6IT3rFhYsoQ/sendDocument?chat_id=" + chat_id + "&caption=" + caption), Method.POST)
            {
                AlwaysMultipartFormData = true,
            };
            //loginRequest.AddFile("document", @"E:\ACELIDA Bank\Amendment_to off line.pdf");
            //loginRequest.AddFileBytes("document", data, file.FullName, "application/pdf");
            loginRequest.AddFileBytes("document", data, fileName, "application/pdf");
            var loginResponse = restClient.Execute(loginRequest);
            if (!loginResponse.IsSuccessful)
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                // Acuminator disable once PX1051 NonLocalizableString [Justification]
                throw new PXException(loginResponse.Content);
        }

        #endregion
    }
}