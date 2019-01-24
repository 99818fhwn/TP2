// -----------------------------------------------------------------------     
// <copyright file="SerializedObject.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Serialization view model.</summary>
// -----------------------------------------------------------------------
namespace LogicDesigner.Model.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using LogicDesigner.ViewModel;

    /// <summary>
    /// Serialization object class.
    /// </summary>
    public class SerializedObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedObject"/> class.
        /// </summary>
        /// <param name="vms">The view models.</param>
        /// <param name="connections">The connections.</param>
        public SerializedObject(ICollection<SerializedComponentVM> vms, ICollection<SerializedConnectionVM> connections)
        {
            this.Components = new List<SerializedComponentVM>(vms);
            this.Connections = new List<SerializedConnectionVM>(connections);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializedObject"/> class.
        /// </summary>
        public SerializedObject()
        {
            this.Components = new List<SerializedComponentVM>();
            this.Connections = new List<SerializedConnectionVM>();
        }

        /// <summary>
        /// Gets or sets the components.
        /// </summary>
        /// <value>
        /// The components.
        /// </value>
        public List<SerializedComponentVM> Components { get; set; }

        /// <summary>
        /// Gets or sets the connections.
        /// </summary>
        /// <value>
        /// The connections.
        /// </value>
        public List<SerializedConnectionVM> Connections { get; set; }
    }
}
