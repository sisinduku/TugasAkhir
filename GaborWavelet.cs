using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TugasAkhir
{
    class GaborWavelet
    {
        public Hashtable gaborWavelet(params object[] list) {
            Hashtable result = new Hashtable();
            // Get arguments and/or default values
            Hashtable param = checkargs(list);
            Image<Gray, Byte> im = (Image<Gray, Byte>)param["im"];
            int nScale = (int)param["nscale"];
            int nOrient = (int)param["norient"];
            int minWaveLength = (int)param["minWaveLength"];
            float mult = (float)param["mult"];
            float sigmaOnf = (float)param["sigmaOnf"];
            float dThetaOnSigma = (float)param["dThetaOnSigma"];
            float k = (float)param["k"];
            float cutOff = (float)param["cutOff"];
            int g = (int)param["g"];

            float epsilon = 0.0001F; // Used to prevent division by zero.
            double thetaSigma = Math.PI / nOrient / dThetaOnSigma; // Calculate the standard deviation of the
                                                                   // angular Gaussian function used to
                                                                   // construct filters in the freq. plane.

            int height = im.Height;
            int width = im.Width;
            Image<Gray, Byte> imagefft = new Image<Gray, byte>(width, height);

            int[,] zeros = new int[height, width];
            for (int i = 0; i < width; i++){
                for (int j = 0; j < height; j++) {
                    zeros[i, j] = 0;
                }
            }
            int[,] totalEnergy = zeros; // Total weighted phase congruency values (energy).
            int[,] totalSumAn = zeros; // Total filter response amplitude values.
            int[,] orientation = zeros; // Matrix storing orientation with greatest
                                        // energy for each pixel.

            int[,] EO = new int[nScale, nOrient]; // Array of convolution results.
            int[,] covx2 = zeros;
            int[,] covy2 = zeros;
            int[,] covxy = zeros;

            int[] estMeanE2n; // BELUM PASTI
            int[,] ifftFilterArray = new int[1, nScale];

            // Pre-compute some stuff to speed up filter construction

            // Set up X and Y matrices with ranges normalised to +/- 0.5
            // The following code adjusts things appropriately for odd and even values
            // of rows and columns.
            if (width % 2 == 1) {

            }
            return result;
        }

        public Hashtable checkargs(params object[] list) {
            Hashtable result = new Hashtable();
            Image<Gray, Byte> image = (Image<Gray, Byte>)list[0];
            int height = image.Height;
            int width = image.Width;
            image.Dispose();
            int nargs = list.Length;

            if (nargs < 1) {
                Console.WriteLine("No image supplied as an argument");
                return result;
            }
            // Setup default value and then overwrite them with supplied value
            Image<Gray, Byte> im = new Image<Gray, byte>(width, height);
            int nScale = 4;             // Number of wavelet scales.
            int nOrient = 6;            // Number of filter orientations.
            int minWaveLength = 3;      // Wavelength of smallest scale filter.
            float mult = 2.1F;          // Scaling factor between successive filters.
            float sigmaOnf = 0.55F;     /* Ratio of the standard deviation of the
                                           Gaussian describing the log Gabor filter's
                                           transfer function in the frequency domain
                                           to the filter center frequency.*/
            float dThetaOnSigma = 1.2F; /* Ratio of angular interval between filter orientations    
                                           and the standard deviation of the angular Gaussian
                                           function used to construct filters in the
                                           freq. plane. */
            float k = 2.0F;             /* No of standard deviations of the noise
                                           energy beyond the mean at which we set the
                                           noise threshold point. */
            float cutOff = 0.5F;        /* The fractional measure of frequency spread
                                           below which phase congruency values get penalized. */
            int g = 10;                 /* Controls the sharpness of the transition in
                                           the sigmoid function used to weight phase
                                           congruency for frequency spread. */

            // Allowed argument reading states
            int allnumeric = 1;     // Numeric argument values in predefined order
            
            int readState = allnumeric;

            if (readState == allnumeric) {
                for (int n = 0; n < nargs; n++) {   
                    if (n == 0) im = (Image<Gray, Byte>)list[n];
                    else if (n == 1) nScale = (int)list[n];
                    else if (n == 2) nOrient = (int)list[n];
                    else if (n == 3) minWaveLength = (int)list[n];
                    else if (n == 4) mult = (float)list[n];
                    else if (n == 5) sigmaOnf = (float)list[n];
                    else if (n == 6) dThetaOnSigma = (float)list[n];
                    else if (n == 7) k = (float)list[n];
                    else if (n == 8) cutOff = (float)list[n];
                    else if (n == 9) g = (int)list[n];
                }
            }

            if (nScale < 1) {
                Console.WriteLine("nscale must be an integer >= 1");
                return result;
            }

            if (nOrient < 1) {
                Console.WriteLine("norient must be an integer >= 1");
                return result;
            }

            if (minWaveLength < 2) {
                Console.WriteLine("It makes little sense to have a wavelength < 2");
                return result;
            }

            if (cutOff < 0 || cutOff > 1) {
                Console.WriteLine("Cut off value must be between 0 and 1");
                return result;
            }

            result.Add("im", im);
            result.Add("nscale", nScale);
            result.Add("norient", nOrient);
            result.Add("minWaveLength", minWaveLength);
            result.Add("mult", mult);
            result.Add("sigmaOnf", sigmaOnf);
            result.Add("dThetaOnSigma", dThetaOnSigma);
            result.Add("k", k);
            result.Add("cutOff", cutOff);
            result.Add("g", g);

            return result;
        }
    }
}
