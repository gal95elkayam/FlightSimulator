using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FlightSimulatorApp
{
    public interface ITelnetClient
    {
        void connect();
        void write(string command);
        string read(); // blocking call 
        void disconnect();
        void setTimeOutRead(int time);
        void Flush();
        void setApp(string ip, int port);
        void Close();
    }
}
