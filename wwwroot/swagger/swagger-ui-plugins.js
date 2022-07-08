// Extension of swagger-ui with wrapActions.
const SwaggerUIStatePlugins = function (system) {
  return {
    statePlugins: {
      auth: {
        wrapActions: {
          // Called when you click in the 'Authorize' in the Authorize pop-up.
          authPopup: (oriAction, system) => (url, swaggerUIRedirectOauth2) => {
            let nonce = system.nonceValue; // see rootInjects
            let urlNew = url.replace('response_type=token', `response_type=token id_token&nonce=${nonce}`);
            console.log('url from authPopup wrapAction: ' + urlNew);
            return oriAction(urlNew, swaggerUIRedirectOauth2)
          },
          authorizeOauth2WithPersistOption: (oriAction, system) => ({ auth, token }) => {
            console.log('auth from authorizeOauth2WithPersistOption wrapAction: ' + JSON.stringify(auth));
            let jwtToken = token['id_token'];
            let jwtTokenNonce = parseJwt(jwtToken)['nonce'];
            let submittedNonce = system.nonceValue; // see rootInjects

            if (submittedNonce !== jwtTokenNonce) {
              const message = `Nonce in JwtToken does not match submitted nonce. Submitted nonce: ${submittedNonce}, Nonce in JwtToken: ${jwtTokenNonce}`;
              token['error'] = message;
              console.error(message);
            }
            return oriAction({ auth, token });
          }
        }
      }
    },
    rootInjects: {
      nonceValue: randomNonce()
    }
  }
}

// Source: https://stackoverflow.com/questions/38552003/how-to-decode-jwt-token-in-javascript-without-using-a-library
function parseJwt(token) {
  var base64Url = token.split('.')[1];
  var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
    return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
  }).join(''));

  return JSON.parse(jsonPayload);
}

// method from https://github.com/swagger-api/swagger-ui/issues/3517
function randomNonce() {
  let length = 12;
  let text = "";
  let possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  for (var i = 0; i < length; i++) {
    text += possible.charAt(Math.floor(Math.random() * possible.length));
  }
  return text;
}

const SwaggerUIPlugins = [SwaggerUIStatePlugins];
export default SwaggerUIPlugins;