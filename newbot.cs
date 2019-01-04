using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace newbot
{
    class MainClass : IterativeRobot
    {
        Decoder myDecoder1;
        Decoder myDecoder2;
        Spark mySpark1;
        Spark mySpark2;
        Spark mySpark3;
        Spark mySpark4;
        double lasty = 0;
        public bool init = false;
        double lasty2  = 0;
        int y = 32767;
        int y2 = 32767;
        public static void Main(string[] args) { new MainClass(); }
        public override void robotstart()
        {
            mySpark1 = new Spark(0, portname);
            mySpark2 = new Spark(1, portname);
            mySpark3 = new Spark(2, portname);
            mySpark4 = new Spark(3, portname);
        }
        public override void robotInit()
        {
            Console.WriteLine("robot enabled");
        }
        public override void teleopPeriodic()
        {
            myDecoder1 = new Decoder(this.data, 0);
            myDecoder2 = new Decoder(this.data, 1);
            if(myDecoder1.getY() != lasty) {
                mySpark1.set(myDecoder1.getY());
                mySpark2.set(myDecoder1.getY());
                lasty = myDecoder1.getY();
            }
            if(myDecoder3.getY() != lasty2) {
                mySpark3.set(myDecoder2.getY());
                mySpark4.set(myDecoder2.getY());
                lasty2 = myDecoder2.getY();
            }
            //Console.WriteLine("teleop periodic");
        }
        public override void robotDisabled()
        {
                mySpark1.set(0);
                mySpark2.set(0);
            Console.WriteLine("robot disabled");
        }
    }
    public class Decoder
    {
        bool[] buttons = new bool[12];
        bool[] buttons2 = new bool[12];
        int[] x = new int[2];
        int[] y = new int[2];
        int header;
        int packetnumber;
        int lastpacketnumber;
        int stick;
        public Decoder(byte[] bytes, int joysticknumber)
        {
            stick = joysticknumber;
            refresh(bytes);
        }
        public double getX()
        {
            double xout = (double)x[stick];
            xout -= 32767;
            xout /= 32767;
            xout *= -1;
            xout = Math.Truncate(xout * 100) / 100;
            return xout;
        }
        public double getY()
        {
            double yout = (double)y[stick];
            yout -= 32767;
            yout /= 32767;
            yout *= -1;
            yout = Math.Truncate(yout * 100) / 100;
            return yout;
        }
        public int getPureX()
        {
            return x[stick];
        }
        public int getPureY()
        {
            return y[stick];
        }
        public int[] getbuttons()
        {
            int[] btnarray = new int[12];
            int intindex = 0;
            if (stick == 0)
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i])
                    {
                        btnarray[intindex] = i;
                        intindex++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < buttons2.Length; i++)
                {
                    if (buttons2[i])
                    {
                        btnarray[intindex] = i;
                        intindex++;
                    }
                }
            }
            return btnarray;
        }
        public bool getRawButtons(int button)
        {
            if (stick == 1 && buttons2[button])
            {
                return true;
            }
            else if (stick == 0 && buttons[button])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void refresh(byte[] bytes)
        {
            header = (int)bytes[0];
            packetnumber = (int)bytes[1];
            bool great = false;
            if ((lastpacketnumber - 5000) > packetnumber)
            {
                great = true;
                lastpacketnumber = 0;
            }
            if (packetnumber > lastpacketnumber || great)
            {
                lastpacketnumber = packetnumber;
                if (header == 3)
                {
                    x[0] = (int)bytes[2] | ((int)bytes[3] << 8);
                    y[0] = (int)bytes[4] | ((int)bytes[5] << 8);
                    for (int r = 0; r < buttons.Length; r++)
                    {
                        buttons[r] = 0 != (bytes[6 + r / 8] & (byte)(1 << (r % 8)));
                    }
                }
                else if (header == 5)
                {
                    x[0] = (int)bytes[2] | ((int)bytes[3] << 8);
                    y[0] = (int)bytes[4] | ((int)bytes[5] << 8);
                    byte[] bytes1 = new byte[8];
                    byte[] bytes2 = new byte[8];
                    int secondval = 0;
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        if (i < 8)
                        {
                            bytes1[i] = bytes[i];
                        }
                        else if (i > 7 && i <= 16)
                        {
                            bytes2[secondval] = bytes[i];
                            secondval = secondval + 1;
                        }
                    }
                    x[1] = (int)bytes2[2] | ((int)bytes2[3] << 8);
                    y[1] = (int)bytes2[4] | ((int)bytes2[5] << 8);
                    for (int r = 0; r < buttons.Length; r++)
                    {
                        buttons[r] = 0 != (bytes1[6 + r / 8] & (byte)(1 << (r % 8)));
                    }
                    for (int t = 0; t < buttons2.Length; t++)
                    {
                        buttons2[t] = 0 != (bytes2[6 + t / 8] & (byte)(1 << (t % 8)));
                    }
                }
            }
        }
        public String getMode()
        {
            if (header == 0)
            {
                return "off";
            }
            else if (header == 1)
            {
                return "autonomous start";
            }
            else if (header == 2)
            {
                return "end auto";
            }
            else if (header == 3)
            {
                return "arcade teleoperated enabled";
            }
            else if (header == 4)
            {
                return "arcade teleoperated disabled";
            }
            else if (header == 5)
            {
                return "tank teleoperated enabled";
            }
            else
            {
                return "tank teleoperated disabled";
            }
        }
    }
    public class Spark
    {
        private const int baudrate = 9600;
        public int PWMNumber;
        private bool enabled = true;
        public String portname;
        private SerialPort port1;
        public Spark(int number, String port)
        {
            PWMNumber = number;
            port1 = new SerialPort(port, baudrate);
            port1.Open();
            port1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }
        public void close() {
            port1.Close();
            port1 = null;
        }
        public void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("testing something");
            Console.Write(port1.ReadExisting());
        }
        public void set(double value)
        {
            if (enabled)
            {
                value *= 10;
                value = Math.Pow(value, 3);
                value = (value / (1000 / 95));
                value += 95;
                /*if (value > 180)
                {
                    value = 180;
                }*/
                port1.WriteLine(PWMNumber + "|" + value);
                Console.WriteLine(port1.ReadLine());
            }
        }
        public void setEnabled(bool state) {
            if (state) {
                port1.WriteLine("e|95");
                enabled = true;
            }
            else {
                port1.WriteLine("d|95");
                enabled = false;
            }
        }
        public bool isEnabled() {
            return enabled;
        }
        public int get()
        {
            return PWMNumber;
        }
    }
    public class IterativeRobot
    {
        public byte[] data = new byte[16];
        private const int listenPort = 36361;
        private const int TCPPort = 36362;
        private class board{
            private String portname;
            private String[] portnames;
            private SerialPort port;
            public board() {
                if (SerialPort.GetPortNames().Count() > 0)
                {
                    if (SerialPort.GetPortNames().Count() == 1)
                    {
                        foreach (String p in SerialPort.GetPortNames())
                        {
                            portname = p;
                            setPort();
                        }
                    }
                    else
                    {
                        portnames = SerialPort.GetPortNames();
                        foreach (String p in portnames)
                        {
                            Console.WriteLine(p);
                        }
                        portname = Console.ReadLine();
                        setPort();
                    }
                }
            }
            public String getPortName() {
                return portname;
            }
            public void close() {
                port.WriteLine("d|95");
            }
            public void setPort() {
                port = new SerialPort(portname, 9600);
                port.Open();
            }
            public void open() {
                port.WriteLine("e|95");
            }
        }
        public virtual void robotstart() {
            
        }
        public void TCPServer()
        {
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.  
            // Dns.GetHostName returns the name of the   
            // host running the application.  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 36362);
            int time = 0;
            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and   
            // listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.  
                bool close = false;
                bool connection = false;
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = listener.Accept();
                    mydata = null;

                    // An incoming connection needs to be processed.  
                    while (!close)
                    {
                        int bytesRec = handler.Receive(bytes);
                        connection = true;
                        mydata = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        //Console.WriteLine("Text received : {0}", mydata);
                        byte[] msg = Encoding.ASCII.GetBytes(mydata);

                        handler.Send(msg);
                        if (mydata == "close")
                        {
                            break;
                        }
                        time = time + 1;
                        if (time > 25 && connection == true)
                        {
                            try
                            {
                                if (mydata == null)
                                {
                                    close = true;
                                }
                                else
                                {
                                    close = false;
                                }
                            }
                            catch (Exception e)
                            {
                                close = true;
                                Console.WriteLine("Connection null invoid discontinuing program");
                                break;
                            }
                            time = 0;
                        }
                        Thread.Sleep(200);
                    }
                    robotDisabled();
                    autonomousDisabled();
                    Console.WriteLine("connection closed");
                    // Echo the data back to the client.  
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
        public static String mydata = null;
        public static String portname;
        private board myboard = new board();
        public IterativeRobot()
        {
            ThreadStart start = new ThreadStart(TCPServer);
            Thread t = new Thread(start);
            t.Start();
            portname = myboard.getPortName();
            robotstart();
            int lastheader = 0;
            Console.WriteLine("waiting for connection...");
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

            try
            {
                while (true)
                {
                    data = listener.Receive(ref groupEP);
                    int header = (int)data[0];
                    if (header != lastheader && (header == 5 || header == 3))
                    {
                        //myboard.open();
                        robotInit();
                    }
                    else if (header != lastheader && (header == 4 || header == 6))
                    {
                        Console.WriteLine("robot inactive");
                       // myboard.close();
                        robotDisabled();
                    }
                    else if (header != lastheader && (header == 1))
                    {
                        //myboard.open();
                        autonomousInit();
                    }
                    else if (header != lastheader && (header == 2))
                    {
                        //myboard.close();
                        autonomousDisabled();
                    }
                    else if (header == lastheader && header == 1)
                    {
                        autonomousPeriodic();
                    }
                    else if (header == lastheader && (header == 3 || header == 5))
                    {
                        teleopPeriodic();
                    }
                    lastheader = header;
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }

        }
        public virtual void autonomousInit()
        {
            Console.WriteLine("autonomous started");
        }
        public virtual void robotDisabled()
        {
            Console.WriteLine("robot disabled");
        }
        public virtual void autonomousPeriodic()
        {

        }
        public virtual void autonomousDisabled()
        {
            Console.WriteLine("autonomous ended");
        }
        public virtual void robotInit()
        {

        }
        public virtual void teleopPeriodic()
        {

        }
    }
}
