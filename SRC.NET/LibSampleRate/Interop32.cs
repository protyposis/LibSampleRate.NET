/*
** Copyright (C) 2002-2011 Erik de Castro Lopo <erikd@mega-nerd.com>
**
** This program is free software; you can redistribute it and/or modify
** it under the terms of the GNU General Public License as published by
** the Free Software Foundation; either version 2 of the License, or
** (at your option) any later version.
**
** This program is distributed in the hope that it will be useful,
** but WITHOUT ANY WARRANTY; without even the implied warranty of
** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
** GNU General Public License for more details.
**
** You should have received a copy of the GNU General Public License
** along with this program; if not, write to the Free Software
** Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307, USA.
*/
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace LibSampleRate
{
    /// <summary>
    /// Interop API methods for x86 DLL. Copied from samplerate.h.
    /// </summary>
    [SuppressUnmanagedCodeSecurity]
    internal class Interop32
    {
        private const string LIBSAMPLERATE = "libsamplerate/samplerate.windows.x86.dll";
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
