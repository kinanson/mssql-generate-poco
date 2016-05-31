using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Http;

namespace GenerateApi.Controllers
{
    public class TableController : BaseController
    {
        public IHttpActionResult GetTable()
        {
            using (SqlConn)
            {
                return Ok(SqlConn.Query<string>("select table_name from INFORMATION_SCHEMA.TABLES"));
            }
        }

        public IHttpActionResult GetPoco(string tableName)
        {
            var builder = new StringBuilder();
            using (SqlConn)
            {
                using (SqlCommand cmd = new SqlCommand($"select * from {tableName}", SqlConn))
                {
                    using (SqlDataReader reader=cmd.ExecuteReader())
                    {
                        do
                        {
                            if (reader.FieldCount <= 1) continue;

                            builder.AppendLine($"public class {tableName}");
                            builder.AppendLine("{");
                            var schema = reader.GetSchemaTable();

                            foreach (DataRow row in schema.Rows)
                            {
                                var type = (Type)row["DataType"];
                                var name = TypeAliases.ContainsKey(type) ? TypeAliases[type] : type.Name;
                                var isNullable = (bool)row["AllowDBNull"] && NullableTypes.Contains(type);
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

    private Dictionary<Type, string> TypeAliases = new Dictionary<Type, string> {
            { typeof(int), "int" },
            { typeof(short), "short" },
            { typeof(byte), "byte" },
            { typeof(byte[]), "byte[]" },
            { typeof(long), "long" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(float), "float" },
            { typeof(bool), "bool" },
            { typeof(string), "string" }
        };

    private HashSet<Type> NullableTypes = new HashSet<Type> {
            typeof(int),
            typeof(short),
            typeof(long),
            typeof(double),
            typeof(decimal),
            typeof(float),
            typeof(bool),
            typeof(DateTime)
        };
}
}