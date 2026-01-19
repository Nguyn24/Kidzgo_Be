A3 – Homework grading
Prefix: /a3【app/main.py】
1) POST /a3/grade-text
Input JSON: GradeTextRequest
context: HomeworkContext
homework_id (string, required)
student_id (string, required)
subject (string, default "english")
skill (string, default "writing") — writing/reading/vocab/grammar/mixed
instructions (string, optional)
rubric (string, optional)
student_answer_text (string, required)
expected_answer_text (string, optional)
language (string, default "vi")
 Nguồn: app/agents/a3_homework/schemas.py.【app/agents/a3_homework/schemas.py】
Output: GradeResponse → GradingResult
score, max_score, summary, strengths, issues, suggestions, …
 Nguồn: app/agents/a3_homework/schemas.py.【app/agents/a3_homework/schemas.py】
2) POST /a3/grade-image (multipart)
Input (form-data):
homework_id, student_id
skill (default "writing"), instructions, expected_answer_text, language
file (UploadFile)
 Nguồn: app/agents/a3_homework/router.py.【app/agents/a3_homework/router.py】
3) POST /a3/grade-link
Input JSON: GradeLinkRequest
context (giống HomeworkContext)
link_url (string)
extracted_text (string, optional)
Note: BE cần download/parse nội dung và truyền text vào đây
expected_answer_text (optional)
language (default "vi")
 Nguồn: app/agents/a3_homework/schemas.py, app/agents/a3_homework/router.py.【app/agents/a3_homework/schemas.py】【app/agents/a3_homework/router.py】

A6 – Monthly reports
Prefix: /a6【app/main.py】
POST /a6/generate-monthly-report
Input JSON: MonthlyReportRequest
student:
student_id, name, age?, program?
range: from_date, to_date
session_feedbacks: list {date, text}
recent_reports: list {month, overview?, strengths[], improvements[], highlights[], goals_next_month[]}
teacher_notes?, language (default "vi")
 Nguồn: app/agents/a6_reports/schemas.py.【app/agents/a6_reports/schemas.py】
Output: MonthlyReportResponse
draft_text
sections: overview, strengths, improvements, highlights, goals_next_month, source_summary
 Nguồn: app/agents/a6_reports/schemas.py.【app/agents/a6_reports/schemas.py】

A7 – Receipts / payment proof OCR
Prefix: /a7【app/main.py】
POST /a7/extract-payment-proof (multipart)
Input (form-data):
file (UploadFile)
direction (default "IN", IN/OUT)
branch_id (default "UNKNOWN")
 Nguồn: app/agents/a7_receipts/router.py.【app/agents/a7_receipts/router.py】
Output: PaymentProofExtractResponse
fields (dict), confidence (dict), raw_text?, warnings[]
 Nguồn: app/agents/a7_receipts/schemas.py.【app/agents/a7_receipts/schemas.py】

A8 – Speaking / Phonics
Prefix: /a8【app/main.py】
1) POST /a8/analyze-transcript
Input JSON: AnalyzeTranscriptRequest
context: SpeakingContext
homework_id, student_id
mode ("phonics"/"speaking")
target_words[]
expected_text?, instructions?, language (default "vi")
transcript (string)
 Nguồn: app/agents/a8_speaking/schemas.py.【app/agents/a8_speaking/schemas.py】
Output: AnalyzeSpeakingResponse
result: overall_score, pronunciation_score, fluency_score, accuracy_score, issues[], suggestions[], …
 Nguồn: app/agents/a8_speaking/schemas.py.【app/agents/a8_speaking/schemas.py】
2) POST /a8/analyze-media (multipart)
Input (form-data):
homework_id, student_id, mode
target_words (string, comma-separated)
expected_text, instructions, language
file (audio/video)
 Nguồn: app/agents/a8_speaking/router.py.【app/agents/a8_speaking/router.py】
 

