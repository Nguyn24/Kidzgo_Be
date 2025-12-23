// KidzGo high-level relational schema for dbdiagram.io
// Scope: multi-branch, shared login with profiles, classes/sessions,
// attendance/makeup, lesson plans/homework, exams, AI monthly reports,
// gamification, CRM/lead, media, finance/payroll, notifications, tickets.

Table email_templates {
  id uuid [pk]
  code varchar(100) [unique]
  subject varchar(255)
  body text
  placeholders jsonb
  is_active boolean
  is_deleted boolean
  created_at timestamptz
  updated_at timestamptz
}

Table branches {
  id uuid [pk]
  code varchar(32) [unique]
  name varchar(255)
  address text
  contact_phone varchar(32)
  contact_email varchar(255)
  is_active boolean
  created_at timestamptz
  updated_at timestamptz
}

Table users {
  id uuid [pk]
  email varchar(255) [unique]
  password_hash text
  role varchar(20) // PARENT/STUDENT/ADMIN/TEACHER/STAFF
  username varchar(255) // required for ADMIN/TEACHER/STAFF; optional for PARENT/STUDENT
  branch_id uuid [ref: - branches.id] // required for TEACHER/STAFF; null for ADMIN/PARENT/STUDENT
  is_active boolean
  image_url text
  created_at timestamptz
  updated_at timestamptz
}


Table profiles {
  id uuid [pk]
  user_id uuid [ref: > users.id]
  profile_type varchar(20) // PARENT/STUDENT only
  display_name varchar(255)
  pin_hash varchar(97) // required when selecting PARENT profile, validate PIN < 10 digits before hash. Format: PBKDF2-SHA512 hash (64 hex) + '-' + salt (32 hex) = 97 chars
  is_active boolean
  created_at timestamptz
  updated_at timestamptz
}


Table parent_student_links {
  id uuid [pk]
  parent_profile_id uuid [ref: > profiles.id]
  student_profile_id uuid [ref: > profiles.id]
  created_at timestamptz
}

// Note: roles and profile_roles are kept for backward compatibility or future use
// For PARENT/STUDENT profiles, can still use profile_roles if needed
// For ADMIN/TEACHER/STAFF, role is directly in users.role enum   

// Table roles {
//   id uuid [pk]
//   code varchar(50) [unique] // ROLE_ADMIN/ROLE_TEACHER/ROLE_STAFF_OPS/ROLE_STAFF_ACC/ROLE_PARENT/ROLE_STUDENT
//   description text
// }
// Table profile_roles {
//   id uuid [pk]
//   profile_id uuid [ref: > profiles.id]
//   role_id uuid [ref: > roles.id]
// }


Table programs {
  id uuid [pk]
  branch_id uuid [ref: > branches.id] // các chi nhánh không dùng chung program
  name varchar(255)
  level varchar(100)
  total_sessions int
  default_tuition_amount numeric
  unit_price_session numeric
  description text
  is_active boolean
  is_deleted boolean
}

Table classrooms {
  id uuid [pk]
  branch_id uuid [ref: > branches.id]
  name varchar(100)
  capacity int
  note text
  is_active boolean
}

Table classes {
  id uuid [pk]
  branch_id uuid [ref: > branches.id]
  program_id uuid [ref: > programs.id]
  code varchar(50) [unique]
  title varchar(255)
  main_teacher_id uuid [ref: - users.id] // teacher user (role=TEACHER)
  assistant_teacher_id uuid [ref: - users.id] // assistant teacher user (role=TEACHER)
  start_date date
  end_date date
  status varchar(20) // PLANNED/ACTIVE/CLOSED
  capacity int
  schedule_pattern text // RRULE/JSON description
  created_at timestamptz
  updated_at timestamptz
}

Table class_enrollments {
  id uuid [pk]
  class_id uuid [ref: > classes.id]
  student_profile_id uuid [ref: > profiles.id]
  enroll_date date
  status varchar(20) // ACTIVE/PAUSED/DROPPED
  tuition_plan_id uuid [ref: - tuition_plans.id]
}

Table sessions {
  id uuid [pk]
  class_id uuid [ref: > classes.id]
  branch_id uuid [ref: > branches.id]
  planned_datetime timestamptz
  planned_room_id uuid [ref: - classrooms.id]
  planned_teacher_id uuid [ref: - users.id] // teacher user (role=TEACHER)
  planned_assistant_id uuid [ref: - users.id] // assistant teacher user (role=TEACHER)
  duration_minutes int
  participation_type varchar(20) // MAIN/MAKEUP/EXTRA_PAID/FREE/TRIAL
  status varchar(20) // SCHEDULED/COMPLETED/CANCELLED
  actual_datetime timestamptz
  actual_room_id uuid [ref: - classrooms.id]
  actual_teacher_id uuid [ref: - users.id] // teacher user (role=TEACHER)
  actual_assistant_id uuid [ref: - users.id] // assistant teacher user (role=TEACHER)
  created_at timestamptz
  updated_at timestamptz
}

Table leave_requests {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  class_id uuid [ref: > classes.id]
  session_date date
  end_date date // nullable for single day
  reason text
  notice_hours int // hours before planned session
  status varchar(20) // PENDING/APPROVED/REJECTED
  requested_at timestamptz
  approved_by uuid [ref: - users.id] // staff user (role=STAFF)
  approved_at timestamptz
}

Table attendances {
  id uuid [pk]
  session_id uuid [ref: > sessions.id]
  student_profile_id uuid [ref: > profiles.id]
  attendance_status varchar(20) // PRESENT/ABSENT/MAKEUP
  absence_type varchar(30) // WITH_NOTICE_24H/UNDER_24H/NO_NOTICE/LONG_TERM
  marked_by uuid [ref: - users.id] // teacher
  marked_at timestamptz

  
}

Table makeup_credits {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  source_session_id uuid [ref: > sessions.id]
  status varchar(20) // AVAILABLE/USED/EXPIRED
  created_reason varchar(30) // APPROVED_LEAVE_24H/LONG_TERM
  expires_at timestamptz
  used_session_id uuid [ref: - sessions.id]
  created_at timestamptz
}

Table makeup_allocations {
  id uuid [pk]
  makeup_credit_id uuid [ref: > makeup_credits.id]
  target_session_id uuid [ref: > sessions.id]
  assigned_by uuid [ref: - users.id]
  assigned_at timestamptz
}

Table lesson_plan_templates {
  id uuid [pk]
  program_id uuid [ref: > programs.id]
  level varchar(100)
  session_index int
  structure_json jsonb
  is_active boolean
  is_deleted boolean
  created_by uuid [ref: - users.id]
  created_at timestamptz
}

Table lesson_plans {
  id uuid [pk]
  session_id uuid [ref: > sessions.id] //1:1 nhé
  template_id uuid [ref: - lesson_plan_templates.id]
  planned_content jsonb
  actual_content jsonb
  actual_homework text
  teacher_notes text
  submitted_by uuid [ref: - users.id]
  submitted_at timestamptz
}

Table homework_assignments {
  id uuid [pk]
  class_id uuid [ref: > classes.id]
  session_id uuid [ref: - sessions.id]
  title varchar(255)
  description text
  due_at timestamptz
  book varchar(255)
  pages varchar(50)
  skills varchar(100)
  submission_type varchar(20) // FILE/IMAGE/TEXT/LINK/QUIZ
  max_score numeric
  reward_stars int
  mission_id uuid [ref: - missions.id]
  created_by uuid [ref: - users.id]
  created_at timestamptz
}

Table homework_student {
  id uuid [pk]
  assignment_id uuid [ref: > homework_assignments.id]
  student_profile_id uuid [ref: > profiles.id]
  status varchar(20) // ASSIGNED/SUBMITTED/GRADED/LATE/MISSING
  submitted_at timestamptz
  graded_at timestamptz
  score numeric
  teacher_feedback text
  ai_feedback jsonb
  ai_version varchar(50) // optional: phiên bản AI dùng để chấm (A3/A8)
  attachments jsonb

 
}

Table exams {
  id uuid [pk]
  class_id uuid [ref: > classes.id]
  exam_type varchar(30) // PLACEMENT/PROGRESS/MIDTERM/FINAL/SPEAKING
  date date
  max_score numeric
  description text
  created_by uuid [ref: - users.id]
}

Table exercises {
  id uuid [pk]
  class_id uuid [ref: - classes.id] // optional: can be assigned to specific class
  title varchar(255)
  description text
  exercise_type varchar(20) // READING/LISTENING/WRITING
  created_by uuid [ref: > users.id] // teacher/admin
  is_active boolean
  is_deleted boolean
  created_at timestamptz
  updated_at timestamptz
}

Table exercise_questions {
  id uuid [pk]
  exercise_id uuid [ref: > exercises.id]
  order_index int // order of question in exercise
  question_text text
  question_type varchar(20) // MULTIPLE_CHOICE/TEXT_INPUT
  options jsonb // JSON array for multiple choice options
  correct_answer text // correct answer
  points int // points awarded for correct answer
  explanation text // explanation of the answer
}

Table exercise_submissions {
  id uuid [pk]
  exercise_id uuid [ref: > exercises.id]
  student_profile_id uuid [ref: > profiles.id]
  answers jsonb // JSON object: {questionId: answer}
  score numeric // total score
  submitted_at timestamptz
  graded_at timestamptz
  graded_by uuid [ref: - users.id] // teacher who graded (for writing exercises)
  
  Indexes {
    (exercise_id, student_profile_id) [unique]
  }
}

Table exercise_submission_answers {
  id uuid [pk]
  submission_id uuid [ref: > exercise_submissions.id]
  question_id uuid [ref: > exercise_questions.id]
  answer text // student's answer
  is_correct boolean // whether answer is correct (for auto-graded questions)
  points_awarded numeric // points awarded for this answer
  teacher_feedback text // teacher feedback (for writing questions)
  
  Indexes {
    (submission_id, question_id) [unique]
  }
}

Table exam_results {
  id uuid [pk]
  exam_id uuid [ref: > exams.id]
  student_profile_id uuid [ref: > profiles.id]
  score numeric
  comment text
  attachment_urls jsonb // JSON array of image URLs (changed from attachment_url string)
  graded_by uuid [ref: - users.id]
  graded_at timestamptz
}

Table monthly_report_jobs {
  id uuid [pk]
  month int
  year int
  branch_id uuid [ref: > branches.id]
  status varchar(20) // PENDING/GENERATING/DONE/FAILED
  started_at timestamptz
  finished_at timestamptz
  ai_payload_ref text
}

Table session_reports {
  id uuid [pk]
  session_id uuid [ref: > sessions.id]
  student_profile_id uuid [ref: > profiles.id]
  teacher_user_id uuid [ref: > users.id]
  report_date date // date of session for filtering monthly reports
  feedback text // teacher's feedback/notes for the student
  ai_generated_summary text // AI-generated summary (for monthly compilation)
  is_monthly_compiled boolean // whether included in monthly report
  created_at timestamptz
  updated_at timestamptz
  
  Indexes {
    (session_id, student_profile_id) [unique] // one report per student per session
    (teacher_user_id, report_date) // for filtering by teacher and date range
  }
}

Table student_monthly_reports {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  month int
  year int
  draft_content jsonb
  final_content jsonb
  status varchar(20) // DRAFT/REVIEW/APPROVED/REJECTED
  ai_version varchar(50)
  submitted_by uuid [ref: - users.id]
  reviewed_by uuid [ref: - users.id]
  reviewed_at timestamptz
  published_at timestamptz
}

Table report_comments {
  id uuid [pk]
  report_id uuid [ref: > student_monthly_reports.id]
  commenter_id uuid [ref: > users.id]
  content text
  created_at timestamptz
}

Table missions {
  id uuid [pk]
  title varchar(255)
  description text
  scope varchar(20) // CLASS/STUDENT/GROUP
  target_class_id uuid [ref: - classes.id]
  target_group jsonb // optional grouping
  mission_type varchar(50) // HOMEWORK_STREAK/READING_STREAK/NO_UNEXCUSED_ABSENCE/CUSTOM
  start_at timestamptz
  end_at timestamptz
  reward_stars int
  reward_exp int // experience points reward
  total_questions int // total questions for question-based missions
  progress_per_question numeric // percentage progress per question (e.g., 10% if 10 questions)
  created_by uuid [ref: - users.id]
  created_at timestamptz
}

Table mission_progress {
  id uuid [pk]
  mission_id uuid [ref: > missions.id]
  student_profile_id uuid [ref: > profiles.id]
  status varchar(20) // ASSIGNED/IN_PROGRESS/COMPLETED/EXPIRED
  progress_value numeric
  completed_at timestamptz
  verified_by uuid [ref: - users.id]

  
}

Table star_transactions {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  amount int // positive or negative
  reason varchar(100)
  source_type varchar(30) // MISSION/MANUAL/HOMEWORK/TEST/ADJUSTMENT/ATTENDANCE
  source_id uuid
  balance_after int
  created_by uuid [ref: - users.id]
  created_at timestamptz
}

Table student_levels {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  current_level varchar(50)
  current_xp int
  updated_at timestamptz
}

Table attendance_streaks {
  id uuid [pk]
  student_profile_id uuid [ref: > profiles.id]
  attendance_date date
  current_streak int // current consecutive days streak
  reward_stars int // stars awarded (e.g., 1)
  reward_exp int // exp awarded (e.g., 5)
  created_at timestamptz
  
  Indexes {
    (student_profile_id, attendance_date) [unique]
  }
}

Table reward_store_items {
  id uuid [pk]
  title varchar(255)
  image_url text
  description text
  cost_stars int
  quantity int
  is_active boolean
  is_deleted boolean
  created_at timestamptz
}

Table reward_redemptions {
  id uuid [pk]
  item_id uuid [ref: > reward_store_items.id]
  item_name varchar(255) // store item name at redemption time
  student_profile_id uuid [ref: > profiles.id]
  status varchar(20) // REQUESTED/APPROVED/DELIVERED/RECEIVED/CANCELLED
  handled_by uuid [ref: - users.id]
  handled_at timestamptz
  delivered_at timestamptz // when staff delivered the reward
  received_at timestamptz // when student confirmed or auto after 3 days
  created_at timestamptz
}

Table leads {
  id uuid [pk]
  source varchar(30) // LANDING/ZALO/REFERRAL/OFFLINE
  contact_name varchar(255)
  phone varchar(50)
  zalo_id varchar(100)
  email varchar(255)
  branch_preference uuid [ref: - branches.id]
  program_interest varchar(255)
  notes text
  status varchar(30) // NEW/CONTACTED/BOOKED_TEST/TEST_DONE/ENROLLED/LOST
  owner_staff_id uuid [ref: - users.id]
  first_response_at timestamptz // SLA phản hồi đầu tiên
  touch_count int // số lần chạm
  next_action_at timestamptz // lịch follow-up tiếp theo
  converted_student_profile_id uuid [ref: - profiles.id] // lưu trace khi ghi danh
  converted_at timestamptz
  created_at timestamptz
  updated_at timestamptz
}

Table placement_tests {
  id uuid [pk]
  lead_id uuid [ref: - leads.id]
  student_profile_id uuid [ref: - profiles.id]
  class_id uuid [ref: - classes.id]
  scheduled_at timestamptz
  status varchar(20) // SCHEDULED/NO_SHOW/COMPLETED/CANCELLED
  room varchar(100)
  invigilator_user_id uuid [ref: - users.id] // teacher/staff user
  result_score numeric
  listening_score numeric
  speaking_score numeric
  reading_score numeric
  writing_score numeric
  level_recommendation varchar(100)
  program_recommendation varchar(100)
  notes text
  attachment_url text
}

Table lead_activities {
  id uuid [pk]
  lead_id uuid [ref: - leads.id]
  activity_type varchar(20) // CALL/ZALO/SMS/EMAIL/NOTE
  content text
  next_action_at timestamptz
  created_by uuid [ref: - users.id]
  created_at timestamptz
}

Table media_assets {
  id uuid [pk]
  uploader_id uuid [ref: > users.id]
  branch_id uuid [ref: > branches.id]
  class_id uuid [ref: - classes.id]
  student_profile_id uuid [ref: - profiles.id]
  month_tag varchar(7) // YYYY-MM
  type varchar(10) // PHOTO/VIDEO
  url text
  caption text
  visibility varchar(20) // CLASS_ONLY/PERSONAL/PUBLIC_PARENT
  created_at timestamptz
}

Table blogs {
  id uuid [pk]
  title varchar(255)
  summary varchar(500) // short summary for preview
  content text // full blog content (can be HTML/Markdown)
  featured_image_url varchar(500)
  created_by uuid [ref: > users.id] // admin/staff
  is_published boolean // whether published on landing page
  is_deleted boolean
  published_at timestamptz
  created_at timestamptz
  updated_at timestamptz
}

Table tuition_plans {
  id uuid [pk]
  branch_id uuid [ref: - branches.id]
  program_id uuid [ref: > programs.id]
  total_sessions int
  tuition_amount numeric
  unit_price_session numeric
  currency varchar(10)
  is_active boolean
  is_deleted boolean
}

Table invoices {
  id uuid [pk]
  branch_id uuid [ref: > branches.id]
  student_profile_id uuid [ref: > profiles.id]
  class_id uuid [ref: - classes.id]
  type varchar(30) // MAIN_TUITION/EXTRA_CLASS/MATERIAL/EVENT/MAKEUP_FEE
  amount numeric
  currency varchar(10)
  due_date date
  status varchar(20) // PENDING/PAID/OVERDUE/CANCELLED
  description text
  payos_payment_link text
  payos_qr text
  issued_at timestamptz
  issued_by uuid [ref: - users.id]
}

Table invoice_lines {
  id uuid [pk]
  invoice_id uuid [ref: > invoices.id]
  item_type varchar(30) // SESSION_MAIN/SESSION_EXTRA/MATERIAL/EVENT
  quantity int
  unit_price numeric
  description text
  session_ids jsonb
}

Table payments {
  id uuid [pk]
  invoice_id uuid [ref: > invoices.id]
  method varchar(20) // PAYOS/CASH/BANK_TRANSFER
  amount numeric
  paid_at timestamptz
  reference_code varchar(100) //Mã tham chiếu giao dịch (từ PayOs)
  confirmed_by uuid [ref: - users.id] //staff
  evidence_url text
}

Table cashbook_entries {
  id uuid [pk]
  branch_id uuid [ref: > branches.id]
  type varchar(10) // CASH_IN/CASH_OUT
  amount numeric
  currency varchar(10)
  description text
  related_type varchar(30) // INVOICE/PAYROLL/EXPENSE/ADJUSTMENT
  related_id uuid //Invoice_id
  entry_date date // là ngày hạch toán của bút toán quỹ (cashbook_entries) — dùng để sắp xếp/khóa sổ theo ngày phát sinh thu/chi (có thể khác created_at nếu ghi nhận muộn).
  created_by uuid [ref: - users.id]
  attachment_url text
  ocr_metadata jsonb // optional: log raw kết quả OCR từ A7 (fields/confidence/raw_text/warnings)
  created_at timestamptz
}

Table contracts {
  id uuid [pk]
  staff_user_id uuid [ref: > users.id]
  contract_type varchar(20) // PROBATION/FIXED_TERM/INDEFINITE/PART_TIME
  start_date date
  end_date date
  base_salary numeric
  hourly_rate numeric //số tiền trả cho 1 giờ làm việc
  allowance_fixed numeric //phụ cấp
  minimum_monthly_hours numeric // số giờ làm tối thiểu mỗi tháng để nhận lương
  overtime_rate_multiplier numeric // hệ số nhân lương overtime (ví dụ: 1.5x, 2x)
  branch_id uuid [ref: > branches.id]
  is_active boolean
}

Table shift_attendance {
  id uuid [pk]
  staff_user_id uuid [ref: > users.id]
  contract_id uuid [ref: - contracts.id]
  shift_date date
  shift_hours numeric
  role varchar(50)
  approved_by uuid [ref: - users.id]
  approved_at timestamptz
}

Table monthly_work_hours {
  id uuid [pk]
  staff_user_id uuid [ref: > users.id]
  contract_id uuid [ref: > contracts.id]
  branch_id uuid [ref: > branches.id]
  year int
  month int // 1-12
  total_hours numeric // total hours worked in the month
  teaching_hours numeric // hours from teaching sessions (for teachers)
  regular_hours numeric // regular hours (from shift attendance)
  overtime_hours numeric // overtime hours (hours exceeding minimum)
  teaching_sessions int // number of sessions taught (for teachers)
  is_locked boolean // whether this month's hours are locked (for payroll calculation)
  
  Indexes {
    (staff_user_id, contract_id, year, month) [unique]
  }
}

Table session_roles {
  id uuid [pk]
  session_id uuid [ref: > sessions.id]
  staff_user_id uuid [ref: > users.id]
  role varchar(30) // MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP
  payable_unit_price numeric //đơn giá trả cho nhân sự trong buổi đó
  payable_allowance numeric //Phụ cấp thêm 
}

Table payroll_runs {
  id uuid [pk]
  period_start date
  period_end date
  branch_id uuid [ref: > branches.id]
  status varchar(20) // DRAFT/APPROVED/PAID
  approved_by uuid [ref: - users.id]
  paid_at timestamptz
  created_at timestamptz
}

Table payroll_lines {
  id uuid [pk]
  payroll_run_id uuid [ref: > payroll_runs.id]
  staff_user_id uuid [ref: > users.id]
  component_type varchar(30) // TEACHING/TA/CLUB/WORKSHOP/BASE/OVERTIME/ALLOWANCE/DEDUCTION
  source_id uuid // session_roles/contract/expense
  amount numeric
  description text
  is_paid boolean
  paid_at timestamptz
}

Table payroll_payments {
  id uuid [pk]
  payroll_run_id uuid [ref: > payroll_runs.id]
  staff_user_id uuid [ref: > users.id]
  amount numeric
  method varchar(20) // BANK_TRANSFER/CASH
  paid_at timestamptz
  cashbook_entry_id uuid [ref: - cashbook_entries.id]
}

Table notifications {
  id uuid [pk]
    recipient_user_id uuid [ref: > users.id] // user (can be any role)
  recipient_profile_id uuid [ref: - profiles.id] // optional: for PARENT/STUDENT profiles
  channel varchar(20) // ZALO_OA/PUSH/EMAIL
  title varchar(255)
  content text
  deeplink text
  status varchar(20) // PENDING/SENT/FAILED
  sent_at timestamptz
  template_id varchar(100)
  created_at timestamptz
}

Table notification_templates {
  id uuid [pk]
  code varchar(100) [unique]
  channel varchar(20) // ZALO_OA/PUSH/EMAIL
  title varchar(255)
  content text
  placeholders jsonb
  is_active boolean
  is_deleted boolean
  created_at timestamptz
  updated_at timestamptz
}

Table tickets {
  id uuid [pk]
  recipient_user_id uuid [ref: > users.id] // user (can be any role)
  recipient_profile_id uuid [ref: - profiles.id] // optional: for PARENT/STUDENT profiles
  branch_id uuid [ref: > branches.id]
  class_id uuid [ref: - classes.id]
  category varchar(30) // HOMEWORK/FINANCE/SCHEDULE/TECH
  message text
  status varchar(20) // OPEN/IN_PROGRESS/RESOLVED/CLOSED
  assigned_to_profile_id uuid [ref: - users.id]
  created_at timestamptz
  updated_at timestamptz
}

Table ticket_comments {
  id uuid [pk]
  ticket_id uuid [ref: > tickets.id]
  commenter_user_id uuid [ref: > users.id] // user (can be any role)
  commenter_profile_id uuid [ref: - profiles.id] // optional: for PARENT/STUDENT profiles
  message text
  attachment_url text
  created_at timestamptz
}

Table audit_logs {
  id uuid [pk]
  actor_user_id uuid [ref: - users.id] // user (can be any role)
  actor_profile_id uuid [ref: - profiles.id] // optional: for PARENT/STUDENT profiles
  action varchar(100)
  entity_type varchar(100)
  entity_id uuid
  data_before jsonb
  data_after jsonb
  created_at timestamptz
}

