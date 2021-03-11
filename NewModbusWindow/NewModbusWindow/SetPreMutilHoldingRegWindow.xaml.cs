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
    /// SetPreMutilHoldingRegWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetPreMutilHoldingRegWindow : UserControl
    {
        public MainWindow myWindow = default;
        public Modbus modbus = default;
        public SetPreMutilHoldingRegWindow(MainWindow Window)
        {
            InitializeComponent();
            modbus = new Modbus();
            this.myWindow = Window;
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
                MessageBox.Show(" open serialPort error !", "warning");
                return;
            }
            else
            {
                modbus.PreSetOneByteHoldingReg(int.Parse(this.SlaveAddressBox.Text), int.Parse(this.StartAddressBox.Text), int.Parse(this.ValueBox.Text));
                this.SetPreOneByteTextbox.AppendText("Send Success !");
                Func<string> func = new Func<string>(CheckReturnText);
                func.BeginInvoke(callback, null);
                void callback(IAsyncResult asyncResult)
                {
                    string str = func.EndInvoke(asyncResult);
                    this.SetPreOneByteTextbox.Dispatcher.Invoke(new Action<string>((s) => { this.SetPreOneByteTextbox.Text = s; }), str);
                    modbus.ClosePort();
                    modbus.ReceString = null;
                }
            }
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
            return "-1";
        }
    }
}
