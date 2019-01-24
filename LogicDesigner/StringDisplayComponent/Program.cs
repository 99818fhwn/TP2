// -----------------------------------------------------------------------     
// <copyright file="Program.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Contains starting class.</summary>    
// -----------------------------------------------------------------------
namespace StringDisplayComponent
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
            var sd = new StringDisplay();

            sd.Execute();

            Console.ReadLine();
        }
    }
}
