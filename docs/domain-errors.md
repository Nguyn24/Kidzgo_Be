

## 1. Error Types

Các loại error trong hệ thống được định nghĩa trong `Kidzgo.Domain.Common.ErrorType`:

| ErrorType | Mô tả |
|-----------|-------|
| `Failure` | Lỗi chung |
| `NotFound` | Không tìm thấy tài nguyên |
| `Problem` | Vấn đề chung |
| `Conflict` | Xung đột dữ liệu |
| `Validation` | Lỗi validation |


## 2. Users Module

### UserErrors (`Kidzgo.Domain/Users/Errors/UserErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Users.EmailNotUnique` | Conflict | The provided email is not unique |
| `User.AdminAlreadyExists` | Conflict | Only one Admin is allowed in the system |
| `Users.NotFound` | NotFound | The user with the Id = '{userId}' was not found |
| `Users.NotFoundByEmail` | NotFound | The user with the specified email was not found |
| `Users.WrongPassword` | Conflict | The password for this account is wrong |
| `Users.InActive` | Conflict | The user is inactive |
| `Users.RefreshTokenInvalid` | Conflict | The refresh token is invalid |
| `Users.InvalidCurrentPassword` | Conflict | The current password provided is incorrect |
| `SamePassword` | Conflict | New password cannot be the same as the current password |
| `Users.InvalidGoogleToken` | Problem | Google token is invalid |
| `NotVerified` | Conflict | Account is not verified |
| `NotMember` | Conflict | User is not member |
| `User.MemberCannotUpdateOthers` | Problem | You are only allowed to update your own donor information |
| `Users.InvalidRole` | Validation | Invalid role value: '{role}'. Valid values: Admin, ManagementStaff, AccountantStaff, Teacher, Student, Parent |
| `Users.RoleRequired` | Validation | Role is required |
| `User.EmailAlreadyExists` | Conflict | Email '{email}' is already in use |
| `User.InvalidRole` | Validation | Only ManagementStaff, AccountantStaff, Teacher, and Parent can be assigned to a branch |
| `User.BranchRequired` | Validation | BranchId is required for ManagementStaff, AccountantStaff, Teacher, and Parent accounts |

### ProfileErrors (`Kidzgo.Domain/Users/Errors/ProfileErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Profile.Invalid` | Problem | Profile is invalid |
| `Profile.NotFound` | NotFound | The profile with the Id = '{profileId}' was not found |
| `Profile.DisplayNameRequired` | Validation | Display name is required |
| `Profile.UserNotFound` | NotFound | User not found |
| `Profile.InvalidProfileType` | Validation | Profile type must be Parent or Student |
| `Profile.StudentNotFound` | NotFound | Student profile not found |
| `Profile.ParentNotFound` | NotFound | Parent profile not found |
| `Profile.LinkAlreadyExists` | Conflict | Parent-Student link already exists |
| `Profile.LinkNotFound` | NotFound | Parent-Student link not found |
| `Profile.EmailNotSet` | Validation | Email is required for PIN reset |
| `Profile.StudentIdNotSelected` | NotFound | No student selected in token |
| `Profile.StudentNotLinkedToParent` | NotFound | Student not linked to this parent |
| `Profile.ProfileNotDeleted` | Validation | Profile is not deleted and cannot be reactivated |

### PinErrors (`Kidzgo.Domain/Users/Errors/PinErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Pin.Invalid` | Validation | PIN must be numeric and less than 10 digits |
| `Pin.Wrong` | Conflict | PIN is incorrect |
| `Pin.NotSet` | Conflict | PIN has not been set yet |

---

## 3. Classes Module

### ClassErrors (`Kidzgo.Domain/Classes/Errors/ClassErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Class.NotFound` | NotFound | Class with Id = '{classId}' was not found |
| `Class.BranchNotFound` | NotFound | Branch not found or inactive |
| `Class.ProgramNotFound` | NotFound | Program not found, deleted, or inactive |
| `Class.CodeExists` | Conflict | Class code already exists |
| `Class.MainTeacherNotFound` | NotFound | Main teacher not found or is not a teacher |
| `Class.MainTeacherBranchMismatch` | Conflict | Main teacher must belong to the same branch as the class |
| `Class.AssistantTeacherNotFound` | NotFound | Assistant teacher not found or is not a teacher |
| `Class.AssistantTeacherBranchMismatch` | Conflict | Assistant teacher must belong to the same branch as the class |
| `Class.HasActiveEnrollments` | Conflict | Cannot delete class with active enrollments |
| `Class.StatusUnchanged` | Validation | Class status is already set to the requested status |
| `Class.InvalidStatusTransition` | Validation | Cannot change status from Closed to Planned |

### ScheduleErrors (`Kidzgo.Domain/Classes/Errors/ClassErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `SchedulePattern.Empty` | Validation | Schedule pattern cannot be empty |
| `SchedulePattern.Invalid` | Validation | Invalid RRULE pattern: {message} |

### EnrollmentErrors (`Kidzgo.Domain/Classes/Errors/EnrollmentErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Enrollment.NotFound` | NotFound | Enrollment with Id = '{enrollmentId}' was not found |
| `Enrollment.StudentNotFound` | NotFound | Student profile not found or is not a student |
| `Enrollment.ClassNotFound` | NotFound | Class not found |
| `Enrollment.ClassNotAvailable` | Conflict | Class is not available for enrollment |
| `Enrollment.AlreadyEnrolled` | Conflict | Student is already enrolled in this class |
| `Enrollment.ClassFull` | Conflict | Class has reached its capacity |
| `Enrollment.TuitionPlanNotFound` | NotFound | Tuition plan not found |
| `Enrollment.TuitionPlanNotAvailable` | Conflict | Tuition plan is not available |
| `Enrollment.TuitionPlanProgramMismatch` | Conflict | Tuition plan must belong to the same program as the class |
| `Enrollment.AlreadyActive` | Conflict | Enrollment is already active |
| `Enrollment.CannotReactivateDropped` | Conflict | Cannot reactivate a dropped enrollment |
| `Enrollment.InvalidStatus` | Conflict | Only active enrollments can be paused |
| `Enrollment.AlreadyDropped` | Conflict | Enrollment is already dropped |

---

## 4. Sessions Module

### SessionErrors (`Kidzgo.Domain/Sessions/Errors/SessionErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Session.NotFound` | NotFound | Session with Id = '{sessionId}' was not found |
| `Session.InvalidStatus` | Validation | Only sessions with Scheduled status can be updated |
| `Session.InvalidClassStatus` | Validation | Sessions can only be created for Planned or Active classes |
| `Session.AlreadyCancelled` | Validation | Session is already cancelled |
| `Session.Cancelled` | Validation | Cancelled sessions cannot be completed |
| `Session.InvalidDuration` | Validation | Duration phải lớn hơn 0 |
| `Session.InvalidBranch` | Validation | Branch với ID {branchId} không tồn tại hoặc không active |
| `Session.InvalidRoom` | Validation | Room với ID {roomId} không tồn tại hoặc không thuộc branch này |
| `Session.InvalidTeacher` | Validation | Main Teacher với ID {teacherId} không tồn tại |
| `Session.InvalidAssistant` | Validation | Assistant Teacher với ID {assistantId} không tồn tại |
| `Session.RoomOccupied` | Validation | Phòng đã bị chiếm dụng bởi lớp '{classCode} - {className}' |
| `Session.SaveFailed` | Validation | Không thể lưu sessions: {details} |
| `Session.UnauthorizedAccess` | Validation | Teacher không được phép tạo report cho session |

### SessionRoleErrors (`Kidzgo.Domain/Sessions/Errors/SessionRoleErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `SessionRole.NotFound` | NotFound | Session role with Id = '{sessionRoleId}' was not found |
| `SessionRole.Exists` | Conflict | Session role already exists for this session and staff user |

### AttendanceErrors (`Kidzgo.Domain/Sessions/Errors/AttendanceErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Attendance.NotFound` | NotFound | The attendance record with Id = '{id}' was not found |
| `Attendance.NotFound` | NotFound | Attendance not found for session '{sessionId}' and student '{studentProfileId}' |
| `Attendance.UpdateWindowClosed` | Validation | Attendance for session '{sessionId}' can only be updated within 24 hours after it ends |

### MakeupCreditErrors (`Kidzgo.Domain/Sessions/Errors/MakeupCreditErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `MakeupCredit.NotFound` | NotFound | The makeup credit with Id = '{id}' was not found |
| `MakeupCredit.NotAvailable` | Validation | Makeup credit '{id}' is not available for allocation |
| `MakeupCredit.Expired` | Validation | Makeup credit '{id}' is expired |

### LeaveRequestErrors (`Kidzgo.Domain/Sessions/Errors/LeaveRequestErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `LeaveRequest.NotFound` | NotFound | The leave request with Id = '{id}' was not found |
| `LeaveRequest.AlreadyApproved` | Conflict | Leave request is already approved |
| `LeaveRequest.AlreadyRejected` | Conflict | Leave request is already rejected |
| `LeaveRequest.NotEnrolled` | Validation | Student profile '{studentProfileId}' is not enrolled in class '{classId}' |
| `LeaveRequest.SessionNotFound` | NotFound | No session found for class '{classId}' on date '{sessionDate}' |

---

## 5. Schools Module

### BranchErrors (`Kidzgo.Domain/Schools/Errors/BranchErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Branch.NotFound` | NotFound | The branch with the Id = '{branchId}' was not found |
| `Branch.CodeNotUnique` | Conflict | The provided branch code is not unique |
| `Branch.NameNotUnique` | Conflict | The provided branch name is not unique |

### ClassroomErrors (`Kidzgo.Domain/Schools/Errors/ClassroomErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Classroom.NotFound` | NotFound | Classroom with Id = '{classroomId}' was not found |
| `Classroom.BranchNotFound` | NotFound | Branch not found or inactive |
| `Classroom.HasSessions` | Conflict | Cannot delete classroom that is being used in sessions |

---

## 6. Programs Module

### ProgramErrors (`Kidzgo.Domain/Programs/Errors/ProgramErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Program.NotFound` | NotFound | Program with Id = '{programId}' was not found |
| `Program.BranchNotFound` | NotFound | Branch not found or inactive |
| `Program.HasActiveClasses` | Conflict | Cannot delete program with active classes |

### TuitionPlanErrors (`Kidzgo.Domain/Programs/Errors/TuitionPlanErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `TuitionPlan.NotFound` | NotFound | Tuition Plan with Id = '{tuitionPlanId}' was not found |
| `TuitionPlan.ProgramNotFound` | NotFound | Program not found or deleted |
| `TuitionPlan.BranchNotFound` | NotFound | Branch not found or inactive |
| `TuitionPlan.HasActiveEnrollments` | Conflict | Cannot delete tuition plan with active enrollments |

---

## 7. Exams Module

### ExamErrors (`Kidzgo.Domain/Exams/Errors/ExamErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Exam.NotFound` | NotFound | Exam with Id = '{examId}' was not found |
| `Exam.ClassNotFound` | NotFound | Class not found or inactive |
| `ExamResult.NotFound` | NotFound | Exam result not found |
| `ExamResult.StudentProfileNotFound` | NotFound | Student profile not found or inactive |
| `ExamResult.AlreadyExists` | Conflict | Exam result already exists for this student |
| `Exam.UserNotFound` | NotFound | User not found |

### ExamQuestionErrors (`Kidzgo.Domain/Exams/Errors/ExamQuestionErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `ExamQuestion.NotFound` | NotFound | Exam Question with Id = '{questionId}' was not found |
| `ExamQuestion.ExamNotFound` | NotFound | Exam with Id = '{examId}' was not found |
| `ExamQuestion.InvalidQuestionType` | Validation | Invalid question type. Must be MultipleChoice or TextInput |
| `ExamQuestion.InvalidOptions` | Validation | Options must be a valid JSON array for MultipleChoice questions |
| `ExamQuestion.MissingCorrectAnswer` | Validation | Correct answer is required |
| `ExamQuestion.HasSubmissions` | Conflict | Cannot delete question that has submission answers |

### ExamSubmissionErrors (`Kidzgo.Domain/Exams/Errors/ExamSubmissionErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `ExamSubmission.NotFound` | NotFound | Exam Submission with Id = '{submissionId}' was not found |
| `ExamSubmission.ExamNotFound` | NotFound | Exam with Id = '{examId}' was not found |
| `ExamSubmission.AlreadyStarted` | Conflict | Student already started this exam |
| `ExamSubmission.NotStarted` | Conflict | Exam submission has not been started yet |
| `ExamSubmission.AlreadySubmitted` | Conflict | Exam submission has already been submitted |
| `ExamSubmission.ExamNotStarted` | Conflict | Exam has not started yet |
| `ExamSubmission.ExamExpired` | Conflict | Exam time limit has expired |
| `ExamSubmission.LateStartNotAllowed` | Conflict | Late start is not allowed for this exam |
| `ExamSubmission.TooLateToStart` | Conflict | Too late to start the exam |
| `ExamSubmission.QuestionNotFound` | NotFound | Exam Question with Id = '{questionId}' was not found |
| `ExamSubmission.AlreadyGraded` | Conflict | Exam submission has already been graded |
| `ExamSubmission.InvalidStatus` | Validation | Can only grade submitted or auto-submitted exams |

### ExerciseErrors (`Kidzgo.Domain/Exams/Errors/ExerciseErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Exercise.NotFound` | NotFound | Exercise with Id = '{exerciseId}' was not found |
| `ExerciseQuestion.NotFound` | NotFound | Exercise question with Id = '{questionId}' was not found |
| `Exercise.InvalidType` | Validation | Invalid exercise type |
| `ExerciseQuestion.InvalidType` | Validation | Invalid question type |
| `ExerciseQuestion.InvalidPoints` | Validation | Points must be >= 0 |
| `ExerciseSubmission.NotFound` | NotFound | Exercise submission with Id = '{submissionId}' was not found |
| `ExerciseSubmission.Unauthorized` | Problem | You are not allowed to access this submission |

---

## 8. CRM Module

### LeadErrors (`Kidzgo.Domain/CRM/Errors/LeadErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Lead.NotFound` | NotFound | The lead with the Id = '{leadId}' was not found |
| `Lead.InvalidContactInfo` | Validation | At least one of ContactName, Phone, or Email must be provided |
| `Lead.InvalidSource` | Validation | Invalid lead source. Valid values: Landing, Zalo, Referral, Offline |
| `Lead.InvalidStatus` | Validation | Invalid lead status. Valid values: New, Contacted, BookedTest, TestDone, Enrolled, Lost |
| `Lead.OwnerNotFound` | NotFound | The staff user with the Id = '{ownerId}' was not found |
| `Lead.OwnerNotStaff` | Validation | The assigned owner must be a Staff user |
| `Lead.BranchNotFound` | NotFound | The specified branch was not found |
| `Lead.DuplicateLead` | Conflict | A lead with the same phone, email, or Zalo ID already exists |
| `Lead.CannotUpdateConvertedLead` | Validation | Cannot update a lead that has been converted to enrollment |
| `Lead.InvalidStatusTransition` | Validation | Invalid status transition |

### PlacementTestErrors (`Kidzgo.Domain/CRM/Errors/PlacementTestErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `PlacementTest.NotFound` | NotFound | The placement test with the Id = '{placementTestId}' was not found |
| `PlacementTest.LeadNotFound` | NotFound | The lead with the Id = '{leadId}' was not found |
| `PlacementTest.StudentProfileNotFound` | NotFound | The student profile with the Id = '{profileId}' was not found |
| `PlacementTest.ClassNotFound` | NotFound | The class with the Id = '{classId}' was not found |
| `PlacementTest.InvigilatorNotFound` | NotFound | The invigilator user with the Id = '{userId}' was not found |
| `PlacementTest.InvalidStatusTransition` | Validation | Invalid status transition |
| `PlacementTest.CannotUpdateCompletedTest` | Validation | Cannot update a placement test that has been completed |
| `PlacementTest.CannotCancelCompletedTest` | Validation | Cannot cancel a placement test that has been completed |
| `PlacementTest.CannotMarkNoShowCompletedTest` | Validation | Cannot mark a completed placement test as NoShow |
| `PlacementTest.LeadAlreadyEnrolled` | Conflict | The lead has already been converted to enrollment |
| `PlacementTest.StudentProfileAlreadyAssigned` | Conflict | The student profile is already assigned to another child |

---

## 9. Finance Module

### InvoiceErrors (`Kidzgo.Domain/Finance/Errors/InvoiceErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Invoice.BranchNotFound` | NotFound | Branch not found |
| `Invoice.StudentProfileNotFound` | NotFound | Student profile not found |
| `Invoice.ClassNotFound` | NotFound | Class not found |
| `Invoice.InvoiceNotFound` | NotFound | Invoice not found |
| `Invoice.InvoiceAlreadyCancelled` | Conflict | Invoice is already cancelled |
| `Invoice.InvoiceAlreadyPaid` | Conflict | Invoice is already paid |
| `Invoice.InvoiceLineNotFound` | NotFound | Invoice line not found |
| `Invoice.CannotCancelPaidInvoice` | Conflict | Cannot cancel a paid invoice |
| `Invoice.ParentNotFound` | NotFound | Parent profile not found |
| `PayOS.InvalidSignature` | Validation | Invalid webhook signature |
| `PayOS.CreateLinkFailed` | Problem | Failed to create PayOS payment link |

---

## 10. LessonPlans Module

### LessonPlanErrors (`Kidzgo.Domain/LessonPlans/Errors/LessonPlanErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `LessonPlan.NotFound` | NotFound | Lesson plan with Id = '{lessonPlanId}' was not found |
| `LessonPlan.SessionNotFound` | NotFound | Session with Id = '{sessionId}' was not found |
| `LessonPlan.TemplateNotFound` | NotFound | Lesson plan template with Id = '{templateId}' was not found |
| `LessonPlan.SessionRequired` | Validation | SessionId is required |
| `LessonPlan.SessionAlreadyHasLessonPlan` | Conflict | Session with Id = '{sessionId}' already has a lesson plan |
| `LessonPlan.Unauthorized` | Validation | You do not have permission to access this lesson plan |

### LessonPlanTemplateErrors (`Kidzgo.Domain/LessonPlans/Errors/LessonPlanTemplateErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `LessonPlanTemplate.NotFound` | NotFound | Lesson plan template with Id = '{templateId}' was not found |
| `LessonPlanTemplate.ProgramNotFound` | NotFound | Program with Id = '{programId}' was not found or inactive |
| `LessonPlanTemplate.ProgramRequired` | Validation | ProgramId is required |
| `LessonPlanTemplate.SessionIndexRequired` | Validation | SessionIndex is required and must be greater than 0 |
| `LessonPlanTemplate.DuplicateSessionIndex` | Conflict | Template with SessionIndex {sessionIndex} already exists for Program {programId} |
| `LessonPlanTemplate.HasActiveLessonPlans` | Conflict | Cannot delete template that has active lesson plans |

### HomeworkErrors (`Kidzgo.Domain/LessonPlans/Errors/HomeworkErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Homework.NotFound` | NotFound | Homework assignment with Id = '{homeworkId}' was not found |
| `Homework.ClassNotFound` | NotFound | Class not found or inactive |
| `Homework.SessionNotFound` | NotFound | Session with Id = '{sessionId}' was not found |
| `Homework.MissionNotFound` | NotFound | Mission with Id = '{missionId}' was not found or inactive |
| `Homework.InvalidDueDate` | Validation | Due date must be in the future |
| `Homework.CannotUpdate` | Validation | Cannot update homework that has submitted or graded submissions |
| `Homework.Unauthorized` | Validation | You do not have permission to access homework |
| `Homework.ClassHasNoActiveStudents` | Validation | Class has no active enrolled students |
| `Homework.InvalidTitle` | Validation | Title cannot be empty or whitespace |
| `Homework.InvalidMaxScore` | Validation | MaxScore must be greater than 0 |
| `Homework.InvalidRewardStars` | Validation | RewardStars must be greater than or equal to 0 |
| `Homework.InvalidSubmissionType` | Validation | Invalid submission type |
| `Homework.InvalidStatusForMarking` | Validation | Status must be either 'LATE' or 'MISSING' |

### Homework Submission Errors

| Code | Type | Description |
|------|------|-------------|
| `HomeworkSubmission.NotFound` | NotFound | Homework submission with Id = '{homeworkStudentId}' was not found |
| `HomeworkSubmission.InvalidScore` | Validation | Score cannot be negative |
| `HomeworkSubmission.ScoreExceedsMax` | Validation | Score cannot exceed maximum score of {maxScore} |
| `HomeworkSubmission.InvalidStatus` | Validation | Status must be either LATE or MISSING |
| `HomeworkSubmission.InvalidStatusTransition` | Validation | Cannot change status from {currentStatus} to {targetStatus} |
| `HomeworkSubmission.Unauthorized` | Validation | You do not have permission to access this homework submission |
| `HomeworkSubmission.AlreadySubmitted` | Validation | This homework has already been submitted |
| `HomeworkSubmission.InvalidData` | Validation | Submission data is required for {submissionType}submission type |
| `HomeworkSubmission.CannotSubmitMissing` | Validation | Cannot submit homework with MISSING status |
| `HomeworkSubmission.NotSubmitted` | Validation | Can only grade homework that has been submitted |

---

## 11. Reports Module

### SessionReportErrors (`Kidzgo.Domain/Reports/Errors/SessionReportErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `SessionReport.NotFound` | NotFound | Session report with Id = '{sessionReportId}' was not found |
| `SessionReport.AlreadyExists` | Conflict | A session report already exists for this session and student |

### MonthlyReportErrors (`Kidzgo.Domain/Reports/Errors/MonthlyReportErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `MonthlyReport.NotFound` | NotFound | Monthly Report with Id = '{reportId}' was not found |
| `MonthlyReportJob.NotFound` | NotFound | Monthly Report Job with Id = '{jobId}' was not found |
| `MonthlyReport.AlreadyExists` | Conflict | Monthly Report for student '{studentProfileId}' in {month}/{year} already exists |
| `MonthlyReport.InvalidStatus` | Validation | Cannot {action} Monthly Report with status '{currentStatus}' |
| `MonthlyReport.AiGenerationFailed` | Failure | AI generation failed: {errorMessage} |
| `MonthlyReport.PdfGenerationFailed` | Failure | PDF generation failed: {errorMessage} |
| `MonthlyReport.DataAggregationFailed` | Failure | Data aggregation failed: {errorMessage} |
| `MonthlyReport.StudentProfileNotFound` | NotFound | Student Profile with Id = '{studentProfileId}' was not found |
| `MonthlyReport.ClassNotFound` | NotFound | Class with Id = '{classId}' was not found |

---

## 12. Gamification Module

### XpErrors (`Kidzgo.Domain/Gamification/Errors/XpErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Xp.ProfileNotFound` | NotFound | Student profile {studentProfileId} not found |

### StarErrors (`Kidzgo.Domain/Gamification/Errors/StarErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Star.InsufficientBalance` | Validation | Student {studentProfileId} has insufficient stars |
| `Star.ProfileNotFound` | NotFound | Student profile {studentProfileId} not found |

### RewardStoreErrors (`Kidzgo.Domain/Gamification/Errors/RewardStoreErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `RewardStore.NotFound` | NotFound | Reward store item {id} not found |
| `RewardStore.InvalidCostStars` | Validation | Cost stars must be greater than 0 |
| `RewardStore.InvalidQuantity` | Validation | Quantity must be greater than or equal to 0 |

### RewardRedemptionErrors (`Kidzgo.Domain/Gamification/Errors/RewardRedemptionErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `RewardRedemption.NotFound` | NotFound | Reward redemption {id} not found |
| `RewardRedemption.ItemNotFound` | NotFound | Reward store item {itemId} not found |
| `RewardRedemption.ItemNotActive` | Validation | Reward store item {itemId} is not active |
| `RewardRedemption.InsufficientQuantity` | Conflict | Reward store item has insufficient quantity |
| `RewardRedemption.InsufficientStars` | Conflict | Student has insufficient stars |
| `RewardRedemption.InvalidStatusTransition` | Validation | Cannot transition from {currentStatus} to {targetStatus} |
| `RewardRedemption.StudentProfileNotFound` | NotFound | Student profile {studentProfileId} not found |

### MissionErrors (`Kidzgo.Domain/Gamification/Errors/MissionErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Mission.NotFound` | NotFound | Mission with Id = '{missionId}' was not found |
| `Mission.ClassNotFound` | NotFound | Target class not found |
| `Mission.InvalidScope` | Validation | Invalid mission scope |
| `Mission.InvalidDateRange` | Validation | EndAt must be after StartAt |
| `Mission.MissionInUse` | Conflict | Cannot delete mission that has progress records |

---

## 13. Notifications Module

### NotificationErrors (`Kidzgo.Domain/Notifications/Errors/NotificationErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Notification.InvalidFilters` | Validation | At least one filter must be specified |
| `Notification.NoRecipients` | Validation | No recipients found matching the filters |
| `Notification.NotFound` | NotFound | Notification with Id = '{notificationId}' was not found |
| `Notification.AccessDenied` | Problem | You do not have permission to mark this notification as read |
| `Notification.AlreadyRead` | Conflict | Notification is already marked as read |

### NotificationTemplateErrors (`Kidzgo.Domain/Notifications/Errors/NotificationTemplateErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `NotificationTemplate.NotFound` | NotFound | Notification Template with Id = '{templateId}' was not found |
| `NotificationTemplate.CodeAlreadyExists` | Conflict | Notification Template with Code = '{code}' already exists |
| `NotificationTemplate.Deleted` | Conflict | Cannot update a deleted notification template |
| `NotificationTemplate.AlreadyDeleted` | Conflict | Notification template is already deleted |

### EmailTemplateErrors (`Kidzgo.Domain/Notifications/Errors/EmailTemplateErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `EmailTemplate.NotFound` | NotFound | Email template was not found |

---

## 14. Media Module

### MediaErrors (`Kidzgo.Domain/Media/Errors/MediaErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Media.NotFound` | NotFound | Media with Id = '{mediaId}' was not found |
| `Media.AlreadyDeleted` | Conflict | Media is already deleted |
| `Media.AlreadyApproved` | Conflict | Media is already approved |
| `Media.AlreadyRejected` | Conflict | Media is already rejected |
| `Media.AlreadyPublished` | Conflict | Media is already published |
| `Media.NotApproved` | Conflict | Media must be approved before publishing |
| `Media.BranchNotFound` | NotFound | Branch not found or inactive |
| `Media.ClassNotFound` | NotFound | Class not found |
| `Media.StudentNotFound` | NotFound | Student profile not found or is not a student |

### BlogErrors (`Kidzgo.Domain/Media/Errors/BlogErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Blog.NotFound` | NotFound | Blog with Id = '{blogId}' was not found |
| `Blog.UserNotFound` | NotFound | User not found or is not Admin/Staff |
| `Blog.Deleted` | Conflict | Cannot update a deleted blog |
| `Blog.NotPublished` | Conflict | Blog is not published |
| `Blog.AlreadyPublished` | Conflict | Blog is already published |
| `Blog.AlreadyDeleted` | Conflict | Blog is already deleted |

---

## 15. Tickets Module

### TicketErrors (`Kidzgo.Domain/Tickets/Errors/TicketErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `Ticket.NotFound` | NotFound | Ticket with Id = '{ticketId}' was not found |
| `Ticket.UserNotFound` | NotFound | User not found |
| `Ticket.BranchNotFound` | NotFound | Branch not found or inactive |
| `Ticket.ClassNotFound` | NotFound | Class not found |
| `Ticket.ProfileNotFound` | NotFound | Profile not found or does not belong to the user |
| `Ticket.AssignedUserNotFound` | NotFound | Assigned user not found or is not Staff/Teacher |
| `Ticket.AssignedUserBranchMismatch` | Conflict | Assigned user must belong to the same branch as the ticket |

### TicketCommentErrors (`Kidzgo.Domain/Tickets/Errors/TicketErrors.cs`)

| Code | Type | Description |
|------|------|-------------|
| `TicketComment.TicketNotFound` | NotFound | Ticket not found |
| `TicketComment.TicketClosed` | Conflict | Cannot add comment to a closed ticket |
| `TicketComment.UserNotFound` | NotFound | User not found |
| `TicketComment.ProfileNotFound` | NotFound | Profile not found or does not belong to the user |

---



