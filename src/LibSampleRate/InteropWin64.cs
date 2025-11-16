// LibSampleRate.NET – managed wrapper for libsamplerate
// Copyright (c) 2011-2025 Mario Guggenberger
//
// This file is distributed under the BSD 2-Clause License.
// LibSampleRate.NET bundles the native libsamplerate library
// (Copyright (c) Eric de Castro Lopo), which is also released
// under the BSD 2-Clause license. See the LICENSE and COPYING
// files for full terms.
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace LibSampleRate
{
    [SuppressUnmanagedCodeSecurity]
    internal class InteropWin64
    {
        private const string LIBSAMPLERATE = "libsamplerate.windows.x64.dll";
        private const CallingConvention CC = CallingConvention.Cdecl;

        [DllImport(LIBSAMPLERATE, CallingConvention = CC)]
        public static extern IntPtr src_new(
            ConverterType converter_type,
            int channels,
            out int error
        );

        [DllImport(LIBSAMPLERATE, CallingConvention = CC)]
        public static extern IntPtr src_delete(IntPtr state);

        [DllImport(LIBSAMPLERATE, CallingConvention = CC)]
        public static extern int src_process(IntPtr state, ref SRC_DATA data);

        [DllImport(LIBSAMPLERATE, CallingConvention = CC)]
        public static extern int src_set_ratio(IntPtr state, double new_ratio);

        [DllImport(LIBSAMPLERATE, CallingConvention = CC)]
        public static extern int src_reset(IntPtr state);

        [DllImport(LIBSAMPLERATE, CallingConvention = CC)]
        public static extern int src_is_valid_ratio(double ratio);

        [DllImport(LIBSAMPLERATE, CallingConvention = CC)]
        public static extern int src_error(IntPtr state);

        [DllImport(LIBSAMPLERATE, CallingConvention = CC)]
        public static extern string src_strerror(int error);
    }
}
