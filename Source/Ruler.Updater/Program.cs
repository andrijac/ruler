using Ruler.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ruler.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            // Expected 4 arguments:
            // args[0] = Source executable path
            // args[1] = Source config path
            // args[2] = Target directory
            // args[3] = Executable name
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: Ruler.Updater <sourceExePath> <sourceConfigPath> <targetDirectory> <executableName>");
                return;
            }

            string sourceRulerExe = args[0];
            string sourceRulerExeConfig = args[1];
            string targetDirectory = args[2];
            string exeName = args[3];
            Console.WriteLine("Ruler Updater initiated...");

            try
            {
                // 1. Give the main application time to completely shut down and release file locks
                Thread.Sleep(1500);

                // Explicitly wait for any lingering process to exit (using name without extension)
                try
                {
                    string processName = Path.GetFileNameWithoutExtension(exeName);
                    foreach (var process in Process.GetProcessesByName(processName))
                    {
                        process.WaitForExit(5000);
                    }
                }
                catch { }

                // 2. Copy only the executable file

                string targetFile = Path.Combine(targetDirectory, exeName);
                string targetFileConfig = Path.Combine(targetDirectory, $"{exeName}.config");

                if (File.Exists(sourceRulerExe))
                {
                    Console.WriteLine($"Copying updated executable to: {targetFile}");
                    File.Copy(sourceRulerExe, targetFile, overwrite: true);
                }
                else
                {
                    Console.WriteLine($"Error: Executable '{exeName}' not found in source staging directory.");
                    return;
                }
                if (File.Exists(sourceRulerExeConfig))
                {
                    Console.WriteLine($"Copying updated config to: {targetFileConfig}");
                    File.Copy(sourceRulerExeConfig, targetFileConfig, overwrite: true);
                }
                else
                {
                    Console.WriteLine($"Warning: Config file '{exeName}.config' not found in source staging directory.");
                }
                // 3. Comprehensive Cleanup Routine
                Console.WriteLine("Cleaning up temporary update files and archives...");
                string sourceDirectory = Path.GetDirectoryName(sourceRulerExe);
                DeleteDirectoryWithRetry(sourceDirectory);
                DeleteFileIfExists(Path.Combine(targetDirectory, UpdateConstants.ManifestFileName));
                DeleteFileIfExists(Path.Combine(targetDirectory, UpdateConstants.ManifestSigFileName));
                DeleteFileIfExists(Path.Combine(targetDirectory, UpdateConstants.ZipFileName));
                PurgeTempUpdateZips();

                // 5. Restart the updated application
                if (File.Exists(targetFile))
                {
                    Console.WriteLine("Restarting application...");
                    Process.Start(targetFile);
                }
                else
                {
                    Console.WriteLine($"Warning: Executable not found at {targetFile}, unable to restart automatically.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error during update execution: {ex.Message}");
            }
        }

        private static void DeleteDirectoryWithRetry(string path, int maxRetries = 3, int delayMs = 500)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, recursive: true);
                        break;
                    }
                }
                catch
                {
                    if (i == maxRetries - 1)
                    {
                        throw;
                    }
                    Thread.Sleep(delayMs);
                }
            }
        }
        private static void DeleteFileIfExists(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to delete file '{filePath}'. Exception: {ex.Message}");
            }
        }

        private static void PurgeTempUpdateZips()
        {
            try
            {
                string tempPath = Path.GetTempPath();
                // Use the shared constant pattern or base name for cleanup
                string searchPattern = $"*{UpdateConstants.ZipFileName}";
                string[] leftoverZips = Directory.GetFiles(tempPath, searchPattern);
                foreach (string zip in leftoverZips)
                {
                    try
                    {
                        File.Delete(zip);
                    }
                    catch
                    {
                        // Suppress individual file deletion locks if open elsewhere 
                    }
                }
            }
            catch
            {
                // Suppress errors during global temp scan
            }
        }
    }
}
