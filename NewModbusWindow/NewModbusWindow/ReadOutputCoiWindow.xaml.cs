using System;
using System.Collections.Generic;
using System.IO.Ports;
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
    /// ReadOutputCoi.xaml 的交互逻辑
    /// </summary>
    public partial class ReadOutputCoi : UserControl
    {
        public Modbus modbus;
        public MainWindow myWindow;
        public ReadOutputCoi(MainWindow mainWindow)
        {
            InitializeComponent();
            this.myWindow = mainWindow;
            modbus = new Modbus();
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            bool bo = false;
            if (myWindow.getThisComboboxSelect() >= 0)
            {
                bo = modbus.OpenCom(9600, myWindow.serialportName[myWindow.getThisComboboxSelect()], 8, Parity.None, StopBits.One);
            }
            else
            {
                MessageBox.Show(" ๑乛◡乛๑ 没有选择串口!");
            }

            if ((bo == false))
            {
                return;
            }
            else
            {
                modbus.ReadOutputStatus(int.Parse(this.SlaveAddressBox.Text), int.Parse(this.StartAddressBox.Text), int.Parse(this.LengthBox.Text));
                this.ReadOutputCoilTextbox.AppendText("seccesse!");
                Func<string> func = new Func<string>(CheckReturnText);
                func.BeginInvoke(callback, null);
                void callback(IAsyncResult asyncResult)
                {
                    string str = func.EndInvoke(asyncResult);
                    this.ReadOutputCoilTextbox.Dispatcher.Invoke(new Action<string>((s) => { this.ReadOutputCoilTextbox.AppendText("\r\n"+s+"\r\n"); }), str);
                    modbus.ClosePort();
                    modbus.ReceString = null;
                }
            }
        }
        public string CheckReturnText()
        {
            while (modbus.ReceString == null)
            {
                if (modbus.RecWordLen > 8)
                { Thread.Sleep(200 * (modbus.RecWordLen / 8 + 1)); }
                else
                {
                    Thread.Sleep(200);
                }
                if (modbus.ReceString != null                      )
                {
                    return modbus.ReceString;
                }
            }
            return "-1";
        }
    }
}

