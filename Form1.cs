using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SharpDX.DirectInput;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;

namespace FRCDriverStation
{
    public class Encoder
    {
        public Encoder()
        {

        }
        public byte[] EncodeMessage(int packetnumber, int joystickxvalue, int joystickyvalue, bool[] buttons, int header)
        {
            byte[] bytes = new byte[9];
            bytes[0] = (byte)header;
            bytes[1] = (byte)packetnumber;
            bytes[2] = (byte)joystickxvalue;
            bytes[3] = (byte)(joystickxvalue >> 8);
            bytes[4] = (byte)joystickyvalue;
            bytes[5] = (byte)(joystickyvalue >> 8);
            for (int r = 0; r < buttons.Length; r++)
            {
                if (buttons[r])
                {
                    bytes[6 + r / 8] |= (byte)(1 << (r % 8));
                }
            }
            return bytes;
        }
        public byte[] Compound(byte[] bytes1, byte[] bytes2)
        {
            byte[] bytes = new byte[16];
            int secondval = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i < 8)
                {
                    bytes[i] = bytes1[i];
                }
                else
                {
                    bytes[i] = bytes2[secondval];
                    secondval = secondval + 1;
                }
            }
            return bytes;
        }
        public void Decompound(byte[] bytes)
        {
            byte[] bytes1 = new byte[8];
            byte[] bytes2 = new byte[8];
            int secondval = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i <= 8)
                {
                    bytes1[i] = bytes[i];
                }
                else if (i > 8 && i <= 16)
                {
                    bytes2[secondval] = bytes[i];
                    secondval = secondval + 1;
                }
            }
        }
    }
    partial class Form1
    {
        public void Arcade(Joystick joystick)
        {
            MessageBox.Show("Arcade Drive not tank Drive");
            arcadeEnabled = true;
            bool[] buttons = new bool[12];
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();
            int sendjoyY = 32767;
            int sendjoyX = 32767;
            while (!close)
            {
                try
                {
                    joystick.Poll();
                }
                catch (Exception e) {
                    this.label8.BackColor = Color.Red;
                }
                var datas = joystick.GetBufferedData();
                foreach (var state in datas)
                {
                    if (state.Offset == JoystickOffset.Buttons0)
                    {
                        if (!buttons[0])
                        {
                            buttons[0] = true;
                        }
                        else
                        {
                            buttons[0] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons1)
                    {
                        if (!buttons[1])
                        {
                            buttons[1] = true;
                        }
                        else
                        {
                            buttons[1] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons2)
                    {
                        if (!buttons[2])
                        {
                            buttons[2] = true;
                        }
                        else
                        {
                            buttons[2] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons3)
                    {
                        if (!buttons[3])
                        {
                            buttons[3] = true;
                        }
                        else
                        {
                            buttons[3] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons4)
                    {
                        if (!buttons[4])
                        {
                            buttons[4] = true;
                        }
                        else
                        {
                            buttons[4] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons5)
                    {
                        if (!buttons[5])
                        {
                            buttons[5] = true;
                        }
                        else
                        {
                            buttons[5] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons6)
                    {
                        if (!buttons[6])
                        {
                            buttons[6] = true;
                        }
                        else
                        {
                            buttons[6] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons7)
                    {
                        if (!buttons[7])
                        {
                            buttons[7] = true;
                        }
                        else
                        {
                            buttons[7] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons8)
                    {
                        if (!buttons[8])
                        {
                            buttons[8] = true;
                        }
                        else
                        {
                            buttons[8] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons9)
                    {
                        if (!buttons[9])
                        {
                            buttons[9] = true;
                        }
                        else
                        {
                            buttons[9] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons10)
                    {
                        if (!buttons[10])
                        {
                            buttons[10] = true;
                        }
                        else
                        {
                            buttons[10] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Y)
                    {
                        sendjoyY = state.Value;
                    }
                    else if (state.Offset == JoystickOffset.X)
                    {
                        sendjoyX = state.Value;
                    }
                }
                Encoder myEncoder = new Encoder();
                byte[] bytes1 = myEncoder.EncodeMessage(0, sendjoyX, sendjoyY, buttons, 3);
                SendClient(myEncoder.EncodeMessage(0, sendjoyX, sendjoyY, buttons, 3));
                /*Decoder myDecoder = new Decoder(bytes1, 0);
                for (int x = 0; x < myDecoder.getbuttons().Length; x++)
                {
                    if (myDecoder.getRawButtons(x))
                    {
                        Console.WriteLine("button " + x + " pressed");
                    }
                }*/
                Thread.Sleep(100);
            }
        }
        public static Guid joystickGuid1 = Guid.Empty;
        public static Guid joystickGuid2 = Guid.Empty;
        public static DirectInput directInput = new DirectInput();
        public static Joystick joystick1;
        public static Joystick joystick2;
        public static bool tankEnabled = false;
        public static bool arcadeEnabled = false;
        public static void ClientInit()
        {
            // Find a Joystick Guid
            joystickGuid1 = Guid.Empty;
            joystickGuid2 = Guid.Empty;
            stick = true;
            int time = 0;
            int jtime = 0;
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AllDevices))
            {
                if (time == 0)
                {
                    joystickGuid1 = deviceInstance.InstanceGuid;
                }
                else if (time == 1)
                {
                    joystickGuid2 = deviceInstance.InstanceGuid;
                }
                time = time + 1;
            }
            // If Gamepad not found, look for a Joystick 
            if (joystickGuid1 == Guid.Empty || joystickGuid2 == Guid.Empty)
            {
                foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AllDevices))
                {
                    if (jtime == 0)
                    {
                        joystickGuid1 = deviceInstance.InstanceGuid;
                    }
                    else if (jtime == 1)
                    {
                        joystickGuid2 = deviceInstance.InstanceGuid;
                    }
                    jtime = jtime + 1;
                }
            }
            // If Joystick not found, throws an error 
        }
        public static String ip = "192.168.0.17";
        public static void SendClient(byte[] data)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Parse(ip);
            /*Ping p = new Ping();
            PingReply reply = p.Send(ip);
            if (reply.Status != IPStatus.Success) {
                MessageBox.Show("Device Unreachable", "Error");
                           }*/
            IPEndPoint ep = new IPEndPoint(ipAddress, 36361);
            s.SendTo(data, ep);
        }
        public void Tank(Joystick joystick1, Joystick joystick2)
        {
            MessageBox.Show("Tank Drive not Arcade Drive");
            tankEnabled = true;
            bool[] buttons = new bool[12];
            bool[] buttons2 = new bool[12];
            joystick1.Properties.BufferSize = 128;
            joystick2.Properties.BufferSize = 128;
            int sendJoyX = 32767;
            int sendJoyX2 = 32767;
            int sendJoyY = 32767;
            int packetnumber = 0;
            int sendJoyY2 = 32767;
            // Acquire the joystick 
            joystick1.Acquire();
            joystick2.Acquire();
            // Poll events from joystick
            while (!close)
            {
                if (close)
                {
                    break;
                }
                stick = true;
                try
                {
                    joystick1.Poll();
                    joystick2.Poll();
                    var datas1 = joystick1.GetBufferedData();
                    var datas2 = joystick2.GetBufferedData();
                    foreach (var state in datas1)
                {
                    if (state.Offset == JoystickOffset.Buttons0)
                    {
                        if (!buttons[0])
                        {
                            buttons[0] = true;
                        }
                        else
                        {
                            buttons[0] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons1)
                    {
                        if (!buttons[1])
                        {
                            buttons[1] = true;
                        }
                        else
                        {
                            buttons[1] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons2)
                    {
                        if (!buttons[2])
                        {
                            buttons[2] = true;
                        }
                        else
                        {
                            buttons[2] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons3)
                    {
                        if (!buttons[3])
                        {
                            buttons[3] = true;
                        }
                        else
                        {
                            buttons[3] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons4)
                    {
                        if (!buttons[4])
                        {
                            buttons[4] = true;
                        }
                        else
                        {
                            buttons[4] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons5)
                    {
                        if (!buttons[5])
                        {
                            buttons[5] = true;
                        }
                        else
                        {
                            buttons[5] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons6)
                    {
                        if (!buttons[6])
                        {
                            buttons[6] = true;
                        }
                        else
                        {
                            buttons[6] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons7)
                    {
                        if (!buttons[7])
                        {
                            buttons[7] = true;
                        }
                        else
                        {
                            buttons[7] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons8)
                    {
                        if (!buttons[8])
                        {
                            buttons[8] = true;
                        }
                        else
                        {
                            buttons[8] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons9)
                    {
                        if (!buttons[9])
                        {
                            buttons[9] = true;
                        }
                        else
                        {
                            buttons[9] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons10)
                    {
                        if (!buttons[10])
                        {
                            buttons[10] = true;
                        }
                        else
                        {
                            buttons[10] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.X)
                    {
                        sendJoyX = state.Value;
                    }
                    if (state.Offset == JoystickOffset.Y)
                    {
                        sendJoyY = state.Value;
                    }
                }
                foreach (var state in datas2)
                {
                    if (state.Offset == JoystickOffset.Buttons0)
                    {
                        if (!buttons2[0])
                        {
                            buttons2[0] = true;

                        }
                        else
                        {
                            buttons2[0] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons1)
                    {
                        if (!buttons2[1])
                        {
                            buttons2[1] = true;
                        }
                        else
                        {
                            buttons2[1] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons2)
                    {
                        if (!buttons2[2])
                        {
                            buttons2[2] = true;
                        }
                        else
                        {
                            buttons2[2] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons3)
                    {
                        if (!buttons2[3])
                        {
                            buttons2[3] = true;
                        }
                        else
                        {
                            buttons2[3] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons4)
                    {
                        if (!buttons2[4])
                        {
                            buttons2[4] = true;
                        }
                        else
                        {
                            buttons2[4] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons5)
                    {
                        if (!buttons2[5])
                        {
                            buttons2[5] = true;
                        }
                        else
                        {
                            buttons2[5] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons6)
                    {
                        if (!buttons2[6])
                        {
                            buttons2[6] = true;
                        }
                        else
                        {
                            buttons2[6] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons7)
                    {
                        if (!buttons2[7])
                        {
                            buttons2[7] = true;
                        }
                        else
                        {
                            buttons2[7] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons8)
                    {
                        if (!buttons2[8])
                        {
                            buttons2[8] = true;
                        }
                        else
                        {
                            buttons2[8] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons9)
                    {
                        if (!buttons2[9])
                        {
                            buttons2[9] = true;
                        }
                        else
                        {
                            buttons2[9] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.Buttons10)
                    {
                        if (!buttons2[10])
                        {
                            buttons2[10] = true;
                        }
                        else
                        {
                            buttons2[10] = false;
                        }
                    }
                    if (state.Offset == JoystickOffset.X)
                    {
                        sendJoyX2 = state.Value;
                    }
                    if (state.Offset == JoystickOffset.Y)
                    {
                        sendJoyY2 = state.Value;
                    }
                }
                    Encoder myEncoder = new Encoder();
                    packetnumber++;
                    if (packetnumber > 5040)
                    {
                        packetnumber = 0;
                    }
                    SendClient(myEncoder.Compound(myEncoder.EncodeMessage(packetnumber, sendJoyX, sendJoyY, buttons, header), myEncoder.EncodeMessage(packetnumber, sendJoyX2, sendJoyY2, buttons2, header)));
                    Thread.Sleep(40);
                }
                catch (Exception e)
                {
                    stick = false;
                }
                if (stick)
                {
                    this.label8.BackColor = Color.LightGreen;
                }
                else{
                    this.label8.BackColor = Color.Red;
                }
            }
        Encoder theEncoder = new Encoder();
        for (int i = 0; i < 5; i++)
        {
             SendClient(theEncoder.Compound(theEncoder.EncodeMessage(packetnumber, sendJoyX, sendJoyY, buttons, 6), theEncoder.EncodeMessage(packetnumber, sendJoyX2, sendJoyY2, buttons2, 6)));
        }
        Thread.Sleep(20);
        }
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        public static int mode = 0;
        public static int header = 0;
        public static bool stick = true;
        public static bool close = false;
        public void myDisabled(object o, System.EventArgs e)
        {
            if (mode == 0)
            {
                header = 6;
            }
            else if (mode == 1)
            {
                header = 2;
            }
            else if (mode == 2)
            {
                header = 4;
            }
            this.button11.BackColor = Color.Black;
            this.button10.BackColor = Color.DimGray;
        }
        public void myEnabled(object o, System.EventArgs e)
        {
            if (mode == 0)
            {
                header = 5;
            }
            else if (mode == 1)
            {
                header = 1;
            }
            else if (mode == 2)
            {
                header = 3;
            }
            this.button11.BackColor = Color.DimGray;
            this.button10.BackColor = Color.Black;
        }
        public void Autonomous(object o, System.EventArgs e)
        {
            mode = 1;
            this.button11.BackColor = Color.Black;
            this.button10.BackColor = Color.DimGray;
            this.button6.BackColor = Color.DimGray;
            this.button7.BackColor = Color.Black;
            this.button8.BackColor = Color.DimGray;
            this.button9.BackColor = Color.DimGray;
            header = 6;
        }
        public void Teleoperated(object o, System.EventArgs e)
        {
            mode = 0;
            this.button11.BackColor = Color.Black;
            this.button10.BackColor = Color.DimGray;
            this.button7.BackColor = Color.DimGray;
            this.button6.BackColor = Color.Black;
            this.button8.BackColor = Color.DimGray;
            this.button9.BackColor = Color.DimGray;
            header = 2;
        }
        public void Test(object o, System.EventArgs e)
        {
            mode = 3;
            this.button7.BackColor = Color.DimGray;
            this.button6.BackColor = Color.DimGray;
            this.button8.BackColor = Color.DimGray;
            this.button9.BackColor = Color.Black;
            header = 2;
        }
        public void Practice(object o, System.EventArgs e)
        {
            mode = 2;
            this.button7.BackColor = Color.DimGray;
            this.button6.BackColor = Color.DimGray;
            this.button8.BackColor = Color.Black;
            this.button9.BackColor = Color.DimGray;
            header = 2;
        }
        //public static String ip = "192.168.0.17";
        public void WindowClosed(object o, EventArgs e) {
            close = true;
        }
        public void Decide() {
            if (joystickGuid1 == Guid.Empty)
            {
                Console.WriteLine("No joystick/Gamepad found.");
                stick = false;
                //MessageBox.Show("no joystick found", "Error");
                this.label8.BackColor = Color.Red;
                while (!close) {
                    if (close) {
                        break;
                    }
                    if (rerun) {
                        this.label5.BackColor = Color.LightGreen;
                        Decide();
                    }
                }
            } // Instantiate the joystick

            else if (joystickGuid1 != Guid.Empty && joystickGuid2 == Guid.Empty)
            {
                Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid1);
                joystick1 = new Joystick(directInput, joystickGuid1);
                Arcade(joystick1);
            }
            else
            {
                joystick2 = new Joystick(directInput, joystickGuid2);
                joystick1 = new Joystick(directInput, joystickGuid1);
                Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid1);
                Console.WriteLine("Found Joystick/Gamepad with GUID: {0}", joystickGuid2);
                Tank(joystick1, joystick2);
            }
        }
        public void StationSetup() {
            ClientInit();
            ThreadStart UDPLoop = new ThreadStart(Decide);
            Thread myThread = new Thread(UDPLoop);
            myThread.Start();
            ThreadStart start = new ThreadStart(refreshLoop);
            Thread t = new Thread(start);
            t.Start();
        }
        public static bool rerun = false;
        public static bool restart = false;
        public void refreshLoop() {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];
            while (!close)
            {
                if (close)
                {
                    break;
                }
                // Connect to a remote device.  
                try
                {
                    // Establish the remote endpoint for the socket.  
                    // This example uses port 11000 on the local computer.  
                    IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                    IPAddress ipAddress = IPAddress.Parse(ip);
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, 36362);

                    // Create a TCP/IP  socket.  
                    Socket sender = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
                    int mytime = 0;
                    // Connect the socket to the remote endpoint. Catch any errors.  
                    try
                    {
                        sender.Connect(remoteEP);
                        connection = true;
                        Console.WriteLine("Socket connected to {0}",
                            sender.RemoteEndPoint.ToString());
                        while (!close)
                        {
                            /*if (joystickGuid1 == Guid.Empty) {
                                ClientInit();
                                if (joystickGuid1 != Guid.Empty) {
                                    rerun = true;
                                    MessageBox.Show("established");
                                }
                                MessageBox.Show("established");
                            }*/
                            // Encode the data string into a byte array.  
                            byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");
                            mytime++;
                            // Send the data through the socket.  
                            int bytesSent = sender.Send(msg);

                            // Receive the response from the remote device.  
                            int bytesRec = sender.Receive(bytes);

                            if (mytime > 25)
                            {
                                mytime = 0;
                                if (connection == true && bytesRec == 0) {
                                    break;
                                    this.label5.BackColor = Color.Red;
                                    this.label7.BackColor = Color.Red;
                                }
                            }
                            if(connection == true && bytesRec == 0) {
                                break;
                                this.label5.BackColor = Color.Red;
                                this.label7.BackColor = Color.Red;
                            }
                            Console.WriteLine("Echoed test = {0}",
                                Encoding.ASCII.GetString(bytes, 0, bytesRec));

                            // Release the socket.  
                            if (close)
                            {
                                break;
                            }
                            if (!stick)
                            {
                                this.label8.BackColor = Color.Red;
                            }
                            if (stick)
                            {
                                this.label8.BackColor = Color.LightGreen;
                            }
                            if (!connection)
                            {
                                this.label5.BackColor = Color.Red;
                                this.label7.BackColor = Color.Red;
                            }
                            if (connection)
                            {
                                this.label5.BackColor = Color.LightGreen;
                                this.label7.BackColor = Color.LightGreen;
                            }
                            Thread.Sleep(200);
                        }
                        sender.Send(Encoding.ASCII.GetBytes("close"));
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                    }
                    catch (ArgumentNullException ane)
                    {
                        Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                        //MessageBox.Show(ane.ToString());
                        this.label5.BackColor = Color.Red;
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine("SocketException : {0}", se.ToString());
                        //MessageBox.Show(se.ToString());
                        this.label5.BackColor = Color.Red;
                        this.label7.BackColor = Color.Red;
                        connection = false;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unexpected exception : {0}", e.ToString());
                        //MessageBox.Show(e.ToString());
                        this.label5.BackColor = Color.Red;
                        this.label7.BackColor = Color.Red;
                        connection = false;
                    }
                    finally
                    {
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        public static bool connection = true;
        public static bool once;
        public static bool code = true;
        public void resetStation(object o, EventArgs e) {
            System.Diagnostics.Process.Start(Application.ExecutablePath);
            close = true;
            this.Close();
        }
        private void InitializeComponent()
        {
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            this.flowLayoutPanel7 = new System.Windows.Forms.FlowLayoutPanel();
            this.button10 = new System.Windows.Forms.Button();
            this.button11 = new System.Windows.Forms.Button();
            this.flowLayoutPanel4 = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label11 = new System.Windows.Forms.Label();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.flowLayoutPanel5 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.button13 = new System.Windows.Forms.Button();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.button12 = new System.Windows.Forms.Button();
            this.flowLayoutPanel6 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel7.SuspendLayout();
            this.flowLayoutPanel4.SuspendLayout();
            this.flowLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // ramCounter
            // 
            //this.ramCounter.CategoryName = "Memory";
            //this.ramCounter.CounterName = "Available MBytes";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel2);
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel3);
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel4);
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel5);
            this.flowLayoutPanel1.Controls.Add(this.flowLayoutPanel6);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(1, 0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(883, 226);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Controls.Add(this.button1);
            this.flowLayoutPanel2.Controls.Add(this.button2);
            this.flowLayoutPanel2.Controls.Add(this.button3);
            this.flowLayoutPanel2.Controls.Add(this.button4);
            this.flowLayoutPanel2.Controls.Add(this.button5);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(68, 237);
            this.flowLayoutPanel2.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Black;
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(64, 39);
            this.button1.TabIndex = 0;
            this.button1.Text = "Control";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.DimGray;
            this.button2.ForeColor = System.Drawing.Color.White;
            this.button2.Location = new System.Drawing.Point(3, 48);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(64, 37);
            this.button2.TabIndex = 1;
            this.button2.Text = "button2";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.DimGray;
            this.button3.ForeColor = System.Drawing.Color.White;
            this.button3.Location = new System.Drawing.Point(3, 91);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(64, 37);
            this.button3.TabIndex = 2;
            this.button3.Text = "Settings";
            this.button3.UseVisualStyleBackColor = false;
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.DimGray;
            this.button4.ForeColor = System.Drawing.Color.White;
            this.button4.Location = new System.Drawing.Point(3, 134);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(64, 36);
            this.button4.TabIndex = 3;
            this.button4.Text = "USB";
            this.button4.UseVisualStyleBackColor = false;
            // 
            // button5
            // 
            this.button5.BackColor = System.Drawing.Color.DimGray;
            this.button5.ForeColor = System.Drawing.Color.White;
            this.button5.Location = new System.Drawing.Point(3, 176);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(64, 39);
            this.button5.TabIndex = 4;
            this.button5.Text = "Power";
            this.button5.UseVisualStyleBackColor = false;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.BackColor = System.Drawing.Color.DimGray;
            this.flowLayoutPanel3.Controls.Add(this.button6);
            this.flowLayoutPanel3.Controls.Add(this.button7);
            this.flowLayoutPanel3.Controls.Add(this.button8);
            this.flowLayoutPanel3.Controls.Add(this.button9);
            this.flowLayoutPanel3.Controls.Add(this.flowLayoutPanel7);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(77, 3);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Size = new System.Drawing.Size(193, 223);
            this.flowLayoutPanel3.TabIndex = 1;
            // 
            // button6
            // 
            this.button6.BackColor = System.Drawing.Color.Black;
            this.button6.ForeColor = System.Drawing.Color.White;
            this.button6.Location = new System.Drawing.Point(3, 3);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(190, 31);
            this.button6.TabIndex = 0;
            this.button6.Text = "TeleOperated";
            this.button6.UseVisualStyleBackColor = false;
            this.button6.Click += new System.EventHandler(this.Teleoperated);
            // 
            // button7
            // 
            this.button7.BackColor = System.Drawing.Color.DimGray;
            this.button7.ForeColor = System.Drawing.Color.White;
            this.button7.Location = new System.Drawing.Point(3, 40);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(190, 27);
            this.button7.TabIndex = 1;
            this.button7.Text = "Autonomous";
            this.button7.UseVisualStyleBackColor = false;
            this.button7.Click += new System.EventHandler(this.Autonomous);
            // 
            // button8
            // 
            this.button8.BackColor = System.Drawing.Color.DimGray;
            this.button8.ForeColor = System.Drawing.Color.White;
            this.button8.Location = new System.Drawing.Point(3, 73);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(190, 30);
            this.button8.TabIndex = 2;
            this.button8.Text = "Practice";
            this.button8.UseVisualStyleBackColor = false;
            this.button8.Click += new System.EventHandler(this.Practice);
            // 
            // button9
            // 
            this.button9.BackColor = System.Drawing.Color.DimGray;
            this.button9.ForeColor = System.Drawing.Color.White;
            this.button9.Location = new System.Drawing.Point(3, 109);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(190, 29);
            this.button9.TabIndex = 3;
            this.button9.Text = "Test";
            this.button9.UseVisualStyleBackColor = false;
            this.button9.Click += new System.EventHandler(this.Test);
            // 
            // flowLayoutPanel7
            // 
            this.flowLayoutPanel7.Controls.Add(this.button10);
            this.flowLayoutPanel7.Controls.Add(this.button11);
            this.flowLayoutPanel7.Location = new System.Drawing.Point(3, 144);
            this.flowLayoutPanel7.Name = "flowLayoutPanel7";
            this.flowLayoutPanel7.Size = new System.Drawing.Size(190, 73);
            this.flowLayoutPanel7.TabIndex = 4;
            // 
            // button10
            // 
            this.button10.BackColor = System.Drawing.Color.DimGray;
            this.button10.ForeColor = System.Drawing.Color.LightGreen;
            this.button10.Location = new System.Drawing.Point(3, 3);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(89, 71);
            this.button10.TabIndex = 0;
            this.button10.Text = "Enabled";
            this.button10.UseVisualStyleBackColor = false;
            this.button10.Click += new System.EventHandler(myEnabled);
            // 
            // button11
            // 
            this.button11.BackColor = System.Drawing.Color.Black;
            this.button11.ForeColor = System.Drawing.Color.Red;
            this.button11.Location = new System.Drawing.Point(98, 3);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(89, 71);
            this.button11.TabIndex = 1;
            this.button11.Text = "Disabled";
            this.button11.UseVisualStyleBackColor = false;
            this.button11.Click += new System.EventHandler(myDisabled);
            // 
            // flowLayoutPanel4
            // 
            this.flowLayoutPanel4.BackColor = System.Drawing.Color.DimGray;
            this.flowLayoutPanel4.Controls.Add(this.label2);
            this.flowLayoutPanel4.Controls.Add(this.label3);
            this.flowLayoutPanel4.Controls.Add(this.label4);
            this.flowLayoutPanel4.Controls.Add(this.progressBar1);
            this.flowLayoutPanel4.Controls.Add(this.label11);
            this.flowLayoutPanel4.Controls.Add(this.progressBar2);
            this.flowLayoutPanel4.Location = new System.Drawing.Point(276, 3);
            this.flowLayoutPanel4.Name = "flowLayoutPanel4";
            this.flowLayoutPanel4.Size = new System.Drawing.Size(194, 223);
            this.flowLayoutPanel4.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 10F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 34);
            this.label2.TabIndex = 0;
            this.label2.Text = "Time: ";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Arial", 10F);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(107, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 34);
            this.label3.TabIndex = 1;
            this.label3.Text = "00:00";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Arial", 10F);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(3, 34);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 33);
            this.label4.TabIndex = 2;
            this.label4.Text = "PC Battery";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(119, 37);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(72, 27);
            this.progressBar1.TabIndex = 3;
            this.progressBar1.Value = 100;
            // 
            // label11
            // 
            this.label11.ForeColor = System.Drawing.Color.White;
            this.label11.Location = new System.Drawing.Point(3, 67);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(110, 30);
            this.label11.TabIndex = 4;
            this.label11.Text = "CPU Usage";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label11.Font = new Font("Arial", 10, FontStyle.Regular);
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(119, 70);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(72, 27);
            this.progressBar2.TabIndex = 5;
            // 
            // flowLayoutPanel5
            // 
            this.flowLayoutPanel5.BackColor = System.Drawing.Color.DimGray;
            this.flowLayoutPanel5.Controls.Add(this.label1);
            this.flowLayoutPanel5.Controls.Add(this.splitContainer1);
            this.flowLayoutPanel5.Controls.Add(this.button12);
            this.flowLayoutPanel5.Location = new System.Drawing.Point(476, 3);
            this.flowLayoutPanel5.Name = "flowLayoutPanel5";
            this.flowLayoutPanel5.Size = new System.Drawing.Size(202, 223);
            this.flowLayoutPanel5.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Team# 3636";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Location = new System.Drawing.Point(3, 29);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.button13);
            this.splitContainer1.Panel1.Controls.Add(this.label10);
            this.splitContainer1.Panel1.Controls.Add(this.label9);
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label8);
            this.splitContainer1.Panel2.Controls.Add(this.label7);
            this.splitContainer1.Panel2.Controls.Add(this.label5);
            this.splitContainer1.Size = new System.Drawing.Size(191, 109);
            this.splitContainer1.SplitterDistance = 131;
            this.splitContainer1.TabIndex = 1;
            // 
            // button13
            // 
            this.button13.BackColor = System.Drawing.Color.DimGray;
            this.button13.ForeColor = System.Drawing.Color.White;
            this.button13.Location = new System.Drawing.Point(56, 77);
            this.button13.Name = "button13";
            this.button13.Size = new System.Drawing.Size(75, 32);
            this.button13.TabIndex = 3;
            this.button13.Text = "Reset";
            this.button13.UseVisualStyleBackColor = false;
            this.button13.Click += new EventHandler(resetStation);
            // 
            // label10
            // 
            this.label10.ForeColor = System.Drawing.Color.White;
            this.label10.Location = new System.Drawing.Point(63, 51);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(68, 23);
            this.label10.TabIndex = 2;
            this.label10.Text = "Joysticks";
            // 
            // label9
            // 
            this.label9.ForeColor = System.Drawing.Color.White;
            this.label9.Location = new System.Drawing.Point(45, 29);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(86, 22);
            this.label9.TabIndex = 1;
            this.label9.Text = "Robot Code";
            // 
            // label6
            // 
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(20, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(111, 22);
            this.label6.TabIndex = 0;
            this.label6.Text = "Communication";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label8
            // 
            this.label8.BackColor = System.Drawing.Color.LightGreen;
            this.label8.Location = new System.Drawing.Point(3, 51);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(46, 17);
            this.label8.TabIndex = 2;
            this.label8.Text = "     ";
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.Color.LightGreen;
            this.label7.Location = new System.Drawing.Point(3, 29);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(46, 17);
            this.label7.TabIndex = 1;
            this.label7.Text = "     ";
            // 
            // label5
            // 
            this.label5.BackColor = System.Drawing.Color.LightGreen;
            this.label5.Location = new System.Drawing.Point(3, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(46, 17);
            this.label5.TabIndex = 0;
            this.label5.Text = "     ";
            // 
            // button12
            // 
            this.button12.BackColor = System.Drawing.Color.DimGray;
            this.button12.ForeColor = System.Drawing.Color.White;
            this.button12.Location = new System.Drawing.Point(3, 144);
            this.button12.Name = "button12";
            this.button12.Size = new System.Drawing.Size(199, 79);
            this.button12.TabIndex = 2;
            this.button12.Text = "Upload Code to Robot";
            this.button12.UseVisualStyleBackColor = false;
            // 
            // flowLayoutPanel6
            // 
            this.flowLayoutPanel6.Location = new System.Drawing.Point(3, 246);
            this.flowLayoutPanel6.Name = "flowLayoutPanel6";
            this.flowLayoutPanel6.Size = new System.Drawing.Size(199, 223);
            this.flowLayoutPanel6.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(896, 228);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WindowClosed);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel7.ResumeLayout(false);
            this.flowLayoutPanel4.ResumeLayout(false);
            this.flowLayoutPanel5.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

            //MessageBox.Show(ip, "it worked");
            getIP();
            StationSetup();
        }
        public void getIP() {
            Form2 textBox = new Form2();
            if (textBox.ShowDialog(this) == DialogResult.OK) {
                ip = textBox.TextBox1.Text;
            }
        }
        #endregion
       
        private TextBox txtResult = new TextBox();
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel7;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel6;
        private System.Windows.Forms.Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private ProgressBar progressBar1;
        private SplitContainer splitContainer1;
        private Label label10;
        private Label label9;
        private Label label6;
        private Label label8;
        private Label label7;
        private Label label5;
        private Button button12;
        private Button button13;
        private PerformanceCounter cpuCounter;
        private PerformanceCounter ramCounter;
        private Label label11;
        private ProgressBar progressBar2;
    }
    public class Form2 : Form
    {
        public TextBox TextBox1;
        private Button Button1;
        private Label label1;
        public Form2()
        {
            this.TextBox1 = new TextBox();
            this.Size = new Size(50,125);
            this.Button1 = new Button();
            this.label1 = new Label();
            this.label1.TextAlign = ContentAlignment.MiddleCenter;
            this.label1.Location = new Point(0,0);
            this.label1.Text = "IP Address";
            this.Controls.Add(label1);
            this.Text = "IP Address";
            this.TextBox1.Location = new Point(0, 25);
            this.Controls.Add(TextBox1);
            this.Button1.Text = "Ok";
            this.Button1.Location = new Point(12, 50);
            this.Button1.Click += new EventHandler(button_click);
            this.Controls.Add(Button1);
            
        }
        public void button_click(object o, EventArgs e) {
            Form1.ip = TextBox1.Text;
            this.Close();
        }
    }
}
