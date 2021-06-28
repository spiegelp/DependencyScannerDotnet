using DependencyScannerDotnet.Core.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyScannerDotnet.Core.Services
{
    public class ImportExportService : IImportExportService
    {
        public ImportExportService() { }

        public async Task ExportScanResultAsync(ScanResult scanResult, string fileName)
        {
            string json = JsonConvert.SerializeObject(scanResult, CreateSettings());

            await File.WriteAllBytesAsync(fileName, Encoding.UTF8.GetBytes(json)).ConfigureAwait(false);
        }

        public async Task<ScanResult> ImportScanResultAsync(string fileName)
        {
            byte[] bytes = await File.ReadAllBytesAsync(fileName).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<ScanResult>(Encoding.UTF8.GetString(bytes), CreateSettings());
        }

        private JsonSerializerSettings CreateSettings()
        {
            return new()
            {
                NullValueHandling = NullValueHandling.Ignore
            };
        }
    }
}
