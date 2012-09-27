using System;
using System.Configuration;
using System.IO;
using JoarOyen.Configuration;
using NUnit.Framework;

namespace JoarOyen.Test.Unit.Configuration
{
    [TestFixture]
    public class ConfigurationFileTests
    {
        [TestCase]
        public void The_path_of_the_current_configuration_file_is_the_same_as_the_deployed_test_assembly()
        {
            string applicationConfigurationPath = ConfigurationFile.ApplicationConfigurationPath;
            string deployedTestAssemblyDirectory = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath);

            Assert.That(applicationConfigurationPath, Is.SamePathOrUnder(deployedTestAssemblyDirectory));
        }

        [TestCase]
        public void A_file_with_the_extension_config_is_a_valid_configuration_file()
        {
            Assert.That(ConfigurationFile.IsConfigurationFile("This_is_a_configuration_file_name.config"), Is.True);
        }

        [TestCase]
        public void The_content_of_the_current_configuration_can_be_refreshed()
        {
            ConfigurationFile.RefreshCurrentConfigurationFile();

            Assert.That(ConfigurationManager.AppSettings.Count, Is.EqualTo(0));
        }
    }
}
