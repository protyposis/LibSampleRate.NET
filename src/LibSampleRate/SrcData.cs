// LibSampleRate.NET – managed wrapper for libsamplerate
// Copyright (c) 2011-2025 Mario Guggenberger
//
// This file is distributed under the BSD 2-Clause License.
// LibSampleRate.NET bundles the native libsamplerate library
// (Copyright (c) Eric de Castro Lopo), which is also released
// under the BSD 2-Clause license. See the LICENSE and COPYING
// files for full terms.
//
// Documentation copied from
// http://www.mega-nerd.com/SRC/api_misc.html#SRC_DATA
using System.Runtime.InteropServices;

namespace LibSampleRate
{
    /// <summary>
    /// SRC_DATA is used to pass data to src_simple() and src_process().
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
