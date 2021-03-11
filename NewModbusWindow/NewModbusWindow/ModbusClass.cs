using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace NewModbusWindow
{
    /// <summary>
    /// 通用Mobus协议类库
    /// </summary>
    public class Modbus
    {
        public SerialPort MySerialPort = null;
        //received byte arry
        private byte[] recTempBytes;
        private byte rByte;

        public int currentAddr = default;//地址
        public int MWWordlen = default;//接受的字节长度
        public int RecWordLen = default;//接受数据数组大小

        public string ReceString = null;//接收的字符串
        public byte[] Recebytes = null;//接收后转换成的字节数组

        public byte[] SendPresetMultipleRegisterBytes;//预置多寄存器的发送代码；
        public bool PresetMultipleRegisterResult;//存储预置多寄存器的结果

        public string SetCoilStr;
        public Modbus()
        {
            MySerialPort = new SerialPort();
        }
        /// <summary>
        /// initialize serialport's property
        /// </summary>
        /// <param name="IbaudRate">波特率【9600】</param>
        /// <param name="iPortNo">端口名称</param>
        /// <param name="iDataBits">数据位【8】</param>
        /// <param name="iParity">校验位</param>
        /// <param name="iStopBits">停止位【1】</param>
        /// <returns></returns>
        public bool OpenCom(int IbaudRate, string iPortNo, int iDataBits, Parity iParity, StopBits iStopBits)
        {
            try
            {
                if (MySerialPort.IsOpen)
                {
                    MySerialPort.Close();
                    return false;
                }
                else//initialize
                {
                    MySerialPort.BaudRate = IbaudRate;
                    MySerialPort.PortName = iPortNo;
                    MySerialPort.DataBits = iDataBits;
                    MySerialPort.Parity = iParity;
                    MySerialPort.StopBits = iStopBits;
                    MySerialPort.ReceivedBytesThreshold = 1;
                    MySerialPort.DataReceived += MySerialPort_ReceivedHandler;
                    MySerialPort.Open();
                    this.ReceivedFinshed = false;//set the ReceivedHanddlerFinshed
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// close the serialport
        /// </summary>
        /// <returns></returns>
        public bool ClosePort()
        {
            if (this.MySerialPort.IsOpen)
            {
                this.MySerialPort.Close();
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ReceivedFinshed = false;

        /// <summary>
        ///  read keeping register
        /// </summary>
        /// <param name="slaveAddr">从站地址</param>
        /// <param name="startAddr">数据起始地址</param>
        /// <param name="length">长度/param>
        /// <returns></returns>
        public byte[] ReadHoldingReg(int slaveAddr, int startAddr, int length)
        {
            this.currentAddr = slaveAddr;
            this.MWWordlen = length;
            this.RecWordLen = (length * 2) + 5;
            //拼接报文
            byte[] Bytes = new byte[8];
            Bytes[0] = (byte)slaveAddr;
            // function code
            Bytes[1] = 0x03;

            Bytes[2] = (byte)((startAddr - (startAddr % 256)) / 256);//高八位
            Bytes[3] = (byte)(startAddr % 256);//低八位

            Bytes[4] = (byte)((length - (length % 256)) / 256);//高八位
            Bytes[5] = (byte)(length % 256);//低八位;             //crc

            string temp = default;//临时byte数组转换的字符串用来做crc校验
            for (int i = 0; i < 6; i++)
            {
                temp += ' ';
                temp += Bytes[i].ToString("X2").PadLeft(2, '0');
            }
            Bytes[6] = strToToHexByte(CRCCalc(temp)[0])[0];
            Bytes[7] = strToToHexByte(CRCCalc(temp)[1])[0];

            //sending message
            try
            {
                MySerialPort.Write(Bytes, 0, 8);
            }
            catch (Exception)
            {
                return null;
            }
            return Bytes;
        }
        /// <summary>
        /// Preset one of the Holding Register
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <param name="startAddr"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public bool PreSetOneByteHoldingReg(int slaveAddr, int startAddr, int setValue)
        {
            this.RecWordLen = 8;
            this.currentAddr = slaveAddr;
            byte[] bytes = new byte[8];
            bytes[0] = (byte)slaveAddr;
            bytes[1] = (byte)0x06;
            bytes[2] = (byte)((startAddr - (startAddr % 256)) / 256);
            bytes[3] = (byte)(startAddr % 256);
            bytes[4] = (byte)((setValue - (setValue % 256)) / 256);
            bytes[5] = (byte)(setValue % 256);
            string temp = null;
            for (int i = 0; i < 6; i++)
            {
                temp += bytes[i].ToString("X2");
                temp += " ";
            }

            bytes[6] = strToToHexByte(CRCCalc(temp)[0])[0];
            bytes[7] = strToToHexByte(CRCCalc(temp)[1])[0];
            try
            {
                MySerialPort.Write(bytes, 0, 8);
            }
            catch (Exception)
            {
                return false;
            }
            //analysis message
            Thread.Sleep(200);
            this.Recebytes = strToToHexByte(this.ReceString);
            return true;
        }
        /// <summary>
        /// Preset multiple register
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <param name="startAddr"></param>
        /// <param name="preSetValue"></param>
        /// <returns></returns>
        public bool PreSetMultiRegister(int slaveAddr, int startAddr, int setCount, string preSetValue)
        {
            this.currentAddr = slaveAddr;
            this.RecWordLen = 8;
            byte[] bytes = new byte[9 + (strToToHexByte(preSetValue)).Length];
            bytes[0] = (byte)slaveAddr;
            bytes[1] = (byte)0x10;
            bytes[2] = (byte)((startAddr - (startAddr % 256)) / 256);
            bytes[3] = (byte)(startAddr % 256);
            bytes[4] = (byte)0x00;
            bytes[5] = (byte)setCount;
            bytes[6] = (byte)(setCount * 2);
            int num = (setCount * 2);
            for (int i = bytes.Length - 3; i > 6; i--)
            {
                bytes[i] = strToToHexByte(preSetValue)[--num];
            }


            string str = null;
            for (int i = 0; i < bytes.Length - 2; i++)
            {
                str += (bytes[i].ToString("X2") + " ");
            }
            bytes[bytes.Length - 2] = strToToHexByte(CRCCalc(str)[0])[0];
            bytes[bytes.Length - 1] = strToToHexByte(CRCCalc(str)[1])[0];

            try
            {
                MySerialPort.Write(bytes, 0, 9 + (strToToHexByte(preSetValue)).Length);
                this.SendPresetMultipleRegisterBytes = bytes;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                this.SendPresetMultipleRegisterBytes = bytes;
            }
            return true;
        }
        /// <summary>
        /// Read output coil
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <param name="startAddr"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadOutputStatus(int slaveAddr, int startAddr, int length)
        {
            this.currentAddr = slaveAddr;
            if (length <= 8)
            {
                this.RecWordLen = 6;
            }
            else if (length > 8 && length % 8 == 0)
            {
                this.RecWordLen = 6 + (length / 8) - 1;
            }
            else if (length > 8 && length % 8 != 0)
            {
                this.RecWordLen = 6 + (length / 8);
            }

            byte[] bytes = new byte[8];

            //拼接报文
            byte[] Bytes = new byte[8];
            Bytes[0] = (byte)slaveAddr;
            // function code
            Bytes[1] = 0x01;

            Bytes[2] = (byte)((startAddr - (startAddr % 256)) / 256);//高八位
            Bytes[3] = (byte)(startAddr % 256);//低八位

            Bytes[4] = (byte)((length - (length % 256)) / 256);//高八位
            Bytes[5] = (byte)(length % 256);//低八位;             //crc

            string temp = default;//临时byte数组转换的字符串用来做crc校验
            for (int i = 0; i < 6; i++)
            {
                temp += ' ';
                temp += Bytes[i].ToString("X2").PadLeft(2, '0');
            }
            Bytes[6] = strToToHexByte(CRCCalc(temp)[0])[0];
            Bytes[7] = strToToHexByte(CRCCalc(temp)[1])[0];

            //sending message
            try
            {
                MySerialPort.Write(Bytes, 0, 8);
            }
            catch (Exception)
            {
                return null;
            }
            return Bytes;

        }
        /// <summary>
        /// Read input coil
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <param name="startAddr"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadinputStatus(int slaveAddr, int startAddr, int length)
        {
            this.currentAddr = slaveAddr;
            this.RecWordLen = 6;
            byte[] bytes = new byte[8];
            //拼接报文
            byte[] Bytes = new byte[8];
            Bytes[0] = (byte)slaveAddr;
            // function code
            Bytes[1] = 0x02;

            Bytes[2] = (byte)((startAddr - (startAddr % 256)) / 256);//高八位
            Bytes[3] = (byte)(startAddr % 256);//低八位

            Bytes[4] = (byte)((length - (length % 256)) / 256);//高八位
            Bytes[5] = (byte)(length % 256);//低八位;             //crc

            string temp = default;//临时byte数组转换的字符串用来做crc校验
            for (int i = 0; i < 6; i++)
            {
                temp += ' ';
                temp += Bytes[i].ToString("X2").PadLeft(2, '0');
            }
            Bytes[6] = strToToHexByte(CRCCalc(temp)[0])[0];
            Bytes[7] = strToToHexByte(CRCCalc(temp)[1])[0];

            //sending message
            try
            {
                MySerialPort.Write(Bytes, 0, 8);
            }
            catch (Exception)
            {
                return null;
            }
            return Bytes;



        }
        /// <summary>
        /// constraint set the coil（set or reset）
        /// </summary>
        /// <param name="slaveAddr"></param>
        /// <param name="coilAddr"></param>
        /// <param name="setValue"></param>
        /// <returns></returns>
        public bool SetCoil(int slaveAddr, int coilAddr, bool setValue)
        {
            this.currentAddr = slaveAddr;
            this.RecWordLen = 8;
            //拼接报文
            byte[] Bytes = new byte[8];
            Bytes[0] = (byte)slaveAddr;
            // function code
            Bytes[1] = 0x05;

            Bytes[2] = (byte)((coilAddr - (coilAddr % 256)) / 256);//高八位
            Bytes[3] = (byte)(coilAddr % 256);//低八位
            if (setValue)
            {
                Bytes[4] = 0xff;
            }
            else
            {
                Bytes[4] = 0x00;
            }
            Bytes[5] = 0x00;



            string temp = default;//临时byte数组转换的字符串用来做crc校验
            for (int i = 0; i < 6; i++)
            {
                temp += ' ';
                temp += Bytes[i].ToString("X2").PadLeft(2, '0');
            }
            Bytes[6] = strToToHexByte(CRCCalc(temp)[0])[0];
            Bytes[7] = strToToHexByte(CRCCalc(temp)[1])[0];

            //sending message
            try
            {
                MySerialPort.Write(Bytes, 0, 8);
                this.SetCoilStr = byteToHexStr(Bytes);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// received and check the data handler
        /// 预置多寄存器 2寄存器 耗时330ms 4寄存器 340
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public void MySerialPort_ReceivedHandler(object obj, EventArgs eventArgs)
        {
            int count = 0;
            this.recTempBytes = new byte[RecWordLen];
            this.PresetMultipleRegisterResult = false;
            while (MySerialPort.ReadBufferSize > 1)
            {
                if (MySerialPort.IsOpen == false)
                {
                    return;
                }
                rByte = (byte)MySerialPort.ReadByte();
                recTempBytes[count++] = rByte;
                if (count > 1024)
                {
                    //清空输入缓存区
                    count = 0;
                    MySerialPort.DiscardInBuffer();
                    this.ReceString = "0";
                }
                //错误码 查询位超出实际寄存器位数
                if (recTempBytes[0] == (byte)currentAddr && recTempBytes[1] == (byte)0x83 && recTempBytes[2] == (byte)0x02)//验证收到的数据（从站地址/操作码/总字节长度）
                {
                    string str = null;
                    foreach (byte item in recTempBytes)
                    {
                        str += " " + item.ToString("X2");
                    }
                    MySerialPort.DiscardInBuffer();
                    this.ReceString = str;
                    this.Recebytes = strToToHexByte(str);
                    this.ClosePort();
                    return;
                }
                else if (recTempBytes[0] == (byte)currentAddr && recTempBytes[1] == 0x03 && count >= (MWWordlen * 2) + 5)//验证收到的数据（从站地址/操作码/总字节长度）
                {
                    string str = null;
                    foreach (byte item in recTempBytes)
                    {
                        str += " " + item.ToString("X2");
                    }
                    MySerialPort.DiscardInBuffer();
                    this.ReceString = str;
                    this.Recebytes = strToToHexByte(str);
                    this.ClosePort();
                    return;
                }
                else if (recTempBytes[0] == (byte)this.currentAddr && recTempBytes[1] == 0x06 && count >= 8)
                {
                    string str = null;
                    foreach (byte item in recTempBytes)
                    {
                        str += "" + item.ToString("X2");
                    }
                    MySerialPort.DiscardInBuffer();
                    this.ReceString = str;
                    this.ClosePort();
                    return;
                }
                else if (ByteArrayEquals(GetCutByteArray(this.SendPresetMultipleRegisterBytes, 0, 6), GetCutByteArray(this.recTempBytes, 0, 6)) && count >= 8)//预置多寄存器返回值判断
                {
                    this.PresetMultipleRegisterResult = true;
                    string str = null;
                    foreach (byte item in recTempBytes)
                    {
                        str += "" + item.ToString("X2");
                    }
                    MySerialPort.DiscardInBuffer();
                    this.ReceString = str;
                    this.ClosePort();
                    return;
                }
                else if (recTempBytes[0] == (byte)this.currentAddr && recTempBytes[1] == 0x90 && count >= 8)//预置多寄存器返回值判断
                {
                    this.PresetMultipleRegisterResult = false;
                    string str = null;
                    foreach (byte item in recTempBytes)
                    {
                        str += "" + item.ToString("X2");
                    }
                    MySerialPort.DiscardInBuffer();
                    this.ReceString = str;
                    this.ClosePort();
                    return;
                }
                //读取输出线圈
                else if (recTempBytes[0] == (byte)this.currentAddr && recTempBytes[1] == (byte)0x01 && count >= this.RecWordLen)
                {
                    string str = null;
                    foreach (byte item in recTempBytes)
                    {
                        str += "" + item.ToString("X2");
                    }
                    MySerialPort.DiscardInBuffer();
                    this.ReceString = str;
                    this.ClosePort();
                    return;
                }
                //读取输入线圈
                else if (recTempBytes[0] == (byte)this.currentAddr && recTempBytes[1] == (byte)0x02 && count >= this.RecWordLen)
                {
                    string str = null;
                    foreach (byte item in recTempBytes)
                    {
                        str += "" + item.ToString("X2");
                    }
                    MySerialPort.DiscardInBuffer();
                    this.ReceString = str;
                    this.ClosePort();
                    return;
                }
                //强制当线圈返回
                else if (recTempBytes[0] == (byte)this.currentAddr && recTempBytes[1] == (byte)0x05 && count >= this.RecWordLen)
                {
                    string str = null;
                    foreach (byte item in recTempBytes)
                    {
                        str +=item.ToString("X2");
                    }
                    MySerialPort.DiscardInBuffer();
                    this.ReceString = str;
                    this.ClosePort();
                    return;
                }
                else
                {
                    this.ReceString = null;
                }
            }
        }

        //========================================华丽的分割线=====================================
        /// <summary>
        /// 字符串转16进制字节数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        private byte[] strToToHexByte(string hexString)//16进制字符串转换2进制字节数组
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString = hexString.PadLeft(2, '0');
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string byteToHexStr(byte[] bytes)//2进制字节数组转换16进制字符串
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                    returnStr += " ";
                }
            }
            return returnStr;
        }
        /// <summary>
        /// 按起始地址+长度截取字节数组
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] GetCutByteArray(byte[] bytes, int start, int length)
        {
            byte[] tempBytes = new byte[length - start];
            if (bytes != null && bytes.Length >= length)
            {
                for (int i = 0; i < length; i++)
                {
                    tempBytes[i] = bytes[i + start];
                }
            }
            return tempBytes;
        }
        public bool ByteArrayEquals(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1 == null || bytes2 == null) return false;
            else if (bytes1.Length != bytes2.Length) return false;

            for (int i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i]) return false;
            }
            return true;
        }
        /// <summary>
        /// CRC校验
        /// </summary>
        /// <param name="data">校验数据</param>
        /// <returns>高低8位</returns>
        public string[] CRCCalc(string data)
        {
            string[] datas = data.Split(' ');//传入的字符串中间要有‘ ’！
            List<byte> bytedata = new List<byte>();

            foreach (string str in datas)
            {
                if (str != "")
                {
                    bytedata.Add(byte.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier));
                }

            }
            byte[] crcbuf = bytedata.ToArray();
            //计算并填写CRC校验码
            int crc = 0xffff;
            int len = crcbuf.Length;
            for (int n = 0; n < len; n++)
            {
                byte i;
                crc = crc ^ crcbuf[n];
                for (i = 0; i < 8; i++)
                {
                    int TT;
                    TT = crc & 1;
                    crc = crc >> 1;
                    crc = crc & 0x7fff;
                    if (TT == 1)
                    {
                        crc = crc ^ 0xa001;
                    }
                    crc = crc & 0xffff;
                }
            }
            string[] redata = new string[2];
            redata[1] = Convert.ToString((byte)((crc >> 8) & 0xff), 16);
            redata[0] = Convert.ToString((byte)((crc & 0xff)), 16);
            return redata;
        }
    }
}
