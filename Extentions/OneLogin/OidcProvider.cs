using System;
using System.Text;
using Newtonsoft.Json;
using RestSharp;

namespace SitefinityWebApp.Extentions.OneLogin
{
    public class OidcProvider
    {
        public OidcTokenResponse LoginUser(string username, string password)
        {
            var uri = $"https://{OidcOptions.Region}.onelogin.com/oidc/2/token";

            var client = new RestClient(uri);
            var request = new RestRequest(Method.POST);

            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            var authBytes = Encoding.ASCII.GetBytes($"{OidcOptions.ClientId}:{OidcOptions.ClientSecret}");
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(authBytes));

            request.AddParameter("username", username);
            request.AddParameter("password", password);
            request.AddParameter("client_id", OidcOptions.ClientId);
            request.AddParameter("grant_type", "password");
            request.AddParameter("scope", "openid profile email");
            request.AddParameter("response_type", "id_token");

            IRestResponse res = client.Execute(request);

            var tokenReponse = JsonConvert.DeserializeObject<OidcTokenResponse>(res.Content);

            return tokenReponse;
        }

        public OidcUser GetUserInfo(string accessToken)
        {
            var uri = $"https://{OidcOptions.Region}.onelogin.com/oidc/2/me";

            var client = new RestClient(uri);
            var request = new RestRequest(Method.GET);

            request.AddHeader("Authorization", $"Bearer {accessToken}");

            IRestResponse res = client.Execute(request);

            var oidcUser = JsonConvert.DeserializeObject<OidcUser>(res.Content);

            return oidcUser;
        }
    }
}