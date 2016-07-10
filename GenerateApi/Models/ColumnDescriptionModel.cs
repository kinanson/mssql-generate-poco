using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GenerateApi.Models
{
    public class ColumnDescriptionModel
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string Description { get; set; }
    }
}