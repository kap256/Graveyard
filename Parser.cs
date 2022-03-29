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
        }

        readonly string HTML;
        readonly string CSV;
        readonly List<Project> Projects = new List<Project>();

        public Parser(string html,string csv)
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
                    project.Name = h2.TextContent;

                    //imgの次のdivから日付を取得
                    var time = tomb.NextElementSibling.QuerySelectorAll("time");
                    if (time.Length != 2) {
                        throw new Exception("time failed.");
                    }
                    project.From = DateTime.Parse(time[0].GetAttribute("datetime"));
                    project.To = DateTime.Parse(time[1].GetAttribute("datetime"));

                    //リストに加える
                    Projects.Add(project);

                } catch (Exception e) {
                    Console.WriteLine($"!ERROR! : {e}");
                    continue;
                }
            }

            //デバッグ出力
            Console.WriteLine($"find tombstones : {Projects.Count}");
        }
    }
}
