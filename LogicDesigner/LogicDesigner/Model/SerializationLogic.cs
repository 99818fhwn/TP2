namespace LogicDesigner.Model
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Class handling serialization logic.
    /// </summary>
    public class SerializationLogic
    {
        #region Declarations
        /// <summary>
        /// The formatter that handles serialization/deserialization.
        /// </summary>
        private readonly BinaryFormatter formatter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationLogic"/> class.
        /// </summary>
        public SerializationLogic()
        {
            this.formatter = new BinaryFormatter();
        }
        #endregion

        #region Serialization
        /// <summary>
        /// Serializes the given object.
        /// </summary>
        /// <param name="path"> Path for the binary file output (without extension). </param>
        /// <param name="serializableObject"> Object to be serialized. </param>
        public void SerializeObject(string path, object serializableObject)
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
            //using (BinaryWriter binWriter = new BinaryWriter(writer))
            {
                try
                {
                    this.formatter.Serialize(writer, serializableObject);
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
            path += ".ldf";
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
