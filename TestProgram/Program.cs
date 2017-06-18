using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProgram
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }

        public void ParseCsv(string filePath)
        {
            FileStream fs = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

            StreamReader sr = new StreamReader(fs, Encoding.UTF8);

            var line = "";
            while((line = sr.ReadLine()) != null)
            {
                List<string> items = LineSplitter(line).ToList();


            }
        }

        private IEnumerable<string> LineSplitter(string line)
        {
            int fieldStart = 0;
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == ',')
                {
                    yield return line.Substring(fieldStart, i - fieldStart);
                    fieldStart = i + 1;
                }
                if (line[i] == '"')
                    for (i++; line[i] != '"'; i++) { }
            }

            yield return line.Substring(line.LastIndexOf(",") + 1);
        }
    }
}
