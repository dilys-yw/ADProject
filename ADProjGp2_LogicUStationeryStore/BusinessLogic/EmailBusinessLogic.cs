using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ADProjGp2_LogicUStationeryStore.BusinessLogic
{
    public class EmailBusinessLogic : Controller
    {
        string returnMessage = "";
        string opcode = "";
        string urlPartial = ConstantsConfig.ipaddress;
        public bool SendEmail(string option, string recipient, string sender, string deptID, string status, string linkref)
        {
            switch(option)
            {
                case "submitNewReq":
                    opcode = "newreq";
                    returnMessage = EmailSubmitNewReq(opcode, recipient, sender, linkref);
                    return true;
                case "updateReqStatus":
                    opcode = "reqstatus";
                    returnMessage = EmailSubmitStatusReq(opcode, recipient, sender, status, linkref);
                    return true;
                case "submitAdj":
                    opcode = "submitAdj";
                    returnMessage = EmailSubmitNewAdj(opcode, recipient, sender, linkref);
                    return true;
                case "approveAdj":
                    opcode = "approveAdj";
                    returnMessage = EmailApproveAdj(opcode, recipient, sender, status, linkref);
                    return true;
                case "sendStoreClerkReq":
                    opcode = "storereq";
                    returnMessage = EmailSendStoreReq(opcode, recipient, deptID, linkref);
                    return true;
                case "assignRep":
                    opcode = "assignRep";
                    returnMessage = EmailAssignRep(opcode, recipient, sender);
                    return true;
                case "removeRep":
                    opcode = "removeRep";
                    returnMessage = EmailRemoveRep(opcode, recipient, sender);
                    return true;
                case "assignAuth":
                    opcode = "assignAuth";
                    returnMessage = EmailAssignAuth(opcode, recipient, sender);
                    return true;
                case "removeAuth":
                    opcode = "removeAuth";
                    returnMessage = EmailRemoveAuth(opcode, recipient, sender);
                    return true;
                case "collectDisburse":
                    opcode = "collectDisburse";
                    returnMessage = EmailCollectDisburse(opcode, deptID, linkref);
                    return true;
                case "confirmDisburse":
                    opcode = "confirmDisburse";
                    returnMessage = EmailConfirmDisburse(opcode, sender, deptID,linkref);
                    return true;
                default:
                    return false;
            }
        }

        //Email Head when Requisition is submitted for approval
        public string EmailSubmitNewReq(string opcode, string recipient, string sender, string linkref)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient",recipient);
                    wc.QueryString.Add("sender", sender);
                    wc.QueryString.Add("deptId", null);
                    wc.QueryString.Add("status", null);
                    wc.QueryString.Add("linkref", linkref);
                    var data = wc.UploadValues(address,"POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

        //Email user when requisition is rejected or approved
        public string EmailSubmitStatusReq(string opcode, string recipient, string sender, string status, string linkref)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient", recipient);
                    wc.QueryString.Add("sender", sender);
                    wc.QueryString.Add("deptId", null);
                    wc.QueryString.Add("status", status);
                    wc.QueryString.Add("linkref", linkref);
                    var data = wc.UploadValues(address, "POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

        //Email store clerk when requisition is approved
        public string EmailSendStoreReq(string opcode, string recipient, string deptId, string linkref)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient", recipient);
                    wc.QueryString.Add("sender", null);
                    wc.QueryString.Add("deptId", deptId);
                    wc.QueryString.Add("status", null);
                    wc.QueryString.Add("linkref", linkref);
                    var data = wc.UploadValues(address, "POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

        public string EmailSubmitNewAdj(string opcode, string recipient, string sender, string linkref)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient", recipient);
                    wc.QueryString.Add("sender", sender);
                    wc.QueryString.Add("deptId", null);
                    wc.QueryString.Add("status", null);
                    wc.QueryString.Add("linkref", linkref);
                    var data = wc.UploadValues(address, "POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

        public string EmailApproveAdj(string opcode, string recipient, string sender, string status, string linkref)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient", recipient);
                    wc.QueryString.Add("sender", sender);
                    wc.QueryString.Add("deptId", null);
                    wc.QueryString.Add("status", status);
                    wc.QueryString.Add("linkref", linkref);
                    var data = wc.UploadValues(address, "POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

        public string EmailAssignRep(string opcode, string recipient, string sender)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient", recipient);
                    wc.QueryString.Add("sender", sender);
                    wc.QueryString.Add("deptId", null);
                    wc.QueryString.Add("status", null);
                    wc.QueryString.Add("linkref", null);
                    var data = wc.UploadValues(address, "POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

        public string EmailRemoveRep(string opcode, string recipient, string sender)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient", recipient);
                    wc.QueryString.Add("sender", sender);
                    wc.QueryString.Add("deptId", null);
                    wc.QueryString.Add("status", null);
                    wc.QueryString.Add("linkref", null);
                    var data = wc.UploadValues(address, "POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

        public string EmailAssignAuth(string opcode, string recipient, string sender)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient", recipient);
                    wc.QueryString.Add("sender", sender);
                    wc.QueryString.Add("deptId", null);
                    wc.QueryString.Add("status", null);
                    wc.QueryString.Add("linkref", null);
                    var data = wc.UploadValues(address, "POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

        public string EmailRemoveAuth(string opcode, string recipient, string sender)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient", recipient);
                    wc.QueryString.Add("sender", sender);
                    wc.QueryString.Add("deptId", null);
                    wc.QueryString.Add("status", null);
                    wc.QueryString.Add("linkref", null);
                    var data = wc.UploadValues(address, "POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

        public string EmailCollectDisburse(string opcode, string deptID, string linkref)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient", null);
                    wc.QueryString.Add("sender", null);
                    wc.QueryString.Add("deptId", deptID);
                    wc.QueryString.Add("status", null);
                    wc.QueryString.Add("linkref", linkref);
                    var data = wc.UploadValues(address, "POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

        public string EmailConfirmDisburse(string opcode, string sender, string deptID, string linkref)
        {
            string statusMessage = "";
            string address = "http://" + urlPartial + "/EmailService/API/EmailNotifier/" + opcode;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.QueryString.Add("recipient", null);
                    wc.QueryString.Add("sender", sender);
                    wc.QueryString.Add("deptId", deptID);
                    wc.QueryString.Add("status", null);
                    wc.QueryString.Add("linkref", linkref);
                    var data = wc.UploadValues(address, "POST", wc.QueryString);
                    try
                    {
                        statusMessage = UnicodeEncoding.UTF8.GetString(data);
                        statusMessage = JsonConvert.DeserializeObject<string>(statusMessage);
                    }
                    catch (Exception e)
                    {
                        return statusMessage + e;
                    }
                    return statusMessage;
                }
            }
            catch (Exception e)
            {
                return statusMessage + e;
            }
        }

    }
}