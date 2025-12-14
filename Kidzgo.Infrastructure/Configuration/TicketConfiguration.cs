using Kidzgo.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.OpenedByUserId)
            .IsRequired();

        builder.Property(x => x.OpenedByProfileId);

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.ClassId);

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Message)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.AssignedToUserId);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.OpenedByUser)
            .WithMany(x => x.OpenedTickets)
            .HasForeignKey(x => x.OpenedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.OpenedByProfile)
            .WithMany(x => x.OpenedTickets)
            .HasForeignKey(x => x.OpenedByProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Class)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedToUser)
            .WithMany(x => x.AssignedTickets)
            .HasForeignKey(x => x.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TicketComments)
            .WithOne(x => x.Ticket)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
