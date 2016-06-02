using Dapper;
using GenerateApi.Models;
using GenerateApi.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Web.Http;

namespace GenerateApi.Controllers
{
    [RoutePrefix("api/table")]
    public class TableController : BaseController
    {
        private TableLogic tableLogic = new TableLogic();

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
                                var name =tableLogic.TypeAliases.ContainsKey(type) ? tableLogic.TypeAliases[type] : type.Name;
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
                                spStructures.Add(tableLogic.GetSpStructure(row));
                            }

                            foreach (var spStructure in spStructures)
                            {
                                tableLogic.GenerateSpParam(builder, spStructure);
                            }

                            tableLogic.RemoveLastComma(builder);
                            builder.AppendLine("AS");
                            builder.AppendLine("BEGIN");
                            builder.AppendLine(string.Format("Insert into {0}", tableName));
                            builder.AppendLine("(");
                            foreach (var item in spStructures)
                            {
                                string camelCaseColumn = tableLogic.ConvertToCamelCase(item.ColumnName);
                                columnBuilder.AppendLine(item.ColumnName + ",");
                                paramBuilder.AppendLine(string.Format("@{0},", camelCaseColumn));
                            }
                            tableLogic.RemoveLastComma(columnBuilder);
                            tableLogic.RemoveLastComma(paramBuilder);
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
                                spStructures.Add(tableLogic.GetSpStructure(row));
                            }

                            foreach (var spStructure in spStructures)
                            {
                                tableLogic.GenerateSpParam(builder, spStructure);
                            }

                            tableLogic.RemoveLastComma(builder);
                            builder.AppendLine("AS");
                            builder.AppendLine("BEGIN");
                            builder.AppendLine(string.Format("update {0} set", tableName));
                            foreach (var item in spStructures)
                            {
                                string camelCaseColumn = tableLogic.ConvertToCamelCase(item.ColumnName);
                                builder.AppendLine(string.Format("{0}=@{1},", item.ColumnName, camelCaseColumn));
                            }
                            tableLogic.RemoveLastComma(builder);
                            builder.AppendLine("END");
                        } while (reader.NextResult());
                    }
                }
                return Ok(builder.ToString());
            }
        }

       
    }
}