// -----------------------------------------------------------------------     
// <copyright file="SerializationLogic.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Contains the serialization logic.</summary>    
// -----------------------------------------------------------------------
namespace LogicDesigner.Model.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Xml;
    using System.Xml.Serialization;
    using LogicDesigner.ViewModel;
    using Polenter.Serialization;

    /// <summary>
    /// Class handling serialization logic.
    /// </summary>
    public class SerializationLogic
    {
        #region Declarations
        /// <summary>
        /// The formatter that handles serialization/deserialization.
        /// </summary>
        private readonly XmlSerializer formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationLogic"/> class.
        /// </summary>
        public SerializationLogic()
        {
            this.formatter = new XmlSerializer(typeof(SerializedObject));
        }
        #endregion

        #region Serialization        
        /// <summary>
        /// Serializes the component.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="serializableObject">The serializable object.</param>
        /// <param name="connections">The connections.</param>
        /// <exception cref="ArgumentNullException">Object must not be null.</exception>
        /// <exception cref="ArgumentException">Path not found.</exception>
        /// <exception cref="SerializationException">Object could not be serialized</exception>
        public void SerializeComponent(string path, ICollection<Tuple<ComponentVM, string>> serializableObject, ICollection<ConnectionVM> connections)
        {
            if (serializableObject == null)
            {
                throw new ArgumentNullException("Object must not be null.");
            }

            string dir = Path.GetDirectoryName(path);

            if (!Directory.Exists(dir))
            {
                throw new ArgumentException("Path not found.");
            }

            using (Stream writer = new FileStream(path, FileMode.Create))
            {
                try
                {
                    // List<string> paths = new List<string>();
                    List<SerializedComponentVM> components = new List<SerializedComponentVM>();
                    foreach (var element in serializableObject)
                    {
                        // if (!paths.Contains(element.Item2))
                        // {
                        //     paths.Add(element.Item2);
                        // }
                        // element.Item1.IsInField = true;
                           
                        // foreach(ConnectionVM vm in connections)
                        // {
                        //     var input = vm.InputPin;
                        //     var output = vm.OutputPin;
                        // }
                        components.Add(new SerializedComponentVM(element.Item1, element.Item2));
                    }

                    List<SerializedConnectionVM> sconnections = new List<SerializedConnectionVM>();

                    foreach (var connection in connections)
                    {
                        sconnections.Add(new SerializedConnectionVM(connection.InputPin.Pin.Label,connection.InputPin.IDNumber, connection.OutputPin.Pin.Label, connection.OutputPin.IDNumber, connection.InputPin.Parent.Name, connection.OutputPin.Parent.Name, connection.InputPin.XPosition, connection.InputPin.YPosition, connection.OutputPin.XPosition, connection.OutputPin.YPosition, connection.ConnectionId));
                    }

                    var serializableElement = new SerializedObject(components, sconnections);
                    this.formatter.Serialize(writer, serializableElement);
                }
                catch (SerializationException ex)
                {
                    throw new SerializationException("Object could not be serialized", ex);
                }
            }
        }
        #endregion

        #region Deserialization
        /// <summary>
        /// Deserializes the given file.
        /// </summary>
        /// <param name="path"> Path of the file to be deserialized. </param>
        /// <returns> The constructed object. </returns>
        public object DeserializeObject(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found at {path}.");
            }

            if (Path.GetExtension(path) != ".ldf")
            {
                throw new FileLoadException("File has wrong extension.");
            }

            using (Stream reader = new FileStream(path, FileMode.Open))
            {
                object obj;
                try
                {
                    obj = this.formatter.Deserialize(reader);
                }
                catch (SerializationException ex)
                {
                    throw new SerializationException("Object could not be deserialized", ex);
                }

                return obj;
            }
        }
        #endregion
    }
}
