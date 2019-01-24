// -----------------------------------------------------------------------     
// <copyright file="SerializedComponentVM.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Serialization view model.</summary>
// -----------------------------------------------------------------------
namespace LogicDesigner.Model.Serialization
{
    using System.Collections.Generic;
    using System.Linq;
    using LogicDesigner.ViewModel;

    /// <summary>
    /// The serialization component view model.
    /// </summary>
    public class SerializedComponentVM
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedComponentVM"/> class.
        /// </summary>
        public SerializedComponentVM()
        {
        }

        /////// <summary>
        /////// Initializes a new instance of the <see cref="SerializedComponentVM" /> class.
        /////// </summary>
        /////// <param name="xpos">The x position.</param>
        /////// <param name="ypos">The y position.</param>
        /////// <param name="path">The path .</param>
        /////// <param name="id">The identifier.</param>
        /////// <param name="inputIDs">The input pin id's.</param>
        /////// <param name="outputIDs">The output pin id's.</param>
        ////public SerializedComponentVM(double xpos, double ypos, string path, string id, int[] inputIDs, int[] outputIDs)
        ////{
        ////    this.XPos = xpos;
        ////    this.YPos = ypos;
        ////    this.AssemblyPath = path;
        ////    this.UniqueName = id;
        ////}

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedComponentVM"/> class.
        /// </summary>
        /// <param name="baseVm">The base view model.</param>
        /// <param name="assemblyPath">The assembly path.</param>
        public SerializedComponentVM(ComponentVM baseVm, string assemblyPath)
        {
            this.XPos = baseVm.XCoord;
            this.YPos = baseVm.YCoord;
            this.AssemblyPath = assemblyPath;
            this.UniqueName = baseVm.Identifier;
            var temp = new List<int>(baseVm.InputPinsVM.ToList().Select(x => x.IDNumber).ToList());
            var temp2 = new List<int>(baseVm.OutputPinsVM.ToList().Select(x => x.IDNumber).ToList());
            temp2.AddRange(temp);
            this.InputPutputIDs = temp2.ToArray();
        }

        /// <summary>
        /// Gets or sets the x position.
        /// </summary>
        /// <value>
        /// The x position.
        /// </value>
        public double XPos { get; set; }

        /// <summary>
        /// Gets or sets the y position.
        /// </summary>
        /// <value>
        /// The y position.
        /// </value>
        public double YPos { get; set; }

        /// <summary>
        /// Gets or sets the assembly path.
        /// </summary>
        /// <value>
        /// The assembly path.
        /// </value>
        public string AssemblyPath { get; set; }

        /// <summary>
        /// Gets or sets the name of the unique.
        /// </summary>
        /// <value>
        /// The name of the unique.
        /// </value>
        public string UniqueName { get; set; }

        /// <summary>
        /// Gets or sets the input output i ds.
        /// </summary>
        /// <value>
        /// The input and output pin id values.
        /// </value>
        public int[] InputPutputIDs { get; set; }
    }
}
