using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhysModelDeveloperGUI
{
    public class SignalPaulTester
    {
        double value = 0;
        double paul_pressure_signal = 0;
        double freq = 15.0;


        public double SignalUpdate()
        {
            double duration = (60 / freq) / 0.015;

            double step_size = Math.PI / duration;

            paul_pressure_signal = Math.Sin(value) * 20;

            value += step_size;

            if (value > Math.PI)
            {
                value = 0;
            }

            return paul_pressure_signal;
        }

    }
}
