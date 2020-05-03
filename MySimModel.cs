using Microsoft.Maps.MapControl.WPF;
using FlightSimulatorApp.Model;
using FlightSimulatorApp.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;

namespace FlightSimulatorApp
{
    public class MySimModel : BaseNotify
    {
        //ITelnetClient tel;
        //bool startAppModel = false;
        int counterRead = 0;
        string err;
        string indicatedHeadingDeg, gpsIndicatedVerticalSpeed, gpsIndicatedGroundSpeedKt, airspeedIndicatorIndicatedSpeedKt,
               gpsIndicatedAltitudeFt, attitudeIndicatorInternalRollDeg, attitudeIndicatorInternalPitchDeg, altimeterIndicatedAltitudeFt;
        Thread thread;
        double latitude = 50, longitude = 10;

        volatile Boolean stop;

        bool isConnect;
        Queue<string> update = new Queue<string>();
        ITelnetClient telnetClient;
        public bool validateIp;
        public bool validatePort;
        public MySimModel(ITelnetClient telnetClient)
        {
            this.telnetClient = telnetClient;
            stop = false;
            isConnect = false;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void UpdateTelnet(string ip, string port)
        {
            try
            {
                int temp = int.Parse(port);
                validateIp = ValidateIPv4(ip);
                validatePort = ValidatePort(int.Parse(port));
                if (validatePort && validateIp)
                {

                    Err = "Status: Connect";
                    telnetClient.setApp(ip, int.Parse(port));
                }
            }
            catch
            {
                Err = "Status: ip or port incorrect please try again";
                MyConnect connect = new MyConnect(this);
                Err = "Status: ip or port incorrect please try again";
                connect.Show();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        private bool ValidateIPv4(string ip)
        {
            // Checking we didnt got an empty string.
            if (string.IsNullOrEmpty(ip))
            {
                return false;
            }
            else
            {
                // Checking we have four segments.
                string[] split = ip.Split('.');
                if (split.Length != 4)
                {
                    return false;
                }
                else
                {
                    // Checking each is valid
                    byte temp;
                    foreach (string s in split)
                    {
                        if (!byte.TryParse(s, out temp))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        private bool ValidatePort(int port)
        {
            return (port > 0);
        }

        public void Connect()
        {
            if (validatePort == false && validateIp == false)
            {
                this.stop = true;
                telnetClient.disconnect();
            }
            try
            {
                isConnect = true;
                telnetClient.connect();
                this.telnetClient.setTimeOutRead(10000);
                this.Start();
            }
            catch
            {
                isConnect = false;
                Err = "server disconnect";
                MyConnect connect = new MyConnect(this);

            }
        }
        public void Disconnect()
        {
            if (isConnect)
            {
                //startAppModel = true;
                telnetClient.Close();
                isConnect = false;
                stop = true;
                Err = "Status: Disconnect";
                MyConnect connect = new MyConnect(this);


            }
            Err = "Status: Disconnect";
        }
        public void Start()
        {
            this.thread = new Thread(delegate ()
            {
                String msg;
                while (!stop)
                {

                    try
                    {
                        //indicate time out
                        if (counterRead == 1)
                        {
                            try
                            {
                                telnetClient.read();
                                counterRead++;
                                Err = "Status: Connect";
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        //Err = "Status: Connect";
                        // 1
                        telnetClient.write("get /instrumentation/heading-indicator/indicated-heading-deg\n");
                        msg = telnetClient.read();

                        IndicatedHeadingDeg = msg;

                        // 2
                        telnetClient.write("get /instrumentation/gps/indicated-vertical-speed\n");
                        msg = telnetClient.read();


                        GpsIndicatedVerticalSpeed = msg;


                        // 3
                        telnetClient.write("get /instrumentation/gps/indicated-ground-speed-kt\n");
                        msg = telnetClient.read();

                        GpsIndicatedGroundSpeedKt = msg;


                        // 4
                        telnetClient.write("get /instrumentation/airspeed-indicator/indicated-speed-kt\n");
                        msg = telnetClient.read();

                        AirspeedIndicatorIndicatedSpeedKt = msg;

                        // 5
                        telnetClient.write("get /instrumentation/gps/indicated-altitude-ft\n");
                        msg = telnetClient.read();

                        GpsIndicatedAltitudeFt = msg;

                        // 6
                        telnetClient.write("get /instrumentation/attitude-indicator/internal-roll-deg\n");
                        msg = telnetClient.read();

                        AttitudeIndicatorInternalRollDeg = msg;

                        // 7
                        telnetClient.write("get /instrumentation/attitude-indicator/internal-pitch-deg\n");
                        msg = telnetClient.read();

                        AttitudeIndicatorInternalPitchDeg = msg;

                        // 8
                        telnetClient.write("get /instrumentation/altimeter/indicated-altitude-ft\n");
                        msg = telnetClient.read();

                        AltimeterIndicatedAltitudeFt = msg;

                        // longitude
                        telnetClient.write("get /position/longitude-deg\n");
                        msg = telnetClient.read();
                        if (!msg.Contains("ERR"))
                        {
                            Longitude = Math.Round(Double.Parse(msg), 5);
                            Err = "Status: Connect";
                        }
                        else
                        {
                            Err = "Status: Err longitude";
                        }

                        // latitude
                        telnetClient.write("get /position/latitude-deg\n");
                        msg = telnetClient.read();
                        if (!msg.Contains("ERR"))
                        {
                            Latitude = Math.Round(Double.Parse(msg), 5);
                            Err = "Status: Connect";
                        }
                        else
                        {
                            Err = "Status: Err latitude";
                        }
                        while (this.update.Count != 0)
                        {
                            string s = "set " + update.Dequeue() + "\n";
                            telnetClient.write(s);
                            string m = telnetClient.read();

                        }
                        Thread.Sleep(250);// read the data 
                    }

                    catch (SocketException)
                    {
                        //stop = true;
                        //Console.WriteLine("read timeout");
                        //this.disconnect();
                        Application.Current.Dispatcher.Invoke((Action)delegate
                        {
                            Err = "Status: Disconnect";
                            telnetClient.Close();

                            MyConnect connect = new MyConnect(this);
                        });
                    }

                    catch (IOException e)
                    {
                        if (e.Message.Contains("time"))
                        {
                            Err = "Status: Received Timeout from the server. You can either wait or click Disconnect (and then click Fly)";
                            counterRead = 1;
                            continue;
                        }
                        else
                        {
                            Err = "Status:Encountered a problem with connecting to the server, please click Disconnect";
                            Application.Current.Dispatcher.Invoke((Action)delegate
                            {

                                telnetClient.Close();

                                MyConnect connect = new MyConnect(this);
                            });
                        }

                    }

                    catch
                    {
                        this.telnetClient.Flush();
                        continue;
                    }
                }

            });
            thread.Start();
        }

        // 8 properties that get from the simulator
        public string IndicatedHeadingDeg
        {
            get { return this.indicatedHeadingDeg; }
            set
            {
                this.indicatedHeadingDeg = value;
                this.NotifyPropertyChanged("IndicatedHeadingDeg");
            }
        }
        public string GpsIndicatedVerticalSpeed
        {
            get { return this.gpsIndicatedVerticalSpeed; }
            set
            {
                this.gpsIndicatedVerticalSpeed = value;
                this.NotifyPropertyChanged("GpsIndicatedVerticalSpeed");
            }
        }
        public string GpsIndicatedGroundSpeedKt
        {
            get { return this.gpsIndicatedGroundSpeedKt; }
            set
            {
                this.gpsIndicatedGroundSpeedKt = value;
                this.NotifyPropertyChanged("GpsIndicatedGroundSpeedKt");
            }
        }
        public string AirspeedIndicatorIndicatedSpeedKt
        {
            get { return this.airspeedIndicatorIndicatedSpeedKt; }
            set
            {
                this.airspeedIndicatorIndicatedSpeedKt = value;
                this.NotifyPropertyChanged("AirspeedIndicatorIndicatedSpeedKt");
            }
        }
        public string GpsIndicatedAltitudeFt
        {
            get { return this.gpsIndicatedAltitudeFt; }
            set
            {
                this.gpsIndicatedAltitudeFt = value;
                this.NotifyPropertyChanged("GpsIndicatedAltitudeFt");
            }
        }
        public string AttitudeIndicatorInternalRollDeg
        {
            get { return this.attitudeIndicatorInternalRollDeg; }
            set
            {
                this.attitudeIndicatorInternalRollDeg = value;
                this.NotifyPropertyChanged("AttitudeIndicatorInternalRollDeg");
            }
        }
        public string AttitudeIndicatorInternalPitchDeg
        {
            get { return this.attitudeIndicatorInternalPitchDeg; }
            set
            {
                this.attitudeIndicatorInternalPitchDeg = value;
                this.NotifyPropertyChanged("AttitudeIndicatorInternalPitchDeg");
            }
        }
        public string AltimeterIndicatedAltitudeFt
        {
            get { return this.altimeterIndicatedAltitudeFt; }
            set
            {
                this.altimeterIndicatedAltitudeFt = value;
                this.NotifyPropertyChanged("AltimeterIndicatedAltitudeFt");
            }
        }

        public double Longitude
        {
            get { return this.longitude; }
            set
            {

                if (value > 180)
                {
                    this.longitude = 180;
                }
                else if (value < -180)
                {
                    this.longitude = -180;
                }
                else
                {
                    this.longitude = value;
                }
                this.NotifyPropertyChanged("Location");

                this.NotifyPropertyChanged("LongitudeT");
            }
        }
        public double Latitude
        {
            get { return this.latitude; }
            set
            {
                if (value > 90)
                {
                    this.latitude = 90;
                }
                else if (value < -90)
                {
                    this.latitude = -90;
                }
                else
                {
                    this.latitude = value;
                }
                
                this.NotifyPropertyChanged("Location");

                this.NotifyPropertyChanged("LatitudeT");
            }
        }


        public Location Location
        {
            get
            {
                return new Location(latitude, longitude);
            }
        }

        #region JoystickProperties
        public double throttle;
        public double Throttle
        {
            set
            {
                // check if in the range
                if (value > 1)
                {
                    this.throttle = 1;
                }
                else if (value < 0)
                {
                    this.throttle = 0;
                }
                else
                {
                    this.throttle = value;
                }
                
                this.update.Enqueue("/controls/engines/current-engine/throttle  " + this.throttle);
                this.NotifyPropertyChanged("Throttle");
            }
            get
            {
                return throttle;
            }
        }

        public double rudder;
        public double Rudder
        {
            set
            {
                // check if in the range
                if (value > 1)
                {
                    this.rudder = 1;
                    
                }
                else if (value <-1)
                {
                    this.rudder = -1;
                    
                }
                else
                {
                    this.rudder = value;
                    
                }
               
                this.update.Enqueue("/controls/flight/rudder " + this.rudder);
                this.NotifyPropertyChanged("Rudder");
            }
            get
            {
                return rudder;
            }
        }

        public double aileron;
        public double Aileron
        {
            set
            {
                if (value > 1)
                {
                    this.aileron = 1;
                }
                else if (value < -1)
                {
                    this.aileron = -1;
                }
                else
                {
                    this.aileron = value;
                }

                this.update.Enqueue("/controls/flight/aileron " + this.aileron);
                this.NotifyPropertyChanged("Aileron");
            }
            get
            {
                return aileron;
            }
        }

        public double elevator;
        //private bool firstShow;

        public double Elevator
        {
            set
            {
                // check if in the range
                if (value > 1)
                {
                    this.elevator = 1;
                }
                else if (value < -1)
                {
                    this.elevator = -1;
                }
                else
                {
                    this.elevator = value;
                }
                
                this.update.Enqueue("/controls/flight/elevator " + this.elevator);
                this.NotifyPropertyChanged("Elevator");
            }
            get
            {
                return elevator;
            }
        }
        #endregion

        public string Err
        {
            get { return this.err; }
            set
            {
                this.err = value;
                this.NotifyPropertyChanged("Err");
            }
        }

    }
}