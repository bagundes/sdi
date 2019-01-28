using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using klib;

namespace SDI
{
    public class ResultSet : klib.implement.IDbClient
    {
        private SAPbobsCOM.Recordset RS = Conn.DI.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset) as SAPbobsCOM.Recordset;
        private SAPbobsCOM.Fields fields;
        private bool firstLine = false;
        private List<string> Columns = new List<string>();

        public bool IsFirstLine => firstLine;

        public int CountColumns()
        {
            return RS.Fields.Count;
        }

        public int CountRows()
        {
            return RS.RecordCount;
        }

        public void Dispose()
        {
            if (RS == null)
                return;

            System.Runtime.InteropServices.Marshal.ReleaseComObject(RS);

            if (fields != null)
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fields);

            GC.Collect();
        }

        public dynamic Do(dynamic alias, params object[] values)
        {
            throw new NotImplementedException();
        }

        public int DoQuery(string sql, params object[] values)
        {
            Columns = new List<string>();
            sql = String.Format(klib.ValuesEx.NS(sql), FixValues(values));

            RS.DoQuery(sql);
            firstLine = true;
            fields = RS.Fields;
            
            for (int i = 0; i < RS.Fields.Count; i++)
                Columns.Add(RS.Fields.Item(i).Name.ToString().ToUpper());

            return RS.RecordCount;
        }

        public int DoQueryManager(string qname, params object[] values)
        {
            throw new NotImplementedException();
        }

        public Values Field(object index)
        {
            if (index.GetType() == typeof(String))
                index = Columns.IndexOf(index.ToString().ToUpper());

            return new Values(fields.Item(index).Value);
        }

        public Dictionary<string, Values> Fields(bool upper = true)
        {
            var line = new Dictionary<string, Values>();
            for (int i = 0; i < CountColumns(); i++)
            {
                if (upper)
                    line.Add(fields.Item(i).Name.ToUpper(), new Values(fields.Item(i).Value));
                else
                    line.Add(fields.Item(i).Name, new Values(fields.Item(i).Value));
            }

            return line;
        }

        public void First()
        {
            RS.MoveFirst();
        }

        public object[] FixValues(object[] values, bool manipulation = false)
        {
            var descr = manipulation ? "''''" : "''";
            var open = manipulation ? "''" : "'";

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    values[i] = "null";
                    continue;
                }

                values[i] = klib.ValuesEx.NS(values[i].ToString());
                values[i] =  $"{open}{values[i].ToString().Replace("'", descr)}{open}";
            }
            

            return values;
        }

        public void Last()
        {
            throw new NotImplementedException();
        }

        public T Load<T>()
        {
            throw new NotImplementedException();
        }

        public bool Next()
        {
            if (firstLine)
                firstLine = false;
            else
                RS.MoveNext();

            if (RS.EoF)
                return false;
            else
            {
                fields = RS.Fields;
                return true;
            }
        }

        public bool NoQuery(string sql, params object[] values)
        {
            sql = String.Format(klib.ValuesEx.NS(sql), FixValues(values, true));
            switch(Conn.DataBase)
            {
                case Conn.TypeConn.MSQL:
                    var sql1 = $"EXEC('{sql}')";
                    RS.DoQuery(sql1); break;
                default:
                    throw new NotImplementedException();
            }

            return true;
        }
    }

    public static class RSEx
    {
        public static List<string> GetQueriesManager(string queryManagerPreFix)
        {
            var sql = $"SELECT QName FROM OUQR WHERE Qname LIKE '{queryManagerPreFix}%'";
            return Column<string>(sql);
        }

        public static bool IsQueryManager(string qname)
        {
            var sql = "SELECT QName FROM OUQR WHERE Qname = {0}";
            using (var rs = new ResultSet())
                return rs.DoQuery(sql, qname) > 0;
        }

        public static ResultSet QueryManager(string qname, params object[] values)
        {
            var sql = @"SELECT QString FROM OUQR WHERE Qname = {0}";
            var qstring = String.Empty;

            using (var rs = new ResultSet())
            {
                rs.DoQuery(sql, qname);

                if (rs.Next())
                    qstring = rs.Field(0).ToString();
                else
                    throw new LException(6, qname);
            }

            var rgx = new Regex(@"\[\%[0-9]\]");
            var foo = rgx.Matches(qstring);

            if(foo.Count != values.Length)
                throw new LException(2, $"Quantity of paramters in {qname} isn't valid ({foo.Count}/{values.Length})");

            for (int i = 0; i < foo.Count; i++)
                qstring = qstring.Replace($"[%{i}]", "{" + i + "}");


            var rs1 = new ResultSet();
            rs1.DoQuery(sql, values);

            return rs1;
            
        }

        public static ResultSet QueryManager(int code, int id, params object[] values)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Verify if value exists in the table
        /// </summary>
        /// <param name="table">Table name</param>
        /// <param name="col">Column to compare</param>
        /// <param name="val1">Value to find</param>
        /// <param name="onlyString">Use regex OnlyLetterAndNumbers</param>
        /// <param name="like">Use like</param>
        /// <returns>Quantity register exists</returns>
        public static int IsExists(string table, string col, object val1, bool onlyString = false, bool like = false)
        {
            var sql = $"SELECT TOP 1 1 FROM [{table}] WHERE ";

            if (onlyString)
                sql += $"dbo.RegExReplace(UPPER({col}), '{klib.E.RegexMask.OnlyLetterAndNumbers}', '', DEFAULT) = '{klib.ValuesEx.RegexReplace(val1, klib.E.RegexMask.OnlyLetterAndNumbers).ToUpper()}' ";
            else
                sql += $"UPPER({col}) = '{val1.ToString().ToUpper()}'";

            if (like)
                sql = sql.Replace("=", "LIKE");

            using (var cnn = new ResultSet())
            {
                return cnn.DoQuery(sql);
            }
        }

        public static Dictionary<string, Values> Top1(string sql, params object[] values)
        {
            var res = new Dictionary<string, Values>();
            using (var rs = new ResultSet())
            {
                if (rs.DoQuery(sql, values) > 0)
                {
                    res = rs.Fields();
                }
            }

            return res;
        }

        /// <summary>
        /// Return all column values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static List<T> Column<T>(string sql, params object[] values)
        {
            var res = new List<T>();
            using (var rs = new ResultSet())
            {
                rs.DoQuery(sql, values);
                while (rs.Next())
                    res.Add(rs.Field(0).Dynamic());
            }

            return res;
        }

        public static Values First(string sql, params object[] values)
        {
            using (var rs = new ResultSet())
            {
                if (rs.DoQuery(sql, values) > 0)
                    return rs.Field(0);
                else
                    return klib.ValuesEx.Empty;
            }
        }

        #region Parameters
        public static Values Parameter(int code, string parameter, string diff1, string diff2, string diff3, object def)
        {
            try
            {
                CreateParameter(code, parameter, diff1, diff2, diff3, def);
                return Parameter(code, parameter, diff1, diff2, diff3);
            }
            catch(Exception ex)
            {
                klib.Shell.WriteLine(R.Project.ID, ex.Message);
                return klib.ValuesEx.To(def);
            }
        }

        public static Values Parameter(int code, string parameter, string diff1 = null, string diff2 = null, string diff3 = null)
        {
            CreateParameter(code, parameter, diff1, diff2, diff3);

            var sql = $"{klib.R.Project.SP_PARAM} '{code}', '{parameter}', '{diff1}', '{diff2}', '{diff3}'";

            using (var cnn = new ResultSet())
            {
                cnn.DoQuery(sql);
                var result = klib.ValuesEx.Empty;
                if (cnn.Next())
                    result = cnn.Fields()["VALUE"];

                if (result.IsEmpty)
                    throw new LException(5, code, parameter);
                else
                    return result;
            }
        }

        public static List<klib.model.Select> ParameterList(int code, string parameter, string diff1 = null, string diff2 = null, string diff3 = null)
        {
            //0001
            var sql = $"{klib.R.Project.SP_PARAM} {code}, '{parameter}', '{diff1}', '{diff2}'";
            var select = new List<klib.model.Select>();

            using (var cnn = new ResultSet())
            {
                cnn.DoQuery(sql);
                var result = klib.ValuesEx.Empty;
                if (cnn.Next())
                {
                    var value = cnn.Field("VALUE").ToString();
                    var id = String.Empty;
                    if (diff3 != null)
                        id = cnn.Field("DIFF3").ToString();
                    if (diff2 != null)
                        id = cnn.Field("DIFF2").ToString();
                    else if (diff1 != null)
                        id = cnn.Field("DIFF1").ToString();
                    else
                        id = cnn.Field("PARAM").ToString();

                    select.Add(new klib.model.Select(id, value));
                }


                return select;
            }
        }

        public static void CreateParameter(int code, string parameter, string diff1 = null, string diff2 = null, string diff3 = null, object def = null, DateTime? dueDate = null)
        {
            var exists = false;

            exists = RSEx.IsExists("@!!_PARAM0", "Code", code) > 0;

            if (!exists)
            {
                using (var rs = new ResultSet())
                {
                    var docEntry = RSEx.First("SELECT MAX(DocEntry) FROM [@!!_PARAM0]").ToInt() + 1;
                    var sql = "INSERT INTO [@!!_PARAM0] (Code, DocEntry, Name, U_PROJECT) VALUES ({0},{1},{2},{3})";
                    rs.NoQuery(sql, code, docEntry, code, code);
                }                
            }

            using (var rs = new ResultSet())
            {
                var sql = $@"
SELECT   1 
FROM     [@!!_PARAM1] 
WHERE    Code = '{code}'
    AND  UPPER(U_Param) = UPPER('{parameter}')
    AND  ISNULL(U_DIFF1,'') = '{diff1}'
    AND  ISNULL(U_DIFF2,'') = '{diff2}'
    AND  ISNULL(U_DIFF3,'') = '{diff3}'
";

                exists = rs.DoQuery(sql) > 0;

                if (!exists)
                {
                    var lastLine = RSEx.First("SELECT MAX(LineId) FROM [@!!_PARAM1] WHERE Code = {0}", code).ToInt();

                    sql = @"
INSERT INTO [@!!_PARAM1] (Code, LineId, Object, U_PARAM, U_DIFF1, U_DIFF2, U_DIFF3, U_ORDER, U_VALUE)
VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8})

";
                    rs.NoQuery(sql, code, lastLine + 1, $"{klib.R.Company.NS}_PARAM", parameter, diff1, diff2, diff3, 0, def);
                }
                
            }


        }

        //public static void CreateParameter(Type p)
        //{
        //        foreach (var proper in p.GetProperties(BindingFlags.Static | BindingFlags.Public))
        //        {
        //            var name = proper.Name;
        //            var foo = proper.GetValue(null, null);

        //        }
        //}
        #endregion

    }
}
