using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDI
{
    /// <summary>
    /// Send messages for users using MessagesService SAP.
    /// </summary>
    public class Messages
    {
        private string LOG => "SMSSG";

        private readonly SAPbobsCOM.MessagesService MsgService;
        private readonly SAPbobsCOM.Message Msg;
        private readonly SAPbobsCOM.RecipientCollection Address;
        public Messages(SAPbobsCOM.BoMsgPriorities priority, string subject, string text)
        {
            var oComService = Conn.DI.GetCompanyService();
            MsgService = (SAPbobsCOM.MessagesService)oComService.GetBusinessService(SAPbobsCOM.ServiceTypes.MessagesService);
            Msg = (SAPbobsCOM.Message)MsgService.GetDataInterface(SAPbobsCOM.MessagesServiceDataInterfaces.msdiMessage);
            Msg.Priority = SAPbobsCOM.BoMsgPriorities.pr_High;
            Msg.Subject = subject;
            Msg.Text = text;
            Address = Msg.RecipientCollection;
            Address.Add();
        }

        #region Attachement
        public void AddSAPAttach(int id)
        {
            Msg.Attachment = id;
        }

        public int AddSAPAttach(FileInfo file)
        {
            Msg.Attachment = Services.AttachFile(file);
            return Msg.Attachment;
        }
        #endregion
        /// <summary>
        /// Destination
        /// </summary>
        /// <param name="to">E-mail valid or User Code</param>
        /// <param name="sap_internal">send Internal</param>
        /// <param name="sap_email">sent email</param>
        public void To(string to, bool sap_internal = true, bool sap_email = false)
        {
            
            var recipt = Address.Item(Address.Count - 1);
            if(!String.IsNullOrEmpty(recipt.UserCode) || !String.IsNullOrEmpty(recipt.EmailAddress))
            {
                Address.Add();
                recipt = Address.Item(Address.Count - 1);
            }

            var thisEmail = klib.ValuesEx.RegexValidate(to, klib.E.RegexMask.Email) && sap_email;
            var emailto = String.Empty;

            try
            {
                if (sap_email && !thisEmail)
                    emailto = klib.DB.ExtensionDb.First("SELECT E_Mail FROM OUSR WHERE USER_CODE = {0}", to).ValEmail();
            }
            catch(SDIException sdi)
            {
                klib.Shell.WriteLine(R.Project.ID, LOG, sdi);
                sap_email = false;
            }
            
                                 
            recipt.SendInternal = SDI.ValuesEx.YesOrNo(sap_internal);
            recipt.SendEmail = SDI.ValuesEx.YesOrNo(sap_email);
            if (sap_email)
                recipt.EmailAddress = emailto;

            recipt.UserCode = !thisEmail ? to : String.Empty;

        }
                     
        public void Send()
        {
            MsgService.SendMessage(Msg);
        }
    }
}
