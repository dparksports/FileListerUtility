using System;
using System.IO;
using System.Linq;

namespace FileLister
{
    public static class AppDataHelper
    {
        private static string _storagePath = string.Empty;

        public static string GetStoragePath()
        {
            if (string.IsNullOrEmpty(_storagePath))
            {
                Initialize();
            }
            return _storagePath;
        }

        public static string GetFilePath(string filename)
        {
            return Path.Combine(GetStoragePath(), filename);
        }

        public static void Initialize()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _storagePath = Path.Combine(appData, "MagicPoint", "MyFiles");

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            MigrateLegacyData();
        }

        private static void MigrateLegacyData()
        {
            try
            {
                string sourceDir = AppDomain.CurrentDomain.BaseDirectory;
                
                // Files to migrate
                string[] patterns = new[] 
                { 
                    "settings.json", 
                    "scan_history.json",
                    "last_active_scan.txt",
                    "scan_*.csv" 
                };

                foreach (var pattern in patterns)
                {
                    try
                    {
                        var files = Directory.GetFiles(sourceDir, pattern);
                        foreach (var file in files)
                        {
                            string fileName = Path.GetFileName(file);
                            string destPath = Path.Combine(_storagePath, fileName);

                            // Only move if it doesn't exist in destination (safety first)
                            // Or overwrite? For settings/history, preserving "new" app data might satisfy user better if previously installed.
                            // But usually migration happens once.
                            if (!File.Exists(destPath))
                            {
                                File.Move(file, destPath);
                            }
                            else
                            {
                                // If it exists in destination, we probably shouldn't overwrite blindly.
                                // But for now, we leave the source file? Or delete it to cleanup?
                                // Let's leave it to be safe, but maybe rename source?
                                // Simpler: Just don't move.
                            }
                        }
                    }
                    catch { /* individual pattern fail */ }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Migration Error: {ex.Message}");
            }
        }
    }
}
