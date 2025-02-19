using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using AzureADUserManager.Models; // Import your model namespace
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Azure.Identity;
using System;

namespace AzureADUserManager.Controllers
{
    public class UsersController : Controller
    {
        private readonly IConfiguration _configuration;

        public UsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var tenantId = _configuration.GetValue<string>("AzureAd:TenantId");
                var clientId = _configuration.GetValue<string>("AzureAd:ClientId");
                var clientSecret = _configuration.GetValue<string>("AzureAd:ClientSecret");

                // Explicitly specify the type for ClientSecretCredential
                var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret);
                GraphServiceClient graphServiceClient = new GraphServiceClient(clientSecretCredential);

                List<AzureADUser> allUsers = new List<AzureADUser>();

                // Use GetAsync with request configuration to select properties
                var usersResponse = await graphServiceClient.Users.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new string[] { "id", "displayName", "mail" };
                });

                if (usersResponse != null && usersResponse.Value != null)
                {
                    foreach (var user in usersResponse.Value)
                    {
                        allUsers.Add(new AzureADUser
                        {
                            Id = user.Id,
                            DisplayName = user.DisplayName,
                            Mail = user.Mail
                        });
                    }
                }

                return View(allUsers);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., authentication issues, Graph API errors)
                Console.WriteLine($"Error: {ex.Message}");
                // Consider logging the exception or displaying an error message to the user.
                ViewBag.ErrorMessage = "An error occurred while retrieving users.";
                return View(new List<AzureADUser>()); // Return an empty list or an error view.
            }
        }
    }
}
