using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDI
{
    public class Conn
    {
        private const string LOG = "SDICON";
        public static bool InitStart = true;
        public enum TypeConn
        {
            Hana,
            MSQL,
            Others,
            None,
        }        

        public static SAPbobsCOM.Company DI;
        internal static TypeConn DataBase
        {
            get
            {
                if (DI == null || !DI.Connected)
                    return TypeConn.None;

                switch((int)DI.DbServerType)
                {
                    case 1: return TypeConn.MSQL;
                    case 4: return TypeConn.MSQL;
                    case 6:
                    case 7:
                    case 8: return TypeConn.MSQL;
                    case 9: return TypeConn.Hana;
                    case 10: return TypeConn.MSQL;
                    default: throw new LException(1, $"Database {DI.DbServerType.ToString()} isn't defined");
                }
            }
        }
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
                klib.Shell.WriteLine(R.Project.ID, LOG, $"Trying to connect {DI.Server} server");
                var res = DI.Connect();
                
                if (res != 0)
                {
                    var error = DI.GetLastErrorDescription();
                    klib.Shell.WriteLine(R.Project.ID, $"SAP DI: Error {error}");
                    throw new LException(1, $"{DI.GetLastErrorCode()} - {error}");
                } else
                {
                    if (InitStart)
                        Init();
                }

                klib.Shell.WriteLine(R.Project.ID, LOG, $"Connected with {DI.UserName} user");
            }
            finally
            {

            }
        }
        public static void Connect(klib.dbase.DbClient cnn, klib.model.Credentials1 cred)
        {
            BoDataServerTypes version;

            switch(cnn.Version)
            {
                case 2005: version = BoDataServerTypes.dst_MSSQL2005; break;
                case 2008: version = BoDataServerTypes.dst_MSSQL2008; break;
                case 2012: version = BoDataServerTypes.dst_MSSQL2012; break;
                case 2014: version = BoDataServerTypes.dst_MSSQL2014; break;
                case 2016: version = BoDataServerTypes.dst_MSSQL2016; break;
                default:
                    throw new LException(1, "Version of SQL Server isn't implementation");

            }

            klib.Shell.WriteLine(R.Project.ID, $"SAP DI: Parameters DbClient");
            Connect((int)version,
                cnn.ConnString.DataSource,
                cnn.ConnString.InitialCatalog,
                cred.User,
                cred.Passwd,
                cnn.ConnString.UserID,
                cnn.ConnString.Password);

        }
        public static void Connect(dynamic oCompany)
        {
            klib.Shell.WriteLine(R.Project.ID, "SAP DI: Connecting SAP DI");
            DI = (SAPbobsCOM.Company)oCompany;
            klib.Shell.WriteLine(R.Project.ID, $"SAP DI: Connected {DI.Server}.{DI.CompanyDB} with {DI.UserName}");
            if (InitStart)
                Init();
        }

        public static string GetMessageError()
        {
            var msg = DI.GetLastErrorDescription();
            var code = DI.GetLastErrorCode();
            klib.Shell.WriteLine(R.Project.ID,LOG, $"{code} - {msg}");
            return msg;
        }
        private static void Init()
        {
            if(InitStart)
            {
                var init = new Init();
                init.Construct();
            }
        }

        public static void Disconnect()
        {
            if(DI != null && DI.Connected)
                DI.Disconnect();
        }

        public static dynamic GetObject(BoObjectTypes Object)
        {
            return DI.GetBusinessObject(Object);
        }
    }
}