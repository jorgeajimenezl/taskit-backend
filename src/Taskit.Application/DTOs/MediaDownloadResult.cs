using System.IO;

namespace Taskit.Application.DTOs;

public record MediaDownloadResult(string FileName, string ContentType, Stream Stream);
