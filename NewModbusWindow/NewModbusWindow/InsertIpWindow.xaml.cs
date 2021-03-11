using System;
using System.Collections.Generic;
using System.Linq;
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
    /// InsertIpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InsertIpWindow : Window
    {
        public ModbusTcpWindow modbusTcpWindow;
        public InsertIpWindow(ModbusTcpWindow tcpWindow)
        {
            InitializeComponent();
            this.modbusTcpWindow = tcpWindow;
        }

        private void TrueButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.InsertIpTextBox.Text != null)
            {
                modbusTcpWindow.ComboBoxInsertItemDelegate(this.InsertIpTextBox.Text.ToString());
            }
            else
            {
                MessageBox.Show("ip could not be null !");
            }
            this.Close();
        }
    }
}
