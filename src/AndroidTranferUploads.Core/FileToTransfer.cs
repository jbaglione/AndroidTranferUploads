namespace AndroidTransferUploads.Service.Core.Models
{
    public class FileToTransfer
    {
        public string CarpetaRaiz { get; set; }
        public string SubCarpeta { get; set; }
        public string Archivo { get; set; }

        public string fileFullPath
        {
            get
            {
                return CarpetaRaiz + "\\" + SubCarpeta + "\\" + Archivo;
            }
        }
    }
}