using Microsoft.EntityFrameworkCore;

namespace FHICORC.ApplicationHost.Hangfire
{
    public class HangfireContext : DbContext
    {
        public HangfireContext(DbContextOptions<HangfireContext> options)
            : base(options) { }
    }
}
