using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LEDComponent
{
    class Program
    {
        static void Main(string[] args)
        {
            var l = new LED();
            l.PictureChanged += LedStateChanged;

            l.Execute();
            l.Inputs.ElementAt(0).Value.Current = true;
            l.Execute();
            l.Execute();
            l.Execute();
            l.Inputs.ElementAt(0).Value.Current = false;
            l.Execute();
            l.Execute();
        }

        private static void LedStateChanged(object sender, EventArgs e)
        {
            Console.WriteLine();
        }
    }
}
