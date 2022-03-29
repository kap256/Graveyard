using System;
using System.Collections.Generic;

namespace Graveyard
{
    class Program
    {
        static void Main(string[] args)
        {

            var list = new List<Parser>();
            list.Add(new Parser("microsoft.html", "microsoft.csv"));
            list.Add(new Parser("google.html","google.csv"));

            foreach (var parser in list) {
                parser.Parse();
                parser.OutputCSV();
            }

        }
    }
}
