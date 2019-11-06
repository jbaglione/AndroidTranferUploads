using System;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Json;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Net.Mail;
using System.Collections.Specialized;


namespace AndroidTransferUploads
{
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
                this.TransferFiles(ConfigurationManager.AppSettings["root"]);
                //}
            }
            catch (Exception ex)
            {
                //addLog(false, "ElapsedHandler", string.Format("Exception. {0}", ex.Message));
            }
            
        }

        private void addLog(bool rdo, string logProcedure, string logDescription)
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
        public void TransferFiles(string root)
        {
            root = GetFullName(root);

            var folders = new Queue<string>();
            folders.Enqueue(root);

            //addLog(true, "TransferFiles", string.Format("folders.Count = {0}.", folders.Count));
            while (folders.Count != 0)
            {
                string currentFolder = folders.Dequeue();
                addLog(true, "TransferFiles", string.Format("currentFolder = {0}", currentFolder));

                try
                {
                    var currentFiles = Directory.EnumerateFiles(currentFolder, "*.jpg");
                    List<AdjuntoAndroid> adjuntosAndroid = new List<AdjuntoAndroid>();

                    addLog(true, "TransferFiles", string.Format("currentFiles.Count = {0}", currentFiles.Count()));

                    if (currentFiles.Count() > 0)
                    {
                        foreach (var origen in currentFiles)
                        {
                            //addLog(true, "TransferFiles", string.Format("origen(currentFile) = {0}", origen));
                            AdjuntoAndroid adjuntoAndroid = new AdjuntoAndroid();
                            EmergencyC.IncGrabaciones grabaciones = new EmergencyC.IncGrabaciones(GetConnectionString());
                            string folderName = new DirectoryInfo(currentFolder).Name;
                            var res = grabaciones.SetAdjuntoAndroid<AdjuntoAndroid>(folderName, Path.GetFileName(origen));
                            if(res.Count > 0)
                            {
                                adjuntoAndroid = res.FirstOrDefault();

                                if (adjuntoAndroid != null)
                                {
                                    //adjuntoAndroid.CarpetaRaiz = "C:\\Paramedic\\AndroidTranferUploads\\AdjuntosParaPrueba\\destino";
                                    if (SaveAndRename(origen, adjuntoAndroid.fileFullPath))
                                        adjuntosAndroid.Add(adjuntoAndroid);
                                }
                            }
                            else
                            {
                                addLog(false, "TransferFiles", string.Format("SetAdjuntoAndroid({0}, {1}) no devolvio ningun resultado",
                                    folderName, Path.GetFileName(origen)));
                            }

                        }
                        EnviarEmail(adjuntosAndroid);
                    }
                    else
                    {
                        //addLog(true, "TransferFiles", string.Format("No se encontraron imagenes en {0}", currentFolder));
                    }
                }
                // Ignore this exceptions
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }
                catch (Exception ex)
                {
                    addLog(false, "TransferFiles", string.Format("Exception. {0}", ex.Message));
                }
                try
                {
                    var currentSubFolders = Directory.GetDirectories(currentFolder);
                    foreach (string current in currentSubFolders)
                    {
                        //addLog(true, "TransferFiles", string.Format("currentSubFolder {0}", current));
                        folders.Enqueue(current);
                    }
                    //addLog(true, "TransferFiles", "out foreach");
                }
                // Ignore this exceptions
                catch (UnauthorizedAccessException) { }
                catch (PathTooLongException) { }
            }
        }

        private string GetFullName(string root)
        {
            //addLog(true, "GetFullName", string.Format("Se inicio con root = {0}", root));
            if (root == null)
            {
                addLog(false, "GetFullName", string.Format("ArgumentNullException root => {0}.", root));
                throw new ArgumentNullException("root");
            }
            if (string.IsNullOrWhiteSpace(root))
            {
                addLog(false, "GetFullName", string.Format("The passed value may not be empty or whithespace root => {0}.", root));
                throw new ArgumentException("The passed value may not be empty or whithespace", "root");
            }

            var rootDirectory = new DirectoryInfo(root);
            if (rootDirectory.Exists == false)
            {
                addLog(false, "GetFullName", string.Format("El directorio {0} no existe.", root));
                throw new ArgumentException("El directorio {0} no existe.", "root");
            }

            return rootDirectory.FullName;
        }

        private static ConnectionStringCache GetConnectionString()
        {
            NameValueCollection appSettings = ConfigurationManager.AppSettings;

            ConnectionStringCache connectionString = new ConnectionStringCache
            {
                Server = appSettings.Get("cacheServer"),
                Port = appSettings.Get("cachePort"),
                Namespace = appSettings.Get("cacheNameSpace"),
                Aplicacion = appSettings.Get("cacheShamanAplicacion"),
                User = appSettings.Get("cacheShamanUser"),
                Centro = appSettings.Get("cacheShamanCentro"),
                Password = appSettings.Get("Password"),
                UserID = appSettings.Get("UserID")
            };
            return connectionString;
        }

        public bool SaveAndRename(string origen, string fileFullPath)
        {
            try
            {
                //addLog(true, "SaveAndRename", string.Format("Origen {0}, destino {1}", origen, fileFullPath));

                if (!File.Exists(origen))
                {
                    throw new DirectoryNotFoundException();
                    //// This statement ensures that the file is created,
                    //// but the handle is not kept.
                    //using (FileStream fs = File.Create(origen)) { }
                }

                ////addLog(true, "SaveAndRename", "File.Exists(fileFullPath), File.Delete(fileFullPath);");
                // Ensure that the target does not exist.
                if (File.Exists(fileFullPath))
                    File.Delete(fileFullPath);

                ////addLog(true, "SaveAndRename", "if (!Directory.Exists(Path.GetDirectoryName(fileFullPath)))");
                if (!Directory.Exists(Path.GetDirectoryName(fileFullPath)))
                {
                    ////addLog(true, "SaveAndRename", "Directory.CreateDirectory(Path.GetDirectoryName(fileFullPath));");
                    Directory.CreateDirectory(Path.GetDirectoryName(fileFullPath));
                }

                //addLog(true, "SaveAndRename", "File.Move(origen, fileFullPath);");
                // Move the file.
                File.Move(origen, fileFullPath);
                //addLog(true, "SaveAndRename: ", string.Format("{0} Se movio a {1}.", origen, fileFullPath));
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

        //private void EnviarEmail(AdjuntoAndroid adjAndroid)
        //{
        //    try
        //    {
        //        //.Destinatarios, adjuntoAndroid.CarpetaRaiz + adjuntoAndroid.SubCarpeta + adjuntoAndroid.Archivo
        //        List<string> To = adjAndroid.Destinatarios.Split(';').ToList();
        //        string Subject = string.Format("Imagenes del Incidente {0}, cliente {1}. Con fecha {2}", adjAndroid.NroIncidente, adjAndroid.Cliente, adjAndroid.FecIncidente.ToShortDateString());
        //        string Body = string.Format("Se adjunta (sarasa) del paciente {0}, edad {1}, nro de afiliado {2}, sexo {3}...", adjAndroid.Paciente, adjAndroid.Edad, adjAndroid.NroAfiliado, adjAndroid.Sexo == "M" ? "Masculino" : "Femenino");

        //        byte[] fileByte = File.ReadAllBytes(adjAndroid.fileFullPath);
        //        using (var stream = new MemoryStream(fileByte))
        //        {
        //            ContentType contentType = new ContentType(MediaTypeNames.Image.Jpeg);
        //            Attachment attachment = new Attachment(stream, adjAndroid.Archivo, contentType.ToString());
        //            EmailHelpers.Send(To, Subject, Body, null, attachment);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        //addLog(false, "EnviarEmail", "Fallo al enviar el email. " + ex.Message);
        //    }
        //}

        private void EnviarEmail(List<AdjuntoAndroid> adjuntosAndroid)
        {
            if (adjuntosAndroid == null || adjuntosAndroid.Count == 0) return;
            try
            {
                var groupedAdjuntosAndroid = adjuntosAndroid
                    .GroupBy(u => u.NroIncidente)
                    .Select(grp => grp.ToList())
                    .ToList();

                bool onlyToday = ConfigurationManager.AppSettings.Get("onlyToday") == "1";

                foreach (List<AdjuntoAndroid> adjuntosAndroidByIncident in groupedAdjuntosAndroid)
                {
                    var adjAndroid = adjuntosAndroidByIncident.FirstOrDefault();
                    string fecIncidenteShort = adjAndroid.FecIncidente.ToShortDateString();

                    ////addLog(true, "EnviarEmail", string.Format("Valores onlyToday={0}, fecIncidenteShort={1}, DateTime.Now.ToShortDateString()={2}, (fec1==Now)={3}", onlyToday, fecIncidenteShort, DateTime.Now.ToShortDateString(), (fecIncidenteShort == DateTime.Now.ToShortDateString()).ToString()));
                    
                    if (!onlyToday || fecIncidenteShort == DateTime.Now.ToShortDateString())
                    {
                        List<string> To = adjAndroid.Destinatarios.Split(';').ToList();//.Where(x => x == "jbaglione@paramedic.com.ar")
                        string Subject = string.Format(ConfigurationManager.AppSettings["MailSubject"], fecIncidenteShort, adjAndroid.NroIncidente);
                        string Body = string.Format(ConfigurationManager.AppSettings["MailBody"],
                            fecIncidenteShort,
                            adjAndroid.NroIncidente,
                            adjAndroid.Cliente,
                            adjAndroid.NroInterno,
                            adjAndroid.NroAfiliado,
                            adjAndroid.Paciente,
                            adjAndroid.Sexo == "M" ? "Masculino" : "Femenino",
                            adjAndroid.Edad
                            );
                        List<string> PathFiles = new List<string>();

                        foreach (var item in adjuntosAndroidByIncident)
                        {
                            PathFiles.Add(item.fileFullPath);
                        }
                        ////addLog(true, "EnviarEmail", string.Format("Valores EmailHelpers.Send(adjAndroid.Destinatarios={0}, Subject={1}, Body={2})", adjAndroid.Destinatarios, Subject, Body));
                        if(EmailHelpers.Send(To, Subject, Body, PathFiles, null))
                        {
                            addLog(true, "EnviarEmail", string.Format("Mail enviado, Valores=> Destinatarios={0}, Subject={1}, Body={2})", adjAndroid.Destinatarios, Subject, Body));
                        }
                        else
                        {
                            addLog(false, "EnviarEmail", string.Format("Mail no enviado, Valores=> Destinatarios={0}, Subject={1}, Body={2})", adjAndroid.Destinatarios, Subject, Body));
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                addLog(false, "EnviarEmail", "Fallo al enviar el email. " + ex.Message);
            }
        }
        #endregion
    }


}
