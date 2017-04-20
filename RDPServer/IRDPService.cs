using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Drawing;

namespace RDPServer
{
    //定义服务契约    

    /// <summary>
    /// 消息发布服务；
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IMessage), Namespace = "http://RDPServie/Lucy")]
    public interface IRDPService
    {       
        [OperationContract]
        bool GetDesktopSharedString(string username, string password, out string cstring, out string msg);

        [OperationContract]
        void SetDesktopSharedRect(Rectangle rect);

        [OperationContract]
        Size GetDesktopSharedRect();

        [OperationContract]
        void Pause();

        [OperationContract]
        void Resume();
    }

    /// <summary>
    /// 消息监听器；
    /// 作为消息发布服务的回调契约；
    /// </summary>
    public interface IMessage
    {
        [OperationContract]
        void NotifyMsg(string msg);
    }
}
