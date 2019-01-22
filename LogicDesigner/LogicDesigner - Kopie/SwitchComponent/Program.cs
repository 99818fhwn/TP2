using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwitchComponent
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new Switch();
            sw.PictureChanged += RecivedEvent;
            sw.Execute();
            sw.Activate();
            sw.Execute();
        }

        private static void RecivedEvent(object sender,EventArgs e)
        {
            Console.WriteLine("Hit");
        }
    }
}
