using System;
using System.Collections.Generic;
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
        private static string DbConnectionString = "Server=tcp:ausemdsmanagedsqldev.public.6464d595512b.database.windows.net,3342;Initial Catalog=MDSODCPresentDev;User ID=a_niqian@development.health.gov.au;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Interactive;";

        public static List<ColumnMetadata> GetTableColumns(string tableName)
        {
            var cmdText = "SELECT c.name AS 'ColumnName', t.name + '(' + cast(c.max_length as varchar(50)) + ')' As 'DataType' FROM sys.columns c JOIN sys.types t ON c.user_type_id = t.user_type_id WHERE c.object_id = Object_id('odc.Activity') and c.name <> 'RecordRowVersion' order by c.name";

            try
            {
                //var inputParams = new SqlParameter[]
                //{
                //    new SqlParameter("@FromDate", SqlDbType.DateTime) {Value = fromDate.Date},
                //    new SqlParameter("@ToDate", SqlDbType.DateTime) {Value = actualToDate}
                //};

                return DatabaseUtils.GetMultipleEntitiesByCmd(DbConnectionString, cmdText, null, 30, ToColumnMetadata);
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
