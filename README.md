# rawStallion

rawStallion is a simple command-line application which reads scan data from a single Thermo Raw file (\*.raw) and writes two CSV files containing the following columns:

- `[rawfilename]-index.csv`
  - Scan number
  - MS order
  - Retention time
  - Precursor m/z
  - Precursor charge
  - Isolation width
  - Compensation voltage (CV)

- `[rawfilename]-noise.csv`
  - Scan number
  - Mass
  - Intensity
  - Baseline
  - Noise
  - Signal-to-noise ratio

## Installation

Download the [latest release](https://github.com/fstanek/rawStallion/releases/latest/download/rawStallion.exe).

## Usage

`rawStallion.exe RAWFILE [ScanNumbers]`

### Parameters
- **`RAWFILE`** (mandatory) path to the raw file
- **`[ScanNumbers]`** (optional) scan numbers separated by space. If omitted, all scans are read.
