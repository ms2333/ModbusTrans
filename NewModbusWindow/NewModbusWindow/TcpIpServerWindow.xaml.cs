using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NewModbusWindow
{
    /// <summary>
    /// TcpIpServerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TcpIpServerWindow : Window
    {
        public TcpIpServerWindow()
        {
            InitializeComponent();
        }

        private void IpTrue_Click(object sender, RoutedEventArgs e)
        {
            if (this.IpTextBox != null)
            {
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(IpTextBox.Text.ToString()), 250);//指定端口250；后期随便改
                Socket Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建套接字
                Listener.Bind(iPEndPoint);//将socket与终结点ip连接
                Listener.Listen(10);//开始监听
                Listener.BeginAccept(new AsyncCallback(this.ConnectRequestHandler), Listener);//开始异步监听
                //Textbox添加提示信息
                this.IpTrue.Content = "Listening";
                this.StateTextbox.AppendText("Listening...\r\n");
            }
            else
            {
                MessageBox.Show("ip is null");
            }
        }

        Socket Client;
        string ClientName = string.Empty;
        byte[] myBuffer = new byte[512];//接受的消息

        private void ConnectRequestHandler(IAsyncResult ar)//异步监听处理器(监听到连接)
        {
            Socket Listener = (Socket)ar.AsyncState;//接收的数据
            Client = Listener.EndAccept(ar);//赋值到Socket
            this.ClientName = Client.RemoteEndPoint.ToString();//获取远程客户端的ip
            this.StateTextbox.Dispatcher.Invoke(new Action(()=> { this.StateTextbox.AppendText(ClientName + "   onLine    " + DateTime.Now.ToString()+"\r\n"); }));//委托操作控件 
            String SendMsg = ClientName + " Connect seccess ！ ";//连接成功发送消息
            byte[] bytes = Encoding.UTF8.GetBytes(SendMsg);//编码=>byte[]流
            Client.Send(bytes, bytes.Length, SocketFlags.None);//收到消息后回复消息
            ReceiveCallBack(Client);//异步接收消息处理
            this.StateTextbox.Dispatcher.Invoke(new Action(() => { this.StateTextbox.AppendText("Send to "+ClientName +" "+ SendMsg+ " "+ DateTime.Now.ToString() + "\r\n"); }));
            Listener.BeginAccept(new AsyncCallback(this.ConnectRequestHandler), Listener);
        }

        /// <summary>
        /// recevice 回调函数(接受消息存入myBuffer )
        /// </summary>
        /// <param name="client"></param>
        private void ReceiveCallBack(Socket client)
        {
            try
            {
                Array.Clear(myBuffer,0,myBuffer.Length);
                AsyncCallback asyncCallback = new AsyncCallback(ReceiveRequestHandler);
                client.BeginReceive(myBuffer, 0, myBuffer.Length, SocketFlags.None, asyncCallback, client);//异步接受消息
            }
            catch (Exception)
            {
                this.StateTextbox.Dispatcher.Invoke(new Action(() => { this.StateTextbox.AppendText("Send to " + ClientName +  " Error ! " + DateTime.Now.ToString() + "\r\n"); }));
            }
        }

        private void ReceiveRequestHandler(IAsyncResult ar)//异步接受消息解析
        {
            Socket socket = (Socket)ar.AsyncState;//解析消息封装到socket
            try
            {
                int ReceiveBytelength = socket.EndReceive(ar);//返回接受的字节长度
                if (ReceiveBytelength > 0)
                {
                    //string ReceiveMsg = Encoding.UTF8.GetString(myBuffer, 0, myBuffer.Length);//读取返回的字节流解析成字符串
                    foreach (var item in myBuffer)
                    {
                        this.StateTextbox.Dispatcher.Invoke(new Action(() => { this.StateTextbox.AppendText(item.ToString() ); }));
                    }
                    this.StateTextbox.Dispatcher.Invoke(new Action(() => { this.StateTextbox.AppendText(" " + DateTime.Now.ToString()  + "\r\n"); }));
                    ReceiveCallBack(socket);//再次开始等待接受消息
                }
                else
                {
                    this.StateTextbox.Dispatcher.Invoke(new Action(() => { this.StateTextbox.AppendText(ClientName+ " not online " + DateTime.Now.ToString() + "\r\n"); }));
                }
            }
            catch (Exception)
            {

                this.StateTextbox.Dispatcher.Invoke(new Action(() => { this.StateTextbox.AppendText("Receive from " + ClientName + "Error ! " + DateTime.Now.ToString() + "\r\n"); }));
            }

        }
    }
}
