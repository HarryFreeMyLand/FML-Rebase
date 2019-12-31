﻿using FSO.Server.Common;
using FSO.Server.Protocol.Aries;
using FSO.Server.Protocol.Utils;
using FSO.Server.Protocol.Voltron.Packets;
using Mina.Core.Future;
using Mina.Core.Service;
using Mina.Core.Session;
using Mina.Filter.Codec;
using Mina.Filter.Logging;
using Mina.Filter.Ssl;
using Mina.Transport.Socket;
using Ninject;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.Clients
{
    public interface IAriesMessageSubscriber
    {
        void MessageReceived(AriesClient client, object message);
    }
    
    public interface IAriesEventSubscriber
    {
        void SessionCreated(AriesClient client);
        void SessionOpened(AriesClient client);
        void SessionClosed(AriesClient client);
        void SessionIdle(AriesClient client);
        void InputClosed(AriesClient session);
    }

    public class NullIOHandler : IoHandler
    {
        public void ExceptionCaught(IoSession session, Exception cause)
        {
        }

        public void InputClosed(IoSession session)
        {
        }

        public void MessageReceived(IoSession session, object message)
        {
        }

        public void MessageSent(IoSession session, object message)
        {
        }

        public void SessionClosed(IoSession session)
        {
        }

        public void SessionCreated(IoSession session)
        {
        }

        public void SessionIdle(IoSession session, IdleStatus status)
        {
        }

        public void SessionOpened(IoSession session)
        {
        }
    }

    public class AriesClient : IoHandler
    {
        //private static Logger LOG = LogManager.GetCurrentClassLogger();

        private IoConnector Connector;
        private IoSession Session;
        private IKernel Kernel;

        private List<IAriesMessageSubscriber> MessageSubscribers = new List<IAriesMessageSubscriber>();
        private List<IAriesEventSubscriber> EventSubscribers = new List<IAriesEventSubscriber>();
        
        public AriesClient(IKernel kernel)
        {
            this.Kernel = kernel;
        }

        public void AddSubscriber(object sub)
        {
            lock (EventSubscribers)
            {
                if (sub is IAriesEventSubscriber)
                {
                    EventSubscribers.Add((IAriesEventSubscriber)sub);
                }
                if (sub is IAriesMessageSubscriber)
                {
                    MessageSubscribers.Add((IAriesMessageSubscriber)sub);
                }
            }
        }

        public void RemoveSubscriber(object sub)
        {
            lock (EventSubscribers)
            {
                if (sub is IAriesEventSubscriber)
                {
                    EventSubscribers.Remove((IAriesEventSubscriber)sub);
                }
                if (sub is IAriesMessageSubscriber)
                {
                    MessageSubscribers.Remove((IAriesMessageSubscriber)sub);
                }
            }
        }

        public void Connect(string address){
            Connect(IPEndPointUtils.CreateIPEndPoint(address));
        }

        public void Disconnect(){
            if (Session != null)
            {
                Session.Close(false);
            }
        }

        public void Connect(IPEndPoint target)
        {
            if (Connector != null)
            {
                //old connector might still be establishing connection...
                //we need to stop that
                Connector.Handler = new NullIOHandler(); //don't hmu
                //we can't cancel a mina.net connector, but we can sure as hell ~~avenge it~~ stop it from firing events.
                //if we tried to dispose it, we'd get random disposed object exceptions because mina doesn't expect you to cancel that early.
                Disconnect(); //if we have already established a connection, make sure it is closed.
            }
            Connector = new AsyncSocketConnector();
            var connector = Connector;
            Connector.ConnectTimeoutInMillis = 10000;
            //Connector.FilterChain.AddLast("logging", new LoggingFilter());
            
            Connector.Handler = this;
            //var ssl = new CustomSslFilter((X509Certificate)null);
            //ssl.SslProtocol = System.Security.Authentication.SslProtocols.Tls;
            //Connector.FilterChain.AddFirst("ssl", ssl);

            Connector.FilterChain.AddLast("protocol", new ProtocolCodecFilter(new AriesProtocol(Kernel)));
            var future = Connector.Connect(target, (IoSession session, IConnectFuture future2) =>
            {
                if (future2.Canceled || future2.Exception != null)
                {
                   if (connector.Handler != null) SessionClosed(session);
                }
                
                if (connector.Handler is NullIOHandler) session.Close(true);
                else this.Session = session;
            });

            Task.Run(() =>
            {
                if (!future.Await(10000)) SessionClosed(null);
                if (future.Canceled || future.Exception != null) SessionClosed(null);
            });
        }

        private void OnConnect(IoSession session, IConnectFuture future)
        {
            if (future.Canceled || future.Exception != null) SessionClosed(session);
            this.Session = session;
        }

        public void Write(params object[] packets)
        {
            if (this.Session != null && this.Session.Connected)
            {
                this.Session.Write(packets);
            }
        }

        public bool IsConnected
        {
            get
            {
                return Session != null && Session.Connected;
            }
        }

        public void SessionCreated(IoSession session)
        {
            List<IAriesEventSubscriber> _subs;
            lock (EventSubscribers)
                _subs = new List<IAriesEventSubscriber>(EventSubscribers);
            _subs.ForEach(x => x.SessionCreated(this));
        }

        public void SessionOpened(IoSession session)
        {
            List<IAriesEventSubscriber> _subs;
            lock (EventSubscribers)
                _subs = new List<IAriesEventSubscriber>(EventSubscribers);
            _subs.ForEach(x => x.SessionOpened(this));
        }

        public void SessionClosed(IoSession session)
        {
            List<IAriesEventSubscriber> _subs;
            lock (EventSubscribers)
                _subs = new List<IAriesEventSubscriber>(EventSubscribers);
            _subs.ForEach(x => x.SessionClosed(this));
        }

        public void SessionIdle(IoSession session, IdleStatus status)
        {
            List<IAriesEventSubscriber> _subs;
            lock (EventSubscribers)
                _subs = new List<IAriesEventSubscriber>(EventSubscribers);
            _subs.ForEach(x => x.SessionIdle(this));
        }

        public void ExceptionCaught(IoSession session, Exception cause)
        {
            if (cause is System.Net.Sockets.SocketException) session.Close(true);
            //else LOG.Error(cause);
        }

        public void MessageReceived(IoSession session, object message)
        {
            if (message is ServerByePDU) session.Close(false);

            List<IAriesMessageSubscriber> _subs;
            lock (EventSubscribers)
                _subs = new List<IAriesMessageSubscriber>(MessageSubscribers);
            _subs.ForEach(x => x.MessageReceived(this, message));
        }

        public void MessageSent(IoSession session, object message)
        {
        }

        public void InputClosed(IoSession session)
        {
            List<IAriesEventSubscriber> _subs;
            lock (EventSubscribers)
                _subs = new List<IAriesEventSubscriber>(EventSubscribers);
            _subs.ForEach(x => x.InputClosed(this));
        }
    }
}
