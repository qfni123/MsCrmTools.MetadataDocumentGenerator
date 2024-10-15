using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;

namespace ODC.Crm.Common
{
    public static class CrmHelper
    {
        public static IOrganizationService GetOrgService()
        {
            return GetOrgService("OrganizationName");
        }

        public static IOrganizationService GetOrgService(string orgNameKey)
        {
            var organizationName = ConfigurationManager.AppSettings[orgNameKey];
            var crmUrlTemplate = ConfigurationManager.AppSettings["CRM.Url"];

            if (string.IsNullOrWhiteSpace(organizationName))
            {
                throw new Exception("Organization name is configured in appsettings.json.");
            }
            if (string.IsNullOrWhiteSpace(crmUrlTemplate))
            {
                throw new Exception("CRM URL template is configured in appsettings.json.");
            }

            var crmUrl = crmUrlTemplate.Replace("{OrganizationName}", organizationName);
            if (string.IsNullOrWhiteSpace(crmUrl))
            {
                Console.Error.WriteLine("CRM.Url is not configured properly. This process is terminated.");
                return null;
            }

            var urlElements = crmUrl.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToArray();
            var username = string.Empty;
            var password = string.Empty;
            var clientId = string.Empty;
            var clientSecret = string.Empty;

            foreach (var urlElement in urlElements)
            {
                var elements = urlElement.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries).Select(e => e.Trim()).ToArray();
                if (elements.Length != 2)
                {
                    Console.Error.WriteLine("Invalid value for 'CRM.Url' in configuration. This process is terminated.");
                    return null;
                }

                if (elements[0].Equals("Username", StringComparison.OrdinalIgnoreCase))
                {
                    username = elements[1];
                    continue;
                }

                if (elements[0].Equals("Password", StringComparison.OrdinalIgnoreCase))
                {
                    password = elements[1];
                    continue;
                }
                if (elements[0].Equals("ClientId", StringComparison.OrdinalIgnoreCase))
                {
                    clientId = elements[1];
                    continue;
                }
                if (elements[0].Equals("ClientSecret", StringComparison.OrdinalIgnoreCase))
                {
                    clientSecret = elements[1];
                    continue;
                }
            }

            string pwdFileFolder = GetPasswordFromFileFolder();
            crmUrl = crmUrl.Replace("${PasswordFileFolder}", pwdFileFolder);

            string[] passwordElements = null;
            if ((username == "{0}" && password == "{1}") || (clientId == "{0}" && clientSecret == "{1}"))
            {
                passwordElements = GetPasswordFromFile(pwdFileFolder);
            }

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var crmConnection = GetCrmConnection(crmUrl, passwordElements);
            if (crmConnection == null)
            {
                // Console..Error.WriteLine("Cannot establish connection to CRM.");
                throw new Exception("Cannot establish connection to CRM: Check your stored CRM password");
            }
            return crmConnection;
        }

        private static CrmServiceClient GetCrmConnection(string crmUrl, string[] passwordElements)
        {
            try
            {
                var url = crmUrl;
                if (passwordElements != null)
                {
                    url = string.Format(url, passwordElements[0], passwordElements[1]);
                }

                var conn = new CrmServiceClient(url);
                if (!conn.IsReady) //Connection failed.
                {
                    Console.Error.WriteLine($"CRM connection failed: ");
                    Console.Error.WriteLine($"CrmConnectOrgUriActual = {conn.CrmConnectOrgUriActual}");
                    Console.Error.WriteLine($"OAuthUserId = {conn.OAuthUserId}");
                    Console.Error.WriteLine($"LastCrmException.Message = {conn.LastCrmException?.Message}");
                    Console.Error.WriteLine($"LastCrmException.StackTrace = {conn.LastCrmException?.StackTrace}");
                    Console.Error.WriteLine($"LastCrmException.InnerException = {conn.LastCrmException?.InnerException}");
                    Console.Error.WriteLine($"LastCrmError = {conn.LastCrmError}");
                    return null;
                }

                return conn; // Connection successful.
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("GetCrmConnection: Error: " + e);
                return null;
            }
        }

        private static bool IsRelativePath(string path)
        {
            if (path == null)
            {
                return false;
            }
            if (path.StartsWith("\\") || path.StartsWith("/"))
            {
                return false;
            }

            return path.IndexOf(":") < 0;
        }

        private static string GetPasswordFromFileFolder()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var pwdFileFolder = ConfigurationManager.AppSettings["PasswordFileFolder"];
            var pwdFile = ConfigurationManager.AppSettings["PasswordFile"];
            if (string.IsNullOrEmpty(pwdFileFolder))
            {
                pwdFileFolder = appDataFolder;
            }
            else if (pwdFileFolder.StartsWith("${AppData}"))
            {
                pwdFileFolder = pwdFileFolder.Replace("${AppData}", appDataFolder);
            }
            return pwdFileFolder;
        }

        private static string[] GetPasswordFromFile(string pwdFileFolder)
        {
            var pwdFile = ConfigurationManager.AppSettings["PasswordFile"];
            var pwdFileFullPath = IsRelativePath(pwdFile) ? Path.Combine(pwdFileFolder, pwdFile) : pwdFile;
            if (!string.IsNullOrWhiteSpace(pwdFileFullPath))
            {
                if (File.Exists(pwdFileFullPath))
                {
                    var fileContent = File.ReadAllLines(pwdFileFullPath);
                    if (fileContent.Length == 2)
                    {
                        return fileContent;
                    }
                    throw new Exception($"Password file is not in a valid format: {pwdFileFullPath}");
                }
            }

            throw new Exception($"Password file is not found or not valid: {pwdFileFullPath}");
        }
    }
}
