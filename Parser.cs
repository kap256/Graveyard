using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Graveyard
{
    class Parser
    {
        struct Project
        {
            public string Name;
            public DateTime From;
            public DateTime To;
            public int Month {
                get {
                    var years = To.Year - From.Year;
                    var month = To.Month - From.Month;

                    return years * 12 + month;
                }
            }
            public int Years {
                get {
                    return Month / 12;
                }
            }

            public static string Header =>
                "プロジェクト名,生年,享年,年齢,月齢";

            public string Body =>
                $"{Name},{ToStr(From)},{ToStr(To)},{Years},{Month}";

            private string ToStr(DateTime d) =>
                d.ToString("yyyy/MM");

        }

        readonly string HTML;
        readonly string CSV;
        readonly List<Project> Projects = new List<Project>();

        public Parser(string html, string csv)
        {
            HTML = html;
            CSV = csv;
        }

        public void Parse()
        {
            //HTMLをパース
            var html = File.ReadAllText(HTML, System.Text.Encoding.UTF8);
            var parser = new HtmlParser();
            var doc = parser.ParseDocument(html);

            //ギロチン（終了予定）ではなく墓石（終了済み）のみ抽出
            var imgs = doc.Body.QuerySelectorAll("li img");
            var tombs = imgs.Where(img => img.GetAttribute("alt") == "Tombstone");

            foreach (var tomb in tombs) {
                try {
                    Project project;

                    //imgの親から次のdivに進んで、プロジェクト名を取得
                    var h2 = tomb.ParentElement.NextElementSibling.QuerySelector("h2");
                    if (h2 == null) {
                        throw new Exception("h2 failed.");
                    }
                    project.Name = TrimMiddle(h2.TextContent);

                    //imgの次のdivから日付を取得
                    var time = tomb.NextElementSibling.QuerySelectorAll("time");
                    if (time.Length != 2) {
                        throw new Exception("time failed.");
                    }
                    project.From = DateTime.Parse(time[0].GetAttribute("datetime"));
                    project.To = DateTime.Parse(time[1].GetAttribute("datetime"));

                    //リストに加える
                    Projects.Add(project);
                    Console.WriteLine($"{project.Body}");

                } catch (Exception e) {
                    Console.WriteLine($"!ERROR! : {e}");
                    continue;
                }
            }

            //デバッグ出力
            Console.WriteLine($"find tombstones : {Projects.Count}");

        }

        public void OutputCSV()
        {
            using (var writer = new StreamWriter(CSV)) {
                //ヘッダ行
                writer.WriteLine(Project.Header);
                //ボディー
                foreach (var project in Projects) {
                    writer.WriteLine(project.Body);
                }
            }
        }


        private string TrimMiddle(string str)
        {
            var separators = new char[] { ' ', '\n', '\t' };
            var names = str.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            var ret = "";
            foreach (var n in names) {
                ret += n + " ";
            }

            return ret.Trim();
        }
    }
}
