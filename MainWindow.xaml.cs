using FlightSimulatorApp;
using FlightSimulatorApp.Model;
using FlightSimulatorApp.ViewModel;
using FlightSimulatorApp.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace FlightSimulatorApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int counterFirstShow = 0;
       // volatile bool firstShow = true;
        //private string messageErr = "";
        public MySimModel model;
        MyDashboardViewModel dashboard;
        MyMapViewModel map;
        MyJoystickViewModel joystick;
        bool startApp = true;
        MyConnect sw1;
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            if (startApp)
            {

                model = new MySimModel(new Telnet("127.0.0.1", 5402));
                err.DataContext = model;
                dashboard = new MyDashboardViewModel(model);
                map = new MyMapViewModel(model);
                joystick = new MyJoystickViewModel(model);
                myMap.DataContext = map;
                myDashboard.DataContext = dashboard;
                myJoystick.DataContext = joystick;
                sw1 = new MyConnect(model);
                model.PropertyChanged += delegate (Object sender, PropertyChangedEventArgs e)
                {

                    NotifyPropertyChanged("VM_" + e.PropertyName);

                };

            }
        }
        public string VM_Err
        {
            get { return model.Err; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }


        private void b1_Click(object sender, RoutedEventArgs e)
        {

            if (counterFirstShow == 0)
            {
                sw1.Show();
                counterFirstShow++;
            }
            else
            {
                model = new MySimModel(new Telnet("127.0.0.1", 5402));
                err.DataContext = model;
                dashboard = new MyDashboardViewModel(model);
                map = new MyMapViewModel(model);
                joystick = new MyJoystickViewModel(model);
                myMap.DataContext = map;
                myDashboard.DataContext = dashboard;
                myJoystick.DataContext = joystick;
                MyConnect sw2 = new MyConnect(model);
                sw2.Show();
            }

        }


        private void myMap_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void b2_Click(object sender, RoutedEventArgs e)
        {
            if (startApp == false)
            {
                this.Close();
            }
            else
            {
                model.Disconnect();
                startApp = true;
                //firstShow = false;
            }

        }

    }

}

