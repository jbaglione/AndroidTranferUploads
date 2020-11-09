using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using AndroidTransferUploads.Service.Core.Models;
using AndroidTransferUploads.Service.Core.Helpers;

namespace AndroidTransferUploads.Service.Core
{
    public class Process
    {
        public delegate void AddLog(bool rdo, string logProcedure, string logDescription, bool clear = false);
        public AddLog addLog;

        //Configs
        string root = ConfigurationManager.AppSettings["root"];
        bool transferFichadas = ConfigurationManager.AppSettings["TransferFichadas"] == "1";
        bool transferElectros = ConfigurationManager.AppSettings["TransferElectros"] == "1";
        string ingresosPathDest = ConfigurationManager.AppSettings["ingresosPathDest"];
        bool onlyToday = ConfigurationManager.AppSettings.Get("onlyToday") == "1";

        string mailSubject = ConfigurationManager.AppSettings["MailSubject"];
        string mailBody = ConfigurationManager.AppSettings["MailBody"];

        bool fullLog = ConfigurationManager.AppSettings["fullLog"] == "1";

        //Descripcion.
        //Posicionarse en root
        //Recorrer, carpeta x carpeta
        //Por cada archivo dentro de cada carpeta:
        //  1. Si es Electro
        //    a.Ejecutar el método IncGrabaciones.SetAdjunto con el nombre del archivo y nombre de subcarpeta(29, 9672084_131897061540023425.jpg)
        //    b.Devuelve un datable con: carpeta raíz \ subcarpeta \ nombre archivo_1 \ info \ destinatarios
        //    c.Crear la subcarpeta si no existe
        //    d.Mover el archivo inicial al archivo de destino
        //    e.Enviar por e - mail a los destinatarios armando el mail con la info recibida en dicha columna
        // 2. Si esFichada
        //      a.
        //      b. 
        //Dentro del directorio hay carpetas con los archivos (nroDeMoviles).
        public void TransferFiles()
        {
            root = GetFullName(root);

            var folders = new Queue<string>();
            folders.Enqueue(root);

            while (folders.Count != 0) //recorro carpetas de moviles
            {
                string currentFolder = folders.Dequeue();

                if(fullLog)
                    addLog(true, "TransferFiles", string.Format("Buscando imagenes en {0}", currentFolder));

                TransferFiles(folders, currentFolder);
            }
        }


        private void TransferFiles(Queue<string> folders, string currentFolder)
        {
            try
            {
                var currentFiles = Directory.EnumerateFiles(currentFolder, "*.jpg");

                if (currentFiles.Count() > 0)
                {
                    addLog(true, "TransferFiles", string.Format("Se encontraron {0} imagenes en {1}", currentFiles.Count(), currentFolder));

                    if (currentFolder.Contains("Fichada") && transferFichadas)
                    {
                        TransferFichadas(currentFiles);
                    } else if (currentFolder.Contains("Images") && transferElectros)
                    {
                        TransferElectros(currentFolder, currentFiles);
                    } else //Fix because some electro images, are saving in the root of movile.
                    {
                        TransferElectros(currentFolder, currentFiles);
                    }
                }
                else
                {
                    if (fullLog)
                        addLog(true, "TransferFiles", string.Format("No se encontraron imagenes en {0}", currentFolder));
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


        private void TransferElectros(string currentFolder, IEnumerable<string> currentFiles)
        {
            addLog(true, "TransferElectros", string.Format("Se transferiran {0} Electros", currentFiles.Count()));
            List<AdjuntoAndroid> adjuntosAndroid = new List<AdjuntoAndroid>();
            foreach (var origen in currentFiles)
            {
                //addLog(true, "TransferFiles", string.Format("origen(currentFile) = {0}", origen));
                AdjuntoAndroid adjuntoAndroid = new AdjuntoAndroid();
                EmergencyC.IncGrabaciones grabaciones = new EmergencyC.IncGrabaciones(GetConnectionString());

                string folderName = new DirectoryInfo(currentFolder).Name;
                if(folderName == "Images")
                {
                    folderName = Directory.GetParent(currentFolder).Name;
                }
                var res = grabaciones.SetAdjuntoAndroid<AdjuntoAndroid>(folderName, Path.GetFileName(origen));

                if (res != null && res.Count > 0)
                {
                    adjuntoAndroid = res.FirstOrDefault();

                    if (adjuntoAndroid != null)
                    {
                        if (SaveAndRename(origen, adjuntoAndroid.fileFullPath))
                            adjuntosAndroid.Add(adjuntoAndroid);
                    }
                }
                else
                {
                    addLog(false, "TransferElectros", string.Format("SetAdjuntoAndroid({0}, {1}) no devolvio ningun resultado",
                        folderName, Path.GetFileName(origen)));
                }
            }
            EnviarEmail(adjuntosAndroid);
        }

        private void TransferFichadas(IEnumerable<string> currentFiles)
        {
            addLog(true, "TransferFichadas", string.Format("Se transferiran {0} Fichadas", currentFiles.Count()));
            List<FileToTransfer> fichadasFileToTransfer = new List<FileToTransfer>();

            foreach (var origen in currentFiles)
            {
                //addLog(true, "TransferFichadas", string.Format("origen(currentFile) = {0}", origen));
                FileToTransfer fichadaFileToTransfer = new FileToTransfer();

                fichadaFileToTransfer.CarpetaRaiz = ingresosPathDest;
                fichadaFileToTransfer.SubCarpeta = GetAnsiDateFromFileName(origen);
                fichadaFileToTransfer.Archivo = Path.GetFileName(origen);
                //adjuntoAndroid.CarpetaRaiz = "C:\\Paramedic\\AndroidTranferUploads\\AdjuntosParaPrueba\\destino";

                if (SaveAndRename(origen, fichadaFileToTransfer.fileFullPath))
                    fichadasFileToTransfer.Add(fichadaFileToTransfer);
            }
            addLog(true, "TransferFichadas", string.Format("Se transfirieron {0} Fichadas correctamente", fichadasFileToTransfer.Count));
        }

        private string GetAnsiDateFromFileName(string origen)
        {
            //origen = "1903606_132491406120165099.jpg";
            long fileTimeUtc = Convert.ToInt64(origen.Split('_').Last().Split('.').First());

            DateTime fCreationTime = DateTime.FromFileTime(fileTimeUtc);


            if (fCreationTime > DateTime.Now)
            {
                addLog(false, "GetAnsiDateFromFileName", $"La fecha del archivo es mayor a la fecha actual ({fCreationTime})");
            }
            else if (fCreationTime == DateTime.MinValue)
            {
                addLog(false, "GetAnsiDateFromFileName", $"Error al obtener la fecha del archivo (min date value: {fCreationTime})");
            }

            return modFechasCs.DtoN(fCreationTime).ToString();
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

        public bool SaveAndRename(string origen, string destino)
        {
            try
            {
                addLog(true, "SaveAndRename", string.Format("Origen {0}, destino {1}", origen, destino));

                if (!File.Exists(origen))
                {
                    throw new DirectoryNotFoundException();
                    //// This statement ensures that the file is created,
                    //// but the handle is not kept.
                    //using (FileStream fs = File.Create(origen)) { }
                }

                ////addLog(true, "SaveAndRename", "File.Exists(fileFullPath), File.Delete(fileFullPath);");
                // Ensure that the target does not exist.
                if (File.Exists(destino))
                    File.Delete(destino);

                ////addLog(true, "SaveAndRename", "if (!Directory.Exists(Path.GetDirectoryName(fileFullPath)))");
                if (!Directory.Exists(Path.GetDirectoryName(destino)))
                {
                    ////addLog(true, "SaveAndRename", "Directory.CreateDirectory(Path.GetDirectoryName(fileFullPath));");
                    Directory.CreateDirectory(Path.GetDirectoryName(destino));
                }

                //addLog(true, "SaveAndRename", "File.Move(origen, fileFullPath);");
                // Move the file.
                File.Move(origen, destino);
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
                addLog(false, "SaveAndRename: ", string.Format("Excepcion, director no encontrado, file: {0}, mensaje: {1}", origen, e.ToString()));
            }
            catch (Exception e)
            {
                addLog(false, "SaveAndRename: ", string.Format("Excepcion, el proceso falló: {0}", e.ToString()));
            }
            return false;
        }

        private void EnviarEmail(List<AdjuntoAndroid> adjuntosAndroid)
        {
            if (adjuntosAndroid == null || adjuntosAndroid.Count == 0) return;
            try
            {
                var groupedAdjuntosAndroid = adjuntosAndroid
                    .GroupBy(u => u.NroIncidente)
                    .Select(grp => grp.ToList())
                    .ToList();

                foreach (List<AdjuntoAndroid> adjuntosAndroidByIncident in groupedAdjuntosAndroid)
                {
                    var adjAndroid = adjuntosAndroidByIncident.FirstOrDefault();
                    string fecIncidenteShort = adjAndroid.FecIncidente.ToShortDateString();

                    if (fullLog)
                        addLog(true, "EnviarEmail", string.Format("Valores onlyToday={0}, fecIncidenteShort={1}, DateTime.Now.ToShortDateString()={2}, (fec1==Now)={3}", onlyToday, fecIncidenteShort, DateTime.Now.ToShortDateString(), (fecIncidenteShort == DateTime.Now.ToShortDateString()).ToString()));

                    if (!onlyToday || fecIncidenteShort == DateTime.Now.ToShortDateString())
                    {
                        List<string> To = adjAndroid.Destinatarios.Split(';').ToList();//.Where(x => x == "jbaglione@paramedic.com.ar")
                        string Subject = string.Format(mailSubject, fecIncidenteShort, adjAndroid.NroIncidente);
                        string Body = string.Format(mailBody,
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

                        if (fullLog)
                            addLog(true, "EnviarEmail", string.Format("Valores EmailHelpers.Send(adjAndroid.Destinatarios={0}, Subject={1}, Body={2})", adjAndroid.Destinatarios, Subject, Body));
                        if (EmailHelpers.Send(To, Subject, Body, PathFiles, null))
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
    }
}
