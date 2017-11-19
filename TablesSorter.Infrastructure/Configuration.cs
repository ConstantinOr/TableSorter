using System.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace TablesSorter.Model
{
    public class Configuration
    {
        public static ConfigurationModel ConfigurationModel { get; set; }

        static Configuration()
        {
            ConfigurationModel = new ConfigurationModel();

            var workDir = ConfigurationManager.AppSettings["work-directory"];
            var resultFileNAme = ConfigurationManager.AppSettings["result-file-name"];
            
            ConfigurationModel.ResultFileName = string.IsNullOrWhiteSpace(resultFileNAme)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Result.html")
                : Path.GetFullPath(FormatFileName(resultFileNAme));

            ConfigurationModel.WorkDirectory = string.IsNullOrWhiteSpace(workDir)
                ? AppDomain.CurrentDomain.BaseDirectory
                : Path.GetFullPath(workDir);
        }

        private static string FormatFileName(string resultFileName)
        {
            var fileName = resultFileName;
            if (!fileName.ToLower().EndsWith(".html"))
                fileName = fileName + ".html";

            return fileName;
        }
    }
}
