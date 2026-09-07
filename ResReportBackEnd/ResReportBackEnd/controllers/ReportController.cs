using Microsoft.AspNetCore.Mvc;
using ResReportBackEnd.services.reports;

namespace ResReportBackEnd.controllers;

[Controller]
[Route("api/v1/reports")]
public class ReportController : Controller
{
    private readonly IEnumerable<IReportService> _reportServices;

    public ReportController(IEnumerable<IReportService> reportServices)
    {
        _reportServices = reportServices;
    }

    [HttpPost("{reportName}")]
    public async Task<IActionResult> RunReport(
        string reportName,
        [FromForm] IFormFile yearAFile,
        [FromForm] IFormFile yearBFile)
    {
        var service = _reportServices.FirstOrDefault(s => s.ReportName == reportName);

        if (service == null)
            return NotFound();

        var parameters = new Dictionary<string, object>
        {
            ["YearAFile"] = yearAFile,
            ["YearBFile"] = yearBFile
        };


        var result = await service.GetReport(parameters);

        if (result.FileBytes != null)
            return File(
                result.FileBytes,
                result.ContentType,
                result.FileName
            );
        return NoContent();
    }
}