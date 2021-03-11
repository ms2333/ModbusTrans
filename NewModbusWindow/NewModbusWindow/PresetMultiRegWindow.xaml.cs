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
    /// PresetMultiRegWindow.xaml 的交互逻辑
    /// </summary>
    public delegate void RecTextDelegate();
    public partial class PresetMultiRegWindow : UserControl
    {
        public MainWindow myWindow;
        public Modbus modbus;
        public PresetMultiRegWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.myWindow = mainWindow;
            modbus = new Modbus();
        }
        private void send_Click(object sender, RoutedEventArgs e)//使用了异步执行检测串口收到的数据
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
                modbus.PreSetMultiRegister(int.Parse(this.SlaveAddressBox.Text), int.Parse(this.StartAddressBox.Text), int.Parse(this.RegisterNumTextBox.Text), this.SetValueTextbox.Text);
                if (modbus.ReceString != null)
                {
                    this.SetPreOneByteTextbox.AppendText("Preset the Register Successed !\r\n");
                    foreach (var item in modbus.ReceString)
                    {
                        this.SetPreOneByteTextbox.AppendText(item.ToString() + " ");
                    }
                }
                else
                {
                    Func<string> func = new Func<string>(CheckReturnText);
                    string str = null;
                    //异步委托，跨线程操作ui控件
                    IAsyncResult asyncResult = func.BeginInvoke(callback,null);
                     void callback(IAsyncResult Result)
                     {
                        str = func.EndInvoke(Result);
                        this.SetPreOneByteTextbox.Dispatcher.Invoke(new Action<string>((s) => { this.SetPreOneByteTextbox.Text = s; }),str);
                        modbus.ClosePort();
                        modbus.ReceString = null;
                     }
                }
            }
        }
        private string CheckReturnText()
        {
            while (modbus.ReceString == null)
            {
                if (modbus.RecWordLen > 8)
                { Thread.Sleep(200*(modbus.RecWordLen/8+1)); }
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
