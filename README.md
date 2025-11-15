SRC.NET Managed Wrapper for LibSampleRate
===================

This is a managed .NET wrapper written in C# for the popular [libsamplerate](http://www.mega-nerd.com/SRC/) (aka Secret Rabbit Code) resampling library. It has been extensively used in the past few years and should work flawlessly, but input is always welcome.

The Visual Studio solution in this repository contains two projects, the managed wrapper which compiles to a library, and a small demo application. The wrapper comes with both x86 and x64 precompiled libsamplerate DLLs and automatically binds to the correct library at runtime. Compiling instructions and a makefile for Visual Studio 2013 are provided as well.


License & Credits
-----------------

Copyright (C) 2011-2025 Mario Guggenberger <mg@protyposis.net>. This project builds upon libsamplerate, Copyright (c) 2012-2016, Erik de Castro Lopo <erikd@mega-nerd.com>. Released under the BSD 2-Clause license.
