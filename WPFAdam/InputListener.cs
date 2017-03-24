using Advantech.Adam;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFAdam
{
    public class InputListener
    {
        private BackgroundWorker wrkr = new BackgroundWorker();
        public int startpoint { get; set; } = 2;
        public int amount { get; set; } = 6;
        public Modbus modbus { get; set; }

        public event btnPressedEventHandler btnPressed;
        public event redSwitchedEventHandler redSwitched;
        
        public InputListener(Modbus mod)
        {
            this.modbus = mod;
            wrkr.DoWork += Wrkr_DoWork;
            wrkr.RunWorkerCompleted += Wrkr_RunWorkerCompleted;

            wrkr.RunWorkerAsync();
        }

        private void Wrkr_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int[] res = (int[]) e.Result;

            if(res[0] == 1 && res[1] == 0 && res[2] == 1)
            {
                btnPressed(res[0]);
            }
            if(res[0] == 2 && res[1] == 0 && res[2] == 1)
            {
                btnPressed(res[0]);
            }
            if(res[0] == 0)
            {
                redSwitched(res[2] == 1);
            }
            wrkr.RunWorkerAsync();
        }

        private void Wrkr_DoWork(object sender, DoWorkEventArgs e)
        {
            bool[] prevs = new bool[amount];
            while (!modbus.ReadCoilStatus(startpoint, amount, out prevs)) { }
            bool[] curs = new bool[amount];
            bool looping = true;
            while(looping)
            {
                while (!modbus.ReadCoilStatus(startpoint, amount, out curs)) { }

                for(int i = 0; i< amount; i++)
                {
                    if(prevs[i] != curs[i])
                    {
                        int p;
                        int c;
                        if (prevs[i]) p = 1; else p = 0;
                        if (curs[i]) c = 1; else c = 0;
                        e.Result = new int[3] {i,p, c };
                        looping = false;
                    }
                }

            }
        }

        public delegate void btnPressedEventHandler(int nr);
        public delegate void redSwitchedEventHandler(bool isOn);
    }
}
