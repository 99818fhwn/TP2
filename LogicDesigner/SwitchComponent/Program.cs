// -----------------------------------------------------------------------     
// <copyright file="Program.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Contains starting class.</summary>    
// -----------------------------------------------------------------------
namespace SwitchComponent
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The main class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            var sw = new Switch();
            sw.PictureChanged += RecivedEvent;
            sw.Execute();
            sw.Activate();
            sw.Execute();
        }

        /// <summary>
        /// Receives the event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void RecivedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Hit");
        }
    }
}
