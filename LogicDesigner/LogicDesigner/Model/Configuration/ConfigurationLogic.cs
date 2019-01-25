// -----------------------------------------------------------------------     
// <copyright file="ConfigurationLogic.cs" company="FHWN">    
// Copyright (c) FHWN. All rights reserved.    
// </copyright>    
// <summary>Contains the class with the configuration logic.</summary>
// -----------------------------------------------------------------------
namespace LogicDesigner.Model.Configuration
{
    using System.Drawing;
    using System.IO;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Class used for configuration.
    /// </summary>
    public class ConfigurationLogic
    {
        /// <summary>
        /// The pin passive color.
        /// </summary>
        private Color pinPassiveColor;

        /// <summary>
        /// The pin active color.
        /// </summary>
        private Color pinActiveColor;

        /// <summary>
        /// The line passive color.
        /// </summary>
        private Color linePassiveColor;

        /// <summary>
        /// The line active color.
        /// </summary>
        private Color lineActiveColor;

        /// <summary>
        /// The module path.
        /// </summary>
        private string modulePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationLogic"/> class.
        /// </summary>
        public ConfigurationLogic()
        {
            if (File.Exists("config.json"))
            {
                var conf = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();

                this.PinActiveColor = Color.FromName(conf.GetSection("Config")["PinActive"]);
                if (this.PinActiveColor == null)
                {
                    this.PinActiveColor = Color.FromName("Green");
                }

                this.PinPassiveColor = Color.FromName(conf.GetSection("Config")["PinPassive"]);
                if (this.PinPassiveColor == null)
                {
                    this.PinPassiveColor = Color.FromName("Grey");
                }

                this.LineActiveColor = Color.FromName(conf.GetSection("Config")["LineActive"]);
                if (this.PinActiveColor == null)
                {
                    this.PinActiveColor = Color.FromName("Red");
                }

                this.LinePassiveColor = Color.FromName(conf.GetSection("Config")["LinePassive"]);
                if (this.PinActiveColor == null)
                {
                    this.PinActiveColor = Color.FromName("Black");
                }

                var worked = int.TryParse(conf.GetSection("Config")["RunDelayInSeconds"], out int delay);
                this.RunDelay = delay * 1000;
                if (!worked)
                {
                    this.RunDelay = 5000;  
                }

                this.modulePath = conf.GetSection("Config")["ModulePath"].ToString();
                if (this.modulePath == null)
                {
                    this.modulePath = "Components";
                }

                this.LogPath = conf.GetSection("Config")["LogPath"].ToString();
                if (this.LogPath == null)
                {
                    this.LogPath = "LogFiles";
                }
            }
        }

        /// <summary>
        /// Gets or sets the color of the pin passive.
        /// </summary>
        /// <value>
        /// The color of the pin passive.
        /// </value>
        public Color PinPassiveColor { get => this.pinPassiveColor; set => this.pinPassiveColor = value; }

        /// <summary>
        /// Gets or sets the color of the pin active.
        /// </summary>
        /// <value>
        /// The color of the pin active.
        /// </value>
        public Color PinActiveColor { get => this.pinActiveColor; set => this.pinActiveColor = value; }

        /// <summary>
        /// Gets or sets the color of the line passive.
        /// </summary>
        /// <value>
        /// The color of the line passive.
        /// </value>
        public Color LinePassiveColor { get => this.linePassiveColor; set => this.linePassiveColor = value; }

        /// <summary>
        /// Gets or sets the color of the line active.
        /// </summary>
        /// <value>
        /// The color of the line active.
        /// </value>
        public Color LineActiveColor { get => this.lineActiveColor; set => this.lineActiveColor = value; }

        /// <summary>
        /// Gets or sets the module path.
        /// </summary>
        /// <value>
        /// The module path.
        /// </value>
        public string ModulePath { get => this.modulePath; set => this.modulePath = value; }

        /// <summary>
        /// Gets or sets the log path.
        /// </summary>
        /// <value>
        /// The log path.
        /// </value>
        public string LogPath { get; set; }

        /// <summary>
        /// Gets or sets the delay.
        /// </summary>
        /// <value>
        /// The delay.
        /// </value>
        public int RunDelay { get; set; }
    }
}
