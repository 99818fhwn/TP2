using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LogicDesigner.Model
{
    public class NodesLoader
    {
        // all nodes in components folder user to choose from
        public List<IDisplayableNode> GetNodes(string filePath)
        {
            List<IDisplayableNode> nodes = new List<IDisplayableNode>();

            if (Directory.Exists(filePath))
            {
                DirectoryInfo dI = new DirectoryInfo(filePath);
                DirectoryInfo[] dirs = dI.GetDirectories();
                List<FileInfo> files = dI.GetFiles().ToList();

                foreach(var dir in dirs)
                {
                    foreach (var f in dir.GetFiles())
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
                                if ((interfc) == typeof(IDisplayableNode))
                                {
                                    try
                                    {
                                        IDisplayableNode node = (IDisplayableNode)Activator.CreateInstance(type);
                                        nodes.Add(node);
                                    }
                                    catch (Exception e)
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
    }
}
