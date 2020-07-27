using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DocumentsUpload.Business.Object.Main.Model;
using Momentuum.DataOperations;

namespace DocumentsUpload.Business.Object.Main
{
    public class DocsDBOps : DBOperation
    {
        readonly string con = ConfigurationManager.AppSettings["Fileassure"].ToString();
        public DocsURL GenerateLink(DocsURL docsURL, Client client, string GUID)
        {
            string sp_name = "sp_AddDocsURL";            
            DocsURL uRL = new DocsURL();
            string SiteURL = ConfigurationManager.AppSettings["SiteURL"].ToString();
            var URL = SiteURL + "login/DocumentsUpload/index.html?id=" + GUID;
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "@ClientId", docsURL.ClientId },
                { "@Division", docsURL.Division},
                { "@GUID", GUID},
                { "@CheckListId", docsURL.CheckListId},
                { "@IPAddress", docsURL.RequestIP},
                { "@RequestedBy", docsURL.RequestedBy},
                { "@Subject", FormatEmail(docsURL.Subject, client, URL).Replace("\n", "<br />")},
                { "@Body", FormatEmail(docsURL.Body, client, URL).Replace("\n", "<br />")},
                { "@ToEmail", docsURL.ToEmail},
            };
            DataTable dt = InitializeAndExecute<DataTable>(sp_name, dict, Operation.select);

            if(dt.Rows.Count == 1)
            {
                uRL = new DocsURL(dt.Rows[0]);
            }

            return uRL;
        }

        public void DocsURLUpload(List<DocsURLUpload> uploads, int DocsURLId)
        {
            foreach(var upload in uploads)
            {
                using (SqlConnection conn = new SqlConnection(con))
                {
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandTimeout = 3000;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_AddDocsURLUpload";
                    cmd.Parameters.Add(new SqlParameter("@Folder", upload.Folder));
                    cmd.Parameters.Add(new SqlParameter("@DocsURLId", DocsURLId));
                    cmd.Parameters.Add(new SqlParameter("@Prefix", upload.Prefix));
                    cmd.Parameters.Add(new SqlParameter("@Comment", upload.Comment));
                    cmd.Parameters.Add(new SqlParameter("@Condition", upload.Condition));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public DocsURL FindByGUID(string GUID)
        {
            DocsURL uRL = new DocsURL();
            string sp_name = "SELECT t1.*,t2.EmployeeName AS RequestedByName FROM tbl_DocsUrl t1 JOIN tbl_Employees t2 ON t1.GUID = @GUID AND t1.RequestedBy = t2.Employee_ID";
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "@GUID", GUID }
            };
            DataTable dt = InitializeAndExecute<DataTable>(sp_name, dict, Operation.select, false);
            if (dt.Rows.Count == 1)
            {
                uRL = new DocsURL(dt.Rows[0]);
            }
            return uRL;
        }

        public List<DocsURLUpload> GetRowsByDocURLId(int id)
        {
            List<DocsURLUpload> docsURLUploads = new List<DocsURLUpload>();
            string sp_name = "select * from tbl_DocsUrl_Upload t1 left join tbl_DocsUrl_Log t2 on t1.Id = t2.DocsUrl_Upload_Id where DocURLId = @ID and t2.IsUploaded is null";
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "@ID", id }
            };
            DataTable dt = InitializeAndExecute<DataTable>(sp_name, dict, Operation.select, false);
            if (dt.Rows.Count > 0)
            {
                foreach(DataRow dr in dt.Rows)
                {
                    docsURLUploads.Add(new DocsURLUpload(dr));
                }
            }
            return docsURLUploads;
        }

        public void LogLinkAccess(DocsUrlAccess docsUrlAccess)
        {
            using (SqlConnection conn = new SqlConnection(con))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = 3000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_AddDocsURLAccess";
                cmd.Parameters.Add(new SqlParameter("@DocURLId", docsUrlAccess.DocURLId));
                cmd.Parameters.Add(new SqlParameter("@IPAddress", docsUrlAccess.IPAddress));
                cmd.Parameters.Add(new SqlParameter("@BrowserName", docsUrlAccess.BrowserName));
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public string FindFileNameById(int ClientId)
        {
            string Filename = string.Empty;
            string sp_name = "select filename from tbl_client where id = @ClientId";
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "@ClientId", ClientId }
            };
            DataTable dt = InitializeAndExecute<DataTable>(sp_name, dict, Operation.select, false);
            if (dt.Rows.Count == 1)
            {
                Filename = dt.Rows[0][0].ToString();
            }
            return Filename;
        }

        public void LogUploadedFileInfo(DocsURLLog log)
        {
            using (SqlConnection conn = new SqlConnection(con))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = 3000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_AddDocsURLLog";
                cmd.Parameters.Add(new SqlParameter("@DocURLUploadId", log.DocURLUploadId));
                cmd.Parameters.Add(new SqlParameter("@FilesSize", log.FilesSize));
                cmd.Parameters.Add(new SqlParameter("@OriginalFileName", log.OriginalFileName));
                cmd.Parameters.Add(new SqlParameter("@NewFileName", log.NewFileName));
                cmd.Parameters.Add(new SqlParameter("@UploadedIPAddress", log.UploadedIPAddress));
                cmd.Parameters.Add(new SqlParameter("@SecurityScanMSG", log.SecurityScanMSG));
                cmd.Parameters.Add(new SqlParameter("@IsUploaded", log.IsUploaded));
                cmd.Parameters.Add(new SqlParameter("@UserComment", log.UserComment));
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public List<NotifyAgentModel> GetAgentEmailFromGUID(string GUID)
        {
            string EmailAddress = string.Empty;
            List<NotifyAgentModel> notifyAgent = new List<NotifyAgentModel>();
            string sp_name = "sp_GetUploadedDocs";
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "@GUID", GUID }
            };
            DataTable dt = InitializeAndExecute<DataTable>(sp_name, dict, Operation.select, true);
            if (dt.Rows.Count > 0)
            {
                foreach(DataRow row in dt.Rows)
                {
                    notifyAgent.Add(new NotifyAgentModel(row));
                }
            }
            return notifyAgent;
        }

        public void DisabledLinkAfterUpload(string GUID)
        {
            using (SqlConnection conn = new SqlConnection(con))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = 3000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_Disable_Document_Link";
                cmd.Parameters.Add(new SqlParameter("@GUID", GUID));
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public void ActivateLink(string GUID)
        {
            using (SqlConnection conn = new SqlConnection(con))
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandTimeout = 3000;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sp_Activate_Document_Link";
                cmd.Parameters.Add(new SqlParameter("@GUID", GUID));
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public List<DocEmail> GetEmailTemplates(string Division)
        {
            List<DocEmail> emails = new List<DocEmail>();
            string sp_name = "sp_GetDocumentEmails";
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "@Division", Division }
            };
            DataTable dt = InitializeAndExecute<DataTable>(sp_name, dict, Operation.select, true);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    emails.Add(new DocEmail(row));
                }
            }
            return emails;
        }

        public Client FindClientById(int ClientId, string Division)
        {
            Client client = new Client();
            string sp_name = "sp_GetClientByID";
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "@ClientId", ClientId },
                { "@Division", Division }
            };
            DataTable dt = InitializeAndExecute<DataTable>(sp_name, dict, Operation.select, true);
            if (dt.Rows.Count == 1)
            {
                client = new Client(dt.Rows[0]);
            }
            return client;
        }

        public string FormatEmail(string Text, Client client, string URL)
        {
            if (!string.IsNullOrWhiteSpace(Text))
            {
                Text = Text.Replace("\"@Firstname\"", client.Firstname);
                Text = Text.Replace("\"@Middlename\"", client.Middlename);
                Text = Text.Replace("\"@Lastname\"", client.Lastname);
                Text = Text.Replace("\"@Filename\"", client.Filename);
                Text = Text.Replace("\"@Estate#\"", client.EstateNumber);
                Text = Text.Replace("\"@URL\"", "<a href=" + URL + ">Click Here</a>");
            }
            return Text;
        }

        public void LogUnuploadedFile(List<UnuploadedFileLog> unuploadedFiles)
        {
            foreach(var unuploadedFile in unuploadedFiles)
            {
                using (SqlConnection conn = new SqlConnection(con))
                {
                    SqlCommand cmd = conn.CreateCommand();
                    cmd.CommandTimeout = 3000;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_LogUnuploadedFile";
                    cmd.Parameters.Add(new SqlParameter("@DocURLUploadId", unuploadedFile.DocURLUploadId));
                    cmd.Parameters.Add(new SqlParameter("@IPAddress", unuploadedFile.IPAddress));
                    cmd.Parameters.Add(new SqlParameter("@UserComment", unuploadedFile.UserComment));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public List<ClientReportVM> GetClientReportViewModel(int ClientId)
        {
            List<ClientReportVM> reportVM = new List<ClientReportVM>();
            string sp_name = "sp_Requested_Docs_By_ClientId";
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "@clientid", ClientId }
            };
            DataTable dt = InitializeAndExecute<DataTable>(sp_name, dict, Operation.select, true);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    reportVM.Add(new ClientReportVM(row));
                }
            }
            return reportVM;
        }

        public List<CheckList> GetCheckLists(string Division)
        {
            List<CheckList> lists = new List<CheckList>();
            string sp_name = "sp_Get_Docs_Checklist";
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "@Division", Division }
            };
            DataTable dt = InitializeAndExecute<DataTable>(sp_name, dict, Operation.select, true);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    lists.Add(new CheckList(row));
                }
            }
            return lists;
        }

        public List<CheckListItem> GetCheckListsItems(string Division, int ADC_ID, int ClientId)
        {
            List<CheckListItem> lists = new List<CheckListItem>();
            string sp_name = "sp_Get_Docs_Checklist_Items";
            Dictionary<string, object> dict = new Dictionary<string, object>
            {
                { "@Division", Division },
                { "@ADC_ID", ADC_ID },
                { "@ClientId", ClientId }
            };
            DataTable dt = InitializeAndExecute<DataTable>(sp_name, dict, Operation.select, true);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    lists.Add(new CheckListItem(row));
                }
            }
            return lists;
        }
    }
}
