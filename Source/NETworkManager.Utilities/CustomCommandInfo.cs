using System;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class to store custom command informations.
    /// </summary>
    public class CustomCommandInfo : ICloneable
    {

        /// <summary>
        /// Unique Identifiert for this custom command.
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// Name of this custom command.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Filepath to the application to execute.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Arguments to start the application.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Create an empty <see cref="CustomCommandInfo"/> with an <see cref="ID"/>
        /// </summary>
        public CustomCommandInfo()
        {
            ID = Guid.NewGuid();
        }

        /// <summary>
        /// Create an <see cref="CustomCommandInfo"/> with an <see cref="ID"/>, <see cref="Name"/> and <see cref="FilePath"/>.
        /// </summary>
        public CustomCommandInfo(Guid id, string name, string filePath)
        {
            ID = id;
            Name = name;
            FilePath = filePath;
        }

        /// <summary>
        /// Create an <see cref="CustomCommandInfo"/> with an <see cref="ID"/>, <see cref="Name"/>, <see cref="FilePath"/> and <see cref="Arguments"/>.
        /// </summary>
        public CustomCommandInfo(Guid id, string name, string filePath, string arguments) : this(id, name, filePath)
        {
            Arguments = arguments;
        }

        /// <summary>
        /// Method to clone this object.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}