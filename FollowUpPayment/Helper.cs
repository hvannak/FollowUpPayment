using Newtonsoft.Json;
using PX.Data;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FollowUpPayment
{
    public static class Helper
    {
        public static void SendByTelegramWithFile(string chat_id, byte[] data, string fileName, string caption)
        {
            var restClient = new RestClient() { Timeout = -1 };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var loginRequest = new RestRequest(new Uri("https://api.telegram.org/bot5252109221:AAEx28rf7PZ6W5l6Ii0FLTWM6IT3rFhYsoQ/sendDocument?chat_id=" + chat_id + "&caption=" + caption), Method.POST)
            {
                AlwaysMultipartFormData = true,
            };
            //loginRequest.AddFile("document", @"E:\ACELIDA Bank\Amendment_to off line.pdf");
            //loginRequest.AddFileBytes("document", data, file.FullName, "application/pdf");
            loginRequest.AddFileBytes("document", data, fileName + ".pdf", "application/pdf");
            var loginResponse = restClient.Execute(loginRequest);
            if (!loginResponse.IsSuccessful)
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                // Acuminator disable once PX1051 NonLocalizableString [Justification]
                //throw new PXException(loginResponse.Content);
                return;
        }

        public static void SendByTelegram(string chat_id, string text_data)
        {
            var restClient = new RestClient() { Timeout = -1 };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var loginRequest = new RestRequest(new Uri("https://api.telegram.org/bot5252109221:AAEx28rf7PZ6W5l6Ii0FLTWM6IT3rFhYsoQ/sendMessage"), Method.POST);
            loginRequest.AddHeader("accept", "application/json");
            loginRequest.AddJsonBody(JsonConvert.SerializeObject(new { chat_id = chat_id, text = text_data, parse_mode = "html" }));
            var loginResponse = restClient.Execute(loginRequest);
            if (!loginResponse.IsSuccessful)
                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                // Acuminator disable once PX1051 NonLocalizableString [Justification]
                //throw new PXException(loginResponse.Content);
                return;
        }

        public static String ValidatePhone(string phoneNumber)
        {
            try
            {
                if (phoneNumber != null)
                {
                    int index = -1;
                    string realPhoneNumber = "+855";
                    if (phoneNumber.Contains('/'))
                    {
                        index = phoneNumber.IndexOf('/');
                        realPhoneNumber = realPhoneNumber + phoneNumber.Substring(1, index-1);
                    }
                    else if (phoneNumber.Contains(','))
                    {
                        index = phoneNumber.IndexOf(',');
                        realPhoneNumber = realPhoneNumber + phoneNumber.Substring(1, index-1);
                    }
                    else if (phoneNumber.Contains(';'))
                    {
                        index = phoneNumber.IndexOf(';');
                        realPhoneNumber = realPhoneNumber + phoneNumber.Substring(1, index - 1);
                    }
                    else
                    {
                        realPhoneNumber = realPhoneNumber + phoneNumber.Substring(1, phoneNumber.Length -1);
                    }
                    return realPhoneNumber.Replace(" ", string.Empty);
                }
                else
                {
                    return phoneNumber;
                }
            }
            catch (PXException ex)
            {

                // Acuminator disable once PX1050 HardcodedStringInLocalizationMethod [Justification]
                // Acuminator disable once PX1051 NonLocalizableString [Justification]
                throw new PXException(ex.Message);
            }
            
        }
    }
}
