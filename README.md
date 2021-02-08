# OneLogin + Sitefinity

This Sitefinity CMS app demonstrates how to connect to an OpenId Connect Provider like OneLogin for user authentication.

Password Grant flow is used to capture the username/password and authenticate against OneLogin without redirecting the user to a hosted login page. 
The main code for login widget can be found in `MVC/Controllers/LoginController.cs`

## Customizing configuration

* Update `ClientId` and `ClintSecret` in `OidcOptions.cs`
* Update settings in `AuthenticationConfig.config`. Read more in [Sitefinity developer documentation](https://www.progress.com/documentation/sitefinity-cms/administration-configure-the-openid-connect-provider).

## Setting up OpenId Connect with OneLogin

In order to make this sample work with OneLogin you will need to create an OpenId Connect app in the OneLogin portal. 
See [OneLogin developer documentation](https://developers.onelogin.com/openid-connect) for more details.

## License

This project is licensed under the MIT license.