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

namespace WPFAdam
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AdamSocket Socket { get; set; }
        public Modbus modbus { get; set; }
        public bool bl { get; set; } = true;
        public bool fan { get; set; } = false;
        public bool AlarmOn { get; set; } = false;
        public InputListener inputs { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Socket = new AdamSocket();
            Socket.Connect("192.168.1.2", ProtocolType.Tcp, 502);
            modbus = new Modbus(Socket);
            inputs = new InputListener(modbus);
            inputs.btnPressed += Inputs_btnPressed;
            inputs.redSwitched += Inputs_redSwitched;
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            while (!modbus.ForceSingleCoil(17, false));
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
                fan = !fan;

                while(!modbus.ForceSingleCoil(17, fan));
            }
            if(nr == 2)
            {
                Console.WriteLine("Black!!");
            }
        }

       
        private void btn_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
