using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteApp
{
    public static class DCommand
    {
        private static RoutedUICommand _vnc;
        public static RoutedUICommand VNC
        {
            get
            {
                if (_vnc == null)
                {
                    KeyGesture key = new KeyGesture(Key.V, ModifierKeys.Control);
                    _vnc = new RoutedUICommand("VNC连接(_V)", "VNC", typeof(RoutedUICommand), new InputGestureCollection() { key });
                }

                return _vnc;
            }
        }

        private static RoutedUICommand _connect;
        public static RoutedUICommand Connect
        {
            get {
                if (_connect == null)
                {
                    KeyGesture key = new KeyGesture(Key.O, ModifierKeys.Control);
                    _connect = new RoutedUICommand("远程电脑(_O)", "Connect", typeof(RoutedUICommand), new InputGestureCollection() { key });
                }

                return _connect;
            }
        }

        private static RoutedUICommand _disconneted;
        public static RoutedUICommand DisConnected
        {
            get {
                if (_disconneted == null)
                {
                    KeyGesture key = new KeyGesture(Key.D, ModifierKeys.Control);
                    _disconneted = new RoutedUICommand("断开连接(_S)", "DisConnected", typeof(RoutedUICommand), new InputGestureCollection() { key });
                }

                return _disconneted;
            }
        }

        private static RoutedUICommand _delete;
        public static RoutedUICommand Delete
        {
            get {
                if (_delete == null)
                {
                    KeyGesture key = new KeyGesture(Key.Delete);
                    _delete = new RoutedUICommand("删除(_D)", "Delete", typeof(RoutedUICommand), new InputGestureCollection() { key });
                }

                return _delete;
            }
        }        

        private static RoutedUICommand _shutdown;
        public static RoutedUICommand Shutdown
        {
            get
            {
                if (_shutdown == null)
                {
                    KeyGesture key = new KeyGesture(Key.F, ModifierKeys.Control);
                    _shutdown = new RoutedUICommand("远程关机(_F)", "Shutdown", typeof(RoutedUICommand), new InputGestureCollection() { key });
                }

                return _shutdown;
            }
        }

        private static RoutedUICommand _reboot;
        public static RoutedUICommand Reboot
        {
            get
            {
                if (_reboot == null)
                {
                    KeyGesture key = new KeyGesture(Key.R, ModifierKeys.Control);
                    _reboot = new RoutedUICommand("远程重启(_R)", "Reboot", typeof(RoutedUICommand), new InputGestureCollection() { key });
                }

                return _reboot;
            }
        }

        private static RoutedUICommand _modifyAlias;
        public static RoutedUICommand ModifyAlias
        {
            get
            {
                if (_modifyAlias == null)
                {
                    KeyGesture key = new KeyGesture(Key.F2);
                    _modifyAlias = new RoutedUICommand("修改别名(_T)", "ModifyAlias", typeof(RoutedUICommand), new InputGestureCollection() { key });
                }

                return _modifyAlias;
            }
        }

        private static RoutedUICommand _edit;
        public static RoutedUICommand Edit
        {
            get
            {
                if (_edit == null)
                {
                    KeyGesture key = new KeyGesture(Key.F3);
                    _edit = new RoutedUICommand("编辑(_E)", "Edit", typeof(RoutedUICommand), new InputGestureCollection() { key });
                }

                return _edit;
            }
        }

        private static RoutedUICommand _sendPW;
        public static RoutedUICommand SendPW
        {
            get
            {
                if (_sendPW == null)
                {
                    KeyGesture key = new KeyGesture(Key.F3);
                    _sendPW = new RoutedUICommand("发送密码(_P)", "SendPW", typeof(RoutedUICommand), new InputGestureCollection() { key });
                }

                return _sendPW;
            }
        }

        private static RoutedCommand _max;
        public static RoutedCommand Max
        {
            get
            {
                if (_max == null)
                {
                    KeyGesture key = new KeyGesture(Key.F12);
                    _max = new RoutedUICommand("全屏(_M)", "Full", typeof(RoutedCommand), new InputGestureCollection() { key });
                }

                return _max;
            }
        }

        private static RoutedCommand _config;
        public static RoutedCommand Config
        {
            get
            {
                if (_config == null)
                {
                    KeyGesture key = new KeyGesture(Key.D, ModifierKeys.Control);
                    _config = new RoutedUICommand("配置(_R)", "Config", typeof(RoutedCommand), new InputGestureCollection() { key });
                }

                return _config;
            }
        }

        private static RoutedCommand _center;
        public static RoutedCommand Center
        {
            get
            {
                if (_center == null)
                {
                    KeyGesture key = new KeyGesture(Key.U, ModifierKeys.Control);
                    _center = new RoutedUICommand("居中(_A)", "Center", typeof(RoutedCommand), new InputGestureCollection() { key });
                }

                return _center;
            }
        }

        private static RoutedCommand _closeOther;
        public static RoutedCommand CloseOther
        {
            get
            {
                if (_closeOther == null)
                {
                    KeyGesture key = new KeyGesture(Key.D, ModifierKeys.Control);
                    InputGestureCollection input = new InputGestureCollection { key };
                    _closeOther = new RoutedUICommand("关闭其他窗口(_D)", "CloseOther", typeof(RoutedCommand), input);
                }

                return _closeOther;
            }
        }

        private static RoutedCommand _closeAll;
        public static RoutedCommand CloseAll
        {
            get
            {
                if (_closeAll == null)
                {
                    KeyGesture key = new KeyGesture(Key.W, ModifierKeys.Control);
                    InputGestureCollection input = new InputGestureCollection { key };
                    _closeAll = new RoutedUICommand("关闭所有窗口(_W)", "CloseAll", typeof(RoutedCommand), input);
                }

                return _closeAll;
            }
        }

        private static RoutedCommand _monitor;
        public static RoutedCommand Monitor
        {
            get
            {
                if (_monitor == null)
                {
                    KeyGesture key = new KeyGesture(Key.P, ModifierKeys.Control);
                    InputGestureCollection input = new InputGestureCollection { key };
                    _monitor = new RoutedUICommand("局部监控(_P)", "Monitor", typeof(RoutedCommand), input);
                }

                return _monitor;
            }
        }
    }
}
