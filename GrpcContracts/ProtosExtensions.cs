using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace GrpcContracts
{
    public static class ProtosExtensions
    {
        public static string GetPath()
        {
            var protoPath = Path.Combine(Directory.GetCurrentDirectory(), "Protos");

            if (!Directory.Exists(protoPath))
            {
                var exePath = Assembly.GetExecutingAssembly().Location;

                var baseDirectory = Path.GetDirectoryName(exePath);

                protoPath = Path.Combine(baseDirectory, "Protos");
            }

            return protoPath;
        }

        public static void GetProtos(this WebApplication app)
        {
            app.MapGet("/protos", (HttpContext context) =>
            {
                var protoPath = GetPath();

                if (!Directory.Exists(protoPath))
                {
                    return Results.NotFound(new { error = "Protos directory not found" });
                }

                var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";

                var protoFiles = Directory.GetFiles(protoPath, "*.proto")
                    .Select(file => new FileInfo(file))
                    .Select(fi => new
                    {
                        name = fi.Name,
                        size = fi.Length,
                        sizeFormatted = FormatFileSize(fi.Length),
                        lastModified = fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        downloadUrl = $"{baseUrl}/protos/download/{fi.Name}",
                        viewUrl = $"{baseUrl}/protos/view/{fi.Name}",
                        rawUrl = $"{baseUrl}/protos/raw/{fi.Name}"
                    })
                    .ToList();

                // اگر درخواست از مرورگر است، صفحه HTML برگردان
                if (context.Request.Headers.Accept.ToString().Contains("text/html"))
                {
                    var html = GenerateHtmlList(protoFiles, baseUrl);
                    return Results.Content(html, "text/html");
                }

                // در غیر این صورت JSON برگردان
                return Results.Json(new
                {
                    count = protoFiles.Count,
                    directory = protoPath,
                    baseUrl = baseUrl,
                    files = protoFiles
                });
            });
        }

        public static void DownloadProto(this WebApplication app)
        {
            app.MapGet("/protos/download/{fileName}", async (string fileName) =>
            {
                var (exists, filePath) = ValidateProtoFile(fileName);
                if (!exists) return Results.NotFound($"Proto file '{fileName}' not found");

                var fileBytes = await File.ReadAllBytesAsync(filePath);
                return Results.File(fileBytes, "application/octet-stream", fileName);
            });
        }

        public static void ViewProto(this WebApplication app)
        {
            app.MapGet("/protos/view/{fileName}", async (string fileName) =>
            {
                var (exists, filePath) = ValidateProtoFile(fileName);
                if (!exists) return Results.NotFound($"Proto file '{fileName}' not found");

                var content = await File.ReadAllTextAsync(filePath);
                var html = GenerateHtmlView(fileName, content);
                return Results.Content(html, "text/html");
            });
        }

        public static void Rawproto(this WebApplication app)
        {
            app.MapGet("/protos/raw/{fileName}", async (string fileName) =>
            {
                var (exists, filePath) = ValidateProtoFile(fileName);
                if (!exists) return Results.NotFound($"Proto file '{fileName}' not found");

                var content = await File.ReadAllTextAsync(filePath);
                return Results.Text(content, "text/plain");
            });
        }

        static (bool exists, string path) ValidateProtoFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".proto"))
                return (false, "");

            var protoPath = GetPath();

            var filePath = Path.Combine(protoPath, fileName);

            return (File.Exists(filePath), filePath);
        }

        static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        static string GenerateHtmlList(IEnumerable<dynamic> files, string baseUrl)
        {
            var filesHtml = string.Join("", files.Select(f => $"""
                <div class="file-item">
                    <div class="file-name">&#128196; {f.name}</div>
                    <div class="file-info">Size: {f.sizeFormatted} | Modified: {f.lastModified}</div>
                    <div class="file-actions">
                        <a href="{f.viewUrl}" class="btn btn-view">&#128065; View</a>
                        <a href="{f.downloadUrl}" class="btn btn-download">&#128229; Download</a>
                        <a href="{f.rawUrl}" class="btn btn-raw">&#128196; Raw</a>
                    </div>
                </div>
            """));

            return "<!DOCTYPE html><html><head><title>Proto Files</title><meta charset=\"UTF-8\"><style>" +
                   "body{font-family:Arial,sans-serif;margin:0;padding:20px;background:#f0f2f5;}" +
                   ".container{max-width:800px;margin:0 auto;}" +
                   ".header{background:white;padding:20px;border-radius:10px;margin-bottom:20px;box-shadow:0 2px 10px rgba(0,0,0,0.1);}" +
                   ".file-list{background:white;border-radius:10px;padding:20px;box-shadow:0 2px 10px rgba(0,0,0,0.1);}" +
                   ".file-item{border-bottom:1px solid #eee;padding:15px 0;}" +
                   ".file-item:last-child{border-bottom:none;}" +
                   ".file-name{font-weight:bold;font-size:16px;margin-bottom:5px;}" +
                   ".file-info{color:#666;font-size:12px;margin-bottom:10px;}" +
                   ".file-actions{display:flex;gap:10px;}" +
                   ".btn{padding:8px 12px;text-decoration:none;border-radius:5px;font-size:12px;}" +
                   ".btn-view{background:#17a2b8;color:white;}" +
                   ".btn-download{background:#28a745;color:white;}" +
                   ".btn-raw{background:#6c757d;color:white;}" +
                   ".btn:hover{opacity:0.8;}" +
                   "</style></head><body>" +
                   "<div class=\"container\">" +
                   "<div class=\"header\">" +
                   "<h1>&#128193; Proto Files</h1>" +
                   $"<p>Total {files.Count()} proto file(s) found</p>" +
                   "</div>" +
                   "<div class=\"file-list\">" +
                   $"{filesHtml}" +
                   "</div>" +
                   "</div>" +
                   "</body></html>";
        }

        static string GenerateHtmlView(string fileName, string content)
        {
            return "<!DOCTYPE html><html><head><title>" + fileName + "</title><meta charset=\"UTF-8\"><style>" +
                   "body{font-family:Arial,sans-serif;margin:0;padding:20px;background:#f0f2f5;}" +
                   ".container{max-width:1000px;margin:0 auto;}" +
                   ".header{background:white;padding:20px;border-radius:10px;margin-bottom:20px;box-shadow:0 2px 10px rgba(0,0,0,0.1);}" +
                   ".content{background:white;border-radius:10px;padding:20px;box-shadow:0 2px 10px rgba(0,0,0,0.1);}" +
                   "pre{background:#1e1e1e;color:#d4d4d4;padding:20px;border-radius:5px;overflow-x:auto;font-family:'Consolas',monospace;}" +
                   ".actions{display:flex;gap:10px;margin-bottom:20px;}" +
                   ".btn{padding:10px 15px;text-decoration:none;border-radius:5px;color:white;}" +
                   ".btn-download{background:#28a745;}" +
                   ".btn-back{background:#6c757d;}" +
                   ".btn:hover{opacity:0.8;}" +
                   "</style></head><body>" +
                   "<div class=\"container\">" +
                   "<div class=\"header\">" +
                   "<h1>&#128196; " + fileName + "</h1>" +
                   "<div class=\"actions\">" +
                   "<a href=\"/protos/download/" + fileName + "\" class=\"btn btn-download\">&#128229; Download</a>" +
                   "<a href=\"/protos\" class=\"btn btn-back\">&#128193; Back to List</a>" +
                   "</div></div>" +
                   "<div class=\"content\">" +
                   "<pre><code>" + System.Net.WebUtility.HtmlEncode(content) + "</code></pre>" +
                   "</div></div></body></html>";
        }
    }
}
