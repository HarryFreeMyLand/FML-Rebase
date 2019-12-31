﻿using FSO.Server.Api.Utils;
using FSO.Server.Common;
using FSO.Server.Servers.Api.JsonWebToken;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace FSO.Server.Api.Controllers.Admin
{
    public class AdminOAuthController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage Post([FromBody] AuthRequest auth)
        {
            if (auth.grant_type == "password")
            {
                var api = Api.INSTANCE;
                using (var da = api.DAFactory.Get())
                {
                    var user = da.Users.GetByUsername(auth.username);
                    if (user == null || user.is_banned || !(user.is_admin || user.is_moderator))
                    {
                        return ApiResponse.Json(System.Net.HttpStatusCode.OK, new OAuthError
                        {
                            error = "unauthorized_client",
                            error_description = "user_credentials_invalid"
                        });
                    }

                    var authSettings = da.Users.GetAuthenticationSettings(user.user_id);
                    var isPasswordCorrect = PasswordHasher.Verify(auth.password, new PasswordHash
                    {
                        data = authSettings.data,
                        scheme = authSettings.scheme_class
                    });

                    if (!isPasswordCorrect)
                    {
                        return ApiResponse.Json(System.Net.HttpStatusCode.OK, new OAuthError
                        {
                            error = "unauthorized_client",
                            error_description = "user_credentials_invalid"
                        });
                    }

                    JWTUser identity = new JWTUser();
                    identity.UserName = user.username;
                    var claims = new List<string>();
                    if (user.is_admin || user.is_moderator)
                    {
                        claims.Add("moderator");
                    }
                    if (user.is_admin)
                    {
                        claims.Add("admin");
                    }

                    identity.Claims = claims;
                    identity.UserID = user.user_id;

                    var token = api.JWT.CreateToken(identity);

                    var response = ApiResponse.Json(System.Net.HttpStatusCode.OK, new OAuthSuccess
                    {
                        access_token = token.Token,
                        expires_in = token.ExpiresIn
                    });

                    return response;
                }
            }

            return ApiResponse.Json(System.Net.HttpStatusCode.OK, new OAuthError
            {
                error = "invalid_request",
                error_description = "unknown grant_type"
            });
        }
    }


    public class OAuthError
    {
        public string error_description { get; set; }
        public string error { get; set; }
    }

    public class OAuthSuccess
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
    }

    public class AuthRequest
    {
        public string grant_type { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}
