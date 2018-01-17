using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalDriveToNetDrive
{
    //copy files from sms db to net drive
    class Program
    {
        public static string connStrSms = "SERVER=10.10.97.71;dsn=sapp_sms;DATABASE=sapp_sms;UID=root;PASSWORD=onlyoffice;default command timeout=999;";
        public static string netFoler = "V:\\Uxtrata\\";
        public static string frequency = "30";
        static void Main(string[] args)
        {
            string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalDriveToNetDrive.ini");
            if (File.Exists(destPath))
            {
                string[] lines = File.ReadAllLines(destPath);
                foreach (string line in lines)
                {
                    if (line.Contains("netFoler:"))
                    {
                        netFoler = line.Replace("netFoler:", "");
                    }
                    if (line.Contains("connStrSms:"))
                    {
                        connStrSms = line.Replace("connStrSms:", "");
                    }
                    if (line.Contains("frequency:"))
                    {
                        frequency = line.Replace("frequency:", "");
                    }
                }
            }
            else
            {
                File.WriteAllText(destPath, "netFoler:" + netFoler + Environment.NewLine + "connStrSms:" + connStrSms + Environment.NewLine + "frequency:" + frequency + Environment.NewLine);
            }
            OdbcConnection connSms = new OdbcConnection(connStrSms);
            connSms.Open();

            while (true)
            {
                try
                { 
                    string sql = "select system_value from system where system_code = 'FILEFOLDER'";
                    OdbcCommand commSms = new OdbcCommand(sql, connSms);
                    OdbcDataReader dbrd = commSms.ExecuteReader();
                    string filefolder = "";
                    if (dbrd.HasRows && dbrd.Read())
                    {
                        filefolder = dbrd[0].ToString();
                    }
                    Directory.CreateDirectory(filefolder);
                    Directory.CreateDirectory(netFoler);
                    foreach (string dirPath in Directory.GetDirectories(filefolder, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(dirPath.Replace(filefolder, netFoler));
                    }
                    foreach (string newPath in Directory.GetFiles(filefolder, "*.*", SearchOption.AllDirectories))
                    {
                        File.Copy(newPath, newPath.Replace(filefolder, netFoler), true);
                    }

                    System.IO.DirectoryInfo di = new DirectoryInfo(filefolder);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    di.Attributes &= ~FileAttributes.ReadOnly;

                    Thread.Sleep(int.Parse(frequency) * 1000);
                }
                catch(Exception e)
                {
                    string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LocalDriveToNetDrive.Error.log");
                    File.AppendAllText(logPath, System.DateTime.Now + ": " + e + Environment.NewLine);

                    Thread.Sleep(int.Parse(frequency) * 1000);
                }

            }
            
        }
    }
}
