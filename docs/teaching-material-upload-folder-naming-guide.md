# Teaching Material Upload Folder Naming Guide

## Muc dich

Tai lieu nay huong dan frontend cach dat ten folder va tung file khi gui teaching materials len backend qua:

```text
POST /api/teaching-materials/upload
```

Tai lieu nay bam theo parser hien tai trong backend:

- `Kidzgo.Application/TeachingMaterials/Shared/TeachingMaterialMetadataParser.cs`
- `Kidzgo.Application/TeachingMaterials/ImportTeachingMaterials/ImportTeachingMaterialsCommandHandler.cs`
- `Kidzgo.Application/TeachingMaterials/ImportTeachingMaterials/ImportTeachingMaterialsUploadBuilder.cs`

## Ket luan nhanh

Neu frontend upload nhieu file hoac upload zip, backend dang doc metadata theo quy tac sau:

1. Folder root dau tien phai la ten program
2. Folder gan file nhat co format `UNIT x-Ly-Title` se duoc dung de parse:
   - `unitNumber`
   - `lessonNumber`
   - `lessonTitle`
3. `relativePaths[index]` phai map dung voi `files[index]`
4. Ten file asset ben trong lesson folder co the dat tu do, khong bat buoc phai chua `UNIT x-Ly`
5. Neu khong co folder lesson dung format, backend se co gang parse tu ten file

## 1. Cach backend nhan dien program

Backend lay segment dau tien trong duong dan file lam `program root`.

Vi du:

```text
Movers/UNIT 1-L2-READING/file.pptx
Starter/UNIT 3-L1-LISTENING/audio.mp3
```

`Movers` va `Starter` la `program root`.

Backend so khop `program root` voi `Program.Name` hoac `Program.Code` sau khi:

- bo khoang trang
- bo ky tu khong phai chu hoac so
- convert lowercase

Vi vay cac ten sau thuong se match cung mot program:

```text
Movers
MOVERS
Move-rs
movers
```

Backend cung dang chap nhan lech singular/plural o muc co ban.

## 2. Format folder lesson bat buoc nen dung

Regex parser hien tai:

```text
UNIT\s*(\d+)\s*-\s*L(\d+)\s*-\s*(.+)
```

Frontend nen dat folder lesson theo dung format:

```text
UNIT 1-L2-READING WRITING
UNIT 3-L1-LISTENING
UNIT 10-L4-LANGUAGE PRACTICE
```

Backend se parse ra:

- `UNIT 1-L2-READING WRITING`
  - `unitNumber = 1`
  - `lessonNumber = 2`
  - `lessonTitle = READING WRITING`

Khuyen nghi:

- Dung chu hoa `UNIT` va `L` de de nhin, du backend parse khong phan biet hoa thuong
- Dung dau `-` dung vi tri
- Khong doi format thanh `Unit1Lesson2` hoac `U1-L2`

## 3. Folder structure de frontend gui len

### Cau truc khuyen nghi

```text
<ProgramRoot>/
  UNIT <unit>-L<lesson>-<LessonTitle>/
    <slide-file>
    assets/
      <audio/image/video files>
    worksheets/
      <pdf/doc/xlsx files>
```

Vi du:

```text
Movers/
  UNIT 1-L2-READING WRITING/
    UNIT 1-L2-READING WRITING.pptx
    assets/
      intro-audio.mp3
      vocab-1.png
    worksheets/
      worksheet-1.pdf
```

### Cach backend parse trong vi du tren

- Program root: `Movers`
- Lesson source: `UNIT 1-L2-READING WRITING`
- File `intro-audio.mp3` van ra:
  - `unitNumber = 1`
  - `lessonNumber = 2`
  - `lessonTitle = READING WRITING`

Ly do: backend lay **folder gan file nhat co format lesson hop le**, khong bat buoc lay tu chinh ten file.

## 4. Cach dat ten tung loai file

## 4.1 File slide chinh

Nen dat ten trung voi ten lesson folder:

```text
UNIT 1-L2-READING WRITING.pptx
UNIT 3-L1-LISTENING.pptx
```

Khong bat buoc 100%, nhung day la cach it loi nhat.

## 4.2 File asset trong lesson

Co the dat ten tu do, vi du:

```text
intro.mp3
warmup.mp4
vocab-card-01.png
teacher-note.pdf
worksheet-final.docx
```

Mien la file nam ben trong lesson folder dung format, backend van suy ra duoc unit/lesson.

## 4.3 File o ngoai lesson folder

Neu file dat o ngoai folder `UNIT x-Ly-Title`, backend co the:

- khong parse duoc `unitNumber`
- khong parse duoc `lessonNumber`
- xep category thanh `ProgramDocument` hoac `Other`

Vi du:

```text
Movers/
  handbook.pdf
```

File nay se duoc xem nhu tai lieu cap program, khong phai lesson material.

## 5. Mapping category ma backend tu suy ra

Neu parser tim duoc `unitNumber` va `lessonNumber`, backend dang map mac dinh nhu sau:

- `.ppt`, `.pptx` -> `LessonSlide`
- `.mp3`, `.wav`, `.m4a`, `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`, `.mp4`, `.mov`, `.avi`, `.webm`, `.mkv` -> `LessonAsset`
- cac file khac trong lesson -> `Supplementary`
- `.pdf` ma khong thuoc lesson -> `ProgramDocument`

## 6. File type duoc backend chap nhan

Backend hien parse cac extension sau:

- Presentation: `.ppt`, `.pptx`
- Document: `.doc`, `.docx`, `.txt`
- Spreadsheet: `.xls`, `.xlsx`, `.csv`
- Pdf: `.pdf`
- Audio: `.mp3`, `.wav`, `.m4a`
- Image: `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`
- Video: `.mp4`, `.mov`, `.avi`, `.webm`, `.mkv`
- Archive: `.zip`, `.rar`, `.7z`

Neu la upload file zip bang field `archive`, backend chi chap nhan:

```text
.zip
```

## 7. Frontend gui `files` va `relativePaths` nhu the nao

Khi frontend upload nhieu file, backend dang map:

- `files[0]` <-> `relativePaths[0]`
- `files[1]` <-> `relativePaths[1]`
- ...

Vi vay:

- Thu tu `relativePaths` phai dung y thu tu `files`
- Moi `relativePath` nen la duong dan day du tinh tu program root

### Vi du dung

```text
files[0] = "UNIT 1-L2-READING WRITING.pptx"
relativePaths[0] = "Movers/UNIT 1-L2-READING WRITING/UNIT 1-L2-READING WRITING.pptx"

files[1] = "intro.mp3"
relativePaths[1] = "Movers/UNIT 1-L2-READING WRITING/assets/intro.mp3"
```

### Vi du sai

```text
files[0] = "intro.mp3"
relativePaths[0] = "Movers/assets/intro.mp3"
```

Sai vi backend khong tim thay segment lesson dung format.

## 8. Upload zip

Neu frontend zip ca folder roi gui qua field `archive`, backend se doc path ben trong zip.

Vi vay ben trong zip van phai giu nguyen cau truc:

```text
Movers/
  UNIT 1-L2-READING WRITING/
    UNIT 1-L2-READING WRITING.pptx
    assets/
      intro.mp3
```

Khong nen zip theo kieu:

```text
my-upload/
  slide1.pptx
  intro.mp3
```

vi backend se khong suy ra dung lesson metadata.

## 9. Quy tac thuc te frontend nen follow

Frontend nen follow dung checklist nay truoc khi upload:

1. Chi cho phep chon 1 program root cho moi lan upload
2. Moi lesson folder phai theo format `UNIT x-Ly-Title`
3. Moi file lesson phai nam ben trong lesson folder do
4. `relativePaths` phai bat dau tu program root
5. Neu upload nhieu file, phai giu dung thu tu `files[]` va `relativePaths[]`
6. Neu upload 1 file don, co the gui kem:
   - `programId`
   - `unitNumber`
   - `lessonNumber`
   - `lessonTitle`
   - `displayName`
   - `category`

Luu y: cac field override nay chi an toan nhat khi upload **1 file don**. Khi upload nhieu file hoac zip, backend uu tien parse tu folder structure.

## 10. Mau folder naming de frontend ap dung ngay

### Mau lesson slide

```text
Movers/UNIT 1-L2-READING WRITING/UNIT 1-L2-READING WRITING.pptx
```

### Mau lesson assets

```text
Movers/UNIT 1-L2-READING WRITING/assets/intro.mp3
Movers/UNIT 1-L2-READING WRITING/assets/vocab-01.png
Movers/UNIT 1-L2-READING WRITING/assets/warmup.mp4
```

### Mau worksheet

```text
Movers/UNIT 1-L2-READING WRITING/worksheets/worksheet-01.pdf
Movers/UNIT 1-L2-READING WRITING/worksheets/homework.docx
Movers/UNIT 1-L2-READING WRITING/worksheets/score-sheet.xlsx
```

### Mau program-level documents

```text
Movers/Program Handbook.pdf
Movers/Syllabus.pdf
```

## 11. Khuyen nghi UI cho frontend

Khi nguoi dung chon folder, frontend nen validate ngay:

- co dung 1 program root hay khong
- co folder nao sai format `UNIT x-Ly-Title` khong
- file nao dang nam ngoai lesson folder
- file nao unsupported extension

Nen hien warning ro file nao sai path, vi backend co the skip file do.

## 12. Tom tat rule ngan gon

Rule don gian nhat de nho:

```text
ProgramRoot/UNIT <unit>-L<lesson>-<LessonTitle>/<any-subfolder>/<file>
```

Vi du:

```text
Movers/UNIT 2-L3-SPEAKING/assets/dialog-01.mp3
```

Path tren la hop le va backend se parse duoc:

- program = `Movers`
- unit = `2`
- lesson = `3`
- lessonTitle = `SPEAKING`
