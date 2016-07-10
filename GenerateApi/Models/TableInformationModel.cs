using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GenerateApi.Models
{
    public class TableInformationModel
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string ColumnDefault { get; set; }
        public string IsNullable { get; set; }
        public string DataType { get; set; }
        public int ColumnLength { get; set; }
    }
}