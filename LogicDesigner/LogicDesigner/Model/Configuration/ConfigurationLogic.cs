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

                this.PinActiveColor = Color.FromName(conf.GetSection("Config")["PinActive"]);
                this.PinPassiveColor = Color.FromName(conf.GetSection("Config")["PinPassive"]);
                this.LineActiveColor = Color.FromName(conf.GetSection("Config")["LineActive"]);
                this.LinePassiveColor = Color.FromName(conf.GetSection("Config")["LinePassive"]);
                this.modulPath = conf.GetSection("Config")["Path"];
            }
        }

        public Color PinPassiveColor { get => pinPassiveColor; set => pinPassiveColor = value; }
        public Color PinActiveColor { get => pinActiveColor; set => pinActiveColor = value; }
        public Color LinePassiveColor { get => linePassiveColor; set => linePassiveColor = value; }
        public Color LineActiveColor { get => lineActiveColor; set => lineActiveColor = value; }
    }
}
