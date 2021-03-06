/*
 * Copyright (c) 2013, SteamDB. All rights reserved.
 * Use of this source code is governed by a BSD-style license that can be
 * found in the LICENSE file.
 */
using System;
using System.Text;
using Meebey.SmartIrc4net;

namespace SteamDatabaseBackend
{
    public class IRC
    {
        private static IRC _instance = new IRC();
        public static IRC Instance { get { return _instance; } }

        public IrcClient Client = new IrcClient();

        public void Init()
        {
            Client.OnChannelMessage += CommandHandler.OnChannelMessage;
            Client.OnConnected += OnConnected;

            Client.Encoding = Encoding.UTF8;
            Client.SendDelay = 700;
            Client.AutoRetry = true;
            Client.AutoRetryDelay = 15;
            Client.AutoRetryLimit = 0;
            Client.AutoRejoin = true;
            Client.AutoRelogin = true;
            Client.AutoReconnect = true;
            Client.AutoRejoinOnKick = true;
            Client.ActiveChannelSyncing = true;

            try
            {
                Client.Connect(Settings.Current.IRC.Servers, Settings.Current.IRC.Port);
                Client.Login(Settings.Current.IRC.Nickname, Settings.Current.BaseURL.AbsoluteUri, 4, Settings.Current.IRC.Nickname, Settings.Current.IRC.Password);
                Client.RfcJoin(new string[] { Settings.Current.IRC.Channel.Main, Settings.Current.IRC.Channel.Announce });
                Client.Listen();

                Kill();
            }
            catch (Exception e)
            {
                Log.WriteError("IRC", "Exception: {0}\n{1}", e.Message, e.StackTrace);
            }
        }

        public void Kill()
        {
            try
            {
                Client.RfcQuit("Exiting, will be back shortly!", Priority.Critical);
                Client.Disconnect();
            }
            catch { }
        }

        public static void OnConnected(object sender, EventArgs e)
        {
            Log.WriteInfo("IRC Proxy", "Connected to IRC successfully");
        }

        public static void Send(string channel, string format, params object[] args)
        {
            if (Settings.Current.IRC.Enabled)
            {
                Instance.Client.SendMessage(SendType.Message, channel, string.Format(format, args));
            }
        }

        public static void SendAnnounce(string format, params object[] args)
        {
            Send(Settings.Current.IRC.Channel.Announce, format, args);
        }

        public static void SendMain(string format, params object[] args)
        {
            Send(Settings.Current.IRC.Channel.Main, format, args);
        }

        public static void SendEmoteAnnounce(string format, params object[] args)
        {
            if (Settings.Current.IRC.Enabled)
            {
                Instance.Client.SendMessage(SendType.Action, Settings.Current.IRC.Channel.Announce, string.Format(format, args));
            }
        }

        public static bool IsSenderOp(string channel, string nickname)
        {
            ChannelUser user = Instance.Client.GetChannelUser(channel, nickname);

            if (user == null)
            {
                return false;
            }

            if (user.IsOp)
            {
                return true;
            }

            IRC.Send(channel, "{0}{1}{2}: You're not op!", Colors.OLIVE, nickname, Colors.NORMAL);

            return false;
        }
    }
}
