﻿using DependencyScannerDotnet.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public interface IImportExportService
    {
        Task ExportScanResultAsync(ScanResult scanResult, string fileName);

        Task<ScanResult> ImportScanResultAsync(string fileName);
    }
}
