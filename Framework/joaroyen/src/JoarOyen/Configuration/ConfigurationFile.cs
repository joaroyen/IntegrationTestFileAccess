using System;
using System.Configuration;
using System.IO;

namespace JoarOyen.Configuration
{
    public class ConfigurationFile
    {
        public static string ApplicationConfigurationPath
        {
            get { return (string)AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE"); }
        }

        public static bool IsConfigurationFile(string filePath)
        {
            return Path.GetExtension(filePath) == ".config";
        }

        public static void RefreshCurrentConfigurationFile()
        {
            var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            foreach (ConfigurationSectionGroup sectionGroup in configuration.SectionGroups)
            {
                RefreshSections(sectionGroup.Sections);
            }
            RefreshSections(configuration.Sections);
        }

        private static void RefreshSections(ConfigurationSectionCollection sections)
        {
            foreach (ConfigurationSection section in sections)
            {
                ConfigurationManager.RefreshSection(section.SectionInformation.SectionName);
            }
        }
    }
}
