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
        public static void Main(string[] args) { new MainClass(); }
        public override void robotInit()
        {
            Console.WriteLine("robot enabled");
        }
        public override void teleopPeriodic()
        {
            myDecoder1 = new Decoder(this.data, 0);
            myDecoder2 = new Decoder(this.data, 1);
            mySpark1 = new Spark(0);
            mySpark2 = new Spark(1);
            bool printleft = false;
            bool printright = false;
            if (myDecoder1.getRawButtons(3))
            {
                printleft = true;
            }
            else if (myDecoder1.getRawButtons(4))
            {
                printleft = false;
            }
            if (myDecoder2.getRawButtons(3))
            {
                printright = true;
            }
            else if (myDecoder2.getRawButtons(4))
            {
                printright = false;
            }
            if (printleft)
            {
                Console.WriteLine(myDecoder1.getY());
            }
            if (printright)
            {
                Console.WriteLine(myDecoder2.getY());
            }
            mySpark1.set(myDecoder1.getY());
            mySpark2.set(myDecoder2.getY());
            if (myDecoder2.getRawButtons(0))
            {
                Console.WriteLine("Trigger Pressed");
            }
            if (myDecoder2.getRawButtons(1))
            {
                Console.WriteLine("button 1 pressed");
            }

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
        private String portname;
        private const int baudrate = 9600;
        public int PWMNumber;
        private String[] portnames;
        private SerialPort port1;
        public Spark(int number)
        {
            PWMNumber = number;
            if (SerialPort.GetPortNames().Count() > 0)
            {
                if (SerialPort.GetPortNames().Count() == 1)
                {
                    foreach (String p in SerialPort.GetPortNames())
                    {
                        portname = p;
                        setPorts();
                    }
                }
                else
                {
                    /*portnames = SerialPort.GetPortNames();
                    foreach (String p in portnames)
                    {
                        Console.WriteLine(p);
                        portname = Console.ReadLine();
                        setPorts();
                    }*/
                    setPorts();
                }
            }
        }
        private void setPorts()
        {
            port1 = new SerialPort(portname, baudrate);
        }
        public void set(double value)
        {
            value += 1;
            value /= 2;
            value *= 190;
            if (value > 180)
            {
                value = 180;
            }
            port1.writeLine(PWMNumber + "|" + value);
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
                        Console.WriteLine("Text received : {0}", mydata);
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
        public IterativeRobot()
        {
            ThreadStart start = new ThreadStart(TCPServer);
            Thread t = new Thread(start);
            t.Start();
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
                        robotInit();
                    }
                    else if (header != lastheader && (header == 4 || header == 6))
                    {
                        robotDisabled();
                    }
                    else if (header != lastheader && (header == 1))
                    {
                        autonomousInit();
                    }
                    else if (header != lastheader && (header == 2))
                    {
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
            new Spark(0).set(0);
            new Spark(1).set(0);
            new Spark(2).set(0);
            new Spark(3).set(0);
            Console.WriteLine("robot disabled");
        }
        public virtual void autonomousPeriodic()
        {

        }
        public virtual void autonomousDisabled()
        {
            new Spark(0).set(0);
            new Spark(1).set(0);
            new Spark(2).set(0);
            new Spark(3).set(0);
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
