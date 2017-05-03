using EidSamples;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WPFAdam
{
    public class eid
    {
        public event CardInsertedEventHandler CardInserted;
        public BackgroundWorker wrkr = new BackgroundWorker();
        public string prevrijk;

        public eid()
        {
            wrkr.DoWork += Wrkr_DoWork;
            wrkr.RunWorkerCompleted += Wrkr_RunWorkerCompleted;
            prevrijk = "";
            wrkr.RunWorkerAsync();
        }

        private void Wrkr_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string[] data = (string[]) e.Result;

            if(data[2] != prevrijk)
            {
                CardInserted(data[0], data[1], data[2]);
                prevrijk = data[2];
            }
            

            wrkr.RunWorkerAsync();
            
        }

        private void Wrkr_DoWork(object sender, DoWorkEventArgs e)
        {
            ReadData dt = new ReadData();
            string naam = "";
            string vnaam = "";
            string rijk = "";
            while ((naam == "" || vnaam == "" || rijk == "")|| rijk == prevrijk)
            {
                vnaam = dt.GetFirstname();
                naam = dt.GetSurname();
                rijk = dt.Getnational_number();
                Thread.Sleep(500);
            }
            
            e.Result = new string[3] { naam, vnaam, rijk };
        }

        public delegate void CardInsertedEventHandler(string naam, string vnaam, string rijk);
    }
}
