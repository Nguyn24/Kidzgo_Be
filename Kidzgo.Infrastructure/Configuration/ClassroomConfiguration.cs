using Kidzgo.Domain.Schools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
{
    public void Configure(EntityTypeBuilder<Classroom> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Capacity)
            .IsRequired();

        builder.Property(x => x.Note);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Classrooms)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data for testing
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var branchHnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var branchHcmId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        builder.HasData(
            new Classroom
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
                BranchId = branchHnId,
                Name = "Phòng A101",
                Capacity = 15,
                Note = "Có máy chiếu, điều hòa",
                IsActive = true
            },
            new Classroom
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
                BranchId = branchHnId,
                Name = "Phòng A102",
                Capacity = 20,
                Note = "Phòng lớn, có bảng tương tác",
                IsActive = true
            },
            new Classroom
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
                BranchId = branchHnId,
                Name = "Phòng B201",
                Capacity = 12,
                Note = "Phòng nhỏ, phù hợp lớp ít học sinh",
                IsActive = true
            },
            new Classroom
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
                BranchId = branchHcmId,
                Name = "Phòng C101",
                Capacity = 18,
                Note = "Có máy chiếu, điều hòa",
                IsActive = true
            },
            new Classroom
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000005"),
                BranchId = branchHcmId,
                Name = "Phòng C102",
                Capacity = 25,
                Note = "Phòng lớn nhất, có bảng tương tác và hệ thống âm thanh",
                IsActive = true
            }
        );
    }
}
