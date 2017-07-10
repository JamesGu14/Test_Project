using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Logger
    {
        
        public static void Log(string content)
        {
            string logPath = "C:\\log\\log.txt";

            if (!File.Exists(logPath))
            {
                File.Create(logPath);
            }

            StreamWriter writer = null;
            try
            {
                writer = File.AppendText(logPath);
                writer.WriteLine("[{0}] - {1}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss"), content);
                writer.Flush();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }

        public static void LogPure(string content)
        {
            string logPath = "C:\\log\\log.txt";

            StreamWriter writer = null;
            try
            {
                writer = File.AppendText(logPath);
                writer.WriteLine("{0}", content);
                writer.Flush();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
        }
    }
}
