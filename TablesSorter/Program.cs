using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using TablesSorter.Infrastructure;
using TablesSorter.Model;

namespace TablesSorter
{
    using Newtonsoft.Json;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            var directory = Configuration.ConfigurationModel.WorkDirectory;
            if (!Directory.Exists(directory))
            {
                Console.WriteLine("Work Directory doesn't exiat. Please correct value in config.");
                Console.Write("To Continue press Enter...");
                Console.ReadLine();
                return;
            }

            var rawfiles = Directory.GetFiles(directory);
            
            var files = rawfiles.Where(w=>!Path.GetFileName(w.ToLower()).StartsWith("template") && !Path.GetFileName(w.ToLower()).StartsWith("result")).ToList();
            if (files.Count == 0)
            {
                Console.WriteLine("Work Directory is empty.");
                Console.Write("To Continue press Enter...");
                Console.ReadLine();
                return;
            }

            try
            {
                var fileProcessor = new FileProcessor(files.ToArray());
                var processAsync = fileProcessor.ProcessAsync();
                Task.WaitAll(processAsync);

                var result = new List<RowModel>();
                foreach (var task in processAsync)
                {
                    result.AddRange(task.Result);
                }

                //Get unique list
                var uniqueResult = result.OrderByDescending(o => o.UpdateOn).GroupBy(g => g.Asin.Content).Select(s => s.First()).ToList();                
                //Get group Diff>40
                var group40 = uniqueResult.Where(w => Convert.ToDecimal(w.Diff.Content) > 40).OrderByDescending(o=>o.UpdateOn).ToList();
                //Get group 40>Diff>30
                var group3040 = uniqueResult.Where(w => Convert.ToDecimal(w.Diff.Content) > 30 && Convert.ToDecimal(w.Diff.Content) < 40).OrderByDescending(o => o.UpdateOn).ToList();

                var templateStream = new StreamReader("Template.html");
                var fileName40 = "";
                var fileName3040 = "";
                try
                {
                    var templateContent = templateStream.ReadToEnd();

                    var txtContent = JsonConvert.SerializeObject(uniqueResult);
                    var content = templateContent.Replace("$$$$$", txtContent);

                    var txtContent40 = JsonConvert.SerializeObject(group40);
                    var content40 = templateContent.Replace("$$$$$", txtContent40);

                    var txtContent3040 = JsonConvert.SerializeObject(group3040); ;
                    var content3040 = templateContent.Replace("$$$$$", txtContent3040);

                    var fileName = Path.GetFileNameWithoutExtension(Configuration.ConfigurationModel.ResultFileName);

                    fileName40 = Configuration.ConfigurationModel.ResultFileName.Replace(fileName, fileName + "40");
                    fileName3040 = Configuration.ConfigurationModel.ResultFileName.Replace(fileName, fileName + "30");

                    //Save result
                    File.WriteAllText(Configuration.ConfigurationModel.ResultFileName, content);
                    File.WriteAllText(fileName40, content40);
                    File.WriteAllText(fileName3040, content3040);

                }
                finally
                {
                    templateStream.Close();
                    Console.WriteLine($"Done your file: {Configuration.ConfigurationModel.ResultFileName}");
                    Console.WriteLine($"Done your file: {fileName40}");
                    Console.WriteLine($"Done your file: {fileName3040}");
                }

            }
            catch (AggregateException error)
            {
                Console.WriteLine(error);
                throw;
            }

        }
    }
}
