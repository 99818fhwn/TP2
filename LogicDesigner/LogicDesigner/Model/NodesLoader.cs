namespace LogicDesigner.Model
{
    using Shared;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;


    public class NodesLoader
    {
        // all nodes in components folder user to choose from
        /// <summary>
        ///   Gets the nodes.
        /// </summary>
        /// <param name="filePath">
        ///   The file path.
        /// </param>
        /// <returns></returns>
        public List<IDisplayableNode> GetNodes(string filePath)
        {
            List<IDisplayableNode> nodes = new List<IDisplayableNode>();

            if (Directory.Exists(filePath))
            {
                DirectoryInfo dI = new DirectoryInfo(filePath);
                List<DirectoryInfo> dirs = dI.GetDirectories().ToList();
                dirs.Add(dI);
                List<FileInfo> files = new List<FileInfo>();

                foreach(var dir in dirs)
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
                        Assembly ass = Assembly.LoadFrom(file.FullName);

                        foreach (var type in ass.GetExportedTypes())
                        {
                            foreach (var interfc in type.GetInterfaces())
                            {
                                if (interfc == typeof(IDisplayableNode))
                                {
                                    try
                                    {
                                        IDisplayableNode node = (IDisplayableNode)Activator.CreateInstance(type);

                                        if (this.ValidateNode(node))
                                        {
                                            nodes.Add(node);
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
        /// <param name="node">The node that contiants the data of a electric component.</param>
        /// <returns>Returns true wether the node is valid or retruns false if not.</returns>
/        private bool ValidateNode(IDisplayableNode node)
        {
            if (node.Description == null || node.Description == string.Empty)
            {
                return false;
            }

            if (node.Inputs == null || node.Outputs == null)
            {
                return false;
            }

            if (node.Picture == null || node.Picture.Width <= 0 || node.Picture.Height <= 1)
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
