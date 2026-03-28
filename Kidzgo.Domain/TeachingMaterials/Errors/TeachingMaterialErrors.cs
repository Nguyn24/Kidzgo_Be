using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.TeachingMaterials.Errors;

public static class TeachingMaterialErrors
{
    public static Error NoFilesProvided() => Error.Validation(
        "TeachingMaterial.NoFilesProvided",
        "No teaching material files were provided");

    public static Error ProgramNotFound(Guid programId) => Error.NotFound(
        "TeachingMaterial.ProgramNotFound",
        $"Program with Id = '{programId}' was not found or inactive");

    public static Error ProgramNameNotFound(string programName) => Error.NotFound(
        "TeachingMaterial.ProgramNameNotFound",
        $"Program matching '{programName}' was not found or inactive");

    public static Error ProgramCouldNotBeInferred() => Error.Validation(
        "TeachingMaterial.ProgramCouldNotBeInferred",
        "Program could not be inferred from uploaded folder structure. Provide ProgramId or include the program folder as the first path segment");

    public static Error MultipleProgramRoots(string roots) => Error.Validation(
        "TeachingMaterial.MultipleProgramRoots",
        $"Uploaded files contain multiple program roots: {roots}");

    public static Error NotFound(Guid materialId) => Error.NotFound(
        "TeachingMaterial.NotFound",
        $"Teaching material with Id = '{materialId}' was not found");

    public static Error LessonBundleNotFound(Guid programId, int unitNumber, int lessonNumber) => Error.NotFound(
        "TeachingMaterial.LessonBundleNotFound",
        $"Teaching material bundle was not found for program '{programId}', unit '{unitNumber}', lesson '{lessonNumber}'");

    public static Error UnsupportedFileType(string extension) => Error.Validation(
        "TeachingMaterial.UnsupportedFileType",
        $"Unsupported teaching material file type: {extension}");

    public static Error FileTooLarge(long maxSizeInMb) => Error.Validation(
        "TeachingMaterial.FileTooLarge",
        $"Teaching material file size exceeds maximum allowed size of {maxSizeInMb}MB");

    public static Error StoredFileMissing(Guid materialId) => Error.NotFound(
        "TeachingMaterial.StoredFileMissing",
        $"Encrypted content for teaching material '{materialId}' was not found");

    public static Error NoSupportedFilesFound() => Error.Validation(
        "TeachingMaterial.NoSupportedFilesFound",
        "No supported teaching material files were found in the upload");
}
