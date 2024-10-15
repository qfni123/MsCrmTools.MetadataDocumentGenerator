using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using MsCrmTools.MetadataDocumentGenerator;
using MsCrmTools.MetadataDocumentGenerator.Generation;
using ODC.Crm.Common;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DbConnector
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var mainApp = new MainApp();
            mainApp.Run();
        }
    }
}
