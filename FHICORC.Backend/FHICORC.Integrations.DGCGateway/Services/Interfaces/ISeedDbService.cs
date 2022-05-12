using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHICORC.Integrations.DGCGateway.Services.Interfaces
{
    public interface ISeedDbService
    {
        public void SeedDatabase();
        public void GetInfoAboutDb();
    }
}
