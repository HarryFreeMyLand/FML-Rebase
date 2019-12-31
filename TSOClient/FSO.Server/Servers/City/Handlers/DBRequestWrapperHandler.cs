﻿using FSO.Common.DatabaseService.Model;
using FSO.Common.Serialization.Primitives;
using FSO.Server.Database.DA;
using FSO.Server.Domain;
using FSO.Server.Framework.Aries;
using FSO.Server.Framework.Voltron;
using FSO.Server.Protocol.Voltron.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.Servers.City.Handlers
{
    public class DBRequestWrapperHandler
    {
        private IDAFactory DAFactory;
        private CityServerContext Context;
        private ServerTop100Domain Top100;

        public DBRequestWrapperHandler(CityServerContext context, IDAFactory da, ServerTop100Domain Top100)
        {
            this.DAFactory = da;
            this.Context = context;
            this.Top100 = Top100;
        }

        public void Handle(IVoltronSession session, DBRequestWrapperPDU packet)
        {
            if(packet.Body is cTSONetMessageStandard)
            {
                HandleNetMessage(session, (cTSONetMessageStandard)packet.Body, packet);
            }
        }

        private void HandleNetMessage(IVoltronSession session, cTSONetMessageStandard msg, DBRequestWrapperPDU packet)
        {
            if (!msg.DatabaseType.HasValue) { return; }
            var requestType = DBRequestTypeUtils.FromRequestID(msg.DatabaseType.Value);

            object response = null;

            switch (requestType)
            {
                case DBRequestType.LoadAvatarByID:
                    response = HandleLoadAvatarById(session, msg);
                    break;

                case DBRequestType.SearchExactMatch:
                    response = HandleSearchExact(session, msg);
                    break;

                case DBRequestType.Search:
                    response = HandleSearchWildcard(session, msg);
                    break;

                case DBRequestType.GetTopResultSetByID:
                    response = HandleGetTop100(session, msg);
                    break;
            }

            if(response != null){
                session.Write(new DBRequestWrapperPDU {
                    SendingAvatarID = packet.SendingAvatarID,
                    Badge = packet.Badge,
                    IsAlertable = packet.IsAlertable,
                    Sender = packet.Sender,
                    Body = response
                });
            }
        }

        private object HandleGetTop100(IVoltronSession session, cTSONetMessageStandard msg)
        {
            var request = msg.ComplexParameter as GetTop100Request;
            if (request == null) { return null; }

            var results = Top100.Query(request.Category);

            return new cTSONetMessageStandard()
            {
                MessageID = 0x69AC83C4,
                DatabaseType = DBResponseType.GetTopResultSetByID.GetResponseID(),
                Parameter = msg.Parameter,

                ComplexParameter = new GetTop100Response()
                {
                    Items = results
                }
            };
        }

        private object HandleLoadAvatarById(IVoltronSession session, cTSONetMessageStandard msg)
        {
            var request = msg.ComplexParameter as LoadAvatarByIDRequest;
            if (request == null) { return null; }

            if(request.AvatarId != session.AvatarId){
                throw new Exception("Permission denied, you cannot load an avatar you do not own");
            }

            using (var da = DAFactory.Get())
            {
                var avatar = da.Avatars.Get(session.AvatarId);
                if (avatar == null) return null;

                var bonus = da.Bonus.GetByAvatarId(avatar.avatar_id);

                return new cTSONetMessageStandard()
                {
                    MessageID = 0x8ADF865D,
                    DatabaseType = DBResponseType.LoadAvatarByID.GetResponseID(),
                    Parameter = msg.Parameter,

                    ComplexParameter = new LoadAvatarByIDResponse()
                    {
                        AvatarId = session.AvatarId,
                        Cash = (uint)avatar.budget,
                        Bonus = bonus.Select(x =>
                        {
                            return new LoadAvatarBonus() {
                                PropertyBonus = x.bonus_property == null ? (uint)0 : (uint)x.bonus_property.Value,
                                SimBonus = x.bonus_sim == null ? (uint)0 : (uint)x.bonus_sim.Value,
                                VisitorBonus = x.bonus_visitor == null ? (uint)0 : (uint)x.bonus_visitor,
                                Date = x.period.ToShortDateString()
                            };
                        }).ToList()
                    }
                };
            };
        }


        private object HandleSearchExact(IVoltronSession session, cTSONetMessageStandard msg)
        {
            var request = msg.ComplexParameter as SearchRequest;
            if (request == null) { return null; }

            using (var db = DAFactory.Get())
            {
                List<SearchResponseItem> results = null;

                if (request.Type == SearchType.SIMS)
                {
                    results = db.Avatars.SearchExact(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = x.avatar_id
                    }).ToList();
                }
                else if (request.Type == SearchType.NHOOD)
                {
                    results = db.Neighborhoods.SearchExact(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = (uint)x.neighborhood_id
                    }).ToList();
                }
                else
                {
                    results = db.Lots.SearchExact(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = x.location
                    }).ToList();
                }

                return new cTSONetMessageStandard()
                {
                    MessageID = 0xDBF301A9,
                    DatabaseType = DBResponseType.SearchExactMatch.GetResponseID(),
                    Parameter = msg.Parameter,

                    ComplexParameter = new SearchResponse()
                    {
                        Query = request.Query,
                        Type = request.Type,
                        Items = results
                    }
                };
            }
        }

        private object HandleSearchWildcard(IVoltronSession session, cTSONetMessageStandard msg)
        {
            var request = msg.ComplexParameter as SearchRequest;
            if (request == null) { return null; }

            using (var db = DAFactory.Get())
            {
                List<SearchResponseItem> results = null;

                if (request.Type == SearchType.SIMS)
                {
                    results = db.Avatars.SearchWildcard(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = x.avatar_id
                    }).ToList();
                }
                else if (request.Type == SearchType.NHOOD)
                {
                    results = db.Neighborhoods.SearchWildcard(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = (uint)x.neighborhood_id
                    }).ToList();
                }
                else
                {
                    results = db.Lots.SearchWildcard(Context.ShardId, request.Query, 100).Select(x => new SearchResponseItem
                    {
                        Name = x.name,
                        EntityId = x.location
                    }).ToList();
                }

                return new cTSONetMessageStandard()
                {
                    MessageID = 0xDBF301A9,
                    DatabaseType = DBResponseType.Search.GetResponseID(),
                    Parameter = msg.Parameter,

                    ComplexParameter = new SearchResponse()
                    {
                        Query = request.Query,
                        Type = request.Type,
                        Items = results
                    }
                };
            }
        }
    }
}
