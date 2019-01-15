using Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProgramMngWpfProj.Model
{
    public class NodesLoader
    {
        // all nodes in components folder user to choose from
        public List<INode> GetNodes(string filePath)
        {
            List<INode> nodes = new List<INode>();

            DirectoryInfo dI = new DirectoryInfo(filePath);
            FileInfo[] files = dI.GetFiles();

            foreach (var file in files)
            {
                try
                {
                    Assembly ass = Assembly.LoadFrom(file.FullName);

                    foreach (var type in ass.GetExportedTypes())
                    {
                        foreach (var interfc in type.GetInterfaces())
                        {
                            if ((interfc) == typeof(INode))
                            {
                                try
                                {
                                    INode node = (INode)Activator.CreateInstance(type);
                                    nodes.Add(node);
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


            return nodes;
        }
    }
}
