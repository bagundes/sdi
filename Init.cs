using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDI
{
    public class Init : klib.Implement.IInit
    {
        private static string LOG => "INIT";
        public static void Start()
        {
            klib.Shell.WriteLine(R.Project.ID, LOG, $"SDI - Loading the module");
            var init = new Init();
            init.Construct();
            init.Configure();
        }

        public void Construct()
        {

            if (!UserDataTable.IsNewVersion(R.Project.Version))
                return;

            var udo = String.Empty;

            #region User Fields Control - Link to
            UserDataTable.AddColumn("OINV", new UserDataTable.Column("Btype", $"{klib.R.Company.NS}: Base Type"));
            UserDataTable.AddColumn("OINV", new UserDataTable.Column("BEntry", $"{klib.R.Company.NS}: Base Entry"));
            UserDataTable.AddColumn("OINV", new UserDataTable.Column("BLine", $"{klib.R.Company.NS}: Base Line"));
            UserDataTable.AddColumn("OINV", new UserDataTable.Column("BComments", $"{klib.R.Company.NS}: Base Comments", 400));
            #endregion

            #region PARAMS - Global Parameters
            var projects = new List<klib.model.Select>();
            projects.Add(new klib.model.Select("1", "Teamsoft parameters"));
            projects.Add(new klib.model.Select("1001", "KLibrary"));
            projects.Add(new klib.model.Select("1102", "SDI Library"));
            projects.Add(new klib.model.Select("1103", "SUI Library"));
            projects.Add(new klib.model.Select("1104", "Report Library"));
            projects.Add(new klib.model.Select("2001", "Suite Package"));            
            projects.Add(new klib.model.Select("2002", "Nightline Service"));
            projects.Add(new klib.model.Select("2003", "QuickBooks import"));
            projects.Add(new klib.model.Select("9001", "Export to file"));
            projects.Add(new klib.model.Select("9001", "Audit Module"));

            udo = "param";
            klib.Shell.WriteLine(R.Project.ID,LOG, $"SDI - Create the tables {udo}.");
            var param = new UserDataTable(SAPbobsCOM.BoUTBTableType.bott_MasterData, udo, "Teamsoft Parameters");
            param.Columns.Add(new UserDataTable.Column { Name = "PROJECT", Description = "Project Name", Size = 10, Type = SAPbobsCOM.BoFieldTypes.db_Numeric, ValidValues = projects });

            var param1 = new UserDataTable(SAPbobsCOM.BoUTBTableType.bott_MasterDataLines, udo, "Teamsoft Params Details");
            param1.Columns.Add(new UserDataTable.Column { Name = "PARAM", Description = "Parameter", Size = 150 });
            param1.Columns.Add(new UserDataTable.Column { Name = "DIFF1", Description = "Differential 1", Size = 150 });
            param1.Columns.Add(new UserDataTable.Column { Name = "DIFF2", Description = "Differential 2", Size = 150 });
            param1.Columns.Add(new UserDataTable.Column { Name = "DIFF3", Description = "Differential 3", Size = 150 });
            param1.Columns.Add(new UserDataTable.Column { Name = "ORDER", Description = "Order", Size = 10, Type = SAPbobsCOM.BoFieldTypes.db_Numeric, DefaultValue = "0" });
            param1.Columns.Add(new UserDataTable.Column { Name = "VALUE", Description = "Valid Value", Size = 250 });
            param1.Columns.Add(new UserDataTable.Column { Name = "BPLID", Description = "Branch", Size = 10, Type = SAPbobsCOM.BoFieldTypes.db_Numeric });
            param1.Columns.Add(new UserDataTable.Column { Name = "DUEDATE", Description = "Due Date", Type = SAPbobsCOM.BoFieldTypes.db_Date });
            
            param.Children.Add(param1);
            param.Save();

            udo = "control";
            klib.Shell.WriteLine(R.Project.ID, LOG, $"SDI - Create the tables {udo}.");
            var control = new UserDataTable(SAPbobsCOM.BoUTBTableType.bott_NoObject, udo, "Teamsoft Control Center");
            control.Columns.Add(new UserDataTable.Column { Name = "PROJECT", Description = "Project Code", Type = SAPbobsCOM.BoFieldTypes.db_Numeric, ValidValues = projects });
            control.Columns.Add(new UserDataTable.Column { Name = "FUNCTION", Description = "Function", Type = SAPbobsCOM.BoFieldTypes.db_Numeric });
            control.Columns.Add(new UserDataTable.Column { Name = "UPDATE", Description = "Update", Type = SAPbobsCOM.BoFieldTypes.db_Numeric });

            control.Save();

            var noobj = "tbls";
            var tables = new UserDataTable(SAPbobsCOM.BoUTBTableType.bott_NoObject, noobj, "Teamsoft Index tables info");
            tables.Columns.Add(new UserDataTable.Column("LOG", "Log table", 50));
            tables.Columns.Add(new UserDataTable.Column("PK", "Primary key of table", 50));
            tables.Columns.Add(new UserDataTable.Column("ObjType", "Object Type", 50));
            tables.Columns.Add(new UserDataTable.Column("FormDef", "Form default in SAP", 50));
            tables.Columns.Add(new UserDataTable.Column("FormPKItem", "Item with pk field", 50));

            #endregion
            UserDataTable.UpdateVersion(R.Project.Version);
        }
        public void Configure()
        {

        }
        
        public void Populate()
        {

        }

        public void Destruct()
        {

        }
    }
}
