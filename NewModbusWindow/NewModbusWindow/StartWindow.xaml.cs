using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// StartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void fc1Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
           // this.Hide();
            mainWindow.Show();
        }

        private void fc2Button_Click(object sender, RoutedEventArgs e)
        {
            ModbusTcpWindow modbusTcpWindow = new ModbusTcpWindow();
           // this.Hide();
            modbusTcpWindow.Show();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ServerButton_Click(object sender, RoutedEventArgs e)
        {
            TcpIpServerWindow tcpIpServerWindow = new TcpIpServerWindow();
            tcpIpServerWindow.Show();
        }
        
    }
}
