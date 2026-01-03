using Kidzgo.Domain.Programs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Kidzgo.Infrastructure.Configuration;

public class ProgramConfiguration : IEntityTypeConfiguration<Program>
{
    public void Configure(EntityTypeBuilder<Program> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Level)
            .HasMaxLength(100);

        builder.Property(x => x.TotalSessions)
            .IsRequired();

        builder.Property(x => x.DefaultTuitionAmount)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.UnitPriceSession)
            .HasColumnType("numeric")
            .IsRequired();

        builder.Property(x => x.Description);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.IsDeleted)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Classes)
            .WithOne(x => x.Program)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.TuitionPlans)
            .WithOne(x => x.Program)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.LessonPlanTemplates)
            .WithOne(x => x.Program)
            .HasForeignKey(x => x.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data for testing
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var branchHnId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var branchHcmId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        builder.HasData(
            new Program
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                BranchId = branchHnId,
                Name = "English for Kids - Beginner",
                Level = "Beginner",
                TotalSessions = 30,
                DefaultTuitionAmount = 5000000,
                UnitPriceSession = 166667,
                Description = "Khóa học tiếng Anh cho trẻ em mới bắt đầu, tập trung vào phát âm và từ vựng cơ bản.",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Program
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                BranchId = branchHnId,
                Name = "English for Kids - Intermediate",
                Level = "Intermediate",
                TotalSessions = 36,
                DefaultTuitionAmount = 6000000,
                UnitPriceSession = 166667,
                Description = "Khóa học tiếng Anh nâng cao cho trẻ em, phát triển kỹ năng giao tiếp và ngữ pháp.",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Program
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                BranchId = branchHcmId,
                Name = "English for Kids - Beginner",
                Level = "Beginner",
                TotalSessions = 30,
                DefaultTuitionAmount = 5000000,
                UnitPriceSession = 166667,
                Description = "Khóa học tiếng Anh cho trẻ em mới bắt đầu, tập trung vào phát âm và từ vựng cơ bản.",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Program
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                BranchId = branchHcmId,
                Name = "English for Teens - Advanced",
                Level = "Advanced",
                TotalSessions = 40,
                DefaultTuitionAmount = 8000000,
                UnitPriceSession = 200000,
                Description = "Khóa học tiếng Anh nâng cao cho thanh thiếu niên, chuẩn bị cho các kỳ thi quốc tế.",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new Program
            {
                Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
                BranchId = branchHnId,
                Name = "English for Kids - Advanced (Inactive)",
                Level = "Advanced",
                TotalSessions = 40,
                DefaultTuitionAmount = 8000000,
                UnitPriceSession = 200000,
                Description = "Khóa học đã tạm ngưng.",
                IsActive = false,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            }
        );
    }
}
