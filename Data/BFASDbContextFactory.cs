using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceBank.Data
{
    /// <summary>
    /// Design-time factory for BFASDbContext to support EF migrations
    /// </summary>
    public class BFASDbContextFactory : IDesignTimeDbContextFactory<BFASDbContext>
    {
        public BFASDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BFASDbContext>();
            var connectionString = "Server=localhost\\SQLEXPRESS;Database=BFASdatabase;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;MultipleActiveResultSets=true;";
            
            optionsBuilder.UseSqlServer(connectionString);

            return new BFASDbContext(optionsBuilder.Options);
        }
    }
}
