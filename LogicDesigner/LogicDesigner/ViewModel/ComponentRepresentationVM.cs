// -----------------------------------------------------------------------     
// <copyright file="ComponentRepresentationVM.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>A logic designer that provides tools for creating circuits.</summary>    
// <author>Fabian Weisser</author>    
// -----------------------------------------------------------------------
namespace LogicDesigner.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media.Imaging;
    using LogicDesigner.Commands;
    using Shared;

    /// <summary>
    /// The class that provides properties for the list view to bind and a command to 
    /// create a new instance of a component in the designer.
    /// </summary>
    public class ComponentRepresentationVM
    {
        /// <summary>
        /// The component that is represented by this class.
        /// </summary>
        private readonly IDisplayableNode node;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentRepresentationVM"/> class.
        /// </summary>
        /// <param name="addCommand">The add command for adding real components to the designer field.</param>
        /// <param name="node">The node that will be added then.</param>
        /// <param name="assemblyPath">The assebly path.</param>
        public ComponentRepresentationVM(Command addCommand, IDisplayableNode node, string assemblyPath)
        {
            this.AddComponentCommand = addCommand;
            this.node = node;
            this.AssemblyPath = assemblyPath;
        }

        /// <summary>
        /// Gets the add component command.
        /// </summary>
        /// <value>
        /// The add component command.
        /// </value>
        public Command AddComponentCommand
        {
            get;
        }

        /// <summary>
        /// Gets the picture.
        /// </summary>
        /// <value>
        /// The picture of the component.
        /// </value>
        public BitmapSource Picture
        {
            get
            {
                try
                {
                    return Imaging.CreateBitmapSourceFromHBitmap(this.node.Picture.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <value>
        /// The component that will be placed by clicking.
        /// </value>
        public IDisplayableNode Node
        {
            get
            {
                return this.node;
            }
        }

        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <value>
        /// The label of the component.
        /// </value>
        public string Label
        {
            get { return this.node.Label; }
        }

        /// <summary>
        /// Gets or sets the assembly path.
        /// </summary>
        /// <value>
        /// The assembly path.
        /// </value>
        public string AssemblyPath
        {
            get;
            set;
        }
    }
}
