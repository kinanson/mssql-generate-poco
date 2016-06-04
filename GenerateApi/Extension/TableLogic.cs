using GenerateApi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace GenerateApi.Extension
{
    public class TableLogic
    {
        private TextInfo textInfo= CultureInfo.CurrentCulture.TextInfo;

        public void GenerateCamelSpParam(StringBuilder builder, SpStructureModel spStructure)
        {
            string camelCaseColumn = ConvertToCamelCase(spStructure.ColumnName);
            if (spStructure.TypeName.ToLower().Contains("char"))
            {
                builder.AppendLine(string.Format("@{0} {1}({2}),", camelCaseColumn,
                    spStructure.TypeName, spStructure.Size));
            }
            else
            {
                builder.AppendLine(string.Format("@{0} {1},", camelCaseColumn,
                    spStructure.TypeName));
            }
        }

        public void GenerateSpParam(StringBuilder builder, SpStructureModel spStructure)
        {
            if (spStructure.TypeName.ToLower().Contains("char"))
            {
                builder.AppendLine(string.Format("@{0} {1}({2}),", spStructure.ColumnName,
                    spStructure.TypeName, spStructure.Size));
            }
            else
            {
                builder.AppendLine(string.Format("@{0} {1},", spStructure.ColumnName,
                    spStructure.TypeName));
            }
        }

        public SpStructureModel GetSpStructure(DataRow row)
        {
            var type = (Type)row["DataType"];
            var spStructureModel = new SpStructureModel
            {
                Size = row["ColumnSize"].ToString(),
                ColumnName = row["ColumnName"].ToString(),
                TypeName = SpTypeAliases.ContainsKey(type) ? SpTypeAliases[type] : type.Name
            };
            return spStructureModel;
        }

        public string ConvertToCamelCase(string columnName)
        {
            string camelCaseColumn = string.Empty;
            foreach (var splitColumn in columnName.Split('_'))
            {
                string insideVarible = string.Empty;
                insideVarible = splitColumn.ToLower();
                insideVarible = textInfo.ToTitleCase(insideVarible);
                camelCaseColumn += insideVarible;
            }

            return camelCaseColumn;
        }

        public void RemoveLastComma(StringBuilder builder)
        {
            builder.Remove(builder.Length - 3, 1);
        }

        public Dictionary<Type, string> TypeAliases = new Dictionary<Type, string> {
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

        public Dictionary<Type, string> SpTypeAliases = new Dictionary<Type, string> {
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

        public HashSet<Type> NullableTypes = new HashSet<Type> {
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