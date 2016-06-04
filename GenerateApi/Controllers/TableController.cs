using Dapper;
using GenerateApi.Extension;
using GenerateApi.Service;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Http;

namespace GenerateApi.Controllers
{
    [RoutePrefix("api/table")]
    public class TableController : BaseController
    {
        private TableLogic tableLogic = new TableLogic();
        private ITableService service;

        public TableController()
        {
            service = new TableService();
        }

        [Route("all")]
        public IHttpActionResult GetTable()
        {
            using (SqlConn)
            {
                return Ok(SqlConn.Query<string>("select table_name from INFORMATION_SCHEMA.TABLES"));
            }
        }

        [Route("poco")]
        public IHttpActionResult GetPoco(string tableName)
        {
            var builder = new StringBuilder();
            using (SqlConn)
            {
                using (SqlCommand cmd = new SqlCommand(string.Format("select * from {0}", tableName), SqlConn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        do
                        {
                            if (reader.FieldCount <= 1) continue;

                            builder.AppendLine(string.Format("public class {0}", tableName));
                            builder.AppendLine("{");
                            var schema = reader.GetSchemaTable();

                            foreach (DataRow row in schema.Rows)
                            {
                                var type = (Type)row["DataType"];
                                var name = tableLogic.TypeAliases.ContainsKey(type) ? tableLogic.TypeAliases[type] : type.Name;
                                var isNullable = (bool)row["AllowDBNull"] && tableLogic.NullableTypes.Contains(type);
                                var collumnName = (string)row["ColumnName"];
                                builder.AppendLine(string.Format("\tpublic {0}{1} {2} {{get; set; }}", name, isNullable ? "?" : string.Empty, collumnName));
                            }

                            builder.AppendLine("}");
                            builder.AppendLine();
                        } while (reader.NextResult());
                    }
                }
                return Ok(builder.ToString());
            }
        }

        [Route("insert")]
        public IHttpActionResult GetInsert(string tableName)
        {
            var builder = service.GenerateInsert(tableName, SqlConn);
            return Ok(builder.ToString());
        }

        [Route("update")]
        public IHttpActionResult GetUpdate(string tableName)
        {
            var builder = service.GenerateUpdate(tableName, SqlConn);
            return Ok(builder.ToString());
        }
    }
}