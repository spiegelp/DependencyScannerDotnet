using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public interface IImportExportService
    {
        string ExportScanResultToJson(ScanResult scanResult);

        Task ExportScanResultToFileAsync(ScanResult scanResult, string fileName);

        ScanResult ImportScanResultFromJson(string json);

        Task<ScanResult> ImportScanResultFromFileAsync(string fileName);
    }
}
