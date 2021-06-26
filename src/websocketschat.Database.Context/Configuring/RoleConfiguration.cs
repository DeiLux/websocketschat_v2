using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using websocketschat.Database.Models;

namespace websocketschat.Database.Context.Configuring
{
    /// <summary>
    /// Configures "Roles" table.
    /// </summary>
    internal class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasIndex(role => role.Id).IsUnique();
            builder.HasIndex(role => role.Name).IsUnique();
            builder.HasKey(role => role.Id);

            builder.Property(role => role.Id);
            builder.Property(role => role.Name).IsRequired();

            builder.HasMany(role => role.Users)
                .WithOne(user => user.Role)
                .HasForeignKey(user => user.RoleId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasData(
                new Role()
                {
                    Id = 1,
                    Name = "Admin"
                },
                new Role()
                {
                    Id = 2,
                    Name = "User",
                },
                new Role()
                {
                    Id = 3,
                    Name = "Bot",
                });
        }
    }
}
