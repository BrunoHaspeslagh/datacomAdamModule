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
using WPFAdam;
using WPFAdam.models;

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
        public User LoggedInUser;
        public List<User> Users;
        bool[] statusled = new bool[5];
        public Modbus modbus { get; set; }
        public bool bl { get; set; } = true;
        public bool fan { get; set; } = false;
        public bool AlarmOn { get; set; } = false;
        public InputListener inputs { get; set; }
        public email mailListener { get; set; }
        public eid eidlistener { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Users = User.LoadUsers();
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
            eidlistener = new eid();
            eidlistener.CardInserted += Eidlistener_CardInserted;
            this.Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            lstUsers.ItemsSource = Users;
            bool a = false;
            foreach(User u in Users)
            {
                if (u.IsAdmin)
                {
                    a = true;
                }
            }
            if (!a)
            {
                VraagVoorAdmin();
            }
            else
            {
                eidlistener.StartListening();
            }
            CheckLoggedInUser();
        }
        public void VraagVoorAdmin()
        {

            AddAdmin addadmin = new AddAdmin(eidlistener);

            if(addadmin.DialogResult == true)
            {
                User admin = addadmin.Admin;
                User usr = null;
                foreach(User u in Users)
                {
                    if(u.RijksRegister == admin.RijksRegister)
                    {
                        usr = u;
                    }
                }
                if(usr != null)
                {
                    usr.IsAdmin = true;

                }
                else
                {
                    Users.Add(admin);
                }
                LoggedInUser = admin;
                CheckLoggedInUser();
            }

            eidlistener.StartListening();
        }
        private void Eidlistener_CardInserted(string naam, string vnaam, string rijk)
        {
            foreach(User u in Users)
            {
                if(u.RijksRegister == rijk)
                {
                    LoggedInUser = u;
                    CheckLoggedInUser();
                    return;
                }
            }
            if(LoggedInUser == null || (LoggedInUser != null && LoggedInUser.RijksRegister != rijk))
            {
                if (MessageBox.Show("new user detected: " + vnaam + " " + naam + ". /nDo you want to add this user to the system?", "Add user?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    User u = new User(vnaam, naam, rijk);
                    Users.Add(u);
                    LoggedInUser = u;
                }
            }
            
            CheckLoggedInUser();
        }
        private void MailListener_mailReceived(string content)
        {
            Console.WriteLine(content);
            if (content.Contains("licht uit")) {
                changeStatusOfAllLedsTo(false);
            }
            if(content.Contains("licht aan"))
            {
                changeStatusOfAllLedsTo(true);
            }
            if(content.Contains("verwarming aan"))
            {
                if (!rotateVentilatorTimer.IsEnabled)
                {
                    StartStopVentilator();
                }
            }
            if(content.Contains("verwarmoing uit"))
            {
                if (rotateVentilatorTimer.IsEnabled)
                {
                    StartStopVentilator();
                }
            }
            if(content.Contains("alarm aan"))
            {
                AlarmOn = true;
                UpdateAlarmbox();
            }
            if(content.Contains("alarm uit"))
            {
                AlarmOn = false;
                UpdateAlarmbox();
                LuidAlarm();
            }
            if (content.Contains("status"))
            {
                string msg = "alarm: " + AlarmOn + "\n";
                int index = 1;
                foreach(bool b in statusled)
                {
                    msg += "Led" + index + ": " + b + "\n";
                    index++;
                }
                msg += "Verwarming: " + rotateVentilatorTimer.IsEnabled;
                mailListener.verzend(msg);
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
            LuidAlarm();
        }
        private void LuidAlarm()
        {
            if (AlarmOn)
            {
                if(LoggedInUser == null || !LoggedInUser.MagBinnen())
                {
                    changeStatusOfAllLedsTo(true);

                    if (dispatcherTimer.IsEnabled)
                    {
                        dispatcherTimer.Stop();
                    }
                    else if (AlarmOn)
                    {
                        dispatcherTimer.Start();
                    }
                }

            }

        }
        private void StartStopVentilator()
        {
            if (rotateVentilatorTimer.IsEnabled)
            {
                rotateVentilatorTimer.Stop();
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    modbus.ForceSingleCoil(17, false);

                }).Start();
            }
            else
            {
                rotateVentilatorTimer.Start();
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    modbus.ForceSingleCoil(17, true);

                }).Start();
            }
        }
        private void SetLights()
        {
            if (statusled[4])
            {
                EllLed4.Fill = new SolidColorBrush(colorLightOn);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    modbus.ForceSingleCoil(18, true);

                }).Start();
                
            }
            else
            {
                EllLed4.Fill = new SolidColorBrush(colorLightOff);
                
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    modbus.ForceSingleCoil(18, false);

                }).Start();
            }

            if (statusled[3])
            {
                EllLed3.Fill = new SolidColorBrush(colorLightOn);
                
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    modbus.ForceSingleCoil(19, true);

                }).Start();
            }
            else
            {
                EllLed3.Fill = new SolidColorBrush(colorLightOff);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    modbus.ForceSingleCoil(19, false);

                }).Start();
            }

            if (statusled[2])
            {
                EllLed2.Fill = new SolidColorBrush(colorLightOn);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    modbus.ForceSingleCoil(20, true);

                }).Start();
            }
            else
            {
                EllLed2.Fill = new SolidColorBrush(colorLightOff);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    modbus.ForceSingleCoil(20, false);

                }).Start();
            }

            if (statusled[1])
            {
                EllLed1.Fill = new SolidColorBrush(colorLightOn);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    modbus.ForceSingleCoil(21, true);

                }).Start();
            }
            else
            {
                EllLed1.Fill = new SolidColorBrush(colorLightOff);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    modbus.ForceSingleCoil(21, false);

                }).Start();
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
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                modbus.ForceSingleCoil(17, false);

            }).Start();
            Socket.Disconnect();
        }
        private void Inputs_redSwitched(bool isOn)
        {
            AlarmOn = isOn;
            UpdateAlarmbox();
            if (isOn)
            {
                Console.WriteLine("ON");

            }
            else
            {
                Console.WriteLine("OFF");
                LuidAlarm();
            }
            
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
                LuidAlarm();
                Console.WriteLine("Black!!");
            }
        }
        private void BtnLed1_Click(object sender, RoutedEventArgs e)
        {
            if(LoggedInUser != null)
            {
                ToggleLed(1);
            }
            
        }
        private void btnLed2_Click(object sender, RoutedEventArgs e)
        {
            if (LoggedInUser != null)
            {
                ToggleLed(2);
            }
        }
        private void btnLed3_Click(object sender, RoutedEventArgs e)
        {
            if (LoggedInUser != null)
            {
                ToggleLed(3);
            }
        }
        private void btnLed4_Click(object sender, RoutedEventArgs e)
        {
            if (LoggedInUser != null)
            {
                ToggleLed(4);
            }
        }
        private void EllLed1_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (LoggedInUser != null)
            {
                ToggleLed(1);
            }
        }
        private void EllLed2_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (LoggedInUser != null)
            {
                ToggleLed(2);
            }
        }
        private void EllLed3_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (LoggedInUser != null)
            {
                ToggleLed(3);
            }
        }
        private void EllLed4_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (LoggedInUser != null)
            {
                ToggleLed(4);
            }
        }
        private void btnVentilator_Click(object sender, RoutedEventArgs e)
        {
            if(LoggedInUser!= null)
            {
                StartStopVentilator();
            }
        }
        private void btnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedIndex > -1)
            {
                User u = lstUsers.SelectedItem as User;
                stUsersEdit.Visibility = Visibility.Collapsed;
                stTimeslotEdit.Visibility = Visibility.Visible;
                lstUsers.Visibility = Visibility.Collapsed;
                lstTimeslots.Visibility = Visibility.Visible;

                lstTimeslots.ItemsSource = u.TijdSloten;
                txtSelectedUser.Text = u.ToString();

                User.SaveUsers(Users);
            }
            CheckUserButtons();
        }
        private void btnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (lstUsers.SelectedIndex > -1)
            {
                User u = lstUsers.SelectedItem as User;
                MessageBoxResult messageBoxResult =MessageBox.Show("Are you sure?", "Delete Confirmation",MessageBoxButton.YesNo);
                if(messageBoxResult == MessageBoxResult.Yes)
                {
                    Users.Remove(u);
                    User.SaveUsers(Users);
                }

            }
            CheckUserButtons();
        }
        private void lstUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckUserButtons();
        }
        public void CheckUserButtons()
        {
            if(lstUsers.SelectedIndex> -1)
            {
                btnDeleteUser.IsEnabled = true;
                btnEditUser.IsEnabled = true;
            }
            else
            {
                btnDeleteUser.IsEnabled = false;
                btnEditUser.IsEnabled = false;
            }


            if(lstTimeslots.SelectedIndex > -1)
            {
                btnAddTimeslot.IsEnabled = true;
                btnDeleteTimeslot.IsEnabled = true;
            }
            else
            {
                btnAddTimeslot.IsEnabled = false;
                btnDeleteTimeslot.IsEnabled = false;
            }
        }
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            this.LoggedInUser = null;
            CheckLoggedInUser();
            eidlistener.prevrijk = "";
        }
        public void CheckLoggedInUser()
        {
            CheckUserButtons();
            if(LoggedInUser != null)
            {
                btnLogOut.IsEnabled = true;

                BtnLed1.IsEnabled = true;
                btnLed2.IsEnabled = true;
                btnLed3.IsEnabled = true;
                btnLed4.IsEnabled = true;
                btnVentilator.IsEnabled = true;
                if (LoggedInUser.IsAdmin)
                {
                    lstUsers.IsEnabled = true;
                    btnEditUser.IsEnabled = true;
                    btnDeleteUser.IsEnabled = true;
                }
                else
                {
                    lstUsers.IsEnabled = false;
                    btnEditUser.IsEnabled = false;
                    btnDeleteUser.IsEnabled = false;
                }
            }
            else
            {
                btnLogOut.IsEnabled = false;

                BtnLed1.IsEnabled = false;
                btnLed2.IsEnabled = false;
                btnLed3.IsEnabled = false;
                btnLed4.IsEnabled = false;
                btnVentilator.IsEnabled = false;

                lstUsers.IsEnabled = false;
                btnEditUser.IsEnabled = false;
                btnDeleteUser.IsEnabled = false;
            }
        }
        private void txtAlarmOn_Click(object sender, MouseButtonEventArgs e)
        {
            AlarmOn = !AlarmOn;
            UpdateAlarmbox();
        }
        public void UpdateAlarmbox()
        {
            if (AlarmOn)
            {
                txtAlarmOn.Text = "Alarm: on";
                txtAlarmOn.Background = new SolidColorBrush(Color.FromRgb(255, 0, 0));
            }
            else
            {
                txtAlarmOn.Text = "Alarm: off";
                txtAlarmOn.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            }
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            stUsersEdit.Visibility = Visibility.Visible;
            stTimeslotEdit.Visibility = Visibility.Collapsed;
            lstUsers.Visibility = Visibility.Visible;
            lstTimeslots.Visibility = Visibility.Collapsed;
        }
        private void btnDeleteTimeslot_Click(object sender, RoutedEventArgs e)
        {
            if(lstTimeslots.SelectedIndex > -1)
            {
                User u = lstUsers.SelectedItem as User;
                Tijdslot t = lstTimeslots.SelectedItem as Tijdslot;

                if(MessageBox.Show("Are you sure?", "Delete Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    u.TijdSloten.Remove(t);
                    User.SaveUsers(Users);
                }
            }
        }
        private void btnAddTimeslot_Click(object sender, RoutedEventArgs e)
        {
            if(lstTimeslots.SelectedIndex > -1)
            {
                User u = lstUsers.SelectedItem as User;

                AddTijdslotDialog dialog = new AddTijdslotDialog();
                dialog.Owner = this;

                dialog.Show();

                if (dialog.DialogResult == true)
                {
                    u.TijdSloten.Add(dialog.tijdslot);
                    User.SaveUsers(Users);
                }

            }
        }
    }
}
