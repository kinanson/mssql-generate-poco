using Dapper;
using GenerateApi.Models;
using System.Data.SqlClient;
using System.Text;
using System.Web.Http;

namespace GenerateApi.Controllers
{
    public class SpController : BaseController
    {
        private string typeName = string.Empty;
        private string columnName = string.Empty;

        public IHttpActionResult GetSpAll()
        {   
            using (SqlConn)
            {
                return Ok(SqlConn.Query<string>("select specific_name from information_schema.routines where routine_type = 'PROCEDURE'"));
            }
        }

        public IHttpActionResult GetInputAll(string spName)
        {
            using (SqlConn)
            {
                var viewModel = new SpViewModel
                {
                    Input = GenerateInput(SqlConn, spName),
                    Result = GenerateResult(SqlConn, spName)
                };
                return Ok(viewModel);
            }
        }

        private string GenerateInput(SqlConnection sqlConn, string spName)
        {
            using (SqlCommand cmd = new SqlCommand(string.Format(@"select * from 
            INFORMATION_SCHEMA.PARAMETERS where specific_name = '{0}'",spName), SqlConn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    var builder = new StringBuilder();
                    builder.AppendLine(string.Format("public class {0}",spName));
                    builder.AppendLine("{");
                    while (reader.Read())
                    {
                        typeName = ConvertTypeToCsharp(reader["DATA_TYPE"].ToString());
                        columnName = reader["PARAMETER_NAME"].ToString();
                        columnName = columnName.Replace('@', ' ');
                        builder.AppendLine(string.Format("\tpublic {0} {1} {{get; set;}}", typeName, columnName));
                    }
                    builder.AppendLine("}");
                    builder.AppendLine();
                    return CheckHasParams(builder);
                }
            }
        }

        private string CheckHasParams(StringBuilder builder)
        {
            var result = builder.ToString();
            if (!result.Contains("get;"))
            {
                result = string.Empty;
            }

            return result;
        }

        private string CheckHasResult(StringBuilder builder)
        {
            return CheckHasParams(builder);
        }

        private string GenerateResult(SqlConnection sqlConn, string spName)
        {
            using (SqlCommand cmd = new SqlCommand(string.Format(@"SELECT * FROM 
            sys.dm_exec_describe_first_result_set_for_object(OBJECT_ID('{0}'),NULL)",spName)
            , SqlConn))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    var builder = new StringBuilder();
                    builder.AppendLine(string.Format("public class {0}",spName));
                    builder.AppendLine("{");
                    while (reader.Read())
                    {
                        typeName = ConvertTypeToCsharp(reader["system_type_name"].ToString());
                        columnName = reader["name"].ToString();
                        columnName = columnName.Replace('@', ' ');
                        builder.AppendLine(string.Format("\tpublic {0} {1} {{get; set;}}", typeName, columnName));
                    }
                    builder.AppendLine("}");
                    builder.AppendLine();
                    return CheckHasResult(builder);
                }
            }
        }

        private string ConvertTypeToCsharp(string type)
        {
            var lowerType = type.ToLower();
            if (lowerType=="int") return "int";
            if (lowerType == "smallint") return "short";
            if (lowerType == "bigint") return "long";
            if (lowerType == "tinyint") return "byte";
            if (lowerType == "binary") return "byte[]";
            if (lowerType.Contains("char") || lowerType.Contains("text")) return "string";
            if (lowerType.Contains("date")) return "DateTime";
            if (lowerType.Contains("decimal") || lowerType.Contains("numeric")) return "decimal";
            if (lowerType == "float") return lowerType;
            if (lowerType.Contains("money")) return "double";
            if (lowerType == "bit") return "bool";
            return null;
        }
    }
}