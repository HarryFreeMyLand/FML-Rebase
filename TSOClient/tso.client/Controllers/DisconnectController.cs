﻿using FSO.Client.Regulators;
using FSO.Client.UI.Screens;
using FSO.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Client.Controllers
{
    public class DisconnectController : IDisposable
    {
        private TransitionScreen View;
        private CityConnectionRegulator CityConnectionRegulator;
        private LotConnectionRegulator LotConnectionRegulator;
        private LoginRegulator LoginRegulator;

        int totalComplete = 0;
        int targetComplete = 2;
        private Action<bool> onDisconnected;

        public DisconnectController(TransitionScreen view, CityConnectionRegulator cityRegulator, LotConnectionRegulator lotRegulator, LoginRegulator logRegulator, Network.Network network)
        {
            View = view;
            View.ShowProgress = false;

            CityConnectionRegulator = cityRegulator;
            CityConnectionRegulator.OnTransition += CityConnectionRegulator_OnTransition;
            LotConnectionRegulator = lotRegulator;
            LoginRegulator = logRegulator;
            LoginRegulator.OnError += LoginRegulator_OnError;
            LoginRegulator.OnTransition += LoginRegulator_OnTransition;
        }

        private void LoginRegulator_OnTransition(string state, object data)
        {
            switch (state)
            {
                case "LoggedIn":
                    if (++totalComplete == targetComplete) onDisconnected(false);
                    break;
            }
        }

        private void LoginRegulator_OnError(object data)
        {
            onDisconnected(true);
        }

        private void CityConnectionRegulator_OnTransition(string state, object data)
        {
            switch (state)
            {
                case "Disconnect":
                    break;
                case "Disconnected":
                    if (++totalComplete == targetComplete) onDisconnected(false);
                    break;
            }
        }

        public void Disconnect(Action<bool> onDisconnected, bool forceLogin)
        {
            totalComplete = 0;
            this.onDisconnected = onDisconnected;

            if (forceLogin)
            {
                targetComplete = 1;
                LoginRegulator.Logout();
            }

            CityConnectionRegulator.Disconnect();
            LotConnectionRegulator.Disconnect();

            if (!forceLogin) LoginRegulator.AsyncTransition("AvatarData");
        }

        public void Dispose()
        {
            CityConnectionRegulator.OnTransition -= CityConnectionRegulator_OnTransition;
            LoginRegulator.OnTransition -= LoginRegulator_OnTransition;
            LoginRegulator.OnError -= LoginRegulator_OnError;
        }
    }
}
