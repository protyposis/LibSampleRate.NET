// LibSampleRate.NET – managed wrapper for libsamplerate
// Copyright (c) 2011-2025 Mario Guggenberger
//
// This file is distributed under the BSD 2-Clause License.
// LibSampleRate.NET bundles the native libsamplerate library
// (Copyright (c) Eric de Castro Lopo), which is also released
// under the BSD 2-Clause license. See the LICENSE and COPYING
// files for full terms.
using System;

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
        /// Creates a new resampler instance for the specified converter type (which defines the resampling quality)
        /// and channel count.
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
        /// Gets the number of bytes currently buffered by the converter. Buffering can occur because the converter
        /// may consume more samples than it produces during a single <see cref="Process"/>
        /// call.
        ///
        /// The value is an estimation and can be off by a few samples due to the calculation approach. See the
        /// private <c>Process</c> overload for details.
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
        /// Sets the resampling ratio. Multiplying the input rate by the ratio factor yields the output rate.
        /// </summary>
        /// <param name="ratio">the resampling ratio</param>
        /// <param name="step">True to change the ratio immediately; false to linearly interpolate to the new ratio during the next processing call.</param>
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
        /// Checks whether a given resampling ratio is valid.
        /// </summary>
        /// <param name="ratio">The ratio to validate.</param>
        /// <returns>True if the ratio is supported; otherwise false.</returns>
        public static bool CheckRatio(double ratio)
        {
            return InteropWrapper.src_is_valid_ratio(ratio) == 1;
        }

        /// <summary>
        /// Processes a block of input samples by resampling it into a block of output samples. This overload
        /// expects 32-bit floating-point samples stored in byte arrays. When the resampler is configured
        /// for multiple channels, the samples must be interleaved. All byte counts refer to the totals across channels.
        /// </summary>
        /// <param name="input">The input sample block.</param>
        /// <param name="inputOffset">The offset in the input block, in bytes.</param>
        /// <param name="inputLength">The length of the input block data, in bytes.</param>
        /// <param name="output">The output sample block.</param>
        /// <param name="outputOffset">The offset in the output block, in bytes.</param>
        /// <param name="outputLength">The available length in the output block, in bytes.</param>
        /// <param name="endOfInput">True to flush buffered samples because no more input samples are available.</param>
        /// <param name="inputLengthUsed">On return, contains the number of bytes read from the input block.</param>
        /// <param name="outputLengthGenerated">On return, contains the number of bytes written to the output block.</param>
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
        /// Processes a block of input samples by resampling it into a block of output samples. This overload
        /// expects 32-bit floating-point samples stored in <see cref="float"/> arrays. When the resampler is configured
        /// for multiple channels, the samples must be interleaved. All sample counts refer to the totals across channels.
        /// </summary>
        /// <param name="input">The input sample block.</param>
        /// <param name="inputOffset">The offset in the input block, in samples.</param>
        /// <param name="inputLength">The length of the input block data, in samples.</param>
        /// <param name="output">The output sample block.</param>
        /// <param name="outputOffset">The offset in the output block, in samples.</param>
        /// <param name="outputLength">The available length in the output block, in samples.</param>
        /// <param name="endOfInput">True to flush buffered samples because no more input samples are available.</param>
        /// <param name="inputLengthUsed">On return, contains the number of samples read from the input block.</param>
        /// <param name="outputLengthGenerated">On return, contains the number of samples written to the output block.</param>
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
