using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RDPServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class RDPService : IRDPService
    {
        public bool GetDesktopSharedString(string un, string pw, out string cs, out string msg)
        {
            IntPtr handle;
            var b1 = Convert.FromBase64String(un);
            var b2 = Convert.FromBase64String(pw);
            Array.Reverse(b1);
            Array.Reverse(b2);

            if (b1.Length <= 128 || b2.Length <= 128)
            {
                cs = string.Empty;
                msg = "用户名或者密码不正确！！！";
                return false;
            }

            for (int i = 0; i < b1.Length; i++)
                b1[i] = (byte)(b1[i] - 88);

            for (int i = 0; i < b2.Length; i++)
                b2[i] = (byte)(b2[i] - 88);

            int ulen, plen;
            ulen = b1.Length - 128;
            plen = b2.Length - 128;

            un = System.Text.Encoding.UTF8.GetString(b1, 64, ulen);
            pw = System.Text.Encoding.UTF8.GetString(b2, 64, plen);

            if (Utils.LogonUser(un, null, pw, Utils.LogonType.Interactive, Utils.LogonProvider.Default, out handle))
            {
                msg = "验证成功。。。";
                cs = RDPServerHost.GetInstance.ConnectionString;
                return true;
            }
            else
            {
                msg = "\r\n登录失败，可能的原因有：\r\n1、用户名或者密码不正确。\r\n2、服务端的权限不够，无法验证。";
                cs = string.Empty;
                return false;
            }
        }

        public void SetDesktopSharedRect(Rectangle rect)
        {
            RDPServerHost.GetInstance.SetDesktopSharedRect(rect);
        }

        public Size GetDesktopSharedRect()
        {
            return RDPServerHost.GetInstance.GetDesktopSharedRect();
        }

        public void Pause()
        {
            RDPServerHost.GetInstance.Pause();
        }

        public void Resume()
        {
            RDPServerHost.GetInstance.Resume();
        }
    }
}
