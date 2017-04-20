using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using RDPCOMAPILib;

namespace RDPServer
{
    public class RDPServerHost
    {
        private static RDPServerHost _host;
        private static object locker = new object();
        public static RDPServerHost GetInstance
        {
            get {
                if (_host == null)
                {
                    lock (locker)
                    {
                        if (_host == null)
                            _host = new RDPServerHost();
                    }
                }

                return _host;
            }
        }

        private System.Threading.Thread Monitor;
        private ServiceHost RHost;
        private bool IsStart = false;
        private RDPSession _rdpSession;
        private IRDPSRAPIInvitation Invitation { get; set; }

        private RDPServerHost() { }

        public string ConnectionString
        {
            get { return this.Invitation.ConnectionString; }
        }              

        /// <summary>
        /// 监控的App
        /// </summary>
        private List<IRDPSRAPIApplication> SourceApps = new List<IRDPSRAPIApplication>();
        /// <summary>
        /// 被监控的App的大小
        /// </summary>
        private Rect SourceRect;
        private void MonitorPosition()
        {            
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "正在监控应用程序[" + Properties.Settings.Default.Application + "]");
            while(true)
            {
                Rect r;
                try
                {
                    do
                    {
                        if (this.SourceApps == null || this.SourceApps.Count <= 0)
                            break;

                        var last = this.SourceApps.Last();
                        var p = System.Diagnostics.Process.GetProcessById(last.Id);
                        if (p == null || p.Id == 0)
                        {
                            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "没有发现这个进程," + last.Id);
                            break;
                        }

                        if (p.MainWindowHandle == IntPtr.Zero)
                        {
                            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "没有窗口," + last.Id);
                            break;
                        }

                        if (Utils.IsMinimized(p.MainWindowHandle))
                            Utils.ShowWindow(p.MainWindowHandle, 1);

                        _rdpSession.Resume();
                        Utils.GetWindowRect(p.MainWindowHandle, out r);
                        if (r.Equals(this.SourceRect))
                            break;

                        this.SourceRect = r;
                        this.SetDesktopSharedRect(r);                        
                    } while (false);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                System.Threading.Thread.Sleep(1000);
            }
        }

        public Size GetDesktopSharedRect()
        {
            int left, right, top, bottom;

            this._rdpSession.GetDesktopSharedRect(out left, out top, out right, out bottom);
            return new Size(right - left, bottom - top);
        }

        public void SetDesktopSharedRect(Rectangle rect)
        {
            if (_rdpSession == null)
                return;

            if(rect.X < 0)                
                rect.X = 0;

            if(rect.Y < 0)      
                rect.Y = 0;

            rect.Width = (rect.Width / 2) * 2;
            rect.Height = (rect.Height / 2) * 2;

            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "正在监控区域," + rect.ToString());
            _rdpSession.SetDesktopSharedRect(rect.X, rect.Y, rect.Right, rect.Bottom);            
        }

        public void SetDesktopSharedRect(Rect r)
        {
            if (_rdpSession == null)
                return;

            var rect = new Rectangle(r.Left, r.Top - 1, r.Right - r.Left, r.Bottom - r.Top);
            this.SetDesktopSharedRect(rect);
        }

        public void ShowAttendees()
        {
            if (_rdpSession == null)
                return;

            foreach(RDPSRAPIAttendee u in _rdpSession.Attendees)
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "[" + u.Id + "," + u.RemoteName + "," + u.ControlLevel + "]");           

        }

        public bool IsLegal(int pid, IRDPSRAPIApplication app)
        {
            var p = System.Diagnostics.Process.GetProcessById(pid);
            if (p == null)
                return false;
            
            if ((p.ProcessName.ToLower() + ".exe") != Properties.Settings.Default.Application.ToLower())
                return false;

            if (p.MainWindowHandle == IntPtr.Zero)
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + p.MainModule.ModuleName + ",这个程序没有发现窗口，将监控默认区域。");
                return false;
            }

            if (Utils.IsMinimized(p.MainWindowHandle))
                Utils.ShowWindow(p.MainWindowHandle, 1);

            Rect r;
            Utils.GetWindowRect(p.MainWindowHandle, out r);
            this.SourceRect = r;
            this.SourceApps.Add(app);
            return true;
        }

        public void Resume()
        {
            this._rdpSession?.Resume();
        }

        public void Pause()
        {
            this._rdpSession?.Pause();
        }

        public void Open()
        {
            if (IsStart)
                return;

            lock(locker)
            {
                if (IsStart)
                    return;

                IsStart = true;
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "正在开启服务，请稍等……");
                // 新建RDP Session
                _rdpSession = new RDPSession();
                _rdpSession.colordepth = 24;

                bool find = false;
                this.SourceRect = new Rect(Properties.Settings.Default.MonitorArea);                               
                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.Application))
                {
                    this.Monitor = new System.Threading.Thread(new System.Threading.ThreadStart(MonitorPosition));
                    this.Monitor.Start();

                    foreach (IRDPSRAPIApplication app in _rdpSession.ApplicationFilter.Applications)
                    {
                        if (string.IsNullOrWhiteSpace(app.Name) 
                            || app.Name?.ToLower() == Properties.Settings.Default.Application?.ToLower())
                        {
                            if (!this.IsLegal(app.Id, app))
                                continue;

                            find = true;                            
                        }
                    }
                }


                // 设置共享区域，如果不设置默认为整个屏幕，当然如果有多个屏幕，还是设置下主屏幕，否则，区域会很大
                if (!find && !string.IsNullOrWhiteSpace(Properties.Settings.Default.Application))
                    Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "系统中没有发现需要监控的应用程序[" + Properties.Settings.Default.Application + "], 暂时监控默认区域。");
                
                if (Properties.Settings.Default.MonitorArea.Width > 0 
                    && Properties.Settings.Default.MonitorArea.Height > 0)
                    this.SetDesktopSharedRect(this.SourceRect);               

                _rdpSession.OnAttendeeConnected += _rdpSession_OnAttendeeConnected;
                _rdpSession.OnAttendeeDisconnected += _rdpSession_OnAttendeeDisconnected;

                _rdpSession.OnControlLevelChangeRequest += RdpSessionOnOnControlLevelChangeRequest;

                _rdpSession.OnApplicationOpen += _rdpSession_OnApplicationOpen;
                _rdpSession.OnApplicationClose += _rdpSession_OnApplicationClose;

                _rdpSession.OnGraphicsStreamPaused += _rdpSession_OnGraphicsStreamPaused;
                _rdpSession.OnGraphicsStreamResumed += _rdpSession_OnGraphicsStreamResumed;
                // 打开会话
                _rdpSession.Open();
                
                Invitation = _rdpSession.Invitations.CreateInvitation(Environment.MachineName, Environment.UserName, "Cepg@2012", 64);  // 创建申请

                RHost = new ServiceHost(typeof(RDPService));
                RHost.Open();
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "服务开启完成！");
            }
        }        

        private void _rdpSession_OnApplicationClose(object pApplication)
        {
            IRDPSRAPIApplication app = pApplication as IRDPSRAPIApplication;
            if (app == null)
                return;

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.Application))
            {
                Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "ApplicationClose~" + app.Id + "," + app.Name);
                this.SourceApps.RemoveAll(m => m.Id == app.Id);
            }
        }

        private void _rdpSession_OnApplicationOpen(object pApplication)
        {
            IRDPSRAPIApplication app = pApplication as IRDPSRAPIApplication;
            if (app == null)
                return;

            Console.WriteLine("OnApplicationOpen:" + app.Id + "," + app.Name);
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.Application))
            {
                if (string.IsNullOrWhiteSpace(app.Name) 
                    || app.Name?.ToLower() == Properties.Settings.Default.Application?.ToLower())
                {
                    if (!this.IsLegal(app.Id, app))
                        return;

                    this.SetDesktopSharedRect(this.SourceRect);
                }
            }
        }

        private void _rdpSession_OnGraphicsStreamResumed()
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "_rdpSession_OnGraphicsStreamResumed");
        }

        private void _rdpSession_OnGraphicsStreamPaused()
        {
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + "_rdpSession_OnGraphicsStreamPaused");
            //_rdpSession.Resume();
        }

        public void Close()
        {
            if (!this.IsStart)
                return;

            lock (locker)
            {
                if (!this.IsStart)
                    return;

                _rdpSession.Close();
                Marshal.ReleaseComObject(_rdpSession);
                _rdpSession = null;

                try { this.RHost.Close(); } catch (Exception e) { Console.WriteLine(e.ToString()); }
                this.Monitor.Abort();
                this.IsStart = false;
            }
        }

        private void RdpSessionOnOnControlLevelChangeRequest(object pObjAttendee, CTRL_LEVEL RequestedLevel)
        {
            IRDPSRAPIAttendee pAttendee = pObjAttendee as IRDPSRAPIAttendee;
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + pAttendee.Id + "," + pAttendee.RemoteName + ":请求更好权限。");
        }

        private void _rdpSession_OnAttendeeDisconnected(object pDisconnectInfo)
        {
            IRDPSRAPIAttendeeDisconnectInfo pinfo = pDisconnectInfo as IRDPSRAPIAttendeeDisconnectInfo;
            if (pinfo == null)
                return;

            IRDPSRAPIAttendee p = pinfo.Attendee;
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + p.Id + "," + p.RemoteName + "," + p.ControlLevel + "," + pinfo.Code + "," + pinfo.Reason + ":断开连接！");
        }

        private void _rdpSession_OnAttendeeConnected(object pObjAttendee)
        {
            IRDPSRAPIAttendee pAttendee = pObjAttendee as IRDPSRAPIAttendee;
            pAttendee.ControlLevel = CTRL_LEVEL.CTRL_LEVEL_VIEW;
            //_rdpSession.Resume();
            Console.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ":" + pAttendee.Id + "," + pAttendee.RemoteName + ":有用户连接成功！"); 
        }
    }
}
