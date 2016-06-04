using GenerateApi.Extension;
using GenerateApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace GenerateApi.Service
{
    public class TableService : ITableService
    {
        private TableLogic tableLogic = new TableLogic();

        public StringBuilder GenerateInsert(string tableName, SqlConnection sqlConn)
        {
            var builder = new StringBuilder();
            var columnBuilder = new StringBuilder();
            var paramBuilder = new StringBuilder();
            List<SpStructureModel> spStructures = new List<SpStructureModel>();
            using (sqlConn)
            {
                using (SqlCommand cmd = new SqlCommand(string.Format("select * from {0}", tableName), sqlConn))
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
                                columnBuilder.AppendLine(item.ColumnName + ",");
                                paramBuilder.AppendLine("@" + item.ColumnName + ",");
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
                return builder;
            }
        }

        public StringBuilder GenerateUpdate(string tableName, SqlConnection sqlConn)
        {
            var builder = new StringBuilder();
            var builderParam = new StringBuilder();
            List<SpStructureModel> spStructures = new List<SpStructureModel>();
            using (sqlConn)
            {
                using (SqlCommand cmd = new SqlCommand(string.Format("select * from {0}", tableName), sqlConn))
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

                            foreach (var item in spStructures)
                            {  
                                tableLogic.GenerateSpParam(builder, item);
                            }

                            tableLogic.RemoveLastComma(builder);
                            builder.AppendLine("AS");
                            builder.AppendLine("BEGIN");
                            builder.AppendLine(string.Format("update {0} set", tableName));
                            foreach (var item in spStructures)
                            {
                                builder.AppendLine(string.Format("{0}=@{1},", item.ColumnName, item.ColumnName));
                            }
                            tableLogic.RemoveLastComma(builder);
                            builder.AppendLine("END");
                        } while (reader.NextResult());
                    }
                }
                return builder;
            }
        }
    }
}