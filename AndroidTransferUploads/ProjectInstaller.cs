using System.ComponentModel;
using System.Configuration.Install;

namespace AndroidTransferUploadsNoShaman
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
