using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult CreateAzureStorageAccount()
        {
            string resourceGroupName = "{your-resource-group-name}";
            string token = GetAuthorizationHeader();
            string subscriptionId = "{your-subscription-id}";
            string stAccName = "brucestorage" + Guid.NewGuid().ToString().Substring(0, 8);

            TokenCredentials tokenCredential = new TokenCredentials(token);
            var storageManagementClient = new StorageManagementClient(tokenCredential);
            storageManagementClient.SubscriptionId = subscriptionId;

            var result = storageManagementClient.StorageAccounts.CreateAsync(resourceGroupName, stAccName,
                new StorageAccountCreateParameters()
                {
                    Kind = Kind.Storage,
                    Location = "West US",
                    Sku = new Sku(SkuName.StandardLRS)
                }).Result;
            return Json(result,JsonRequestBehavior.AllowGet);
        }

        private string GetAuthorizationHeader()
        {
            string tenantId = "{your-tenant-id}";
            string clientId = "your-aad-app-client-id";
            string clientSecrets = "{your-aad-app-client-secret}";
            AuthenticationResult result = null;

            var context = new AuthenticationContext(String.Format("https://login.windows.net/{0}", tenantId));

            var thread = new Thread(() =>
            {
                var authParam = new PlatformParameters(PromptBehavior.Never, null);
                result = context.AcquireTokenAsync(
                     "https://management.azure.com/"
                    , new ClientCredential(clientId, clientSecrets)
                    ).Result;
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "AquireTokenThread";
            thread.Start();
            thread.Join();

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }
            string token = result.AccessToken;
            return token;
        }
    }
}