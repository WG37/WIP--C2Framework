using Microsoft.EntityFrameworkCore;
using TeamServer.Domain.Entities.Agents;
using TeamServer.Domain.Entities.Listeners.HttpListeners;

namespace TeamServer.Infrastructure.Data
{
    public static class ModelConfig
    {
        public static void ConfigureModel(ModelBuilder builder)
        {
            ConfigureHttpListener(builder);
            ConfigureAgent(builder);
        }

        public static void ConfigureHttpListener(ModelBuilder builder)
        {
            builder.Entity<HttpListenerEntity>(e =>
            {
                e.ToTable("HttpListeners", t =>
                {
                    t.HasCheckConstraint("CK_HttpListener_BindPort", "[BindPort] BETWEEN 1024 AND 65535");
                });

                e.HasKey(l => l.Id);
                e.Property(l => l.Id).ValueGeneratedNever();
                e.Property(l => l.Name).IsRequired();
                e.Property(l => l.BindPort).IsRequired();
            });
        }

        public static void ConfigureAgent(ModelBuilder builder)
        {
            builder.Entity<Agent>(e =>
            {
                e.ToTable("Agents");
                e.HasKey(a => a.Id);
                e.Property(a => a.UniqueId).IsRequired();
                e.HasIndex(a => a.UniqueId).IsUnique();

                e.OwnsOne(a => a.Metadata, md =>
                {
                    md.Property(m => m.Hostname).HasMaxLength(30).IsRequired();
                    md.Property(m => m.Username).HasMaxLength(30).IsRequired();
                    md.Property(m => m.ProcessName).HasMaxLength(30).IsRequired();
                    md.Property(m => m.ProcessId).IsRequired();
                    md.Property(m => m.Architecture).IsRequired();
                    md.Property(m => m.Integrity).IsRequired();
                });

                e.Navigation(a => a.Metadata).IsRequired();
            });
        }
    }
}
