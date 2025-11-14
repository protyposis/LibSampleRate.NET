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
using System.Runtime.InteropServices;

namespace LibSampleRate
{
    /// <summary>
    /// SRC_DATA is used to pass data to src_simple() and src_process().
    /// Copied from http://www.mega-nerd.com/SRC/api_misc.html#SRC_DATA
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential)]
    internal unsafe struct SRC_DATA
    {
        /// <summary>
        /// A pointer to the input data samples.
        /// </summary>
        public float* data_in;

        /// <summary>
        /// A pointer to the output data samples.
        /// </summary>
        public float* data_out;

        /// <summary>
        /// The number of frames of data pointed to by data_in.
        /// </summary>
        public int input_frames;

        /// <summary>
        /// Maximum number of frames pointer to by data_out.
        /// </summary>
        public int output_frames;

        /// <summary>
        /// When the src_process function returns output_frames_gen will be set to the number of output frames
        /// generated and input_frames_used will be set to the number of input frames consumed to generate the
        /// provided number of output frames.
        /// </summary>
        public int input_frames_used;

        /// <summary>
        /// When the src_process function returns output_frames_gen will be set to the number of output frames
        /// generated and input_frames_used will be set to the number of input frames consumed to generate the
        /// provided number of output frames.
        /// </summary>
        public int output_frames_gen;

        /// <summary>
        /// Equal to 0 if more input data is available and 1 otherwise.
        /// </summary>
        public int end_of_input;

        /// <summary>
        /// Equal to output_sample_rate / input_sample_rate.
        /// </summary>
        public double src_ratio;
    }
}
