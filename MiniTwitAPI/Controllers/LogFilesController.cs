using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MiniTwitAPI.Controllers
{
    [ApiController]
    [Route("log-files")]
    public class LogFilesController : ControllerBase
    {
        private readonly string _logFilePath = "./logs";

        // GET /log-files
        [HttpGet]
        public ActionResult<List<string>> GetLogFiles()
        {
            if (!Directory.Exists(_logFilePath))
                return NotFound(new { message = $"Log folder not found: {_logFilePath}" });

            var files = Directory
                .EnumerateFiles(_logFilePath, "*.log", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .ToList();

            return Ok(files);
        }

        // GET /log-files/download/{*fileName}
        [HttpGet("download/{*fileName}")]
        public IActionResult DownloadLogFile(string fileName)
        {
            // prevent directory traversal
            if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
                return BadRequest("Invalid file name.");

            var fullPath = Path.Combine(_logFilePath, fileName);
            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { message = $"{fileName} not found." });

            // streams it as an attachment
            return PhysicalFile(fullPath, "text/plain", fileName);
        }
    }
}
