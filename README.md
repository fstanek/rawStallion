* rawStallion

rawStallion is a small commandline application. It takes a single Thermo Raw file (*.raw) and puts out two files:
- a CSV file named `{RawFileName}-index.csv`
- a CSV file named `{RawFileName}-noise.csv`

** Installation

Go to releases and download the latest version.

** Usage

Usage: rawStallion.exe RAWFILE [ScanNumber1] [ScanNumber2] ...

By default rawStallion writes considers all scan numbers of the raw file.

Writes the following fields to TSV (for each given scan number and its centroids, for all MS2 scans if no scan number is given):
- ScanNumber
- Mass
- Intensity
- Baseline
- Noise
- SignalToNoise