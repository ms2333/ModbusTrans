using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NewModbusWindow
{
    /// <summary>
    /// readHoldingRegWindow.xaml 的交互逻辑
    /// </summary>
    public partial class readHoldingRegWindow : UserControl
    {
        public Modbus modbus = null;
        public MainWindow myWindow = default;
        public readHoldingRegWindow(MainWindow window)
        {
            InitializeComponent();
            modbus = new Modbus();
            this.myWindow = window;
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {

            byte[] b = null;
            bool bo = false;
            if (myWindow.getThisComboboxSelect() >= 0)
            {
                bo = modbus.OpenCom(9600, myWindow.serialportName[myWindow.getThisComboboxSelect()], 8, Parity.None, StopBits.One);
                receivedWindows recrwin = new receivedWindows(modbus);
                myWindow.ChangeWindowDelegate.Invoke(recrwin);
                this.Visibility = Visibility.Hidden;
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
                b = modbus.ReadHoldingReg(int.Parse(this.SlaveAddressBox.Text), int.Parse(this.StartAddressBox.Text), int.Parse(this.LengthBox.Text));
                receivedWindows.setSendTextDele.Invoke("\r\n" + "Tx:   ");
                foreach (byte item in b)
                {
                    receivedWindows.setSendTextDele.Invoke(item.ToString("X2"));
                    receivedWindows.setSendTextDele.Invoke(" ");
                }

            }
        }

    }
}
