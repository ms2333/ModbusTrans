# ModbusTrans
Base on ModbusProtocol ,Developed on the WPF platform,Integrated Modbus Serial/TCP To facilitate extension

软件介绍：

1.本程序基于plc通讯协议Modbus serial/tcp/ip开发，使用.net wpf平台界面，功能界面预留多处可扩展空间。
2.通讯协议集成在了ModbusClass.cs类中（单例模式），通用的Modbus协议类库。
3.查看协议源码：https://github.com/ms2333/ModbusTrans/blob/9fa1541d7e9bad1aa4d42f0511e6a9e596061cb2/NewModbusWindow/NewModbusWindow/ModbusClass.cs

开始界面：

![image](https://user-images.githubusercontent.com/64838899/110805434-08eacc00-82bc-11eb-901f-bbecc79aafc2.png)

一、serial界面分为不同的操作指令：
![image](https://user-images.githubusercontent.com/64838899/110805543-25870400-82bc-11eb-9a41-88b3fd05225b.png)

读取保持型寄存器：
![image](https://user-images.githubusercontent.com/64838899/110805603-36d01080-82bc-11eb-9430-506fbf19ceff.png)
预支单字节型保持寄存器
![image](https://user-images.githubusercontent.com/64838899/110805662-451e2c80-82bc-11eb-8431-82f1d5162715.png)
预支多个寄存器：
![image](https://user-images.githubusercontent.com/64838899/110805701-4d766780-82bc-11eb-9b72-bb850b228481.png)
读取多输出线圈：
![image](https://user-images.githubusercontent.com/64838899/110805744-5830fc80-82bc-11eb-88d4-28b9b3903068.png)
读取多输入线圈：
![image](https://user-images.githubusercontent.com/64838899/110805786-60893780-82bc-11eb-81ab-546afb89e620.png)
强制复位：
![image](https://user-images.githubusercontent.com/64838899/110805822-697a0900-82bc-11eb-81e2-92a17b88a084.png)

二、Tcp/ip传输界面

这里将由您选择注入常用的Ip地址方便下次使用
下拉功能菜单栏中选项的变化将直接改变输入的参数的标签，便于日后您在程序底层添加更多功能，届时无需更多输入框部件
黑色区域为返回的数据显示区域，采用黑色背景数据，以突出数据

![image](https://user-images.githubusercontent.com/64838899/110806001-9b8b6b00-82bc-11eb-984e-8eb378c9b669.png)
![image](https://user-images.githubusercontent.com/64838899/110806809-63385c80-82bd-11eb-8106-a808584389fe.png)

基于Tcp/Ip的ModbusServer：
这里将用于接收和处理由其他服务站，或者主站传来的数据流，这里只做了简单的数据处理及显示，当然，您可以选择加密处理这些数据并转发给其他客户端
