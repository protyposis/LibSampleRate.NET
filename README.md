# LibSampleRate.NET

A managed wrapper for the popular [libsamplerate](https://libsndfile.github.io/libsamplerate/) (aka Secret Rabbit Code) resampling library, written in C#.

## Platform Support

The library currently supports Windows x86 and x64. Other platforms can be easily added, but are currently not included because the [upstream repository](https://github.com/libsndfile/libsamplerate) only provides Windows builds.

## Usage

The easiest way to use this library is by installing the package through [NuGet](https://www.nuget.org/packages/LibSampleRate).

The entire public API surface lives in and is documented in [`SampleRateConverter.cs`](https://github.com/protyposis/LibSampleRate.NET/blob/main/src/LibSampleRate/SampleRateConverter.cs). A minimal example:

```csharp
using System;
using LibSampleRate;

var channels = 1;
var ratio = 2.0;
var inputBuffer = new float[1000];
var outputBuffer = new float[2000];

var src = new SampleRateConverter(ConverterType.SRC_SINC_BEST_QUALITY, channels);
src.SetRatio(ratio);

src.Process(
    inputBuffer,
    0,
    inputBuffer.Length,
    outputBuffer,
    0,
    outputBuffer.Length,
    endOfInput: true,
    out var inputSampleCount,
    out var outputSampleCount
);

Console.WriteLine($"{inputSampleCount} input samples resampled to {outputSampleCount} output samples.");
```

See the `LibSampleRate.Demo` project for a more complete streaming scenario that shows how to push blocks of audio through the converter.

## Development

The solution contains two projects: the managed wrapper library, and a small console app demonstrating streaming resampling and serving as an integration test.

Run the helper script before building with Visual Studio or `dotnet build` so the native DLLs and their notices are available:

```powershell
pwsh scripts/fetch-libsamplerate.ps1
```

## License & Credits

Copyright (C) 2011-2025 Mario Guggenberger <mg@protyposis.net>. Released under the BSD 2-Clause license.

This project builds upon libsamplerate, Copyright (c) 2012-2016, Erik de Castro Lopo <erikd@mega-nerd.com>. Released under the BSD 2-Clause license.
