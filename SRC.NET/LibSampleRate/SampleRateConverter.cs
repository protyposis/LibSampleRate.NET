/*
** Copyright (C) 2011, 2015 Mario Guggenberger <mg@protyposis.net>
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
using System.Collections.Generic;
using System.Text;

namespace LibSampleRate
{
    /// <summary>
    /// A sample rate converter backed by libsamplerate.
    /// </summary>
    public class SampleRateConverter : IDisposable
    {
        private IntPtr srcState = IntPtr.Zero;
        private SRC_DATA srcData;
        private int error;
        private int channels;
        private double ratio;
        private double bufferedSamples;

        /// <summary>
        /// Creates a new resampler instance with the supplied converter type (which equals the resampling quality)
        /// for a supplied number of channels.
        /// </summary>
        /// <param name="type">the type of the internal conversion algorithm (quality level)</param>
        /// <param name="channels">the number of channels that will be provided to the processing method</param>
        public SampleRateConverter(ConverterType type, int channels)
        {
            srcState = InteropWrapper.src_new(type, channels, out error);
            ThrowExceptionForError(error);
            srcData = new SRC_DATA();

            SetRatio(1d);

            this.channels = channels;
            this.bufferedSamples = 0;
        }

        /// <summary>
        /// Gets the number of bytes buffered by the SRC. Buffering may happen since the SRC may read more
        /// data than it outputs during one #Process call.
        ///
        /// This is a calculated estimation that may be off by a few samples due to the way it
        /// is calculated. See the private #Process method for details.
        /// </summary>
        public int BufferedBytes
        {
            get { return (int)(bufferedSamples * 4); }
        }

        /// <summary>
        /// Resets the resampler, which essentially clears the internal buffer.
        /// </summary>
        public void Reset()
        {
            error = InteropWrapper.src_reset(srcState);
            ThrowExceptionForError(error);
            bufferedSamples = 0;
        }

        /// <summary>
        /// Sets the resampling ratio through an instant change.
        /// </summary>
        /// <param name="ratio">the resampling ratio</param>
        public void SetRatio(double ratio)
        {
            SetRatio(ratio, true);
        }

        /// <summary>
        /// Sets the resampling ratio. Multiplying the input rate with the ratio factor results in the output rate.
        /// </summary>
        /// <param name="ratio">the resampling ratio</param>
        /// <param name="step">true for an instant change in the ratio, false for a gradual linear change during the next #Process call</param>
        public void SetRatio(double ratio, bool step)
        {
            if (step)
            {
                // force the ratio for the next #Process call instead of linearly interpolating from the previous
                // ratio to the current ratio
                error = InteropWrapper.src_set_ratio(srcState, ratio);
                ThrowExceptionForError(error);
            }
            this.ratio = ratio;
        }

        /// <summary>
        /// Checks if a given resampling ratio is valid.
        /// </summary>
        /// <param name="ratio">true if the ratio is valid, else false</param>
        /// <returns></returns>
        public static bool CheckRatio(double ratio)
        {
            return InteropWrapper.src_is_valid_ratio(ratio) == 1;
        }

        /// <summary>
        /// Processes a block of input samples by resampling it to a block of output samples. This method
        /// expects 32-bit floating point samples stored in byte arrays. When the resampler is configured
        /// for multiple channels, samples must be interleaved. The byte counts in the parameters are always
        /// the total counts summed over all channels.
        /// </summary>
        /// <param name="input">the input sample block</param>
        /// <param name="inputOffset">the offset in the input block in bytes</param>
        /// <param name="inputLength">the length of the input block data in bytes</param>
        /// <param name="output">the output sample block</param>
        /// <param name="outputOffset">the offset in the output block in bytes</param>
        /// <param name="outputLength">the available length in the output block data in bytes</param>
        /// <param name="endOfInput">set to true to get the buffered samples from the resampler if no more input samples are supplied</param>
        /// <param name="inputLengthUsed">the number of bytes read from the input block</param>
        /// <param name="outputLengthGenerated">the number of bytes written to the output block</param>
        public void Process(
            byte[] input,
            int inputOffset,
            int inputLength,
            byte[] output,
            int outputOffset,
            int outputLength,
            bool endOfInput,
            out int inputLengthUsed,
            out int outputLengthGenerated
        )
        {
            unsafe
            {
                fixed (
                    byte* inputBytes = &input[inputOffset],
                        outputBytes = &output[outputOffset]
                )
                {
                    Process(
                        (float*)inputBytes,
                        inputLength / 4,
                        (float*)outputBytes,
                        outputLength / 4,
                        endOfInput,
                        out inputLengthUsed,
                        out outputLengthGenerated
                    );
                    inputLengthUsed *= 4;
                    outputLengthGenerated *= 4;
                }
            }
        }

        /// <summary>
        /// Processes a block of input samples by resampling it to a block of output samples. This method
        /// expects 32-bit floating point samples stored in float arrays. When the resampler is configured
        /// for multiple channels, samples must be interleaved. The sample counts in the parameters are always
        /// the total counts summed over all channels.
        /// </summary>
        /// <param name="input">the input sample block</param>
        /// <param name="inputOffset">the offset in the input block in samples</param>
        /// <param name="inputLength">the length of the input block data in samples</param>
        /// <param name="output">the output sample block</param>
        /// <param name="outputOffset">the offset in the output block in samples</param>
        /// <param name="outputLength">the available length in the output block data in samples</param>
        /// <param name="endOfInput">set to true to get the buffered samples from the resampler if no more input samples are supplied</param>
        /// <param name="inputLengthUsed">the number of samples read from the input block</param>
        /// <param name="outputLengthGenerated">the number of samples written to the output block</param>
        public void Process(
            float[] input,
            int inputOffset,
            int inputLength,
            float[] output,
            int outputOffset,
            int outputLength,
            bool endOfInput,
            out int inputLengthUsed,
            out int outputLengthGenerated
        )
        {
            unsafe
            {
                fixed (
                    float* inputFloats = &input[inputOffset],
                        outputFloats = &output[outputOffset]
                )
                {
                    Process(
                        inputFloats,
                        inputLength,
                        outputFloats,
                        outputLength,
                        endOfInput,
                        out inputLengthUsed,
                        out outputLengthGenerated
                    );
                }
            }
        }

        private unsafe void Process(
            float* input,
            int inputLength,
            float* output,
            int outputLength,
            bool endOfInput,
            out int inputLengthUsed,
            out int outputLengthGenerated
        )
        {
            srcData.data_in = input;
            srcData.data_out = output;
            srcData.end_of_input = endOfInput ? 1 : 0;
            srcData.input_frames = inputLength / channels;
            srcData.output_frames = outputLength / channels;
            srcData.src_ratio = ratio;

            error = InteropWrapper.src_process(srcState, ref srcData);
            ThrowExceptionForError(error);

            inputLengthUsed = srcData.input_frames_used * channels;
            outputLengthGenerated = srcData.output_frames_gen * channels;

            bufferedSamples += inputLengthUsed - (outputLengthGenerated / ratio);
        }

        private void ThrowExceptionForError(int error)
        {
            if (error != 0)
            {
                throw new Exception(InteropWrapper.src_strerror(error));
            }
        }

        /// <summary>
        /// Disposes this instance of the resampler, freeing its memory.
        /// </summary>
        public void Dispose()
        {
            if (srcState != IntPtr.Zero)
            {
                srcState = InteropWrapper.src_delete(srcState);
                if (srcState != IntPtr.Zero)
                {
                    throw new Exception("could not delete the sample rate converter");
                }
            }
        }

        ~SampleRateConverter()
        {
            Dispose();
        }
    }
}
