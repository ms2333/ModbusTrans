using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace NewModbusWindow
{
    /// <summary>
    /// receivedWindows.xaml 的交互逻辑
    /// </summary>

    public delegate void SetTextDelegate(string str);
    public partial class receivedWindows : UserControl
    {
        public static SetTextDelegate setSendTextDele = default;
        public static SetTextDelegate setRecTextDele = default;
        public Modbus modbus;
        public receivedWindows(Modbus modbus)
        {
            this.modbus = modbus;
            InitializeComponent();
            Func<string> func = new Func<string>(CheckReturnText);
            func.BeginInvoke(callback, null);
            void callback(IAsyncResult asyncResult)
            {
                string str = func.EndInvoke(asyncResult);
                this.receiveTextBox.Dispatcher.Invoke(new Action<string>((s) => { this.receiveTextBox.Text = s; }), str);
            }
            setSendTextDele += (s) => { this.sendTextBox.AppendText(s); };
            setRecTextDele += (s) => { this.receiveTextBox.AppendText(s); };

        }
        private string CheckReturnText()
        {
            while (modbus.ReceString == null)
            {
                if (modbus.RecWordLen > 8)
                { Thread.Sleep(200 * (modbus.RecWordLen / 8 + 1)); }
                else
                {
                    Thread.Sleep(200);
                }
                if (modbus.ReceString != null)
                {
                    return modbus.ReceString;
                }
            }
            return "failed";
        }
    }
}
