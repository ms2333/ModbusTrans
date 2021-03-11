using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NewModbusWindow
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>

    public delegate void  Mydele<T>( T window);
    public partial class MainWindow : Window
    {
        public  List<string> serialportName = new List<string>();
        public  Mydele<receivedWindows> ChangeWindowDelegate = default;
        public MainWindow()
        {
            InitializeComponent();
            ChangeWindowDelegate += this.ChangeWindow<receivedWindows>;//委托1
            foreach (var item in SerialPort.GetPortNames())
            {
                serialportName.Add(item);
            }
            this.combobox.ItemsSource = serialportName;
        }


        private void readHoldingRegButton_Click(object sender, RoutedEventArgs e)
        {
            image.Visibility = Visibility.Collapsed;
            readHoldingRegWindow rw  = new readHoldingRegWindow(this);
            SonWindow.Children.Add(rw);
        }
        private void ChangeWindow<T>(dynamic windows)//委托1处理器
        {
            this.SonWindow.Children.Clear();
            this.SonWindow.Children.Add(windows);
        }

        public int getThisComboboxSelect()
        {
            return combobox.SelectedIndex;
        }

        private void PreSetHoldingRegButton_Click(object sender, RoutedEventArgs e)
        {
            image.Visibility = Visibility.Hidden;
            SetPreMutilHoldingRegWindow sw = new SetPreMutilHoldingRegWindow(this);
            SonWindow.Children.Add(sw);

        }

        private void PreSetMultiRegButton_Click(object sender, RoutedEventArgs e)
        {
            image.Visibility = Visibility.Hidden;
            PresetMultiRegWindow pW= new PresetMultiRegWindow(this);
            SonWindow.Children.Add(pW);
        }

        private void ReadOutputCoil_Click(object sender, RoutedEventArgs e)
        {
            image.Visibility = Visibility.Hidden;
            ReadOutputCoi roc = new ReadOutputCoi(this);
            SonWindow.Children.Add(roc);
        }

        private void ReadInputCoil_Click(object sender, RoutedEventArgs e)
        {
            image.Visibility = Visibility.Hidden;
            ReadInputCoil ric = new ReadInputCoil(this);
            SonWindow.Children.Add(ric);
        }

        private void ConstraintSetButton_Click(object sender, RoutedEventArgs e)
        {
            image.Visibility = Visibility.Hidden;
            ConstraintSetCoilwindow cc = new ConstraintSetCoilwindow(this);
            SonWindow.Children.Add(cc);
        }
    }
}
