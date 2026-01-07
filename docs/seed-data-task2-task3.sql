-- ============================================
-- Seed Data for Task 2 & Task 3 API Testing
-- PostgreSQL Script
-- ============================================
--
-- HƯỚNG DẪN SỬ DỤNG:
-- 1. Chạy script này trong PostgreSQL database của bạn
-- 2. Password hash trong script có thể không hoạt động
--    → Bạn cần tạo password hash hợp lệ bằng cách:
--       a. Sử dụng API Register để tạo users mới, hoặc
--       b. Chạy script này và sau đó đổi password qua API ChangePassword
--       c. Hoặc tạo hash từ BCrypt: BCrypt.HashPassword("Password123!")
-- 3. Tất cả test users có password mặc định: "Password123!"
-- 4. Test data được đánh dấu với prefix "TEST_" hoặc "Test " để dễ xóa sau
--
-- LƯU Ý: Script này sử dụng ON CONFLICT DO NOTHING để tránh lỗi khi chạy lại
-- ============================================

-- Clear existing test data (optional - comment out if you want to keep existing data)
-- DELETE FROM "Attendances" WHERE "SessionId" IN (SELECT "Id" FROM "Sessions" WHERE "ClassId" IN (SELECT "Id" FROM "Classes" WHERE "Code" LIKE 'TEST_%'));
-- DELETE FROM "LessonPlans" WHERE "SessionId" IN (SELECT "Id" FROM "Sessions" WHERE "ClassId" IN (SELECT "Id" FROM "Classes" WHERE "Code" LIKE 'TEST_%'));
-- DELETE FROM "Sessions" WHERE "ClassId" IN (SELECT "Id" FROM "Classes" WHERE "Code" LIKE 'TEST_%');
-- DELETE FROM "ClassEnrollments" WHERE "ClassId" IN (SELECT "Id" FROM "Classes" WHERE "Code" LIKE 'TEST_%');
-- DELETE FROM "Classes" WHERE "Code" LIKE 'TEST_%';
-- DELETE FROM "Programs" WHERE "Name" LIKE 'Test %';
-- DELETE FROM "Classrooms" WHERE "Name" LIKE 'Test %';
-- DELETE FROM "Profiles" WHERE "DisplayName" LIKE 'Test %';
-- DELETE FROM "Users" WHERE "Email" LIKE 'test%@example.com';
-- DELETE FROM "Branches" WHERE "Code" LIKE 'TEST_%';

-- ============================================
-- 1. BRANCHES
-- ============================================
INSERT INTO "Branches" ("Id", "Code", "Name", "Address", "ContactPhone", "ContactEmail", "IsActive", "CreatedAt", "UpdatedAt")
VALUES
    ('11111111-1111-1111-1111-111111111111', 'TEST_BRANCH_01', 'Test Branch 01', '123 Test Street, Ho Chi Minh City', '0901234567', 'branch01@test.com', true, NOW(), NOW()),
    ('22222222-2222-2222-2222-222222222222', 'TEST_BRANCH_02', 'Test Branch 02', '456 Test Avenue, Ha Noi', '0907654321', 'branch02@test.com', true, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- 2. USERS (với các roles khác nhau)
-- ============================================
-- Password hash cho "Password123!" (BCrypt)
-- LƯU Ý: Bạn cần tạo password hash hợp lệ bằng cách:
-- 1. Sử dụng API Register hoặc
-- 2. Tạo hash từ BCrypt: BCrypt.HashPassword("Password123!")
-- Hash mẫu dưới đây có thể không hoạt động, bạn cần thay thế bằng hash thực tế
-- 
-- Hoặc bạn có thể chạy script này và sau đó đổi password qua API ChangePassword
INSERT INTO "Users" ("Id", "Email", "PasswordHash", "Role", "Username", "Name", "BranchId", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
VALUES
    -- Admin (Role = 0)
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'admin@test.com', '$2a$11$rKqXZQZQZQZQZQZQZQZQZ.QZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 0, 'admin', 'Test Admin', NULL, true, false, NOW(), NOW()),
    -- Staff (Role = 1, Branch 1)
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'staff01@test.com', '$2a$11$rKqXZQZQZQZQZQZQZQZQZ.QZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 1, 'staff01', 'Test Staff 01', '11111111-1111-1111-1111-111111111111', true, false, NOW(), NOW()),
    -- Teachers (Role = 2, Branch 1)
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'teacher01@test.com', '$2a$11$rKqXZQZQZQZQZQZQZQZQZ.QZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 2, 'teacher01', 'Test Teacher 01', '11111111-1111-1111-1111-111111111111', true, false, NOW(), NOW()),
    ('dddddddd-dddd-dddd-dddd-dddddddddddd', 'teacher02@test.com', '$2a$11$rKqXZQZQZQZQZQZQZQZQZ.QZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 2, 'teacher02', 'Test Teacher 02', '11111111-1111-1111-1111-111111111111', true, false, NOW(), NOW()),
    -- Parents (Role = 4)
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'parent01@test.com', '$2a$11$rKqXZQZQZQZQZQZQZQZQZ.QZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 4, 'parent01', 'Test Parent 01', NULL, true, false, NOW(), NOW()),
    ('ffffffff-ffff-ffff-ffff-ffffffffffff', 'parent02@test.com', '$2a$11$rKqXZQZQZQZQZQZQZQZQZ.QZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 4, 'parent02', 'Test Parent 02', NULL, true, false, NOW(), NOW()),
    -- Students (Role = 3, nhưng sẽ có Profile với ProfileType = Student)
    ('11111111-1111-1111-1111-111111111101', 'student01@test.com', '$2a$11$rKqXZQZQZQZQZQZQZQZQZ.QZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 3, 'student01', 'Test Student 01', NULL, true, false, NOW(), NOW()),
    ('11111111-1111-1111-1111-111111111102', 'student02@test.com', '$2a$11$rKqXZQZQZQZQZQZQZQZQZ.QZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 3, 'student02', 'Test Student 02', NULL, true, false, NOW(), NOW()),
    ('11111111-1111-1111-1111-111111111103', 'student03@test.com', '$2a$11$rKqXZQZQZQZQZQZQZQZQZ.QZQZQZQZQZQZQZQZQZQZQZQZQZQZQZQ', 3, 'student03', 'Test Student 03', NULL, true, false, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- 3. PROFILES (Student và Parent)
-- ============================================
INSERT INTO "Profiles" ("Id", "UserId", "ProfileType", "DisplayName", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
VALUES
    -- Student Profiles
    ('11111111-1111-1111-1111-111111111100', '11111111-1111-1111-1111-111111111101', 1, 'Test Student 01', true, false, NOW(), NOW()),
    ('22222222-2222-2222-2222-222222222200', '11111111-1111-1111-1111-111111111102', 1, 'Test Student 02', true, false, NOW(), NOW()),
    ('33333333-3333-3333-3333-333333333300', '11111111-1111-1111-1111-111111111103', 1, 'Test Student 03', true, false, NOW(), NOW()),
    -- Parent Profiles
    ('44444444-4444-4444-4444-444444444400', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 0, 'Test Parent 01', true, false, NOW(), NOW()),
    ('55555555-5555-5555-5555-555555555500', 'ffffffff-ffff-ffff-ffff-ffffffffffff', 0, 'Test Parent 02', true, false, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- 4. PARENT-STUDENT LINKS
-- ============================================
INSERT INTO "ParentStudentLinks" ("Id", "ParentProfileId", "StudentProfileId", "CreatedAt")
VALUES
    ('11111111-1111-1111-1111-111111111110', '44444444-4444-4444-4444-444444444400', '11111111-1111-1111-1111-111111111100', NOW()),
    ('22222222-2222-2222-2222-222222222210', '55555555-5555-5555-5555-555555555500', '22222222-2222-2222-2222-222222222200', NOW()),
    ('33333333-3333-3333-3333-333333333310', '55555555-5555-5555-5555-555555555500', '33333333-3333-3333-3333-333333333300', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- 5. PROGRAMS (với các levels khác nhau)
-- ============================================
INSERT INTO "Programs" ("Id", "BranchId", "Name", "Level", "TotalSessions", "DefaultTuitionAmount", "UnitPriceSession", "Description", "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01', '11111111-1111-1111-1111-111111111111', 'Test Phonics Program', 'Beginner', 30, 5000000, 200000, 'Test Phonics Program for beginners', true, false, NOW(), NOW()),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', '11111111-1111-1111-1111-111111111111', 'Test Phonics Program', 'Intermediate', 30, 5500000, 220000, 'Test Phonics Program for intermediate', true, false, NOW(), NOW()),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03', '11111111-1111-1111-1111-111111111111', 'Test Cambridge Program', 'Starter', 40, 6000000, 250000, 'Test Cambridge Program starter level', true, false, NOW(), NOW()),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa04', '11111111-1111-1111-1111-111111111111', 'Test Communication Program', 'Advanced', 20, 7000000, 300000, 'Test Communication Program advanced', true, false, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- 6. CLASSROOMS
-- ============================================
INSERT INTO "Classrooms" ("Id", "BranchId", "Name", "Capacity", "IsActive")
VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', '11111111-1111-1111-1111-111111111111', 'Test Room 01', 15, true),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', '11111111-1111-1111-1111-111111111111', 'Test Room 02', 20, true),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', '11111111-1111-1111-1111-111111111111', 'Test Room 03', 12, true)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- 7. CLASSES
-- ============================================
INSERT INTO "Classes" ("Id", "BranchId", "ProgramId", "Code", "Title", "MainTeacherId", "AssistantTeacherId", "StartDate", "EndDate", "Status", "Capacity", "SchedulePattern", "CreatedAt", "UpdatedAt")
VALUES
    ('cccccccc-cccc-cccc-cccc-cccccccccc01', '11111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01', 'TEST_CLASS_01', 'Test Class 01 - Phonics Beginner', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'dddddddd-dddd-dddd-dddd-dddddddddddd', '2025-01-01', '2025-06-30', 1, 15, 'Mon, Wed, Fri 18:00-19:30', NOW(), NOW()),
    ('cccccccc-cccc-cccc-cccc-cccccccccc02', '11111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03', 'TEST_CLASS_02', 'Test Class 02 - Cambridge Starter', 'cccccccc-cccc-cccc-cccc-cccccccccccc', NULL, '2025-01-15', '2025-07-15', 1, 20, 'Tue, Thu 19:00-20:30', NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- 8. CLASS ENROLLMENTS
-- ============================================
INSERT INTO "ClassEnrollments" ("Id", "ClassId", "StudentProfileId", "EnrollDate", "Status", "CreatedAt", "UpdatedAt")
VALUES
    -- Student 01 enrolled in Class 01
    ('dddddddd-dddd-dddd-dddd-dddddddddd01', 'cccccccc-cccc-cccc-cccc-cccccccccc01', '11111111-1111-1111-1111-111111111100', '2025-01-01', 0, NOW(), NOW()),
    -- Student 02 enrolled in Class 01
    ('dddddddd-dddd-dddd-dddd-dddddddddd02', 'cccccccc-cccc-cccc-cccc-cccccccccc01', '22222222-2222-2222-2222-222222222200', '2025-01-01', 0, NOW(), NOW()),
    -- Student 03 enrolled in Class 01
    ('dddddddd-dddd-dddd-dddd-dddddddddd03', 'cccccccc-cccc-cccc-cccc-cccccccccc01', '33333333-3333-3333-3333-333333333300', '2025-01-01', 0, NOW(), NOW()),
    -- Student 01 also enrolled in Class 02
    ('dddddddd-dddd-dddd-dddd-dddddddddd04', 'cccccccc-cccc-cccc-cccc-cccccccccc02', '11111111-1111-1111-1111-111111111100', '2025-01-15', 0, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- 9. SESSIONS (cho timetable)
-- ============================================
INSERT INTO "Sessions" ("Id", "ClassId", "BranchId", "PlannedDatetime", "PlannedRoomId", "PlannedTeacherId", "PlannedAssistantId", "DurationMinutes", "ParticipationType", "Status", "ActualDatetime", "ActualRoomId", "ActualTeacherId", "ActualAssistantId", "CreatedAt", "UpdatedAt")
VALUES
    -- Class 01 Sessions (Mon, Wed, Fri pattern)
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01', 'cccccccc-cccc-cccc-cccc-cccccccccc01', '11111111-1111-1111-1111-111111111111', '2025-01-06 18:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 90, 0, 1, '2025-01-06 18:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'dddddddd-dddd-dddd-dddd-dddddddddddd', NOW(), NOW()),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02', 'cccccccc-cccc-cccc-cccc-cccccccccc01', '11111111-1111-1111-1111-111111111111', '2025-01-08 18:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 90, 0, 1, '2025-01-08 18:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'dddddddd-dddd-dddd-dddd-dddddddddddd', NOW(), NOW()),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03', 'cccccccc-cccc-cccc-cccc-cccccccccc01', '11111111-1111-1111-1111-111111111111', '2025-01-10 18:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 90, 0, 1, '2025-01-10 18:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'dddddddd-dddd-dddd-dddd-dddddddddddd', NOW(), NOW()),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee04', 'cccccccc-cccc-cccc-cccc-cccccccccc01', '11111111-1111-1111-1111-111111111111', '2025-01-13 18:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 90, 0, 0, NULL, NULL, NULL, NULL, NOW(), NOW()),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee05', 'cccccccc-cccc-cccc-cccc-cccccccccc01', '11111111-1111-1111-1111-111111111111', '2025-01-15 18:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', 'cccccccc-cccc-cccc-cccc-cccccccccccc', 'dddddddd-dddd-dddd-dddd-dddddddddddd', 90, 0, 0, NULL, NULL, NULL, NULL, NOW(), NOW()),
    -- Class 02 Sessions (Tue, Thu pattern)
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee06', 'cccccccc-cccc-cccc-cccc-cccccccccc02', '11111111-1111-1111-1111-111111111111', '2025-01-14 19:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'cccccccc-cccc-cccc-cccc-cccccccccccc', NULL, 90, 0, 1, '2025-01-14 19:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'cccccccc-cccc-cccc-cccc-cccccccccccc', NULL, NOW(), NOW()),
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeee07', 'cccccccc-cccc-cccc-cccc-cccccccccc02', '11111111-1111-1111-1111-111111111111', '2025-01-16 19:00:00', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', 'cccccccc-cccc-cccc-cccc-cccccccccccc', NULL, 90, 0, 0, NULL, NULL, NULL, NULL, NOW(), NOW())
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- 10. LESSON PLANS
-- ============================================
INSERT INTO "LessonPlans" ("Id", "SessionId", "PlannedContent", "ActualContent", "ActualHomework", "TeacherNotes", "SubmittedBy", "SubmittedAt")
VALUES
    ('ffffffff-ffff-ffff-ffff-ffffffff0001', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01', 'Planned content for session 1', 'Actual content taught', 'Homework assignment 1', 'Good session', 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    ('ffffffff-ffff-ffff-ffff-ffffffff0002', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02', 'Planned content for session 2', 'Actual content taught', 'Homework assignment 2', 'Students engaged well', 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    ('ffffffff-ffff-ffff-ffff-ffffffff0003', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03', 'Planned content for session 3', NULL, NULL, NULL, NULL, NULL)
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- 11. ATTENDANCES (cho session detail)
-- ============================================
INSERT INTO "Attendances" ("Id", "SessionId", "StudentProfileId", "AttendanceStatus", "AbsenceType", "MarkedBy", "MarkedAt")
VALUES
    -- Session 01 Attendances
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa001', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01', '11111111-1111-1111-1111-111111111100', 0, NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa002', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01', '22222222-2222-2222-2222-222222222200', 0, NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa003', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01', '33333333-3333-3333-3333-333333333300', 1, NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    -- Session 02 Attendances
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa004', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02', '11111111-1111-1111-1111-111111111100', 0, NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa005', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02', '22222222-2222-2222-2222-222222222200', 0, NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa006', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee02', '33333333-3333-3333-3333-333333333300', 0, NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    -- Session 03 Attendances
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa007', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03', '11111111-1111-1111-1111-111111111100', 0, NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa008', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03', '22222222-2222-2222-2222-222222222200', 2, NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa009', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee03', '33333333-3333-3333-3333-333333333300', 0, NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW()),
    -- Session 06 Attendances (Class 02)
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaa010', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeee06', '11111111-1111-1111-1111-111111111100', 0, NULL, 'cccccccc-cccc-cccc-cccc-cccccccccccc', NOW())
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- VERIFICATION QUERIES
-- ============================================
-- Uncomment to verify data:

-- SELECT COUNT(*) as branch_count FROM "Branches" WHERE "Code" LIKE 'TEST_%';
-- SELECT COUNT(*) as user_count FROM "Users" WHERE "Email" LIKE 'test%@example.com';
-- SELECT COUNT(*) as profile_count FROM "Profiles" WHERE "DisplayName" LIKE 'Test %';
-- SELECT COUNT(*) as program_count FROM "Programs" WHERE "Name" LIKE 'Test %';
-- SELECT COUNT(*) as class_count FROM "Classes" WHERE "Code" LIKE 'TEST_%';
-- SELECT COUNT(*) as enrollment_count FROM "ClassEnrollments" WHERE "ClassId" IN (SELECT "Id" FROM "Classes" WHERE "Code" LIKE 'TEST_%');
-- SELECT COUNT(*) as session_count FROM "Sessions" WHERE "ClassId" IN (SELECT "Id" FROM "Classes" WHERE "Code" LIKE 'TEST_%');
-- SELECT COUNT(*) as attendance_count FROM "Attendances" WHERE "SessionId" IN (SELECT "Id" FROM "Sessions" WHERE "ClassId" IN (SELECT "Id" FROM "Classes" WHERE "Code" LIKE 'TEST_%'));

-- ============================================
-- TEST DATA SUMMARY
-- ============================================
-- Branches: 2
-- Users: 9 (1 Admin, 1 Staff, 2 Teachers, 2 Parents, 3 Students)
-- Profiles: 5 (3 Students, 2 Parents)
-- Programs: 4 (với levels: Beginner, Intermediate, Starter, Advanced)
-- Classrooms: 3
-- Classes: 2 (Class 01 với MainTeacher + AssistantTeacher, Class 02 chỉ MainTeacher)
-- Enrollments: 4 (Student 01 in 2 classes, Student 02 & 03 in Class 01)
-- Sessions: 7 (5 for Class 01, 2 for Class 02)
-- LessonPlans: 3
-- Attendances: 10

-- ============================================
-- TESTING GUIDE
-- ============================================
-- Task 2 APIs:
-- 1. GET /api/branches
--    - Login as admin@test.com → should see 2 branches
--    - Login as staff01@test.com → should see 1 branch (Branch 01)
--    - Login as teacher01@test.com → should see 1 branch (Branch 01)
--
-- 2. GET /api/levels
--    - Should return: ["Advanced", "Beginner", "Intermediate", "Starter"]
--
-- 3. GET /api/roles
--    - Should return SessionRoleType enum values
--
-- 4. GET /api/lookups
--    - Should return all enum values for various statuses
--
-- Task 3 APIs:
-- 1. GET /api/teacher/classes
--    - Login as teacher01@test.com → should see 2 classes (Class 01 as MainTeacher, Class 02 as MainTeacher)
--    - Login as teacher02@test.com → should see 1 class (Class 01 as AssistantTeacher)
--
-- 2. GET /api/students/{studentId}/classes
--    - Use student profile ID: 11111111-1111-1111-1111-111111111100 → should see 2 classes
--    - Use student profile ID: 22222222-2222-2222-2222-222222222200 → should see 1 class
--
-- 3. GET /api/students/{studentId}/timetable?from=2025-01-01&to=2025-01-31
--    - Use student profile ID: 11111111-1111-1111-1111-111111111100 → should see sessions from both classes
--
-- 4. GET /api/teacher/timetable?from=2025-01-01&to=2025-01-31
--    - Login as teacher01@test.com → should see all sessions from Class 01 and Class 02
--
-- 5. GET /api/sessions/{sessionId}
--    - Use session ID: eeeeeeee-eeee-eeee-eeee-eeeeeeeeee01 → should see session detail with attendance summary
--    - Attendance summary: TotalStudents=3, PresentCount=2, AbsentCount=1, MakeupCount=0, NotMarkedCount=0

