using System;
using System.IO;
using System.Reflection;
using JoarOyen.Configuration;

namespace JoarOyen.Test.IO
{
    public class TestFile
    {
        public string DeploymentDirectory { get; private set; }
        public string Name { get; private set; }
        public string FullName { get { return Path.Combine(DeploymentDirectory, Name); } }
        public string BackupName { get { return Name + ".bak"; } }
        public string BackupFullName { get { return Path.Combine(DeploymentDirectory, BackupName); } }

        private TestFile(string deploymentDirectory, string fileName)
        {
            DeploymentDirectory = deploymentDirectory;
            Name = fileName;
        }

        public static TestFile FromDeployedFile(string fileName)
        {
            var deploymentDirectory = GetAssemblyDirectoryName(Assembly.GetCallingAssembly());
            return new TestFile(deploymentDirectory, fileName);
        }

        private static string GetAssemblyDirectoryName(Assembly assembly)
        {
            return Path.GetDirectoryName(new Uri(assembly.CodeBase).LocalPath);
        }

        public static TestFile FromCurrentConfigurationFile()
        {
            var applicationConfigurationPath = ConfigurationFile.ApplicationConfigurationPath;
            return new TestFile(Path.GetDirectoryName(applicationConfigurationPath), Path.GetFileName(applicationConfigurationPath));
        }

        public static TestFile FromEmbeddedResource(Assembly testAssembly, string resourceNamespace, string fileName)
        {
            var deploymentDirectory = GetAssemblyDirectoryName(testAssembly);
            var deployedFile = new TestFile(deploymentDirectory, fileName);
            SaveEmbeddedResource(testAssembly, resourceNamespace, deployedFile);
            return deployedFile;
        }

        private static void SaveEmbeddedResource(Assembly testAssembly, string resourceNamespace, TestFile testFile)
        {
            var resourceName = string.Format("{0}.{1}", resourceNamespace, testFile.Name);
            var manifestResourceStream = testAssembly.GetManifestResourceStream(resourceName);

            if (manifestResourceStream == null)
            {
                throw new ArgumentNullException(resourceName, "Unable to locate resource");
            }

            using (var fileStream = new FileStream(testFile.FullName, FileMode.Create, FileAccess.Write))
            {
                manifestResourceStream.CopyTo(fileStream);
            }
        }

        public virtual void Backup()
        {
            ClearReadOnlyAttribute(BackupFullName);
            File.Copy(FullName, BackupFullName, true);
        }

        public virtual void Restore()
        {
            if (!File.Exists(BackupFullName))
            {
                throw new FileNotFoundException(string.Format("Backup file {0} not found", BackupFullName));
            }

            ClearReadOnlyAttribute(FullName);
            ReplaceFile(BackupFullName, FullName);
        }

        public virtual void ReplaceWith(TestFile file)
        {
            ReplaceFile(file.FullName, FullName);
        }

        private static void ReplaceFile(string sourceFilePath, string destinationFilePath)
        {
            ClearReadOnlyAttribute(destinationFilePath);
            File.Copy(sourceFilePath, destinationFilePath, true);

            if (ConfigurationFile.IsConfigurationFile(sourceFilePath))
            {
                ConfigurationFile.RefreshCurrentConfigurationFile();
            }
        }

        public virtual void Delete()
        {
            if (File.Exists(FullName))
            {
                File.Delete(FullName);
            }
        }

        public void InvokeAndPreserve(Action action)
        {
            InvokeAndPreserve(testFile => action());
        }

        public void InvokeAndPreserve(Action<TestFile> action)
        {
            Backup();
            try
            {
                action(this);
            }
            finally
            {
                Restore();
            }
        }

        public void InvokeWithReplacement(TestFile replacement, Action action)
        {
            Backup();
            try
            {
                ReplaceWith(replacement);
                action();
            }
            finally
            {
                Restore();
            }
        }

        private static void ClearReadOnlyAttribute(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.SetAttributes(filePath, File.GetAttributes(filePath) & ~FileAttributes.ReadOnly);
            }
        }
    }
}
