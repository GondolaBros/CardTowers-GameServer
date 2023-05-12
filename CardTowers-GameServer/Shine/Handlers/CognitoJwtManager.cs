using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;

namespace CardTowers_GameServer.Shine.Handlers
{
    public class CognitoJwtManager
    {
        private const int CacheDurationInMinutes = 60; // Adjust as needed
        private List<JsonWebKey> _keysCache;
        private DateTime _cacheExpiration;
        private readonly string _userPoolId;
        private readonly string _region;

        public CognitoJwtManager(string userPoolId, string region)
        {
            _userPoolId = userPoolId;
            _region = region;
        }

        private async Task<List<JsonWebKey>> FetchJsonWebKeysAsync()
        {
            if (_keysCache != null && DateTime.UtcNow < _cacheExpiration)
            {
                return _keysCache;
            }

            using (var httpClient = new HttpClient())
            {
                var keysJson = await httpClient.GetStringAsync($"https://cognito-idp.{_region}.amazonaws.com/{_userPoolId}/.well-known/jwks.json");
                _keysCache = JObject.Parse(keysJson)["keys"].ToObject<List<JsonWebKey>>();
                _cacheExpiration = DateTime.UtcNow.AddMinutes(CacheDurationInMinutes);
                return _keysCache;
            }
        }
        

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var keys = await FetchJsonWebKeysAsync();

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://cognito-idp.{_region}.amazonaws.com/{_userPoolId}",
                ValidateLifetime = true,
                IssuerSigningKeys = keys,
                ValidateAudience = false,
            };

            try
            {
                handler.ValidateToken(token, parameters, out _);
                return true;
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                // If validation fails because of missing key, refresh keys from JWKS endpoint
                _keysCache = null;
                keys = await FetchJsonWebKeysAsync();
                parameters.IssuerSigningKeys = keys;

                try
                {
                    handler.ValidateToken(token, parameters, out _);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }


        public string? GetUsernameFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.FirstOrDefault(c => c.Type == "cognito:username")?.Value;
        }


        public string? GetSubjectFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
        }

    }
}

