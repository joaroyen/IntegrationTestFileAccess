using System;
using System.Configuration;
using System.IO;
using JoarOyen.Test.IO;
using NUnit.Framework;

namespace JoarOyen.Test.Test.Integration.IO
{
    [TestFixture]
    public class TestFileTests
    {
        private const string DeployedTextFile1Name = "TestData\\DeployedTextFile1.txt";
        private const string DeployedTextFile2Name = "TestData\\DeployedTextFile2.txt";
        private const string DeployedConfigurationFileName = "TestData\\DeployedConfiguration.config";
        private const string EmbeddedConfigurationResourceName = "JoarOyen.Test.Test.Integration.TestData";
        private const string EmbeddedConfigurationFileName = "EmbeddedConfiguration.config";

        [TestCase]
        public void The_deployment_directory_for_the_file_is_the_same_as_the_one_where_the_test_assemblies_are_deployed()
        {
            var deployedFile = TestFile.FromDeployedFile(DeployedTextFile1Name);
            var localPath = new Uri(GetType().Assembly.CodeBase).LocalPath;

            Assert.That(localPath, Is.SamePathOrUnder(deployedFile.DeploymentDirectory));
        }

        [TestCase]
        public void A_deployed_file_can_be_backed_up()
        {
            var deployedFile = TestFile.FromDeployedFile(DeployedTextFile1Name);
            deployedFile.Backup();

            Assert.That(File.Exists(deployedFile.BackupFullName), Is.True);
        }

        [TestCase]
        public void A_deployed_file_can_be_restored_from_backup()
        {
            var deployedFile = TestFile.FromDeployedFile(DeployedTextFile1Name);
            deployedFile.Backup();
            File.Delete(deployedFile.FullName);
            deployedFile.Restore();

            Assert.That(File.Exists(deployedFile.FullName));
        }

        [TestCase]
        public void Restoring_a_file_that_is_not_backed_up_throws_an_exception()
        {
            Assert.Throws<FileNotFoundException>(() =>
                {
                    var deployedFile = TestFile.FromDeployedFile(DeployedTextFile1Name);
                    if (File.Exists(deployedFile.BackupFullName)) File.Delete(deployedFile.BackupFullName);
                    deployedFile.Restore();
                });
        }

        [TestCase]
        public void The_content_of_a_deployed_file_can_be_preserved_after_code_that_modify_the_file_is_invoked()
        {
            var textFile1 = TestFile.FromDeployedFile(DeployedTextFile1Name);
            textFile1.InvokeAndPreserve(() => File.WriteAllText(textFile1.FullName, "Modified content"));

            Assert.That(File.ReadAllText(textFile1.FullName), Is.EqualTo("This is deployed text file number 1"));
        }

        [TestCase]
        public void The_content_of_a_deployed_file_can_be_preserved_after_code_with_the_file_as_an_argument_have_modified_the_file_is_invoked()
        {
            var textFile1 = TestFile.FromDeployedFile(DeployedTextFile1Name);
            textFile1.InvokeAndPreserve(
                deployedFile => File.WriteAllText(deployedFile.FullName, "Modified content"));

            Assert.That(File.ReadAllText(textFile1.FullName), Is.EqualTo("This is deployed text file number 1"));
        }

        [TestCase]
        public void A_deployed_file_can_be_replaced_with_another_file()
        {
            TestFile.FromDeployedFile(DeployedTextFile1Name).InvokeAndPreserve(deployedFile =>
                {
                    var anotherDeployedTextFile = TestFile.FromDeployedFile(DeployedTextFile2Name);
                    deployedFile.ReplaceWith(anotherDeployedTextFile);

                    Assert.That(File.ReadAllText(deployedFile.FullName), Is.EqualTo("This is deployed text file number 2"));
                });
        }

        [TestCase]
        public void The_content_of_a_deployed_file_can_be_preserved_after_code_that_runs_with_a_replaced_file_is_invoked()
        {
            var textFile1 = TestFile.FromDeployedFile(DeployedTextFile1Name);
            var anotherDeployedTextFile = TestFile.FromDeployedFile(DeployedTextFile2Name);

            textFile1.InvokeWithReplacement(anotherDeployedTextFile, () => File.WriteAllText(textFile1.FullName, "Modified content"));

            Assert.That(File.ReadAllText(textFile1.FullName), Is.EqualTo("This is deployed text file number 1"));
        }


        [TestCase]
        public void A_deployed_file_can_be_deleted()
        {
            TestFile.FromDeployedFile(DeployedTextFile1Name).InvokeAndPreserve(deployedFile =>
                {
                    deployedFile.Delete();

                    Assert.That(File.Exists(deployedFile.FullName), Is.False);
                });
        }

        [TestCase]
        public void The_current_configuration_file_can_be_used_as_a_test_file()
        {
            var currentConfigurationFile = TestFile.FromCurrentConfigurationFile();
            Assert.That(File.Exists(currentConfigurationFile.FullName));
        }

        [TestCase]
        public void A_deployed_configuration_file_can_replace_the_current_configuration()
        {
            TestFile.FromCurrentConfigurationFile().InvokeAndPreserve(currentConfigurationFile =>
                {
                    var deployedFile = TestFile.FromDeployedFile(DeployedConfigurationFileName);
                    currentConfigurationFile.ReplaceWith(deployedFile);

                    Assert.That(ConfigurationManager.AppSettings["DeployedConfiguration"], Is.EqualTo("This is a deployed configuration"));
                });
        }

        [TestCase]
        public void An_embedded_configuration_file_can_be_saved_to_disk()
        {
            var embeddedConfigurationFile = TestFile.FromEmbeddedResource(typeof(TestFileTests).Assembly, EmbeddedConfigurationResourceName, EmbeddedConfigurationFileName);
            Assert.That(File.Exists(embeddedConfigurationFile.FullName));
        }

        [TestCase]
        public void Trying_to_use_a_resource_that_does_not_exists_is_not_ok()
        {
            Assert.Throws<ArgumentNullException>(() =>
                TestFile.FromEmbeddedResource(typeof(TestFileTests).Assembly, "UnknownResource", "UnknownResource"));
        }

        [TestCase]
        public void An_embedded_configuration_file_can_replace_the_current_configuration()
        {
            TestFile.FromCurrentConfigurationFile().InvokeAndPreserve(currentConfigurationFile =>
                {
                    var embeddedFile = TestFile.FromEmbeddedResource(typeof (TestFileTests).Assembly, EmbeddedConfigurationResourceName, EmbeddedConfigurationFileName);
                    currentConfigurationFile.ReplaceWith(embeddedFile);

                    Assert.That(ConfigurationManager.AppSettings["EmbeddedConfiguration"], Is.EqualTo("This is an embedded configuration"));
                });
        }
    }
}