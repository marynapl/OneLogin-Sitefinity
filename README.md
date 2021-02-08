# OneLogin + Sitefinity

This Sitefinity CMS app demonstrates how to connect to an OpenId Connect Provider like OneLogin for user authentication.

Password Grant flow is used to capture the username/password and authenticate against OneLogin without redirecting the user to a hosted login page. 
The main code for login widget can be found in MVC/Controllers/LoginController.cs

## Setting up OpenId Connect with OneLogin

In order to make this sample work with OneLogin you will need to create an OpenId Connect app in the OneLogin portal. 
See [OneLogin developer documentation](https://developers.onelogin.com/openid-connect) for more details.
