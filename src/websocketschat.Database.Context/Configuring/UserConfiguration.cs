using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using websocketschat.Database.Models;

namespace websocketschat.Database.Context.Configuring
{
    /// <summary>
    /// Configures "Users" table.
    /// </summary>
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasIndex(user => user.Id).IsUnique();
            builder.HasIndex(user => user.Username).IsUnique();
            builder.HasIndex(user => user.RoleId);
            builder.HasKey(user => user.Id);

            builder.Property(user => user.Id);
            builder.Property(user => user.RoleId);
            builder.Property(user => user.Username).IsRequired();
            builder.Property(user => user.PasswordHash).IsRequired();
            builder.Property(user => user.PasswordHashingKey).IsRequired();
            builder.Property(user => user.IsDeleted).IsRequired();

            builder.HasOne(user => user.Role)
                .WithMany(role => role.Users)
                .HasForeignKey(user => user.RoleId);
        }
    }
}
