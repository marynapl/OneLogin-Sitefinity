using System;
using System.Collections.Generic;
using System.Linq;
using SitefinityWebApp.Extentions.OneLogin;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Authentication.Configuration;
using Telerik.Sitefinity.Authentication.Configuration.SecurityTokenService.ExternalProviders;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Security;
using Telerik.Sitefinity.Security.Model;
using Telerik.Sitefinity.Services;

namespace SitefinityWebApp.Extentions
{
    public class UserService
    {
        public User Authenticate(OidcUser oidcUser)
        {
            // Get Identity Provider
            var providerName = "OpenIDConnect";
            AuthenticationProviderElement externalProvider = Config.Get<AuthenticationConfig>().SecurityTokenService
                .AuthenticationProviders.Values
                .FirstOrDefault(x => x.Name == providerName);
             
            if (externalProvider != null)
            {
                UserManager userManager = UserManager.GetManager(externalProvider.DataProviderName);

                // Find sitefinity user 
                var sitefinityUser = userManager.GetUser(oidcUser.Email);

                if (sitefinityUser != null)
                {
                    // Update sitefinity user

                    SystemManager.RunWithElevatedPrivilege(p => {
                        try
                        {
                            UpdateUser(externalProvider.DataProviderName, externalProvider.Name, oidcUser.Email,
                                oidcUser.Id.ToString(), oidcUser.FirstName, oidcUser.LastName);
                        }
                        catch (Exception ex)
                        {
                            Log.Write($"Failed to update user. Message: {ex.Message}", ConfigurationPolicy.Authentication);
                            throw;
                        }
                    });

                }
                else
                {
                    // Create sitefinity user

                    SystemManager.RunWithElevatedPrivilege(p => {
                        try
                        {
                            CreateUser(externalProvider.DataProviderName, externalProvider.Name, oidcUser.Email,
                                oidcUser.Id.ToString(), oidcUser.FirstName, oidcUser.LastName);
                        }
                        catch (Exception ex)
                        {
                            Log.Write($"Failed to create user. Message: {ex.Message}", ConfigurationPolicy.Authentication);
                            throw;
                        }
                    });
                }

                sitefinityUser = userManager.GetUser(oidcUser.Email);

                return sitefinityUser;
            }

            return null;
        }

        private void CreateUser(string dataProviderName, string externalProviderName, string email, string externalId, string firstName, string lastName)
        {
            // Start transaction
            Guid guid = Guid.NewGuid();
            string transactionName = string.Concat("ExternalLoginCreateUserTransaction", guid.ToString());

            UserManager userManager = UserManager.GetManager(dataProviderName, transactionName);
            UserProfileManager profileManager = UserProfileManager.GetManager(string.Empty, transactionName);
            
            // Create new user
            User newUser = userManager.CreateUser(null);
            newUser.IsBackendUser = false;
            newUser.IsApproved = true;
            newUser.Email = email;
            newUser.ExternalProviderName = externalProviderName;
            newUser.ExternalId = externalId;
            newUser.SetUserName(email);

            // Update user roles
            List<string> autoAssignedRoles = (
                from s in Config.Get<AuthenticationConfig>().SecurityTokenService
                    .AuthenticationProviders[externalProviderName].AutoAssignedRoles
                    .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                select s.Trim()).ToList();

            RoleManager roleManager = RoleManager.GetManager(string.Empty, transactionName);
            List<Role> roles = roleManager.GetRoles().ToList();
            foreach (string newRole in autoAssignedRoles.Distinct())
            {
                Role role = (
                    from r in roles
                    where r.Name == newRole
                    select r).FirstOrDefault();

                if (role == null)
                {
                    continue;
                }

                roleManager.AddUserToRole(newUser, role);
            }

            // Update user profile
            SitefinityProfile newProfile =
                profileManager.CreateProfile(newUser, Guid.NewGuid(), typeof(SitefinityProfile)) as SitefinityProfile;

            if (newProfile != null)
            {
                newProfile.FirstName = firstName;
                newProfile.LastName = lastName;
            }

            // Commit transaction
            TransactionManager.FlushTransaction(transactionName);
            profileManager.RecompileItemUrls(newProfile);
            TransactionManager.CommitTransaction(transactionName);
        }

        private void UpdateUser(string dataProviderName, string externalProviderName, string email, string externalId, string firstName, string lastName)
        {
            // Start transaction
            Guid guid = Guid.NewGuid();
            string transactionName = string.Concat("ExternalLoginUpdateUserTransaction", guid.ToString());

            UserManager userManager = UserManager.GetManager(dataProviderName, transactionName);
            var user = userManager.GetUser(email);
            if (user == null)
            {
                return;
            }
            bool updateIsRequired = false;
            if (user.ExternalProviderName != externalProviderName)
            {
                user.ExternalProviderName = externalProviderName;
                updateIsRequired = true;
            }
            if (user.ExternalId != externalId)
            {
                user.ExternalId = externalId;
                updateIsRequired = true;
            }

            // Update user roles
            List<string> autoAssignedRoles = (
                from s in Config.Get<AuthenticationConfig>().SecurityTokenService
                    .AuthenticationProviders[externalProviderName].AutoAssignedRoles
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                select s.Trim()).ToList();

            RoleManager roleManager = RoleManager.GetManager(string.Empty, transactionName);
            List<Role> roles = roleManager.GetRoles().ToList();
            foreach (string newRole in autoAssignedRoles.Distinct())
            {
                Role role = (
                    from r in roles
                    where r.Name == newRole
                    select r).FirstOrDefault();

                if (role == null)
                {
                    continue;
                }
                roleManager.AddUserToRole(user, role);
                updateIsRequired = true;
            }

            // Update user profile
            UserProfileManager profileManager = UserProfileManager.GetManager(string.Empty, transactionName);
            SitefinityProfile userProfile = profileManager.GetUserProfile<SitefinityProfile>(user) ??
                                            profileManager.CreateProfile(user,
                                                    "Telerik.Sitefinity.Security.Model.SitefinityProfile") as
                                                SitefinityProfile;
            if (userProfile != null)
            {
                userProfile.FirstName = firstName;
                userProfile.LastName = lastName;
                updateIsRequired = true;
            }

            // Commit transaction
            if (updateIsRequired)
            {
                profileManager.RecompileItemUrls(userProfile);
                TransactionManager.CommitTransaction(transactionName);
            }
        }
    }
}