using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryConverterComponent
{
    class Program
    {
        static void Main(string[] args)
        {
            var bc = new BinaryConverter();
            bc.Execute();
            bc.Inputs.ElementAt(1).Value.Current = true;
            bc.Execute();
        }
    }
}
