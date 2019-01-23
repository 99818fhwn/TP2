using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicDesigner.Model.Configuration
{
    public class ConfigurationLogic
    {
        private Color pinPassiveColor;

        private Color pinActiveColor;

        private Color linePassiveColor;

        private Color lineActiveColor;

        private string modulPath;
        
        public ConfigurationLogic()
        {
            if (File.Exists("config.json"))
            {
                var conf = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();

                this.pinActiveColor = Color.FromName(conf.GetSection("Config")["PinActive"]);
                this.pinPassiveColor = Color.FromName(conf.GetSection("Config")["PinPassive"]);
                this.lineActiveColor = Color.FromName(conf.GetSection("Config")["LineActive"]);
                this.linePassiveColor = Color.FromName(conf.GetSection("Config")["LinePassive"]);
                this.modulPath = conf.GetSection("Config")["Path"];
                

            }
        }
    }
}
