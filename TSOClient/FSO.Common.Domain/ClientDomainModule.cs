﻿using FSO.Common.Domain.Realestate;
using FSO.Common.Domain.Shards;
using FSO.Common.Domain.Top100;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Common.Domain
{
    public class ClientDomainModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IShardsDomain>().To<Shards.ClientShards>().InSingletonScope();
            Bind<IRealestateDomain>().To<FSO.Common.Domain.Realestate.RealestateDomain>().InSingletonScope();
            Bind<ITop100Domain>().To<Top100Domain>().InSingletonScope();
        }
    }
}
