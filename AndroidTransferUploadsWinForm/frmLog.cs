using System;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;

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

            /*------> Proceso <--------*/
            this.TransferFiles(ConfigurationManager.AppSettings["root"]);

            this.tmrRefresh.Enabled = true;
        }

        #region proceso

        //Descripcion.
        //Siendo archivos01 = 192.168.0.239
        //Posicionarse en \\archivos01\ftp paramedic\express\AndroidUploads\4678913118
        //Recorrer, carpeta x carpeta
        //Por cada archivo dentro de cada carpeta:
        //    a.Ejecutar el método IncGrabaciones.SetAdjunto con el nombre del archivo y nombre de subcarpeta(29, 9672084_131897061540023425.jpg)
        //    b.Devuelve un datable con: carpeta raíz \ subcarpeta \ nombre archivo_1 \ info \ destinatarios
        //    c.Crear la subcarpeta si no existe
        //    d.Mover el archivo inicial al archivo de destino
        //    e.Enviar por e - mail a los destinatarios armando el mail con la info recibida en dicha columna
        //Dentro del directorio hay carpetas con los archivos (nroDeMoviles).
        //Los archivos son Nroincidente_FechaHora.jpg 

        public List<string> TransferFiles(string root)
        {
            if (root == null) { throw new ArgumentNullException("root"); }
            if (string.IsNullOrWhiteSpace(root)) { throw new ArgumentException("The passed value may not be empty or whithespace", "root"); }

            var files = new List<string>();

            var rootDirectory = new DirectoryInfo(root);
            if (rootDirectory.Exists == false) { return files; }

            //overcome problem about slash and backslash with using Contains()
            root = rootDirectory.FullName;
            //if (isFolderValid(root) == false) { return files; }

            var folders = new Queue<string>();
            folders.Enqueue(root);
            while (folders.Count != 0)
            {
                string currentFolder = folders.Dequeue();

                try
                {
                    var currentFiles = Directory.EnumerateFiles(currentFolder, "*.jpg");

                    List<AdjuntoAndroid> adjuntosAndroid = new List<AdjuntoAndroid>();

                    if (currentFiles.Count() > 0)
                    {
                        foreach (var origen in currentFiles)
                        {
                            //DataTable dt = new DataTable();
                            AdjuntoAndroid adjuntoAndroid = new AdjuntoAndroid();
                            EmergencyC.IncGrabaciones grabaciones = new EmergencyC.IncGrabaciones();
                            adjuntoAndroid = grabaciones.SetAdjuntoAndroid<AdjuntoAndroid>(new DirectoryInfo(currentFolder).Name, Path.GetFileName(origen)).FirstOrDefault();

                            if (adjuntoAndroid != null)
                            {
                                ////TODO: Borrar
                                adjuntoAndroid.CarpetaRaiz = "C:\\Paramedic\\AndroidTranferUploads\\AdjuntosParaPrueba\\destino";
                                if (SaveAndRename(origen, adjuntoAndroid.fileFullPath))
                                    adjuntosAndroid.Add(adjuntoAndroid);
                            }
                        }
                        EnviarEmail(adjuntosAndroid);
                    }
                    else
                    {
                        addLog(true, "TransferFiles", string.Format("No se encontraron imagenes en {0}", root));
                    }
                }
                // Ignore this exceptions
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }

                try
                {
                    var currentSubFolders = Directory.GetDirectories(currentFolder);//.Where(f => isFolderValid(f));
                    foreach (string current in currentSubFolders)
                    {
                        folders.Enqueue(current);
                    }
                }
                // Ignore this exceptions
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }
            }
            return files;
        }

        public bool SaveAndRename(string origen, string fileFullPath)
        {
            try
            {
                if (!File.Exists(origen))
                {
                    throw new DirectoryNotFoundException();
                    //// This statement ensures that the file is created,
                    //// but the handle is not kept.
                    //using (FileStream fs = File.Create(origen)) { }
                }

                // Ensure that the target does not exist.
                if (File.Exists(fileFullPath))
                    File.Delete(fileFullPath);

                if (!Directory.Exists(Path.GetDirectoryName(fileFullPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileFullPath));
                }

                // Move the file.
                File.Move(origen, fileFullPath);
                addLog(true, "SaveAndRename: ", string.Format("{0} Se movio a {1}.", origen, fileFullPath));
                // See if the original exists now.
                if (File.Exists(origen))
                    addLog(false, "SaveAndRename: ", "No se pudo eliminar el archivo original.");
                else
                    addLog(true, "SaveAndRename: ", "Se elimino el archivo original correctamente.");

                return true;

            }
            catch (DirectoryNotFoundException e)
            {
                addLog(false, "SaveAndRename: ", string.Format("Directory not found Exception, file: {0}, mensaje: {1}", origen, e.ToString()));
            }
            catch (Exception e)
            {
                addLog(false, "SaveAndRename: ", string.Format("The process failed: {0}", e.ToString()));
            }
            return false;
        }

        private void EnviarEmail(AdjuntoAndroid adjAndroid)
        {
            try
            {
                //.Destinatarios, adjuntoAndroid.CarpetaRaiz + adjuntoAndroid.SubCarpeta + adjuntoAndroid.Archivo
                List<string> To = adjAndroid.Destinatarios.Split(';').ToList();
                string Subject = string.Format("Imagenes del Incidente {0}, cliente {1}. Con fecha {2}", adjAndroid.NroIncidente, adjAndroid.Cliente, adjAndroid.FecIncidente.ToShortDateString());
                string Body = string.Format("Se adjunta (sarasa) del paciente {0}, edad {1}, nro de afiliado {2}, sexo {3}...", adjAndroid.Paciente, adjAndroid.Edad, adjAndroid.NroAfiliado, adjAndroid.Sexo == "M" ? "Masculino" : "Femenino");

                byte[] fileByte = File.ReadAllBytes(adjAndroid.fileFullPath);
                using (var stream = new MemoryStream(fileByte))
                {
                    ContentType contentType = new ContentType(MediaTypeNames.Image.Jpeg);
                    Attachment attachment = new Attachment(stream, adjAndroid.Archivo, contentType.ToString());
                    EmailHelpers.Send(To, Subject, Body, null, attachment);
                }

            }
            catch (Exception ex)
            {
                addLog(false, "EnviarEmail", "Fallo al enviar el email. " + ex.Message);
            }
        }

        private void EnviarEmail(List<AdjuntoAndroid> adjuntosAndroid)
        {
            if (adjuntosAndroid == null || adjuntosAndroid.Count == 0) return;
            try
            {

                var adjAndroid = adjuntosAndroid.FirstOrDefault();

                List<string> To = adjAndroid.Destinatarios.Split(';').ToList();
                string Subject = string.Format(ConfigurationManager.AppSettings["MailSubject"], adjAndroid.FecIncidente.ToShortDateString(), adjAndroid.NroIncidente);
                string Body = string.Format(ConfigurationManager.AppSettings["MailBody"],
                    adjAndroid.FecIncidente.ToShortDateString(),
                    adjAndroid.NroIncidente,
                    adjAndroid.Cliente,
                    adjAndroid.NroInterno,
                    adjAndroid.NroAfiliado,
                    adjAndroid.Paciente,
                    adjAndroid.Sexo == "M" ? "Masculino" : "Femenino",
                    adjAndroid.Edad
                    );
                List<string> PathFiles = new List<string>();

                foreach (var item in adjuntosAndroid)
                {
                    PathFiles.Add(item.fileFullPath);
                }
                EmailHelpers.Send(To, Subject, Body, PathFiles, null);
            }
            catch (Exception ex)
            {
                addLog(false, "EnviarEmail", "Fallo al enviar el email. " + ex.Message);
            }
        }
        #endregion
    }
}
