## Swagger-UI example using ASP.NET Core (.NET 6) minimal web API project with implicit OAuth flow JSON Web Token authentication

### Goals

1. Show how to request an id_token with the implicit auth flow from swagger-ui.
2. Show how to validate a JWT in ASP.NET Core with a few lines of code and without needing IdentityServer or database tables.
3. Show how to use a self-build swagger-ui artifacts that allow debugging swagger-ui sources. 

### How to run

Clone the repository  
`git clone https://github.com/inouiw/SwaggerUIJsonWebToken.git`

Open the solution with VsCode  
`code SwaggerUIJsonWebToken`

Press `F5` to debug

### About the code

File `Program.cs`

- Configures a ASP.NET minimal web api application.
- Adds the middleware: Swagger, SwaggerUI, Authentication with JwtBearer.
- Verifies the id-token Signature, Issuer, Audience and ExpirationTime using the `Microsoft.AspNetCore.Authentication.JwtBearer` package. Note that the public key is cached and refreshed, if expired, by the middleware.
- Adds an endpoint that, when called, returns the authentication status of the caller and the claims in the Bearer JWT. The claims are accessible in `context.User.Claims`.
- Adds a route which loads swagger-ui artifacts from `wwwroot/swagger` instead of using the sources from the middleware.

File `wwwroot/swagger-extensions/my-index.html`

- Contains the swagger-ui index.html modified to load the default export of `my-swagger-ui-plugins.js` as swagger-ui plugin.

File `wwwroot/swagger-extensions/my-swagger-ui-plugins.js`

- Contains `wrapActions` that add custom behavior to the swagger-ui actions `authPopup` and `authorizeOauth2WithPersistOption`. The actions modify the token endpoint URL to include `response_type=token id_token` and `nonce=<random value>` and verify the returned `nonce` value.

Folder `wwwroot/swagger`

- Contains a build of wagger-ui-4.12.0 with sourcemaps enabled.


### Details

Swagger UI does not support the OAuth 2.0 implicit grant flow with id_token. Swagger supports OAuth2 implicit flow but it always sets `response_type=token` in the request (see https://github.com/swagger-api/swagger-ui/blob/570d26a0908e7d8cc3c3193e5d9ecbe63e494c0e/src/core/oauth2-authorize.js#L23), however `response_type=token id_token` is required.

As alternative to OAuth, swagger-ui also supports OpenID Connect (OIDC), however not the implicit flow but only the authorization code flow. In that case the response from the authorization provider with the code is redirected to the server to a `oauth2-redirect.html` file, then the client secret is sent to the browser together with the code. For me this seems wrong because the client secret should stay on the server. Also the implicit id-token flow has [advantages](#what-are-the-advantages-of-the-implicit-flow).

However, swagger-ui offers very nice ways to extend and customize almost everything. So, I made use of `wrapActions` that are just JavaScript methods which get called any time when a call to the wrapped action is made. (See [Plugin system overview](https://swagger.io/docs/open-source-tools/swagger-ui/customization/overview/))  
I created two wrapActions, one to modify the response_type and set a random nonce and one to validate the nonce value. See `wwwroot/swagger-extensions/my-swagger-ui-plugins.js`

### What is the difference between the implicit flow with id-token and the authorization code flow?

With the implicit flow the client receives the claims reply as JWT/id-token. The client can use the claims only in the single-page-app or it can use the id-token to authenticate (and maybe also authorize) with the server. To authenticate with the server the client app sends the id-token in the authorization header. The server can verify if the claims in the id-token can be trusted by verifying the token signature using a public key from the `.well-known/openid-configuration`.  
Note that you can always use an id-token to authenticate the user but to authorize a user with an id-token it would be required that the token contains all claims that you need to determine if a user is allowed to access a resource. 

With the authorization code flow the client requests a authorization code which is then send to the server. The server uses the code and the client secret to request a access token and refresh token. Then the server can use the access token to get the user claims. The server will have to set some cookie or generate a JWT for the client to know that it is now authenticated. An advantage if you generate the JWT yourself is that you can add custom claims, although the token should not be too large.

### What are the advantages of the implicit flow?

As can be seen in the above description, with the implicit flow, the id-token can be used by the client to authenticate. With the authorization code flow a server request is needed and the code cannot be used by the client to authenticate because it expires soon.

### Some notes

To intercept the HTTPS Traffic, I use the cross-platform open source HTTP Toolkit.

If you get the error `400: redirect_uri_mismatch` from the authentication provider, as google, then first restart the server and check again, if the error appears again, you will likely need to add your URIs as `https://localhost:7253/swagger/oauth2-redirect.html` to the "Authorized redirect URIs". For google you can set it in the Google Cloud Console. In this application it is never redirected to the redirect URL because the client receives the response from the authentication provider but the redirect link is not followed because it is not needed.

The main file `Program.cs` contains a client-id that I created. You can use [Google Could Console](https://console.cloud.google.com/) to generate your own google client-id.

To read more about the implicit OAuth flow you may start at the google documentation for [OAuth 2.0 for Client-side Web Applications](https://developers.google.com/identity/protocols/oauth2/javascript-implicit-flow) and [OpenID Connect](https://developers.google.com/identity/protocols/oauth2/openid-connect), or the [Microsoft Azure Implicit Grant Flow Docs](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-implicit-grant-flow).

### Debugging

After starting the solution and opening the browser developer tools, you should be able to see `SwaggerUIBundle` in the Sources tab. You can browse the Swagger-UI source code and set breakpoints.

![Source Maps Debugging Screenshot](source-maps-debugging-screenshot.png)

### Keywords

OAuth, Delegated authentication, authorization server, access token, access_token, scopes (openid, email, profile), resource owner, clientid, client_id, clientsecret, bearer, credentials, secret, OAuth2, nonce, identity, client-id, oauth2RedirectUrl

Swagger Plugin API, swagger-ui, Swashbuckle, wrapActions example, x-tokenName

ASP.NET Core 6 minimal Web API, validate JwtBearer, UseSwaggerUI (OAuthClientId, IndexStream, SwaggerEndpoint, RoutePrefix), UseSwagger, AddSwaggerGen (OpenApiOAuthFlows, SwaggerDoc, AddSecurityDefinition, SecuritySchemeType, OpenApiOAuthFlows, OpenApiSecurityRequirement, OpenApiReference), AddAuthentication(AddJwtBearer, MetadataAddress, TokenValidationParameters), AddEndpointsApiExplorer, UseAuthentication

JavaScript, extension, plugin, debug, chrome, sourcemaps, swagger-ui-bundle.js, swagger-ui-standalone-preset.js, ConfigObject, OAuthConfigObject, oauth2-redirect.html, requestInterceptor, ResponseInterceptorFunction, SwaggerUIBundle, initOAuth, authPopup, authorizeOauth2WithPersistOption

### Contributors

David Neuy