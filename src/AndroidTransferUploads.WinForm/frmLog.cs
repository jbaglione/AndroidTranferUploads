using System;
using System.IO;
using System.Windows.Forms;
using AndroidTransferUploads.Service.Core;

namespace AndroidTransferUploadsWinForm
{
    public partial class frmLog : Form
    {
        public frmLog()
        {
            InitializeComponent();
            this.tmrRefresh.Enabled = true;
            this.tmrRefresh_Tick(null, null);
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
                    this.txtLog.Text = "Log " + DateTime.Now.Date + Environment.NewLine;
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
                if (clear) { this.txtLog.Text = ""; }
                this.txtLog.Text = this.txtLog.Text + DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00") + "\t" + rdoStr + "\t" + logProcedure + "\t" + logDescription + Environment.NewLine;

            }

        }

        private void tmrRefresh_Tick(object sender, System.EventArgs e)
        {
            this.tmrRefresh.Enabled = false;


            //List<AdjuntoAndroid> adjuntosAndroid = new List<AdjuntoAndroid>
            //{
            //    new AdjuntoAndroid
            //    {
            //        Destinatarios = "jbaglione@paramedic.com.ar",
            //        NroIncidente = "2F4",
            //        FecIncidente = DateTime.Now,
            //        Cliente = "OSDE",
            //        NroInterno = "1057",
            //        NroAfiliado = "62174581101",
            //        Paciente = "BIONDI, MARGARITA",
            //        Sexo = "Femenino",
            //        Edad = "70",
            //        CarpetaRaiz= "C:\\Users\\Paramedic\\Documents\\",
            //        SubCarpeta = "Baglione",
            //        Archivo = "Error logeo-deslogeo.png"
            //    }
            //};
            //this.EnviarEmail(adjuntosAndroid);
            /*------> Proceso <--------*/
            //this.TransferFiles(ConfigurationManager.AppSettings["root"]);

            Process process = new Process();
            process.addLog += addLog;
            process.TransferFiles();

            this.tmrRefresh.Enabled = true;
        }
    }
}
