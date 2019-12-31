﻿using FSO.Server.Api.Utils;
using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Linq;

namespace FSO.Server.Api.Controllers.Admin
{
    public class AdminHostsController : ApiController
    {
        public HttpResponseMessage Get()
        {
            var api = Api.INSTANCE;
            api.DemandAdmin(Request);
            var hosts = api.HostPool.GetAll();

            return ApiResponse.Json(HttpStatusCode.OK, hosts.Select(x => new {
                role = x.Role,
                call_sign = x.CallSign,
                internal_host = x.InternalHost,
                public_host = x.PublicHost,
                connected = x.Connected,
                time_boot = x.BootTime
            }));
        }
    }
}
