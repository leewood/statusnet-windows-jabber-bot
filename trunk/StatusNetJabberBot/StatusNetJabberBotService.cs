using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceProcess;
using agsXMPP;
using agsXMPP.protocol.client;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Configuration;

namespace StatusNetJabberBot
{
    class StatusNetJabberBotService : ServiceBase
    {
        private XmppClientConnection _xmppCon;
        private bool _bWait;
        private System.Timers.Timer _timer;

        // configuration variables
        private string _phpPath;
        private string _statusNetPath;
        private string _jabberAccount;
        private string _jabberPassword;
        private string _statusNetEncoding;
        private string _windowsEncoding;


        public StatusNetJabberBotService()
        {
            this.ServiceName = "StatusNetJabberBotService";
            this.EventLog.Log = "Application";
            
            this.CanHandlePowerEvent = true;
            this.CanHandleSessionChangeEvent = true;
            this.CanPauseAndContinue = true;
            this.CanShutdown = true;
            this.CanStop = true;
        }

        static void Main()
        {
            ServiceBase.Run(new StatusNetJabberBotService());
        }

        private void readConfig()
        {
            _phpPath = System.Configuration.ConfigurationManager.AppSettings["phpPath"];
            _statusNetPath = System.Configuration.ConfigurationManager.AppSettings["statusNetPath"];
            _jabberAccount = System.Configuration.ConfigurationManager.AppSettings["jabberAccount"];
            _jabberPassword = System.Configuration.ConfigurationManager.AppSettings["jabberPassword"];
            _statusNetEncoding = System.Configuration.ConfigurationManager.AppSettings["statusNetEncoding"];
            _windowsEncoding = System.Configuration.ConfigurationManager.AppSettings["windowsEncoding"];

         }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            readConfig();

            _xmppCon = new XmppClientConnection();

            Jid jid = new Jid(_jabberAccount);

            _xmppCon.Password = _jabberPassword;

            _xmppCon.Username = jid.User;
            _xmppCon.Server = jid.Server;
            _xmppCon.AutoAgents = true;
            _xmppCon.AutoPresence = true;
            _xmppCon.KeepAlive = true;
            _xmppCon.AutoRoster = true;
            _xmppCon.AutoResolveConnectServer = true;

            _xmppCon.OnPresence += new PresenceHandler(xmppCon_OnPresence);
            _xmppCon.OnMessage += new MessageHandler(xmppCon_OnMessage);
            _xmppCon.OnLogin += new ObjectHandler(xmppCon_OnLogin);
            _xmppCon.OnError +=new ErrorHandler(xmppCon_OnError);
            _xmppCon.OnSocketError += new ErrorHandler(xmppCon_OnError);
            _xmppCon.OnClose += new ObjectHandler(_xmppCon_OnClose);

            _xmppCon.OnXmppConnectionStateChanged += new XmppConnectionStateHandler(_xmppCon_OnXmppConnectionStateChanged);
            _xmppCon.Open();

            Wait("");

            _timer = new Timer();
            _timer.Interval = 2000;
            _timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

        void _xmppCon_OnClose(object sender)
        {
            this.EventLog.WriteEntry("Closed");
        }

        void _xmppCon_OnXmppConnectionStateChanged(object sender, XmppConnectionState state)
        {
            this.EventLog.WriteEntry("Status changed : " + state.ToString());
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _xmppCon.SendMyPresence();
        }

        private string Execute(string jid, string cmd)
        {

            Process p = new Process();

            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = _phpPath;

            // encode 
            byte[] chars = Encoding.Convert(Encoding.GetEncoding(_windowsEncoding), Encoding.UTF8, Encoding.GetEncoding(_statusNetEncoding).GetBytes(cmd));
            cmd = Encoding.GetEncoding(_statusNetEncoding).GetString(chars); 

            // combine executing command
            p.StartInfo.Arguments = "\"" + Path.Combine(_statusNetPath, "scripts\\statusnet_jabber_service_api.php") + "\"" + " " + jid + " \"" + cmd + "\"";
            p.Start();

            string output = p.StandardOutput.ReadToEnd();

            p.WaitForExit();

            this.EventLog.WriteEntry(output);

            // reencode
            chars = Encoding.Convert(Encoding.GetEncoding(_statusNetEncoding), Encoding.GetEncoding(_windowsEncoding), Encoding.GetEncoding(_windowsEncoding).GetBytes(output));
            output = Encoding.GetEncoding(_windowsEncoding).GetString(chars); 


            return output;
        }


        private void Wait(string statusMessage)
        {
            int i = 0;
            _bWait = true;

            while (_bWait)
            {
                i++;
                if (i == 60)
                    _bWait = false;

                System.Threading.Thread.Sleep(300);
            }
        }

        void xmppCon_OnLogin(object sender)
        {
            _bWait = false;
        }


        void xmppCon_OnPresence(object sender, Presence pres)
        {
            if (pres.Type == PresenceType.subscribe)
            {
                ((XmppClientConnection)sender).PresenceManager.ApproveSubscriptionRequest(pres.From);
            }
        }

        void xmppCon_OnError(object sender, Exception ex)
        {
            this.EventLog.WriteEntry("Error: " + ex.Message);
        }

        void xmppCon_OnMessage(object sender, Message msg)
        {
            if (msg.Body != null)
            {
                this.EventLog.WriteEntry(msg.Body);
                //queue.Add(msg.From.Bare, msg.Body);
                ((XmppClientConnection)sender).Send(new Message(new Jid(msg.From.Bare), Execute(msg.From.Bare, msg.Body)));

            }
        }      


        protected override void OnStop()
        {
            base.OnStop();

            _xmppCon.Close();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        protected override void OnContinue()
        {
            base.OnContinue();
        }

        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        protected override void OnCustomCommand(int command)
        {

            base.OnCustomCommand(command);
        }


        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            return base.OnPowerEvent(powerStatus);
        }

        protected override void OnSessionChange(
                  SessionChangeDescription changeDescription)
        {
            base.OnSessionChange(changeDescription);
        }
    }}
