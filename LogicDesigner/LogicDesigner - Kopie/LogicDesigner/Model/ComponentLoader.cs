using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Shared;

namespace LogicDesigner.Model
{
    public class ComponentLoader
    {
        public List<IDisplayableNode> GetNode(string[] paths)
        {
            var nodes = new List<IDisplayableNode>();

            foreach (var path in paths)
            {
                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(path);
                }
                if (File.Exists(path))
                {
                    try
                    {
                        var asm = Assembly.LoadFrom(path);

                        var asmItems = asm.GetExportedTypes().Where(
                            a => a.GetInterfaces().Any(b => b.IsGenericType == true && 
                            b.GetGenericTypeDefinition() == typeof(IDisplayableNode)));

                        foreach (var item in asmItems)
                        {
                            var instance = (IDisplayableNode)Activator.CreateInstance(item);
                            nodes.Add(instance);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message);
                    }
                }
            }

            return nodes;
        }
    }
}
