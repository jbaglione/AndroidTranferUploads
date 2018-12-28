using System;

namespace AndroidTransferUploadsWinForm
{
    public class AdjuntoAndroid
    {
        public string CarpetaRaiz { get; set; }
        public string SubCarpeta { get; set; }
        public string Archivo { get; set; }
        public DateTime FecIncidente { get; set; } //<--Ansi long
        public string NroIncidente { get; set; }
        public string Cliente { get; set; }
        public string NroAfiliado { get; set; }
        public string Paciente { get; set; }
        public string Sexo { get; set; }
        public string Edad { get; set; }
        public string Destinatarios { get; set; }
        public string NroInterno { get; set; }

        public string fileFullPath
        {
            get
            {
                return CarpetaRaiz + "\\" + SubCarpeta + "\\" + Archivo;
            }
        }

    }
}
