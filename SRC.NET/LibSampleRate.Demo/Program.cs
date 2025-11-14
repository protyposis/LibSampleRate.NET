/*
** Copyright (C) 2015 Mario Guggenberger <mg@protyposis.net>
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
using System.IO;
using System.Text;

namespace LibSampleRate.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Demo configuration
            double seconds = 10;
            double inputRate = 44100;
            double outputRate = 96000;
            double frequency = 440.0; // Hz

            // Create a raw sine wave signal as source
            float[] sourceData = new float[(int)(inputRate * seconds)];
            for (int i = 0; i < sourceData.Length; i++)
            {
                sourceData[i] = (float)Math.Sin((2 * Math.PI) / inputRate * i * frequency);
            }

            // Create and configure resampler
            var src = new SampleRateConverter(ConverterType.SRC_SINC_MEDIUM_QUALITY, 1);
            src.SetRatio(outputRate / inputRate);

            // Setup variables for the resampling stream processing logic
            int inputSampleCount;
            int outputSampleCount;
            int totalInputSampleCount = 0;
            int totalOutputSampleCount = 0;
            float[] inputBuffer = new float[1000];
            int inputBufferFillLevel = 0;
            int inputBufferReadOffset = 0;
            float[] outputBuffer = new float[1000];

            /*
             * Do the resampling block by block until all data has been read and no more data comes out
             *
             * The following block simulates a stream processing approach, where an inputBuffer is filled
             * from a source stream (here the sourceData array) block by block, and each block gets resampled
             * to an outputBuffer, until all the end of the stream has been reached and all buffered samples
             * have been output.
             */
            do
            {
                bool endOfInput = totalInputSampleCount == sourceData.Length;

                if (inputBufferFillLevel == 0)
                {
                    // Refill input buffer
                    inputBufferFillLevel = Math.Min(
                        1000,
                        sourceData.Length - totalInputSampleCount
                    );
                    inputBufferReadOffset = 0;
                    Array.Copy(
                        sourceData,
                        totalInputSampleCount,
                        inputBuffer,
                        0,
                        inputBufferFillLevel
                    );
                }

                src.Process(
                    inputBuffer,
                    inputBufferReadOffset,
                    inputBufferFillLevel,
                    outputBuffer,
                    0,
                    outputBuffer.Length,
                    endOfInput,
                    out inputSampleCount,
                    out outputSampleCount
                );

                inputBufferReadOffset += inputSampleCount;
                inputBufferFillLevel -= inputSampleCount;

                totalInputSampleCount += inputSampleCount;
                totalOutputSampleCount += outputSampleCount;
            } while (inputSampleCount > 0 || outputSampleCount > 0);

            // Print result
            Console.WriteLine(
                "{0} samples resampled to {1} samples (expected {2})",
                totalInputSampleCount,
                totalOutputSampleCount,
                sourceData.Length / inputRate * outputRate
            );
        }
    }
}
