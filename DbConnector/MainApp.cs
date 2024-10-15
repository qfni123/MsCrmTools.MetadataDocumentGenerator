using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using MsCrmTools.MetadataDocumentGenerator;
using MsCrmTools.MetadataDocumentGenerator.Generation;
using MsCrmTools.MetadataDocumentGenerator.Helper;
using ODC.Crm.Common;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.ServiceModel;

namespace DbConnector
{
    class MainApp
    {
        private static string DbConnectionString = "Server=tcp:ausemdsmanagedsqldev.public.6464d595512b.database.windows.net,3342;Initial Catalog=MDSODCPresentDev;User ID=a_niqian@development.health.gov.au;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Interactive;";

        public void Run()
        {
            // var x = DatabaseMetadataUtils.GetTableColumns("Activity");
            var orgService = CrmHelper.GetOrgService();

            using (var conn = new SqlConnection(DbConnectionString))
            {
                conn.Open();
                this.GenerateMetedataDocument(orgService, conn);
            }
        }

        private void GenerateMetedataDocument(IOrganizationService orgService, SqlConnection sqlConnection)
        {
            var docGenerator = new ExcelDocument();
            var settings = new GenerationSettings();
            settings.AddAuditInformation = false;
            settings.AddEntitiesSummary = true;
            settings.AddFieldSecureInformation = false;
            settings.AddRequiredLevelInformation = false;
            settings.AddValidForAdvancedFind = false;
            settings.AddFormLocation = false;
            settings.GenerateOnlyOneTable = false;
            settings.ExcludeVirtualAttributes = false;

            settings.DisplayNamesLangugageCode = 1033;
            settings.FilePath = "z:\\temp\\text.xlsx";
            settings.OutputDocumentType = Output.Excel;
            settings.AttributesSelection = AttributeSelectionOption.AllAttributes;
            settings.IncludeOnlyAttributesOnForms = false;
            settings.Prefixes = new List<string>
            {
                "doh_"
            };

            var solutions = this.RetrieveSolutions(orgService);
            var entities = MetadataHelper.GetEntities(solutions.Entities.ToList(), orgService);
            settings.EntitiesToProceed = entities.Select(e =>
            new EntityItem
            {
                Attributes = new List<string>(),
                Name = e.LogicalName,
                Forms = new List<Guid>()
            }).ToList();

            docGenerator.Settings = settings;
            docGenerator.Generate(orgService, sqlConnection);
        }

        private EntityCollection RetrieveSolutions(IOrganizationService service)
        {
            try
            {
                QueryExpression qe = new QueryExpression("solution");
                qe.Distinct = true;
                qe.ColumnSet = new ColumnSet(true);
                qe.Criteria = new FilterExpression();
                qe.Criteria.AddCondition(new ConditionExpression("isvisible", ConditionOperator.Equal, true));
                qe.Criteria.AddCondition(new ConditionExpression("uniquename", ConditionOperator.Equal, "ODCLicenceRegisterCRM"));

                return service.RetrieveMultiple(qe);
            }
            catch (Exception error)
            {
                if (error.InnerException is FaultException)
                {
                    throw new Exception("Error while retrieving solutions: " + error.InnerException.Message);
                }

                throw new Exception("Error while retrieving solutions: " + error.Message);
            }
        }
    }
}
