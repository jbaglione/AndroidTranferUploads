using System;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using System.Configuration;
using AndroidTransferUploads.Service.Core;

namespace AndroidTransferUploads.Service
{
    //run on activia
    public partial class Service1 : ServiceBase
    {
        Timer t = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            t.Elapsed += delegate { ElapsedHandler(); };
            t.Interval = 40000;
            t.Start();
        }

        protected override void OnPause()
        {
            t.Stop();
        }

        protected override void OnContinue()
        {
            t.Start();
        }

        protected override void OnStop()
        {
            t.Stop();
        }

        public void ElapsedHandler()
        {
            try
            {
                ///*------> Conecto a DB <---------*/
                //if (this.setConexionDB())
                //{
                /*------> Proceso <--------*/
                //this.TransferFiles(ConfigurationManager.AppSettings["root"]);
                //}
                Process process = new Process();
                process.addLog += addLog;
                process.TransferFiles();
            }
            catch (Exception ex)
            {
                addLog(false, "ElapsedHandler", string.Format("Exception. {0}", ex.Message));
            }
        }

        private void addLog(bool rdo, string logProcedure, string logDescription, bool clear = false)
        {

            string path;

            path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            path = path + "\\" + modFechasCs.DateToSql(DateTime.Now).Replace("-", "_") + ".log";

            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("Log " + DateTime.Now.Date);
                }
            }

            using (StreamWriter sw = File.AppendText(path))
            {
                string rdoStr = "Ok";
                if (rdo == false)
                {
                    rdoStr = "Error";
                }
                sw.WriteLine(DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + "\t" + rdoStr + "\t" + logProcedure + "\t" + logDescription);
            }
        }
    }
}
