using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Xrmwise.Xrm.Framework;

namespace MsCrmTools.MetadataDocumentGenerator
{
    public class ColumnMetadata
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
    }

    public class DatabaseMetadataUtils
    {
        public static List<ColumnMetadata> GetTableColumns(SqlConnection conn, string tableName)
        {
            var cmdText = "SELECT c.name AS 'ColumnName', t.name + '(' + cast(c.max_length as varchar(50)) + ')' As 'DataType' FROM sys.columns c JOIN sys.types t ON c.user_type_id = t.user_type_id WHERE c.object_id = Object_id(@TableName) and c.name <> 'RecordRowVersion' order by c.name";

            try
            {
                var inputParams = new SqlParameter[]
                {
                    new SqlParameter("@TableName", SqlDbType.VarChar, 100) {Value = tableName},
                };
                return DatabaseUtils.GetMultipleEntities<ColumnMetadata>(conn, cmdText, inputParams, CommandType.Text, 30, ToColumnMetadata, null, true);
            }
            catch (Exception e)
            {
            }

            return new List<ColumnMetadata>();
        }

        private static ColumnMetadata ToColumnMetadata(SqlDataReader dataReader)
        {
            if (dataReader == null)
            {
                return null;
            }

            return new ColumnMetadata
            {
                ColumnName = dataReader.GetFieldValue<string>("ColumnName"),
                DataType = dataReader.GetFieldValue<string>("DataType"),
            };
        }
    }
}
