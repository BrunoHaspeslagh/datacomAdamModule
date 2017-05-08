using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using WPFAdam.models;

namespace WPFAdam
{
    /// <summary>
    /// Interaction logic for AddAdmin.xaml
    /// </summary>
    public partial class AddAdmin : Window
    {
        public eid eidListener { get; set; }
        public User Admin { get; set; }
        public AddAdmin(eid listener)
        {
            InitializeComponent();
            this.eidListener = listener;
            eidListener.FindAdmin();
            eidListener.adminInserted += EidListener_adminInserted;
        }

        private void EidListener_adminInserted(string naam, string vnaam, string rijk)
        {
            txtNaam.Text = naam;
            txtVNaam.Text = vnaam;

            Admin = new User(vnaam, naam, rijk) {IsAdmin = true };

            btnSelectAdmin.IsEnabled = true;
        }

        private void btnSelectAdmin_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
