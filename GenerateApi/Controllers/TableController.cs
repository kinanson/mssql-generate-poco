using Dapper;
using GenerateApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.Http;

namespace GenerateApi.Controllers
{
    [RoutePrefix("api/table")]
    public class TableController : BaseController
    {
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

        [Route("insert")]
        public IHttpActionResult GetInsert(string tableName)
        {
            var builder = new StringBuilder();
            var columnBuilder = new StringBuilder();
            var paramBuilder = new StringBuilder();
            List<SpStructureModel> spStructures = new List<SpStructureModel>();
            using (SqlConn)
            {
                using (SqlCommand cmd = new SqlCommand(string.Format("select * from {0}", tableName), SqlConn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        do
                        {
                            if (reader.FieldCount <= 1) continue;
                            builder.AppendLine(string.Format("Create PROCEDURE [dbo].[sp_{0}_Add]", tableName));
                            var schema = reader.GetSchemaTable();
                            foreach (DataRow row in schema.Rows)
                            {
                                var type = (Type)row["DataType"];
                                var spStructureModel = new SpStructureModel
                                {
                                    Size = row["ColumnSize"].ToString(),
                                    ColumnName = row["ColumnName"].ToString(),
                                    TypeName = SpTypeAliases.ContainsKey(type) ? SpTypeAliases[type] : type.Name
                                };
                                spStructures.Add(spStructureModel);
                            }

                            foreach (var item in spStructures)
                            {
                                if (item.TypeName.ToLower().Contains("char"))
                                {
                                    builder.AppendLine(string.Format("@{0} {1}({2}),", item.ColumnName,
                                        item.TypeName, item.Size));
                                }
                                else
                                {
                                    builder.AppendLine(string.Format("@{0} {1},", item.ColumnName, item.TypeName));
                                }
                            }

                            RemoveLastComma(builder);
                            builder.AppendLine("AS");
                            builder.AppendLine("BEGIN");
                            builder.AppendLine(string.Format("Insert into {0}", tableName));
                            builder.AppendLine("(");
                            foreach (var item in spStructures)
                            {
                                columnBuilder.AppendLine(item.ColumnName + ",");
                                paramBuilder.AppendLine("@" + item.ColumnName + ",");
                            }
                            RemoveLastComma(columnBuilder);
                            RemoveLastComma(paramBuilder);
                            builder.Append(columnBuilder.ToString());
                            builder.AppendLine(")");
                            builder.AppendLine("values ");
                            builder.AppendLine("(");
                            builder.AppendLine(paramBuilder.ToString() + ")");
                            builder.AppendLine("END");
                        } while (reader.NextResult());
                    }
                }
                return Ok(builder.ToString());
            }
        }

        [Route("update")]
        public IHttpActionResult GetUpdate(string tableName)
        {
            var builder = new StringBuilder();
            var builderParam = new StringBuilder();
            List<SpStructureModel> spStructures = new List<SpStructureModel>();
            using (SqlConn)
            {
                using (SqlCommand cmd = new SqlCommand(string.Format("select * from {0}", tableName), SqlConn))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        do
                        {
                            if (reader.FieldCount <= 1) continue;
                            builder.AppendLine(string.Format("Create PROCEDURE [dbo].[sp_{0}_Update]", tableName));
                            var schema = reader.GetSchemaTable();
                            foreach (DataRow row in schema.Rows)
                            {
                                var type = (Type)row["DataType"];
                                var spStructureModel = new SpStructureModel
                                {
                                    Size = row["ColumnSize"].ToString(),
                                    ColumnName = row["ColumnName"].ToString(),
                                    TypeName = SpTypeAliases.ContainsKey(type) ? SpTypeAliases[type] : type.Name
                                };
                                spStructures.Add(spStructureModel);
                            }

                            foreach (var item in spStructures)
                            {
                                if (item.TypeName.ToLower().Contains("char"))
                                {
                                    builder.AppendLine(string.Format("@{0} {1}({2}),", item.ColumnName,
                                        item.TypeName, item.Size));
                                }
                                else
                                {
                                    builder.AppendLine(string.Format("@{0} {1},", item.ColumnName, item.TypeName));
                                }
                            }

                            RemoveLastComma(builder);
                            builder.AppendLine("AS");
                            builder.AppendLine("BEGIN");
                            builder.AppendLine(string.Format("update {0} set",tableName));
                            foreach (var item in spStructures)
                            {
                                builder.AppendLine(string.Format("{0}=@{1},", item.ColumnName, item.ColumnName));
                            }
                            RemoveLastComma(builder);
                            builder.AppendLine("END");
                        } while (reader.NextResult());
                    }
                }
                return Ok(builder.ToString());
            }
        }

        private void RemoveLastComma(StringBuilder builder)
        {
            builder.Remove(builder.Length - 3, 1);
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

        private Dictionary<Type, string> SpTypeAliases = new Dictionary<Type, string> {
            { typeof(int), "Int" },
            { typeof(short), "SmallInt" },
            { typeof(byte), "TinyInt" },
            { typeof(byte[]), "Binary" },
            { typeof(long), "BigInt" },
            { typeof(double), "Double" },
            { typeof(decimal), "Numeric" },
            { typeof(float), "Double" },
            { typeof(bool), "Bit" },
            { typeof(string), "NVarChar" }
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