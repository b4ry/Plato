using Microsoft.EntityFrameworkCore;
using Plato.DatabaseContext.Entities;
using System.Reflection;

namespace Plato.DatabaseContext
{
    public class ApplicationDbContext() : DbContext()
    {
        private readonly string _entityMethodName = "Entity";
        private readonly string _databaseContextAssemblyName = "Plato.DatabaseContext";

        public DbSet<MessageEntity> Messages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Unlike in the API. Could not inject options to this class, therefore this workaround.
            optionsBuilder.UseSqlite("Data Source=messages.sqlite");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityMethod = typeof(ModelBuilder)
                .GetMethods()
                .FirstOrDefault(x => x.Name == _entityMethodName && x.IsGenericMethodDefinition);

            var entities = Assembly
                .Load(_databaseContextAssemblyName)
                .GetTypes()
                .Where(x => x.GetTypeInfo().BaseType == typeof(BaseEntity));

            foreach (var entity in entities)
            {
                var constructedMethod = entityMethod!.MakeGenericMethod(entity);

                constructedMethod.Invoke(modelBuilder, null);
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}
