using ComponentLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestANDGate
{
    class Program
    {
        static void Main(string[] args)
        {
            var andG = new ANDGate();

            foreach (var i in andG.Inputs)
            {
                i.Value.Current = true;
            }

            andG.Execute();

            Console.ReadLine();
        }
    }
}
