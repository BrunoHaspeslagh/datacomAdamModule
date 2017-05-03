using Advantech.Adam;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace WPFAdam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AdamSocket Socket { get; set; }
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        DispatcherTimer rotateVentilatorTimer = new DispatcherTimer();
        int ventilatorDraaistatus;
        Color colorLightOn = Color.FromRgb(255, 0, 0);
        Color colorLightOff = Color.FromRgb(0, 0, 0);
        bool[] statusled = new bool[5];
        public Modbus modbus { get; set; }
        public bool bl { get; set; } = true;
        public bool fan { get; set; } = false;
        public bool AlarmOn { get; set; } = false;
        public InputListener inputs { get; set; }
        public email mailListener { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0,0,0,0,500);
            rotateVentilatorTimer.Tick += new EventHandler(rotateVentilatorTimer_Tick);
            rotateVentilatorTimer.Interval = new TimeSpan(0, 0, 0, 0, 25);
            Socket = new AdamSocket();
            Socket.Connect("192.168.1.2", ProtocolType.Tcp, 502);
            modbus = new Modbus(Socket);
            inputs = new InputListener(modbus);
            inputs.btnPressed += Inputs_btnPressed;
            inputs.redSwitched += Inputs_redSwitched;
            mailListener = new email();
            mailListener.mailReceived += MailListener_mailReceived;
            this.Closing += MainWindow_Closing;
        }

        private void MailListener_mailReceived(string content)
        {
            Console.WriteLine(content);
            if (content.Contains("licht uit")) {

            }
            if(content.Contains("licht aan"))
            {

            }
            if(content.Contains("verwarming aan"))
            {

            }
            if(content.Contains("verwarmoing uit"))
            {

            }
            if(content.Contains("alarm aan"))
            {

            }
            if(content.Contains("alarm uit"))
            {

            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ToggleLed(1);
            ToggleLed(2);
            ToggleLed(3);
            ToggleLed(4);
        }

        private void ToggleLed(int led)
        {
            statusled[led] = !statusled[led];
            SetLights();
        }
        private void changeStatusOfAllLedsTo(bool v)
        {
            int teller = 0;
            foreach (bool a in statusled)
            {
                statusled[teller] = true;
                teller++;
            }
            SetLights();
        }
        private void BtnAlarmTest_Click(object sender, RoutedEventArgs e)
        {
            changeStatusOfAllLedsTo(true);

            if (dispatcherTimer.IsEnabled)
            {
                dispatcherTimer.Stop();
            }
            else
            {
                dispatcherTimer.Start();
            }
        }
        private void StartStopVentilator()
        {
            if (rotateVentilatorTimer.IsEnabled)
            {
                rotateVentilatorTimer.Stop();
                modbus.ForceSingleCoil(17, false);
            }
            else
            {
                rotateVentilatorTimer.Start();
                modbus.ForceSingleCoil(17, true);
            }
        }
        private void SetLights()
        {
            if (statusled[4])
            {
                EllLed4.Fill = new SolidColorBrush(colorLightOn);
                modbus.ForceSingleCoil(18, true);
            }
            else
            {
                EllLed4.Fill = new SolidColorBrush(colorLightOff);
                modbus.ForceSingleCoil(18, false);
            }

            if (statusled[3])
            {
                EllLed3.Fill = new SolidColorBrush(colorLightOn);
                modbus.ForceSingleCoil(19, true);
            }
            else
            {
                EllLed3.Fill = new SolidColorBrush(colorLightOff);
                modbus.ForceSingleCoil(19, false);
            }

            if (statusled[2])
            {
                EllLed2.Fill = new SolidColorBrush(colorLightOn);
                modbus.ForceSingleCoil(20, true);
            }
            else
            {
                EllLed2.Fill = new SolidColorBrush(colorLightOff);
                modbus.ForceSingleCoil(20, false);
            }

            if (statusled[1])
            {
                EllLed1.Fill = new SolidColorBrush(colorLightOn);
                modbus.ForceSingleCoil(21, true);
            }
            else
            {
                EllLed1.Fill = new SolidColorBrush(colorLightOff);
                modbus.ForceSingleCoil(21, false);
            }
        }


        private void rotateVentilatorTimer_Tick(object sender, EventArgs e)
        {
            ventilatorDraaistatus += 5;
            RotateTransform rotateTransform = new RotateTransform(ventilatorDraaistatus);
            imgVentilator.RenderTransform = rotateTransform;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            modbus.ForceSingleCoil(17, false);
            Socket.Disconnect();
        }

        private void Inputs_redSwitched(bool isOn)
        {
            if(isOn)
            {
                Console.WriteLine("ON");

            }
            else Console.WriteLine("OFF");
            AlarmOn = isOn;
        }

        private void Inputs_btnPressed(int nr)
        {
            if(nr == 1)
            {
                Console.WriteLine("Green!!");
                StartStopVentilator();
            }
            if(nr == 2)
            {
                Console.WriteLine("Black!!");
            }
        }

       
        private void btn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnLed1_Click(object sender, RoutedEventArgs e)
        {
            ToggleLed(1);
        }
        private void btnLed2_Click(object sender, RoutedEventArgs e)
        {
            ToggleLed(2);
        }

        private void btnLed3_Click(object sender, RoutedEventArgs e)
        {
            ToggleLed(3);
        }

        private void btnLed4_Click(object sender, RoutedEventArgs e)
        {
            ToggleLed(4);
        }

        private void EllLed1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ToggleLed(1);
        }

        private void EllLed2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ToggleLed(2);
        }

        private void EllLed3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ToggleLed(3);
        }

        private void EllLed4_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ToggleLed(4);
        }

        private void btnVentilator_Click(object sender, RoutedEventArgs e)
        {
            StartStopVentilator();
        }
    }
}
