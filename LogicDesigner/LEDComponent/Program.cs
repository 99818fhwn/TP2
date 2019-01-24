// -----------------------------------------------------------------------     
// <copyright file="Program.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Contains starting class.</summary>    
// -----------------------------------------------------------------------
namespace LEDComponent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The starting class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
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

        /// <summary>
        /// Callback when state changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void LedStateChanged(object sender, EventArgs e)
        {
            Console.WriteLine();
        }
    }
}
