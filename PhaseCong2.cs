using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TugasAkhir
{
    class PhaseCong2
    {
        public Hashtable gaborWavelet(params object[] list) {
            Hashtable result = new Hashtable();
            // Get arguments and/or default values
            Hashtable param = checkargs(list);
            Image<Gray, Byte> im = (Image<Gray, Byte>)param["im"];
            int nScale = (int)param["nscale"];
            int nOrient = (int)param["norient"];
            int minWaveLength = (int)param["minWaveLength"];
            double mult = (double)param["mult"];
            double sigmaOnf = (double)param["sigmaOnf"];
            double dThetaOnSigma = (double)param["dThetaOnSigma"];
            double k = (double)param["k"];
            double cutOff = (double)param["cutOff"];
            int g = (int)param["g"];

            double epsilon = 0.0001D; // Used to prevent division by zero.
            double thetaSigma = Math.PI / nOrient / dThetaOnSigma; // Calculate the standard deviation of the
                                                                   // angular Gaussian function used to
                                                                   // construct filters in the freq. plane.

            int rows = im.Height;
            int cols = im.Width;
            Matrix<double> imagefft = new Matrix<double>(rows, cols);
            CvInvoke.Dft(im, imagefft, Emgu.CV.CvEnum.DxtType.Forward, 0); // CEK LAGI

            Matrix<double> zeros = new Matrix<double>(rows, cols);
            for (int i = 0; i < zeros.Rows; i++){
                for (int j = 0; j < zeros.Cols; j++) {
                    zeros.Data[i, j] = 0;
                }
            }

            Matrix<double> totalEnergy = zeros; // Total weighted phase congruency values (energy).
            Matrix<double> totalSumAn = zeros;  // Total filter response amplitude values.
            Matrix<double> orientation = zeros; // Matrix storing orientation with greatest
                                           // energy for each pixel.

            List<List<Matrix<double>>> EO = new List<List<Matrix<double>>>(); // Array of convolution results.
            for (int s = 0; s < nScale; s++) {
                EO.Add(new List<Matrix<double>>());
            }

            Matrix<double> covx2 = zeros;
            Matrix<double> covy2 = zeros;
            Matrix<double> covxy = zeros;

            double[] estMeanE2n = new double[nOrient];
            List<Matrix<double>> ifftFilterArray = new List<Matrix<double>>();

            // Pre-compute some stuff to speed up filter construction

            // Set up X and Y matrices with ranges normalised to +/- 0.5
            // The following code adjusts things appropriately for odd and even values
            // of rows and columns.
            double[] xrange = Utillity.createArray(cols);
            double[] yrange = Utillity.createArray(rows);

            Matrix<double> newXrange = new Matrix<double>(xrange);
            newXrange = newXrange.Transpose();
            Matrix<double> x = new Matrix<double>(rows, cols);

            Matrix<double> newYrange = new Matrix<double>(yrange);
            newYrange = newYrange.Transpose();
            Matrix<double> y = new Matrix<double>(rows, cols);

            CvInvoke.Repeat(newXrange.Reshape(1, 1), newYrange.Cols, 1, x);
            CvInvoke.Repeat(newYrange.Reshape(1, 1).Transpose(), 1, newXrange.Cols, y);

            int baris = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(rows) / 2)) - 1;
            int kolom = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(cols) / 2)) - 1;
            Matrix<double> multX = Utillity.power(x.Clone(), 2);
            Matrix<double> multY = Utillity.power(y.Clone(), 2);
            Matrix<double> radius = Utillity.square(multX.Add(multY)); // Matrix values contain *normalised* radius from centre.
            radius.Data[baris, kolom] = 1; // Get rid of the 0 radius value in the middle 
                                           // so that taking the log of the radius will 
                                           // not cause trouble.
            Matrix<double> theta = Utillity.atanMinY(y,x); // Matrix values contain polar angle.
            radius = Utillity.ifftshift(radius);    // Quadrant shift radius and theta so that filters
            theta = Utillity.ifftshift(theta);      // are constructed with 0 frequency at the corners.

            Matrix<double> sintheta = Utillity.sin(theta);
            Matrix<double> costheta = Utillity.cos(theta);
            x.Dispose(); y.Dispose(); theta.Dispose();

            /* Filters are constructed in terms of two components.
            * 1) The radial component, which controls the frequency band that the filter
            * responds to
            * 2) The angular component, which controls the orientation that the filter
            * responds to.
            * The two components are multiplied together to construct the overall filter.

            * Construct the radial filter components...

            * First construct a low - pass filter that is as large as possible, yet falls
            * away to zero at the boundaries.All log Gabor filters are multiplied by
            * this to ensure no extra frequencies at the 'corners' of the FFT are
            * incorporated as this seems to upset the normalisation process when
            * calculating phase congrunecy. */
            Matrix<double> lp = Utillity.lowpassfilter(rows, cols, 0.45D, 15);

            List<Matrix<double>> logGabor = new List<Matrix<double>>();

            for (int i = 0; i < nScale; i++) {
                double waveLength = minWaveLength * Math.Pow(mult, ((i + 1) - 1));
                double fo = 1.0D / waveLength;  // Centre frequency of filter.
                Matrix<double> temp1 = Utillity.min(Utillity.power(Utillity.log(Utillity.div(radius, fo)), 2), 0);
                double temp2 = (2 * Math.Pow(Math.Log(sigmaOnf), 2));
                logGabor.Add(Utillity.exp(Utillity.div(temp1, temp2)));
                logGabor[i] = Utillity.multArray(logGabor[i], lp);    // Apply low-pass filter
                logGabor[i].Data[0, 0] = 0;    // Set the value at the 0 frequency point of the filter
                                               // back to zero (undo the radius fudge).
            }

            // Then construct the angular filter components...
            List<Matrix<double>> spread = new List<Matrix<double>>();

            for (int o = 0; o < nOrient; o++) {
                double angl = (o + 1 - 1) * Math.PI / nOrient;  // Filter angle.

                //  For each point in the filter matrix calculate the angular distance from
                //  the specified filter orientation.To overcome the angular wrap - around
                //  problem sine difference and cosine difference values are first computed
                //  and then the atan2 function is used to determine angular distance.

                Matrix<double> ds = sintheta * Math.Cos(angl) - costheta * Math.Sin(angl);  // Difference in sine.
                Matrix<double> dc = costheta * Math.Cos(angl) + sintheta * Math.Sin(angl);  // Difference in cosine.
                Matrix<double> dtheta = Utillity.abs(Utillity.atan(ds, dc));    // Absolute angular distance.
                spread.Add(Utillity.exp(Utillity.div(Utillity.min(Utillity.power(dtheta, 2), 0), 2.0D * Math.Pow(thetaSigma, 2)))); // Calculate the angular filter component.
            }

            // The main loop...
            List<Matrix<double>> PC = new List<Matrix<double>>();
            List<Matrix<double>> featType = new List<Matrix<double>>();
            Matrix<double> Energy = zeros;
            for (int o = 1; o <= nOrient; o++) { // For each orientation.
                double angl = (o - 1) * Math.PI / nOrient;  // Filter angle.
                Matrix<double> sumE_ThisOrient = zeros;          // Initialize accumulator matrices.
                Matrix<double> sumO_ThisOrient = zeros;
                Matrix<double> sumAn_ThisOrient = zeros;
                double EM_n = new double();
                Matrix<double> maxAn = new Matrix<double>(rows, cols);

                for (int s = 1; s <= nScale; s++) { // For each scale.
                    Matrix<double> filter = Utillity.multArray(logGabor[s - 1], spread[o - 1]); // Multiply radial and angular
                                                                                                // components to get the filter. 
                    Matrix<double> matBDftBlank = filter.CopyBlank();
                    Matrix<double> dftIn = new Matrix<double>(filter.Rows, filter.Cols, 2);
                    using (VectorOfMat mv = new VectorOfMat(new Mat[] { filter.Mat, matBDftBlank.Mat }))
                        CvInvoke.Merge(mv, dftIn);

                    Matrix<double> dftOut = new Matrix<double>(rows, cols, 2);

                    CvInvoke.Dft(dftIn, dftOut, Emgu.CV.CvEnum.DxtType.InvScale, 0);

                    //The real part of the Fourior Transform
                    using (Matrix<double> outReal = new Matrix<double>(filter.Size)) {
                        //The imaginary part of the Fourior Transform
                        using (Matrix<double> outIm = new Matrix<double>(filter.Size)) {
                            using (VectorOfMat vm = new VectorOfMat())
                            {
                                vm.Push(outReal.Mat);
                                vm.Push(outIm.Mat);
                                CvInvoke.Split(dftOut, vm);
                            }

                            Matrix<double> ifftFilt = outReal * Math.Sqrt(rows * cols); //Note rescaling to match power
                            ifftFilterArray.Add(ifftFilt);  // record ifft2 of filter
                        }
                    }

                    // Convolve image with even and odd filters returning the result in EO
                    using (Matrix<double> resultImage = new Matrix<double>(rows, cols, 0)) {
                        CvInvoke.Multiply(imagefft, filter, resultImage);
                        Matrix<double> imagefftIn = new Matrix<double>(resultImage.Rows, resultImage.Cols, 2);
                        Matrix<double> imagefftInBlank = resultImage.CopyBlank();
                        Matrix<double> imagefftOut = new Matrix<double>(rows, cols, 2);
                        using (VectorOfMat mv = new VectorOfMat(new Mat[] { resultImage.Mat, imagefftInBlank.Mat }))
                            CvInvoke.Merge(mv, imagefftIn);

                        CvInvoke.Dft(imagefftIn, imagefftOut, Emgu.CV.CvEnum.DxtType.InvScale, 0);
                        EO[s-1].Insert(o - 1, imagefftOut);
                    }

                    //The real part of EO
                    Matrix<double> EORealPart = new Matrix<double>(rows, cols);
                    //The imaginary part of EO
                    Matrix<double> EOImPart = new Matrix<double>(rows, cols);

                    using (VectorOfMat vm = new VectorOfMat())
                    {
                        vm.Push(EORealPart.Mat);
                        vm.Push(EOImPart.Mat);
                        CvInvoke.Split(EO[s - 1][o - 1], vm);
                    }
                    // Amplitude of even & odd filter response.
                    Matrix<double> An = new Matrix<double>(rows, cols);
                    using (Matrix<double> temp = Utillity.power(EORealPart, 2) + Utillity.power(EOImPart, 2)) {
                        CvInvoke.Sqrt(temp, An);
                    }

                    sumAn_ThisOrient += An;         // Sum of amplitude responses.
                    sumE_ThisOrient += EORealPart;  // Sum of even filter convolution results.
                    sumO_ThisOrient += EOImPart;    // Sum of odd filter convolution results.

                    if (s == 1) {
                        EM_n = CvInvoke.Sum(Utillity.power(filter, 2)).V0;   // Record mean squared filter value at smallest
                                                                                    // scale. This is used for noise estimation.
                        maxAn = An;                                                 // Record the maximum An over all scales.
                    } else {
                        CvInvoke.Max(maxAn, An, maxAn);
                    }
                }                                   // ... and process the next scale
                // Get weighted mean filter response vector, this gives the weighted mean
                // phase angle.

                Matrix<double> xEnergy = new Matrix<double>(rows, cols);
                CvInvoke.Sqrt(xEnergy, xEnergy);
                xEnergy += epsilon;
                Matrix<double> meanE = new Matrix<double>(rows, cols);
                CvInvoke.Divide(sumE_ThisOrient, xEnergy, meanE);
                Matrix<double> meanO = new Matrix<double>(rows, cols);
                CvInvoke.Divide(sumO_ThisOrient, xEnergy, meanO);

                // Now calculate An(cos(phase_deviation) - | sin(phase_deviation)) | by
                // using dot and cross products between the weighted mean filter response
                // vector and the individual filter response vectors at each scale.  This
                // quantity is phase congruency multiplied by An, which we call energy.

                //The real part of EO
                Matrix<double> E = new Matrix<double>(rows, cols);
                //The imaginary part of EO
                Matrix<double> O = new Matrix<double>(rows, cols);

                for (int s = 1; s <= nScale; s++) {

                    // Extract even and odd convolution results.
                    using (VectorOfMat vm = new VectorOfMat()) {
                        vm.Push(E.Mat);
                        vm.Push(O.Mat);
                        CvInvoke.Split(EO[s - 1][o - 1], vm);
                    }
                    using (Matrix<double> abs = new Matrix<double>(rows, cols)) {
                        CvInvoke.AbsDiff(Utillity.multArray(E, meanO), Utillity.multArray(O, meanE), abs);
                        Energy += Utillity.multArray(E, meanE) + Utillity.multArray(O, meanO) - abs;
                    }
                }

                // Compensate for noise
                // We estimate the noise power from the energy squared response at the
                // smallest scale.If the noise is Gaussian the energy squared will have a
                // Chi - squared 2DOF pdf.We calculate the median energy squared response
                // as this is a robust statistic.From this we estimate the mean.
                // The estimate of noise power is obtained by dividing the mean squared
                // energy value by the mean squared filter value
                double medianE2n;
                using (Matrix<double> realPart = new Matrix<double>(rows, cols)) {
                    using (Matrix<double> imaginaryPart = new Matrix<double>(rows, cols)) {
                        using (VectorOfMat vm = new VectorOfMat()) {
                            vm.Push(realPart.Mat);
                            vm.Push(imaginaryPart.Mat);
                            CvInvoke.Split(EO[0][o - 1], vm);
                        }
                        using (Matrix<double> temp = Utillity.power(realPart, 2) + Utillity.power(imaginaryPart, 2)) {
                            using (Matrix<double> abs = temp.Reshape(1, rows*cols)) { // Ga perlu di sqrt terus di kuadrat
                                medianE2n = Utillity.median(abs);
                            }
                        }
                    }
                }
                double meanE2n = -medianE2n / Math.Log(0.5);
                estMeanE2n[o - 1] = meanE2n;
                double noisePower = meanE2n / EM_n;     // Estimate of noise power.

                Matrix<double> EstSumAn2 = zeros;
                for (int s = 0; s < nScale; s++) {
                    EstSumAn2 += Utillity.power(ifftFilterArray[s], 2);
                }

                Matrix<double> EstSumAiAj = zeros;
                for (int si = 0; si < nScale - 1; si++) {
                    for (int sj = si + 1; sj < nScale; sj++) {
                        EstSumAiAj += Utillity.multArray(ifftFilterArray[si], ifftFilterArray[sj]);
                    }
                }
                double sumEstSumAn2 = CvInvoke.Sum(EstSumAn2).V0;
                double sumEstSumAiAj = CvInvoke.Sum(EstSumAiAj).V0;

                double EstNoiseEnergy2 = 2 * noisePower * sumEstSumAn2 + 4 * noisePower * sumEstSumAiAj;
                double tau = Math.Sqrt(EstNoiseEnergy2/2);                              // Rayleigh parameter
                double EstNoiseEnergy = tau * Math.Sqrt(Math.PI/2);                     // Expected value of noise energy
                double EstNoiseEnergySigma = Math.Sqrt((2-Math.PI/2)*Math.Pow(tau, 2));

                double T = EstNoiseEnergy + k * EstNoiseEnergySigma;                    // Noise threshold

                // The estimated noise effect calculated above is only valid for the PC_1 measure.
                // The PC_2 measure does not lend itself readily to the same analysis.However
                // empirically it seems that the noise effect is overestimated roughly by a factor
                // of 1.7 for the filter parameters used here.

                T = T / 1.7;    // Empirical rescaling of the estimated noise effect to 
                                // suit the PC_2 phase congruency measure

                CvInvoke.Max(Energy - T, zeros, Energy);        // Apply noise threshold

                // Form weighting that penalizes frequency distributions that are
                // particularly narrow.Calculate fractional 'width' of the frequencies
                // present by taking the sum of the filter response amplitudes and dividing
                // by the maximum amplitude at each point on the image.

                Matrix<double> width = new Matrix<double>(rows, cols);
                CvInvoke.Divide(sumAn_ThisOrient, (maxAn + epsilon), width);
                width /= nScale;

                // Now calculate the sigmoidal weighting function for this orientation.
                Matrix<double> weight =  Utillity.power(1 + Utillity.exp((cutOff - width) * g), -1);

                // Apply weighting to energy and then calculate phase congruency

                using (Matrix<double> temp = new Matrix<double>(rows, cols)) {
                    CvInvoke.Multiply(weight, Energy, temp);
                    CvInvoke.Divide(temp, sumAn_ThisOrient, temp);
                    PC.Insert(o - 1, temp);                     // Phase congruency for this orientation
                }
                featType.Insert(o - 1, E + O);

                //Build up covariance data for every point
                Matrix<double> covx = PC[o-1] * Math.Cos(angl);
                Matrix<double> covy = PC[o-1] * Math.Sin(angl);
                covx2 += Utillity.power(covx2, 2);
                covy2 += Utillity.power(covy2, 2);
                using (Matrix<double> temp = new Matrix<double>(rows, cols)) {
                    CvInvoke.Multiply(covx, covy, temp);
                    covxy += temp;
                }
            }   // For each orientation

            result.Add("PC", PC);
            result.Add("featType", featType);
            result.Add("EO", EO);
            result.Add("EO", Energy);

            // Edge and Corner calculations
            // The following is optimised code to calculate principal vector
            // of the phase congruency covariance data and to calculate
            // the minimumum and maximum moments -these correspond to
            // the singular values.

            // First normalise covariance values by the number of orientations/ 2
            covx2 /= (nOrient / 2);
            covy2 /= (nOrient / 2);
            covxy /= nOrient;       // This gives us 2*covxy/(norient/2)

            Matrix<double> denom = new Matrix<double>(rows, cols);
            CvInvoke.Sqrt(Utillity.power(covxy, 2) + Utillity.power(covx2 - covy2, 2) + epsilon, denom);
            Matrix<double> sin2theta = new Matrix<double>(rows, cols);
            Matrix<double> cos2theta = new Matrix<double>(rows, cols);
            CvInvoke.Divide(covxy, denom, sin2theta);
            CvInvoke.Divide((covx2 - covy2), denom, cos2theta);
            Matrix<double> or = Utillity.div(Utillity.atan(sin2theta, cos2theta), 2);   // Orientation perpendicular to edge.
            or = Utillity.round(or * 180/ Math.PI);                                     //Return result rounded to integer
                                                                                        // degrees.
            Matrix<double> neg = Utillity.neg(or);
            or = Utillity.multArray(Utillity.negasi(neg), or) + Utillity.multArray(neg, or + 180);      // Adjust range from -90 to 90
                                                                                                        // to 0 to 180.
            Matrix<double> M = Utillity.div(covy2 + covx2 + denom, 2);                                  // Maximum moment
            Matrix<double> m = Utillity.div(covy2 + covx2 - denom, 2);                                  // Minimum moment

            result.Add("or", or);
            result.Add("M", M);
            result.Add("m", m);

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
