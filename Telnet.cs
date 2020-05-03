using FlightSimulatorApp.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;


namespace FlightSimulatorApp
{
    public class Telnet : ITelnetClient
    {
        //TcpClient tcpclntNew;
       // bool tcpNew;
        TcpClient tcpclnt;
        NetworkStream stream;
        private string ip;
        private int port;
        //private NetworkStream stream;
        private BinaryReader reader;

        public void setApp(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public Telnet(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }
        public void connect()
        {

            this.tcpclnt = new TcpClient();
            try
            {
                // try connect the server
                tcpclnt.Connect(ip, port);
                Console.WriteLine("CONNECTED");
                stream = tcpclnt.GetStream();
            }
            catch (Exception)
            {
                Exception e1 = new Exception("not connected");
                throw e1;
            }
        }
        public void disconnect()
        {
            tcpclnt.Close();
        }
        public string read()
        {

            reader = new BinaryReader(tcpclnt.GetStream());
            string message = ""; // input will be stored here
            char s;
            while ((s = reader.ReadChar()) != '\n') message += s;
            return message;
        }
        public void write(string command)
        {

            stream = this.tcpclnt.GetStream();
            byte[] send = Encoding.ASCII.GetBytes(command.ToString());
            stream.Write(send, 0, send.Length);

        }
        public void setTimeOutRead(int time)
        {
            this.tcpclnt.ReceiveTimeout = time;
        }
        public void Flush()
        {
            this.stream.Flush();
        }

        public void Close()
        {
            this.stream.Close();
            this.tcpclnt.Dispose();
            this.tcpclnt.Close();

        }
    }
}