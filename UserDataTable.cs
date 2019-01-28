using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDI
{
    public interface ITable
    {
        void Save();

    }

    public partial class UserDataTable : ITable
    {
        private const string LOG = "UDT";
        public bool SAPNotification = false;
        /// <summary>
        /// Create without rules default;
        /// </summary>
        public bool Odd = false;
        private enum TableType
        {
            udt,
            saptables,
        }
        public class UDOParams
        {
            /// <summary>
            /// Create without rules default;
            /// </summary>
            public bool Odd = false;
            public bool CanArchive = false;
            public bool CanCancel = true;
            public bool CanClose = false;
            public bool CanCreateDefaultForm = false;
            public bool CanDelete = false;
            public bool CanFind = true;
            public bool CanLog = false;
            public bool CanYearTransfer = false;
            public bool ManageSeries = false;
        }

        #region properties
        private readonly TableType typeTable;
        private readonly string infix;
        private readonly string prefix = klib.R.Company.NS;
        private readonly BoUTBTableType type;
        private readonly string description;
        private readonly string sapTable;
        private string tableName;
        private int tableNum;

        public List<Column> Columns = new List<Column>();
        public List<UserDataTable> Children = new List<UserDataTable>();
        public string TableName { get => tableName.ToUpper(); }
        public UDOParams UDOProper = new UDOParams();
        #endregion

        #region publics
        public UserDataTable(BoUTBTableType type, string infix, string description)
        {
            this.type = type;
            this.infix = infix.ToUpper();
            this.description = description;
            typeTable = TableType.udt;
        }

        public UserDataTable(string sapTable)
        {
            this.sapTable = sapTable;
            typeTable = TableType.saptables;
        }

        public void Save()
        {
            Save(0);
        }

        /// <summary>
        /// Save the objects in DataBase
        /// </summary>
        /// <param name="number"></param>
        private void Save(int number)
        {
            //number = number == null ? 0 : number;
            this.tableName = GetUserDataName(number);

            #region tables
            if (typeTable == TableType.udt)
            {
                SaveTable();
                for (int i = 0; i < Children.Count; i++)
                {
                    Children[i].Save(i + 1);
                }
            }
            #endregion

            #region columns
            SaveColumns();
            #endregion

            #region UDO'S
            if (type == BoUTBTableType.bott_Document || type == BoUTBTableType.bott_MasterData)
            {
                AddFatherUdo();
                AddChildUdo();
            }

            #endregion
        }
        #endregion

        #region privates
        private string GetUserDataName(int? addnumber)
        {
            if (addnumber != null)
                tableNum = (int)addnumber;


            switch (typeTable)
            {
                case TableType.udt:
                    var name = $"@{prefix}_{infix}{addnumber}";
                    return name;
                case TableType.saptables:
                    return sapTable;
                default:
                    throw new LException(3);
            }


        }

        private void SaveTable()
        {
            #region Error -1120 : Error: Ref count for this object is higher then 0
            // Solution : https://archive.sap.com/discussions/thread/1958196
            var oRS = Conn.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset) as SAPbobsCOM.Recordset;

            System.Runtime.InteropServices.Marshal.ReleaseComObject(oRS);
            oRS = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            #endregion


            int error = 0;

            var oUserTableMD = Conn.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserTables) as SAPbobsCOM.UserTablesMD;
            var tableNameWithoutAt = tableName.Replace("@", "");

            oRS = Conn.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset) as SAPbobsCOM.Recordset;

            System.Runtime.InteropServices.Marshal.ReleaseComObject(oRS);
            oRS = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var table = $"{prefix}: {description}".Replace("@", "");

            try
            {
                var update = oUserTableMD.GetByKey(tableNameWithoutAt);
                oUserTableMD.TableName = tableNameWithoutAt;
                if (typeTable == TableType.udt)
                {
                    oUserTableMD.TableDescription = table;
                    oUserTableMD.TableType = this.type;
                }


                if (update)
                    error = oUserTableMD.Update();
                else
                    error = oUserTableMD.Add();

                if (error != 0)
                    throw new LException(4, tableName, error, Conn.DI.GetLastErrorDescription());
                else
                    klib.Shell.WriteLine(R.Project.ID, LOG, $"The {tableName} table was created");


            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserTableMD);
                oUserTableMD = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private void SaveColumns()
        {
            foreach (var column in Columns)
            {
                #region Error -1120 : Error: Ref count for this object is higher then 0
                // Solution : https://archive.sap.com/discussions/thread/1958196
                var oRS = Conn.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset) as SAPbobsCOM.Recordset;

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oRS);
                oRS = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                #endregion

                var name = column.Name.ToUpper();

                if (name.Substring(0, 2) == "U_")
                    name = name.Substring(2);

                if (typeTable == TableType.saptables)
                    name = $"{prefix}{name}".ToUpper();

                klib.Shell.WriteLine(R.Project.ID, LOG, $"Creating/Updating the {tableName}.{name} column");
                var oUserFieldsMD = Conn.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserFields) as SAPbobsCOM.UserFieldsMD;

                try
                {
                    string sql = String.Empty;
                    switch (Conn.DataBase)
                    {
                        case Conn.TypeConn.MSQL:
                        //sql = "SELECT FieldId FROM CUFD WHERE TableID = '{0}' AND AliasID = '{1}'"; break;
                        case Conn.TypeConn.Hana:
                            sql = @" SELECT ""FieldID"" FROM ""CUFD"" WHERE ""TableID"" = {0} AND ""AliasID"" = {1} "; break;
                        default: throw new LException(1, "The database isn't developed");
                    }

                    var fieldId = RSEx.First(sql, tableName, name).ToNInt();
                    var update = false;
                    var error = 0;

                    if (fieldId != null)
                        update = oUserFieldsMD.GetByKey(tableName, Convert.ToInt32(fieldId));

                    if (column.Size > 254 && column.Type == BoFieldTypes.db_Alpha && column.SubType == BoFldSubTypes.st_None)
                    {
                        klib.Shell.WriteLine(R.Project.ID, LOG, $"The {tableName}{name} column resize from {column.Size} to 254");
                        column.Size = 254;
                    }

                    oUserFieldsMD.TableName = TableName;
                    oUserFieldsMD.Name = name;
                    oUserFieldsMD.Description = column.Description;
                    oUserFieldsMD.Type = column.Type;
                    oUserFieldsMD.SubType = column.SubType;
                    oUserFieldsMD.EditSize = column.Size;

                    if (!String.IsNullOrEmpty(column.DefaultValue))
                        oUserFieldsMD.DefaultValue = column.DefaultValue;

                    if (column.LikedTable != null)
                        oUserFieldsMD.LinkedTable = column.LikedTable.TableName;


                    #region Loading valid values
                    if (column.ValidValues.Count > 0)
                    {
                        
                        var vvalues = new List<klib.model.Select>();
                        #region Loading existing valid values
                        using (var rs = new klib.dbase.DbClient())
                        {
                            var sql1 = @"
SELECT	 UFD1.IndexID
		,UFD1.FldValue
FROM	 UFD1
INNER JOIN CUFD
	ON	 CUFD.TableID = UFD1.TableID
	AND	 UFD1.FieldID = CUFD.FieldID
WHERE	 CUFD.TableID = {0}
	AND	 CUFD.AliasID = {1}";

                            rs.DoQuery(sql1, tableName, name);
                            while(rs.Next())
                                vvalues.Add(new klib.model.Select(rs.Field("FldValue").ToString(), rs.Field("IndexID")));
                        }
                        #endregion

                        for(int i = 0; i < column.ValidValues.Count; i++)
                        {
                            oUserFieldsMD.ValidValues.SetCurrentLine(oUserFieldsMD.ValidValues.Count - 1);

                            var index = vvalues.Where(t => t.Name == column.ValidValues[i].Name).Select(t => t.Value.ToNInt()).FirstOrDefault();
                            if (index.HasValue)
                                oUserFieldsMD.ValidValues.SetCurrentLine(index.Value);
                            else if (!String.IsNullOrEmpty(oUserFieldsMD.ValidValues.Value))
                            {
                                oUserFieldsMD.ValidValues.Add();
                                oUserFieldsMD.ValidValues.SetCurrentLine(oUserFieldsMD.ValidValues.Count - 1);
                            } 

                            oUserFieldsMD.ValidValues.Value = column.ValidValues[i].Name;
                            oUserFieldsMD.ValidValues.Description = column.ValidValues[i].Value.IsEmpty ? column.ValidValues[i].Name : column.ValidValues[i].Value.ToString();
                            klib.Shell.WriteLine(R.Project.ID, LOG, $"Valid Values: {oUserFieldsMD.ValidValues.Value} - {oUserFieldsMD.ValidValues.Description}");
                        }
                    }
                    #endregion

                    if (update)
                        error = oUserFieldsMD.Update();
                    else
                        error = oUserFieldsMD.Add();


                    // @bfagundes - Error -2035 Ignored beacause the column already exist.
                    if (error != 0 && error != -2035 && error != -1029)
                        throw new LException(4, tableName, error, Conn.GetMessageError());                        
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserFieldsMD);
                    oUserFieldsMD = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }
        }


        private void AddFatherUdo()
        {
            int error = 0;
            var oUserObjectsMD = Conn.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD) as SAPbobsCOM.UserObjectsMD;

            try
            {
                var name = GetUserDataName(null).Replace("@", "");
                var update = oUserObjectsMD.GetByKey(name);

                oUserObjectsMD.Code = name;
                oUserObjectsMD.Name = description;
                oUserObjectsMD.ObjectType = (BoUDOObjType)type;
                oUserObjectsMD.TableName = this.tableName.Replace("@", "");
                oUserObjectsMD.CanArchive = ValuesEx.YesOrNo(UDOProper.CanArchive);
                oUserObjectsMD.CanCancel = ValuesEx.YesOrNo(UDOProper.CanCancel);
                oUserObjectsMD.CanClose = ValuesEx.YesOrNo(UDOProper.CanClose);
                oUserObjectsMD.CanCreateDefaultForm = ValuesEx.YesOrNo(UDOProper.CanCreateDefaultForm);
                if (UDOProper.CanCreateDefaultForm)
                    oUserObjectsMD.EnableEnhancedForm = BoYesNoEnum.tNO; // Create a type of matrix form
                oUserObjectsMD.CanDelete = ValuesEx.YesOrNo(UDOProper.CanDelete);
                oUserObjectsMD.CanFind = ValuesEx.YesOrNo(UDOProper.CanFind);
                oUserObjectsMD.CanLog = ValuesEx.YesOrNo(UDOProper.CanLog);
                oUserObjectsMD.CanYearTransfer = ValuesEx.YesOrNo(UDOProper.CanYearTransfer);
                oUserObjectsMD.ManageSeries = ValuesEx.YesOrNo(UDOProper.ManageSeries);

                if (update)
                    error = oUserObjectsMD.Update();
                else
                    error = oUserObjectsMD.Add();

                if (error != 0 && error != -1029) // Error -1029 : Not possible update the UDO.
                    throw new LException(4, name, error, Conn.DI.GetLastErrorDescription());
            }
            finally
            {

                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserObjectsMD);
                oUserObjectsMD = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

        }

        private void AddChildUdo()
        {
            int error = 0;
            var oUserObjectsMD = Conn.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oUserObjectsMD) as SAPbobsCOM.UserObjectsMD;

            try
            {
                var name = GetUserDataName(null).Replace("@", "");
                var update = oUserObjectsMD.GetByKey(name);

                if (!update)
                    throw new LException(4, name, error, "UDO not found");


                foreach (var child in Children)
                {
                    var line = oUserObjectsMD.ChildTables.Count - 1;
                    oUserObjectsMD.ChildTables.SetCurrentLine(line);
                    oUserObjectsMD.ChildTables.TableName = child.TableName.Replace("@", "");
                }

                error = oUserObjectsMD.Update();

                if (error != 0)
                    throw new LException(4, name, error, Conn.DI.GetLastErrorDescription());

            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oUserObjectsMD);
                GC.Collect();
            }
        }
        #endregion

    }

    public partial class UserDataTable
    {
        public static void AddColumn(string tableName, Column column)
        {

            var udt = new UserDataTable(tableName);
            udt.Columns.Add(column);
            udt.Save();
        }
        // Using klib.model.Select
        //public class UFValidValue
        //{
        //    public UFValidValue(Enum enumVal)
        //    {
        //        //Value = ((int)enumVal).ToString();
        //        //Description
        //    }

        //    public UFValidValue(string value, string description = null)
        //    {
        //        Value = value;
        //        Description = description ?? value;
        //    }

        //    public string Value { get; set; }
        //    public string Description { get; set; }
        //}

        public class Column
        {
            public string Name;
            public string Description;
            public BoFieldTypes Type = BoFieldTypes.db_Alpha;
            public SAPbobsCOM.BoFldSubTypes SubType = BoFldSubTypes.st_None;
            public string DefaultValue = null;
            public int Size = 10;
            public UserDataTable LikedTable;
            public List<klib.model.Select> ValidValues = new List<klib.model.Select>();

            public Column() { }
            public Column(string name, string description, int size = 150)
            {
                Name = name;
                Description = description;
                Size = size;
            }
        }

        /// <summary>
        /// Table name in format @[PRFX]_[NAME][TABLE]
        /// </summary>
        /// <param name="name">Table's name</param>
        /// <param name="table">Table's number</param>
        /// <returns></returns>
        public static String GetTableName(string name, int table)
        {
            var prefix = klib.R.Company.NS;
            return $"@{prefix}_{name}{table}".ToUpper();
        }

        public static String GetUDOName(string name)
        {
            var prefix = klib.R.Company.NS;
            return $"@{prefix}_{name}".ToUpper();
        }

        public static string GetColumnName(string name)
        {
            return $"U_{ klib.R.Company.NS}{name}".ToUpper();
        }


        public static bool IsNewVersion(Version v)
        {
            try
            {
                var verString = RSEx.First($"SELECT Name FROM [@TS_CONTROL0] WHERE U_PROJECT = {v.Major}");
                if (!verString.IsEmpty)
                {
                    var ver1 = new Version(verString.ToString());
                    return (ver1.CompareTo(v) < 0);
                }
                else
                {
                    return true;
                }
            }catch(Exception ex)
            {
                klib.Shell.WriteLine(R.Project.ID, ex.Message);
                return true;
            }
        }
        /// <summary>
        /// Update control version
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static void UpdateVersion(Version v)
        {
            if (!IsNewVersion(v))
                return;
            // TODO - It's necessary to change General Service
            var lastcode = RSEx.First("SELECT ISNULL(MAX(Code),0) Code FROM [@TS_CONTROL0]").ToInt();
            var sql = @"INSERT INTO [@TS_CONTROL0] VALUES({0},{1},{2},{3},{4})";

            using (var rs = new SDI.ResultSet())
                rs.NoQuery(sql, lastcode + 1, v, v.Major, v.Build, v.Revision);

            klib.Shell.WriteLine(R.Project.ID, $"VRSION: Updated to version {v.ToString()}");
        }
    }
   
}


