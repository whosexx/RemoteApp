using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using RemoteApp.UI;

namespace RemoteApp
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString());
            e.Handled = true;
        }

        public bool RunningInstance()
        {
            System.Diagnostics.Process Curr = System.Diagnostics.Process.GetCurrentProcess();
            System.Diagnostics.Process[] list = System.Diagnostics.Process.GetProcessesByName(Curr.ProcessName);

            foreach (var v in list)
            {
                if (v.Id != Curr.Id)
                {
                    try
                    {
                        var cname = v.MainModule.FileName;
                        var name = Curr.MainModule.FileName;
                        if (name == cname)
                            return true;

                    }
                    catch (System.ComponentModel.Win32Exception) { }
                }
            }
            return false;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (RunningInstance())
            {
                MessageBox.Show("同一目录下只能有一个实例程序运行，请先退出或者使用任务管理器关闭正在运行的实例！", "警告", MessageBoxButton.OK, MessageBoxImage.Stop);
                Environment.Exit(0);
            }
            base.OnStartup(e);
        }

        /// <summary>
        /// 关机的时候
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            e.Cancel = false;

            var main = App.Current.MainWindow as MainWindow;
            if(main != null)
                main.IsRealClose = true;

            App.Current.MainWindow.Close();
        }
    }
}
