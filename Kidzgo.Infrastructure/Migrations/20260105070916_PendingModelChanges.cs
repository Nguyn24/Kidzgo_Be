using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration was created to capture pending model changes,
            // but all changes have already been applied in previous migrations:
            // - Name column for Users: Added in AddNameToUser
            // - CreatedAt, UpdatedAt, Name for TuitionPlans: Added in AddNameCreatedAtUpdatedAtToTuitionPlan
            // - CreatedAt, UpdatedAt for Programs: Added in AddCreatedAtUpdatedAtToProgram
            // - Seed data: Added in AddSeedDataForBranchesUsersPrograms, AddNameToUser, AddClassroomSeedData
            // No changes needed - this is a no-op migration
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No changes to revert - this is a no-op migration
        }
    }
}
