using Kidzgo.Domain.Tickets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.TicketId)
            .IsRequired();

        builder.Property(x => x.CommenterUserId)
            .IsRequired();

        builder.Property(x => x.CommenterProfileId);

        builder.Property(x => x.Message)
            .IsRequired();

        builder.Property(x => x.AttachmentUrl);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Ticket)
            .WithMany(x => x.TicketComments)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CommenterUser)
            .WithMany(x => x.TicketComments)
            .HasForeignKey(x => x.CommenterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CommenterProfile)
            .WithMany(x => x.TicketComments)
            .HasForeignKey(x => x.CommenterProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
