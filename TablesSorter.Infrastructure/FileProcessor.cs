using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using TablesSorter.Model;


namespace TablesSorter.Infrastructure
{
    using System.Diagnostics.CodeAnalysis;

    public class FileProcessor
    {
        private const int TOTAL_CELL_IN_ROW = 20;
        private string[] _files;

        public FileProcessor(string[] files)
        {
            _files = files;
        }

        public Task<RowModel[]>[] ProcessAsync()
        {
            var counter = 0;
            var taskList = new List<Task<RowModel[]>>();
            foreach (var file in _files)
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    continue;
                }

                var task = Task.Run(() => ReadFromFile(file));
                taskList.Add(task);
            }

            return taskList.ToArray();
        }

        private Task<RowModel[]> ReadFromFile(string file)
        {
            var stream = new StreamReader(file);
            var result = new TaskCompletionSource<RowModel[]>();
            var rawResult = new List<RowModel>();

            Console.WriteLine($"Process file:{file}");
            try
            {
                var content = stream.ReadToEnd();

                var document = new HtmlDocument();
                document.LoadHtml(content);

                var rows = document.DocumentNode.SelectNodes("//body/table[1]/tr");
                if (rows != null && rows.Count > 0)
                {
                    var count = 0;
                    foreach (var row in rows)
                    {
                        count++;
                        if (count == 1)
                            continue;

                        if (row.ChildNodes.Count < TOTAL_CELL_IN_ROW)
                        {
                            continue;
                        }

                        var rowModel = new RowModel();
                        var nodes = row.ChildNodes;

                        var numberNode = nodes[0];
                        rowModel.Number = GetValue(numberNode);

                        var updatedOnNode = nodes[1];
                        rowModel.UpdateOn = GetValue(updatedOnNode);

                        var titleNode = nodes[2];
                        rowModel.Title = GetValue(titleNode);

                        var asinNode = nodes[3];
                        rowModel.Asin = GetValueFromLink(asinNode);

                        var lowestMFNode = nodes[4];
                        rowModel.LowestMF = GetValueFromTable(lowestMFNode);

                        var goodNode = nodes[5];
                        rowModel.Good = GetValueFromTable(goodNode);

                        var diffNode = nodes[6];
                        rowModel.Diff = GetValue(diffNode);

                        var lowestNode = nodes[7];
                        rowModel.AvgLowestUsed = GetValue(lowestNode);

                        var lowestNewNode = nodes[8];
                        rowModel.LowestNew = GetValueFromLink(lowestNode);

                        var lowestFBA = nodes[9];
                        rowModel.LowestFBA = GetValueFromLink(lowestFBA);

                        var userCountNode = nodes[10];
                        rowModel.UsedCount = GetValue(userCountNode);

                        rowModel.UsedBB = new Complex(nodes[11].InnerHtml);

                        rowModel.Amazon = new Complex(nodes[12].InnerHtml);

                        rowModel.Profit = new Complex(nodes[13].InnerHtml);

                        rowModel.ROI = new Complex(nodes[14].InnerHtml);

                        rowModel.AverageRank = new Complex(nodes[15].InnerHtml);

                        rowModel.SalesCount = GetValue(nodes[16]);

                        rowModel.Publish = new Complex(nodes[17].InnerHtml);

                        rowModel.Weight = new Complex(nodes[18].InnerHtml);

                        rowModel.Research = new Complex(nodes[19].InnerHtml);

                        rawResult.Add(rowModel);
                    }
                }

                result.SetResult(rawResult.ToArray());

            }
            catch (AggregateException error)
            {
                Console.WriteLine(error);
                throw;
            }
            finally
            {
                stream.Close();
            }

            return result.Task;
        }

        private Complex GetValueFromLink(HtmlNode lowestNode)
        {
            return new Complex() { Content = lowestNode.InnerHtml, ValueForSort = lowestNode.ChildNodes[0].InnerText };
        }

        private Complex GetValueFromTable(HtmlNode node)
        {
            var rawSortValue = node.SelectSingleNode("//table/tr[1]/td[1]/a[1]").InnerText;
            var sortValueMatch = Regex.Match(rawSortValue, "\\d+?\\.\\d+");
            var sortValue = sortValueMatch.Success ? sortValueMatch.Value : "0";

            var result = new Complex() { Content = node.InnerHtml, ValueForSort = sortValue };

            return result;
        }

        private Complex GetValue(HtmlNode node)
        {
            var result = new Complex() { ValueForSort = node.InnerHtml, Content = node.InnerHtml };
            return result;
        }
    }
}
