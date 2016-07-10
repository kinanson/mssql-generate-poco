using ClosedXML.Excel;
using Dapper;
using GenerateApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace GenerateApi.Controllers
{
    public class ExcelController : BaseController
    {
        public string TablesSql { get; set; }
        public string TableInformationSql { get; set; }
        public string ForeignTableSql { get; set; }
        public string ColumnDescriptionSql { get; set; }

        public ExcelController()
        {
            TablesSql = @"select table_name from INFORMATION_SCHEMA.TABLES";
            TableInformationSql = @"
                select TABLE_NAME as TableName,
                COLUMN_NAME as ColumnName,
                COLUMN_DEFAULT as ColumnDefault,
                IS_NULLABLE as IsNullable,
                DATA_TYPE as DataType,
                CHARACTER_MAXIMUM_LENGTH as ColumnLength
                from INFORMATION_SCHEMA.COLUMNS";
            ForeignTableSql = @"
                SELECT OBJECT_NAME(f.parent_object_id) ForeignTable,
                COL_NAME(fc.parent_object_id,fc.parent_column_id) ColumnName,
                OBJECT_NAME (f.referenced_object_id) TableName
                FROM 
                sys.foreign_keys AS f
                INNER JOIN 
                sys.foreign_key_columns AS fc 
                ON f.OBJECT_ID = fc.constraint_object_id
                INNER JOIN 
                sys.tables t 
                ON t.OBJECT_ID = fc.referenced_object_id";
            ColumnDescriptionSql = @"
                select st.name as TableName,sc.name as ColumnName,sep.value as Description
                from sys.tables st
                inner join sys.columns sc on st.object_id = sc.object_id
                left join sys.extended_properties sep on st.object_id = sep.major_id
                and sc.column_id = sep.minor_id
                and sep.name = 'MS_Description'";
        }

        public HttpResponseMessage Get()
        {
            using (SqlConn)
            {
                var tables = SqlConn.Query<string>(TablesSql);
                var tableInformations = SqlConn.Query<TableInformationModel>(TableInformationSql).ToList();
                var foregnTables = SqlConn.Query<ForeignTableModel>(ForeignTableSql).ToList();
                var ColumnDescriptions = SqlConn.Query<ColumnDescriptionModel>(ColumnDescriptionSql).ToList();
                int tablePosition = 2;
                var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("All Table");
                ws.Cell(1, 1).Value = "Table Name";

                foreach (var table in tables)
                {
                    ws.Cell(tablePosition, 1).Value = table;
                    ws.Cell(tablePosition, 1).Hyperlink = new XLHyperlink(string.Format("'{0}'!A1", GetMaxTableName(table)));
                    GenerateSheetsForTableInformation(workbook, table,
                        tableInformations, foregnTables, ColumnDescriptions);
                    tablePosition++;
                }
                ws.Columns().AdjustToContents();
                return DownloadExcel(workbook, "Table Schema");
            }
        }

        private void GenerateSheetsForTableInformation(XLWorkbook workbook, string table,
            List<TableInformationModel> tableInformations,
            List<ForeignTableModel> foregnTables,
            List<ColumnDescriptionModel> columnDescriptions)       
        {
            var ws1 = workbook.Worksheets.Add(GetMaxTableName(table));
            ws1.Cell(1, 1).Value = "back to All table";
            ws1.Cell(1, 1).Hyperlink = new XLHyperlink("'All Table'!A1");
            ws1.Cell(2, 1).Value = "Column Name";
            ws1.Cell(2, 2).Value = "Default Value";
            ws1.Cell(2, 3).Value = "Nullable";
            ws1.Cell(2, 4).Value = "Data Type";
            ws1.Cell(2, 5).Value = "Max Length";
            ws1.Cell(2, 6).Value = "Description";
            ws1.Cell(2, 7).Value = "Foreign Key";
            ws1.Cell(2, 8).Value = "Foreign Table";
            FillTableInformation(table, ws1, tableInformations,columnDescriptions);
            FillForegnInformation(table, ws1, foregnTables);
            ws1.Columns().AdjustToContents();
        }

        private void FillForegnInformation(string table, IXLWorksheet ws1, List<ForeignTableModel> foregnTables)
        {
            int foregnPosition = 3;
            foreach (var foregnTable in foregnTables.Where(x => x.TableName == table))
            {
                ws1.Cell(foregnPosition, 7).Value = foregnTable.ColumnName;
                ws1.Cell(foregnPosition, 8).Value = foregnTable.ForeignTable;
                ws1.Cell(foregnPosition, 8).Hyperlink = new XLHyperlink(string.Format("'{0}'!A1", GetMaxTableName(foregnTable.ForeignTable)));
                foregnPosition++;
            }
        }

        /// <summary>
        /// Fills the table information.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="tableInformations">The table informations from db</param>
        /// <param name="wooksheets">sheets</param>
        private static void FillTableInformation(string table, IXLWorksheet ws1,
            List<TableInformationModel> tableInformations,
            List<ColumnDescriptionModel> columnDescriptions)
        {
            int tableInformationPosition = 3;
            foreach (var tableInformation in tableInformations.Where(x => x.TableName == table))
            {
                ws1.Cell(tableInformationPosition, 1).Value = tableInformation.ColumnName;
                ws1.Cell(tableInformationPosition, 2).Value = tableInformation.ColumnDefault;
                ws1.Cell(tableInformationPosition, 3).Value = tableInformation.IsNullable;
                ws1.Cell(tableInformationPosition, 4).Value = tableInformation.DataType;
                ws1.Cell(tableInformationPosition, 5).Value = tableInformation.ColumnLength;
                ws1.Cell(tableInformationPosition, 6).Value = columnDescriptions.FirstOrDefault(x =>
                x.TableName == tableInformation.TableName &&
                x.ColumnName == tableInformation.ColumnName)?.Description;
                tableInformationPosition++;
            }
        }

        /// <summary>
        /// Because excel tab limit 31 character so I need to limit table Name
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public string GetMaxTableName(string table)
        {
            int tableLength = table.Length >= 30 ? 30 : table.Length;
            table = table.Substring(0, tableLength);
            return table;
        }

        public HttpResponseMessage DownloadExcel(XLWorkbook workbook, string fileName)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new ByteArrayContent(memoryStream.GetBuffer());
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentLength = memoryStream.Length;
                response.Content.Headers.ContentDisposition.FileName = fileName + DateTime.Now + ".xlsx";
                return response;
            }
        }
    }
}