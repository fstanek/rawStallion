using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace RawStallion
{
    class Program
    {
        static Program()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Reading raw file...");

            var inputFileName = args.FirstOrDefault();
            using var file = RawFileReaderFactory.ReadFile(inputFileName);
            file.SelectMsData();

            var headers = file.GetTrailerExtraHeaderInformation().Select(i => i.Label).ToArray();
            var precursorChargeIndex = Array.IndexOf(headers, "Charge State:");
            var compensationVoltage = Array.IndexOf(headers, "FAIMS CV:");

            var indexColumns = new (string name, Func<Scan, IScanEvent, object> factory)[]
            {
                ("ScanNumber",
                    (s, e) => s.ScanStatistics.ScanNumber),
                ("MsOrder",
                    (s, e) => (int)e.MSOrder),
                ("RetentionTime",
                    (s, e) => s.ScanStatistics.StartTime),
                ("PrecursorMz",
                    (s, e) => e.MSOrder > MSOrderType.Ms
                        ? e.GetReaction(0).PrecursorMass
                        : null),
                ("PrecursorCharge",
                    (s, e) => file.GetTrailerExtraValue(s.ScanStatistics.ScanNumber, precursorChargeIndex)),
                ("IsolationWidth",
                    (s, e) => e.MSOrder > MSOrderType.Ms
                        ? e.GetIsolationWidth(0)
                        : null),
                ("CV",
                    (s, e) => compensationVoltage != -1
                        ? file.GetTrailerExtraValue(s.ScanStatistics.ScanNumber, compensationVoltage)
                        : null)
            };

            var indexFileName = GetOutputFileName(inputFileName, "-index");
            using var indexWriter = new StreamWriter(indexFileName);
            WriteLine(indexWriter, indexColumns.Select(c => c.name));

            var noiseColumns = new (string name, Func<Scan, IScanEvent, ICentroidPeak, object> factory)[]
            {
                ("ScanNumber",
                    (s, e, c) => s.ScanStatistics.ScanNumber),
                ("Mass",
                    (s, e, c) => c.Mass),
                ("Intensity",
                    (s, e, c) => c.Intensity),
                ("Baseline",
                    (s, e, c) => c.Baseline.ToString("F2")),
                ("Noise",
                    (s, e, c) => c.Noise.ToString("F2")),
                ("SignalToNoise",
                    (s, e, c) => c.SignalToNoise.ToString("F2"))
            };

            var noiseFileName = GetOutputFileName(inputFileName, "-noise");
            using var noiseWriter = new StreamWriter(noiseFileName);
            WriteLine(noiseWriter, noiseColumns.Select(c => c.name));

            Console.WriteLine($"{file.RunHeader.LastSpectrum - file.RunHeader.FirstSpectrum} scans found.");

            foreach (var scan in file.GetScans(file.RunHeader.FirstSpectrum, file.RunHeader.LastSpectrum))
            {
                var scanNumber = scan.ScanStatistics.ScanNumber;
                var scanEvent = file.GetScanEventForScanNumber(scanNumber);
                WriteLine(indexWriter, indexColumns.Select(c => c.factory(scan, scanEvent)));

                foreach (var centroid in scan.CentroidScan.GetCentroids())
                    WriteLine(noiseWriter, noiseColumns.Select(c => c.factory(scan, scanEvent, centroid)));
            }

            Console.WriteLine("Finished.");
        }

        private static string GetOutputFileName(string fileName, string suffix)
        {
            var outputFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
            return $"{outputFileName}{suffix}.tsv";
        }

        private static void WriteLine(StreamWriter writer, IEnumerable<object> values)
        {
            writer.WriteLine(string.Join('\t', values));
        }
    }
}