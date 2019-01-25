// -----------------------------------------------------------------------     
// <copyright file="NodesLoader.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Loads the nodes.</summary>
// -----------------------------------------------------------------------
namespace LogicDesigner.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Shared;

    /// <summary>
    /// The nodes loader class.
    /// </summary>
    public class NodesLoader
    {
        /// <summary>
        /// Loads the single assembly.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="configPath">The configuration path.</param>
        /// <returns>Returns list of node tuples with relative path to assembly.</returns>
        public static List<Tuple<IDisplayableNode, string>> LoadSingleAssembly(string filePath, string configPath)
        {
            List<Tuple<IDisplayableNode, string>> nodes = new List<Tuple<IDisplayableNode, string>>();
            var fullConf = Path.GetFullPath(configPath);

            var combinedConf = fullConf + filePath;

            if (File.Exists(combinedConf) && (Path.GetExtension(combinedConf) == ".dll" || Path.GetExtension(combinedConf) == ".exe"))
            {
                try
                {
                    Assembly ass = Assembly.LoadFrom(@combinedConf);

                    // Assembly ass = Assembly.Load(file.FullName);
                    foreach (var type in ass.GetExportedTypes())
                    {
                        foreach (var interfc in type.GetInterfaces())
                        {
                            if (interfc == typeof(IDisplayableNode))
                            {
                                try
                                {
                                    IDisplayableNode node = (IDisplayableNode)Activator.CreateInstance(type);

                                    if (ValidateNode(node))
                                    {
                                        nodes.Add(new Tuple<IDisplayableNode, string>(node, combinedConf));
                                    }
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                }
            }

            return nodes;
        }

        /// <summary>
        /// Gets the nodes.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="configPath">The configuration path.</param>
        /// <returns>Returns a list with tuples node and path.</returns>
        public List<Tuple<IDisplayableNode, string>> GetNodes(string filePath, string configPath)
        {
            List<Tuple<IDisplayableNode, string>> nodes = new List<Tuple<IDisplayableNode, string>>();

            if (Directory.Exists(filePath))
            {
                DirectoryInfo dI = new DirectoryInfo(filePath);
                List<DirectoryInfo> dirs = dI.GetDirectories().ToList();
                dirs.Add(dI);
                List<FileInfo> files = new List<FileInfo>();

                foreach (var dir in dirs)
                {
                    foreach (var f in dir.GetFiles("*.dll"))
                    {
                        files.Add(f);
                    }

                    foreach (var f in dir.GetFiles("*.exe"))
                    {
                        files.Add(f);
                    }
                }

                foreach (var file in files)
                {
                    try
                    {
                        string fullpath = Path.GetFullPath(file.FullName);
                        var splitPath = fullpath.Replace(Path.GetFullPath(configPath), string.Empty);
                        var fullConf = Path.GetFullPath(configPath);

                        var combinedConf = fullConf + splitPath;

                        Assembly ass = Assembly.LoadFrom(@combinedConf);

                        // Assembly ass = Assembly.Load(file.FullName);
                        foreach (var type in ass.GetExportedTypes())
                        {
                            foreach (var interfc in type.GetInterfaces())
                            {
                                if (interfc == typeof(IDisplayableNode))
                                {
                                    try
                                    {
                                        IDisplayableNode node = (IDisplayableNode)Activator.CreateInstance(type);

                                        if (ValidateNode(node))
                                        {
                                            nodes.Add(new Tuple<IDisplayableNode, string>(node, splitPath));
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(filePath);
            }

            return nodes;
        }

        /// <summary>
        /// Validates the node content and checks for missing necessary properties.
        /// </summary>
        /// <param name="node">The node that contains the data of a electric component.</param>
        /// <returns>Returns true if the node is valid or returns false if not.</returns>
        private static bool ValidateNode(IDisplayableNode node)
        {
            if (node.Description == null || node.Description == string.Empty)
            {
                return false;
            }

            if (node.Inputs == null || node.Outputs == null)
            {
                return false;
            }

            if (node.Picture == null || node.Picture.Width <= 0 || node.Picture.Height <= 0)
            {
                return false;
            }

            if (node.Label == null || node.Label == string.Empty)
            {
                return false;
            }

            return true;
        }
    }
}
