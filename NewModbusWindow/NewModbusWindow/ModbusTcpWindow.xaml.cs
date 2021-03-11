using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Resources;
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
    public delegate void Mydelegate(string str);
    
    /// <summary>
    /// ModbusTcpWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ModbusTcpWindow : Window
    {
        public  Mydelegate ComboBoxInsertItemDelegate = null;
        Modbus modbus;
        public ModbusTcpWindow()
        {
            InitializeComponent();
            this.FuncCombobox.Items.Add("Read Coil");
            this.FuncCombobox.Items.Add("Constraint single coil");
            this.selectIpCombobox.Items.Add("127.0.0.1");
            this.selectIpCombobox.SelectedIndex = 0;
            modbus = new Modbus();
            
            ComboBoxInsertItemDelegate += this.ComboBoxitemsInsert;
    }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.selectIpCombobox.SelectedItem != null && this.ServerPortTextBox != null)
            {
                if (this.ConnectButton.Content.ToString() == "Connect")
                {
                    if (Connect(this.selectIpCombobox.Text.Trim(), Convert.ToInt32(this.ServerPortTextBox.Text)))
                    {

                        this.Connectstatelabel.Content = "Connect seccess";
                    }
                    else
                    {
                        MessageBox.Show("Connect Faild ! Unknow error ");
                    }
                }
                else
                {
                    tcpClient.Disconnect(true);
                    this.ConnectButton.Content = "DisConnect";
                }
            }
            else
            {
                MessageBox.Show("Ip or Port is null");
            }
        }

        Socket tcpClient;
        private bool Connect(string ipAddr, int port)
        {
            IPEndPoint ie = new IPEndPoint(IPAddress.Parse(ipAddr), port);
            tcpClient = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                tcpClient.Connect(ie);
                return true;
            }
            catch (SocketException)
            {
                return false;
            }
        }
        byte[] bytes;
        /// <summary>
        /// Read Output Coil
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <param name="startAddr"></param>
        /// <param name="Length"></param>
        /// <returns></returns>
        private byte[] ReadOutputState(int slaveAddr,int startAddr, int Length)
        {  
            //发送数据
            bytes = new byte[12];
            bytes[0] = 0;
            bytes[1] = 0;
            bytes[2] = 0;
            bytes[3] = 0;
            bytes[4] = 0;
            bytes[5] = 6;

            bytes[6] = (byte)slaveAddr;
            bytes[7] = 1;
            bytes[8] = Convert.ToByte((startAddr - (startAddr % 256)) / 256);
            bytes[9] = Convert.ToByte(startAddr % 256);
            bytes[10] =Convert.ToByte((Length - (Length % 256)) / 256); 
            bytes[11] =Convert.ToByte(Length % 256);
            //测试用的字符串
            string str = "你好";
            byte[] bytes1 = Encoding.UTF8.GetBytes(str);
            tcpClient.Send(bytes1,bytes1.Length, SocketFlags.None);

            //返回报文检查
            int length = 0;
            if (Length % 8 == 0)
            {
                length = Length / 8;
            }
            else
            {
                length = (Length / 8) + 1;
            }

            //接收数据(!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!这里会阻塞！改用异步或者多线程就好了！)
            byte[] recData = new byte[128];
            tcpClient.Receive(recData, 9 + length, SocketFlags.None);

            //解析报文
            byte[] Result = null;
            if (length == recData[7])
            {
                Result = modbus.GetCutByteArray(recData, 0, 9 + length);
            }
            return Result;
        }

        private byte[] ContraintmultiCoil(int slaveAddr, int CoilAddr, bool state)
        {
            byte[] bytes = new byte[12] { 0,0,0,0,0,6,1,5,0,0,0,0};
            bytes[8]  = (byte)((CoilAddr - (CoilAddr % 256)) / 256);
            bytes[9] = (byte)(CoilAddr % 256);
            if (state)
            {
                bytes[10] = 0xff;
            }
            else
            {
                bytes[10] = 0x00;
            }
            bytes[11] = 0x00;
            tcpClient.Send(bytes, 12, SocketFlags.None);
            //Return check
            byte[] recData = new byte[12];
            
            tcpClient.Receive(recData, 12, SocketFlags.None);

            //解析报文
            byte[] Result = null;
            if (5== (byte)recData[7])
            {
                Result = modbus.GetCutByteArray(recData, 0, 12);
            }
            return Result;

        }

        private void readButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Connectstatelabel.Content.ToString() == "Connect seccess")
            {
                if (this.tcpClient.Connected == true)
                {
                    if (this.FuncCombobox.SelectedItem.ToString() == "Read Coil")
                    {
                        byte[] bytes = this.ReadOutputState
                    (
                    Convert.ToInt32(this.SlaveAddrTextBox.Text.ToString().Trim()),
                    Convert.ToInt32(this.StartAddrTextbox.Text.ToString().Trim()),
                    Convert.ToInt32(this.LengthTextbox.Text.ToString().Trim()
                    ));
                        this.ResultTextbox.AppendText(modbus.byteToHexStr(bytes) + "\r\n");
                    }

                    else if (this.FuncCombobox.SelectedItem.ToString() == "Constraint single coil")
                    { 
                          byte[] bytes = this.ContraintmultiCoil
                    (
                    Convert.ToInt32(this.SlaveAddrTextBox.Text.ToString().Trim()),
                    Convert.ToInt32(this.StartAddrTextbox.Text.ToString().Trim()),
                    Convert.ToBoolean(this.LengthTextbox.Text.ToString().Trim()));
                        this.ResultTextbox.AppendText(modbus.byteToHexStr(bytes) + "\r\n");
                    }
                }
                else
                {
                    this.ConnectButton_Click(null,null);
                    byte[] bytes = this.ReadOutputState
                    (
                    Convert.ToInt32(this.SlaveAddrTextBox.Text.ToString().Trim()),
                    Convert.ToInt32(this.StartAddrTextbox.Text.ToString().Trim()),
                    Convert.ToInt32(this.LengthTextbox.Text.ToString().Trim()
                    ));
                    this.ResultTextbox.AppendText(modbus.byteToHexStr(bytes) + "\r\n");
                }
            }
            else
            {
                MessageBox.Show("Please check the connect condition! ");
            }

        }

        private void FuncCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.FuncCombobox.SelectedItem.ToString().Trim() == "Read Coil")
            {
                this.label1.Content = "  SlaveAddress";
                this.label2.Content = "  StartAddress";
                this.label3.Content = "       Length";
                this.readButton.Content = "Read Coil";
            }
            else if (this.FuncCombobox.SelectedItem.ToString().Trim() == "Constraint single coil")
            {
                this.label1.Content = "  SlaveAddress";
                this.label2.Content = "  CoilAddress";
                this.label3.Content = "       Value";
                this.readButton.Content = "Write Coil";
            }  
        }

        private void InsertIpButton_Click(object sender, RoutedEventArgs e)
        {
            InsertIpWindow insertIpWindow = new InsertIpWindow(this);
            insertIpWindow.Show();
        }
        public void ComboBoxitemsInsert(string str)
        {
            if (!(this.selectIpCombobox.Items.Contains(str)))
            {
                this.selectIpCombobox.Items.Add(str);
            }
        }
    }
    
}
