using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDI
{

    /// <summary>
    /// Class to connect SAP using DI API.
    /// </summary>
    public class Conn
    {        
        private static string LOG => "CONN";

        /// <summary>
        /// Execute the Init class
        /// </summary>

        /// <summary>
        /// On the last attempt there was a connection error
        /// </summary>
        public static bool Fail { get; protected set; } = false;
        /// <summary>
        /// DI connection
        /// </summary>
        public static SAPbobsCOM.Company DI { get; protected set; }
                
        public static E.DataBase.Types DataBase
        {
            get
            {
                if (DI == null || !DI.Connected)
                    return E.DataBase.Types.None;

                switch((int)DI.DbServerType)
                {
                    case 1: return E.DataBase.Types.MSQL;
                    case 4: return E.DataBase.Types.MSQL;
                    case 6:
                    case 7:
                    case 8: return E.DataBase.Types.MSQL;
                    case 9: return E.DataBase.Types.Hana;
                    case 10: return E.DataBase.Types.MSQL;
                    default: throw new SDIException(1, $"SDI: Database {DI.DbServerType.ToString()} isn't defined");
                }
            }
        }

        #region Connections
        public static void Connect(int serverType, string server, string database, string user, string passwd, string dbuser, string dbpasswd)
        {            
            try
            {
                
                DI = new SAPbobsCOM.Company();
                DI.DbServerType = (BoDataServerTypes)serverType;
                DI.Server = server;
                DI.language = SAPbobsCOM.BoSuppLangs.ln_English;
                DI.CompanyDB = database;
                DI.UserName = user;
                DI.Password = passwd;
                DI.DbUserName = dbuser;
                DI.DbPassword = dbpasswd;
                klib.Shell.WriteLine(R.Project.ID, LOG, $"SDI: Trying to connect {DI.Server} server");
                var res = DI.Connect();
                
                if (res != 0)
                {
                    Fail = true;
                    var error = DI.GetLastErrorDescription();
                    klib.Shell.WriteLine(R.Project.ID, LOG, $"SDI: Error {error}");
                    throw new SDIException(1, $"{DI.GetLastErrorCode()} - {error}");
                } else
                {
                    Fail = false;
                }

                klib.Shell.WriteLine(R.Project.ID, LOG, $"SDI: Connected with {DI.UserName} user");
            }
            finally
            {

            }
        }
        public static void Connect(klib.DB.DbClient cnn, klib.model.Credentials1 cred)
        {
            BoDataServerTypes version;

            switch(cnn.Version())
            {
                case 9: version = BoDataServerTypes.dst_MSSQL2005; break;
                case 10: version = BoDataServerTypes.dst_MSSQL2008; break;
                case 11: version = BoDataServerTypes.dst_MSSQL2012; break;
                case 12: version = BoDataServerTypes.dst_MSSQL2014; break;
                case 13: version = BoDataServerTypes.dst_MSSQL2016; break;
                default:
                    throw new SDIException(1, "Version of SQL Server isn't implementation");

            }

            klib.Shell.WriteLine(R.Project.ID, LOG, $"SDI: Parameters DbClient");
            Connect((int)version,
                cnn.StringConn.DataSource,
                cnn.StringConn.InitialCatalog,
                cred.User,
                cred.Passwd,
                cnn.StringConn.UserID,
                cnn.StringConn.Password);

        }
        public static void Connect(dynamic oCompany)
        {
            klib.Shell.WriteLine(R.Project.ID,LOG, "SDI - Connecting ...");
            DI = (SAPbobsCOM.Company)oCompany;
            klib.Shell.WriteLine(R.Project.ID, LOG, $"SDI - Connected {DI.Server}.{DI.CompanyDB} with {DI.UserName}");
        }
        public static void Disconnect()
        {
            if (DI != null && DI.Connected)
                DI.Disconnect();
        }
        #endregion

        public static string GetMessageError()
        {
            var msg = DI.GetLastErrorDescription();
            var code = DI.GetLastErrorCode();
            klib.Shell.WriteLine(R.Project.ID,LOG, $"SDI: {code} - {msg}");
            return msg;
        }
    }
}