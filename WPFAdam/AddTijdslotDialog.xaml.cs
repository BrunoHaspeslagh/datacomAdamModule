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
    /// Interaction logic for AddTijdslotDialog.xaml
    /// </summary>
    public partial class AddTijdslotDialog : Window
    {
        public Tijdslot tijdslot { get; set; }
        public AddTijdslotDialog()
        {
            InitializeComponent();
            this.Loaded += AddTijdslotDialog_Loaded;

        }

        private void AddTijdslotDialog_Loaded(object sender, RoutedEventArgs e)
        {
            CheckIfAdd();
        }

        private void CheckText(object sender, RoutedEventArgs e)
        {
            CheckIfAdd();
        }


        private void AddTijdslotOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void CheckIfAdd()
        {
            int StartUur;
            int StartMinuut;
            int EindUur;
            int EindMinuut;

            if((int.TryParse(txbTijdslotStartUur.Text, out StartUur) 
                && int.TryParse(txbTijdslotStartMinuut.Text, out StartMinuut)
                && int.TryParse(txbTijdslotEindUur.Text, out EindUur)
                && int.TryParse(txbTijdslotEindMinuut.Text, out EindMinuut)) 
                && (StartUur > 0 && StartUur < 24
                    && StartMinuut > 0 && StartMinuut < 60
                    && EindUur > 0 && EindUur < 24
                    && EindMinuut > 0 && EindMinuut < 60))
            {
                tijdslot = new Tijdslot() { Start = new DateTime(0,0,0,StartUur, StartMinuut, 0), Stop = new DateTime(0,0,0,EindUur, EindMinuut, 0)};

                AddTijdslotOK.IsEnabled = true;
            }
            else
            {
                AddTijdslotOK.IsEnabled = false;
            }
        }
    }
}
