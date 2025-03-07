# DetectCodec README

## Overview
C# .NET Framework console app to detect H.265 (HEVC) codec in MP4 files by parsing file structure.

## Usage
Run: `DetectCodec <filename>`
- Supports only MP4 files.
- Outputs if H.265 is detected.

## Test Files
Includes H.264 and H.265 sample videos from [Sample Videos](https://sample-videos.com/).

## Debug Mode
- Uses `Samplevideo_h265.mp4` by default.
- Change to `Samplevideo_h264.mp4` in code to test H.264.

## Requirements
- .NET Framework
- MP4 file as input
