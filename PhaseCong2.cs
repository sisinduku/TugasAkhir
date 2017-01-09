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
        public Hashtable calcPhaseCong2(Image<Gray, double> input, List<Matrix<double>> PC, Matrix<double> or)
        {
            Hashtable result = new Hashtable();
            // Get arguments and/or default values
            Hashtable param = checkargs();
            Image<Gray, double> im = input;
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
            int rows = im.Rows;
            int cols = im.Cols;
            Matrix<double> img = new Matrix<double>(im.Rows, im.Cols);
            im.CopyTo(img);
            im.Dispose();
            Matrix<double> imagefft = new Matrix<double>(rows, cols, 2);
            Matrix<double> imageBlank = img.CopyBlank();
            Matrix<double> imageIn = new Matrix<double>(img.Rows, img.Cols, 2);
            using (VectorOfMat mv = new VectorOfMat(new Mat[] { img.Mat, imageBlank.Mat }))
                CvInvoke.Merge(mv, imageIn);

            CvInvoke.Dft(imageIn, imagefft, Emgu.CV.CvEnum.DxtType.Forward, 0);
            //The real part of imagefft
            Matrix<double> imagefftRealPart = new Matrix<double>(img.Rows, img.Cols);
            //The imaginary part of imagefft
            Matrix<double> imagefftImPart = new Matrix<double>(img.Rows, img.Cols);

            using (VectorOfMat vm = new VectorOfMat())
            {
                vm.Push(imagefftRealPart.Mat);
                vm.Push(imagefftImPart.Mat);
                CvInvoke.Split(imagefft, vm);
            }
            img.Dispose(); imagefft.Dispose(); imageBlank.Dispose(); imageIn.Dispose();
            Matrix<double> zeros = new Matrix<double>(rows, cols);

            //Matrix<double> totalEnergy = new Matrix<double>(rows, cols); // Total weighted phase congruency values (energy).
            //Matrix<double> totalSumAn = new Matrix<double>(rows, cols);  // Total filter response amplitude values.
            //Matrix<double> orientation = new Matrix<double>(rows, cols); // Matrix storing orientation with greatest
            // energy for each pixel.

            List<List<Matrix<double>>> EO = new List<List<Matrix<double>>>(); // Array of convolution results.
            for (int s = 0; s < nScale; s++)
            {
                EO.Add(new List<Matrix<double>>());
            }

            Matrix<double> covx2 = new Matrix<double>(rows, cols);
            Matrix<double> covy2 = new Matrix<double>(rows, cols);
            Matrix<double> covxy = new Matrix<double>(rows, cols);

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
            int baris, kolom;
            if (rows % 2 == 1)
            {
                baris = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(rows) / 2)) - 1;
            }
            else
            {
                baris = rows / 2;
            }
            if (cols % 2 == 1)
            {
                kolom = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(cols) / 2)) - 1;
            }
            else
            {
                kolom = cols / 2;
            }

            Matrix<double> multX = new Matrix<double>(rows, cols);
            CvInvoke.Pow(x, 2, multX);
            Matrix<double> multY = new Matrix<double>(rows, cols);
            CvInvoke.Pow(y, 2, multY);
            Matrix<double> radius = Utillity.square(multX.Add(multY)); // Matrix values contain *normalised* radius from centre.
            radius.Data[baris, kolom] = 1; // Get rid of the 0 radius value in the middle 
                                           // so that taking the log of the radius will 
                                           // not cause trouble.
            Matrix<double> theta = Utillity.atanMinY(y, x); // Matrix values contain polar angle.
            radius = Utillity.ifftshift(radius);    // Quadrant shift radius and theta so that filters
            theta = Utillity.ifftshift(theta);      // are constructed with 0 frequency at the corners.
            Matrix<double> sintheta = Utillity.sin(theta);
            Matrix<double> costheta = Utillity.cos(theta);
            x.Dispose(); y.Dispose(); theta.Dispose(); multX.Dispose(); multY.Dispose();

            //Filters are constructed in terms of two components.
            // 1) The radial component, which controls the frequency band that the filter
            // responds to
            // 2) The angular component, which controls the orientation that the filter
            // responds to.
            // The two components are multiplied together to construct the overall filter.

            // Construct the radial filter components...

            // First construct a low - pass filter that is as large as possible, yet falls
            // away to zero at the boundaries.All log Gabor filters are multiplied by
            // this to ensure no extra frequencies at the 'corners' of the FFT are
            // incorporated as this seems to upset the normalisation process when
            // calculating phase congrunecy. 
            Matrix<double> lp = Utillity.lowpassfilter(rows, cols, 0.45D, 15);
            List<Matrix<double>> logGabor = new List<Matrix<double>>();

            for (int i = 0; i < nScale; i++)
            {
                double waveLength = minWaveLength * Math.Pow(mult, ((i + 1) - 1));
                double fo = 1.0D / waveLength;  // Centre frequency of filter.
                //Matrix<double> temp1 = Utillity.min(Utillity.power(Utillity.log(Utillity.div(radius, fo)), 2), 0);
                Matrix<double> temp1 = new Matrix<double>(rows, cols);
                using (Matrix<double> power = new Matrix<double>(rows, cols))
                {
                    using (Matrix<double> log = new Matrix<double>(rows, cols))
                    {
                        CvInvoke.Log(radius / fo, log);
                        CvInvoke.Pow(log, 2, power);
                        temp1 = Utillity.min(power, 0);
                    }
                }
                double temp2 = (2 * Math.Pow(Math.Log(sigmaOnf), 2));
                logGabor.Add(Utillity.exp(temp1 / temp2));
                logGabor[i] = Utillity.multArray(logGabor[i], lp);    // Apply low-pass filter
                logGabor[i].Data[0, 0] = 0;    // Set the value at the 0 frequency point of the filter
                                               // back to zero (undo the radius fudge).
                temp1.Dispose();
            }

            // Then construct the angular filter components...
            List<Matrix<double>> spread = new List<Matrix<double>>();

            for (int o = 0; o < nOrient; o++)
            {
                double angl = (o + 1 - 1) * Math.PI / nOrient;  // Filter angle.

                //  For each point in the filter matrix calculate the angular distance from
                //  the specified filter orientation.To overcome the angular wrap - around
                //  problem sine difference and cosine difference values are first computed
                //  and then the atan2 function is used to determine angular distance.

                Matrix<double> ds = sintheta * Math.Cos(angl) - costheta * Math.Sin(angl);  // Difference in sine.
                Matrix<double> dc = costheta * Math.Cos(angl) + sintheta * Math.Sin(angl);  // Difference in cosine.
                Matrix<double> dtheta = Utillity.abs(Utillity.atan(ds, dc));    // Absolute angular distance.
                spread.Add(Utillity.exp(Utillity.div(Utillity.min(Utillity.power(dtheta, 2), 0), 2.0D * Math.Pow(thetaSigma, 2)))); // Calculate the angular filter component.
                ds.Dispose(); dc.Dispose(); dtheta.Dispose();
            }

            // The main loop...
            //PC = new List<Matrix<double>>();
            List<Matrix<double>> featType = new List<Matrix<double>>();
            Matrix<double> Energy = new Matrix<double>(rows, cols);
            for (int o = 1; o <= nOrient; o++)
            { // For each orientation.
                ifftFilterArray = new List<Matrix<double>>();
                double angl = (o - 1) * Math.PI / nOrient;  // Filter angle.
                Energy = new Matrix<double>(rows, cols);
                Matrix<double> sumE_ThisOrient = new Matrix<double>(rows, cols);          // Initialize accumulator matrices.
                Matrix<double> sumO_ThisOrient = new Matrix<double>(rows, cols);
                Matrix<double> sumAn_ThisOrient = new Matrix<double>(rows, cols);
                double EM_n = new double();
                Matrix<double> maxAn = new Matrix<double>(rows, cols);

                for (int s = 1; s <= nScale; s++)
                { // For each scale.
                    Matrix<double> filter = Utillity.multArray(logGabor[s - 1], spread[o - 1]); // Multiply radial and angular
                                                                                                // components to get the filter. 
                    
                    Matrix<double> matBDftBlank = filter.CopyBlank();
                    Matrix<double> dftIn = new Matrix<double>(filter.Rows, filter.Cols, 2);
                    using (VectorOfMat mv = new VectorOfMat(new Mat[] { filter.Mat, matBDftBlank.Mat }))
                        CvInvoke.Merge(mv, dftIn);

                    Matrix<double> dftOut = new Matrix<double>(rows, cols, 2);

                    CvInvoke.Dft(dftIn, dftOut, Emgu.CV.CvEnum.DxtType.InvScale, 0);
                    matBDftBlank.Dispose(); dftIn.Dispose();
                    //The real part of the Fourior Transform
                    using (Matrix<double> outReal = new Matrix<double>(filter.Size))
                    {
                        //The imaginary part of the Fourior Transform
                        using (Matrix<double> outIm = new Matrix<double>(filter.Size))
                        {
                            using (VectorOfMat vm = new VectorOfMat())
                            {
                                vm.Push(outReal.Mat);
                                vm.Push(outIm.Mat);
                                CvInvoke.Split(dftOut, vm);
                            }
                            Matrix<double> ifftFilt = outReal * Math.Sqrt(rows * cols); //Note rescaling to match power
                            ifftFilterArray.Add(ifftFilt.Clone());  // record ifft2 of filter
                            ifftFilt.Dispose();
                        }
                    }
                    dftOut.Dispose();
                    // Convolve image with even and odd filters returning the result in EO
                    using (Matrix<double> resultImage = new Matrix<double>(rows, cols, 2))
                    {
                        Matrix<double> mult1 = new Matrix<double>(rows, cols);
                        Matrix<double> mult2 = new Matrix<double>(rows, cols);
                        CvInvoke.Multiply(imagefftRealPart, filter, mult1);
                        CvInvoke.Multiply(imagefftImPart, filter, mult2);
                        using (VectorOfMat mv = new VectorOfMat(new Mat[] { mult1.Mat, mult2.Mat }))
                            CvInvoke.Merge(mv, resultImage);
                        Matrix<double> imagefftOut = new Matrix<double>(rows, cols, 2);

                        CvInvoke.Dft(resultImage, imagefftOut, Emgu.CV.CvEnum.DxtType.InvScale, 0);
                        EO[s - 1].Insert(o - 1, imagefftOut.Clone());
                        mult1.Dispose(); mult2.Dispose(); imagefftOut.Dispose();
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

                    /*if (o == 2 && s == 3)
                    {
                        for (int i = 0; i < EORealPart.Rows; i++)
                        {
                            for (int j = 0; j < EORealPart.Cols; j++)
                            {
                                Console.WriteLine("[" + (i + 1) + ", " + (j + 1) + "] = " + EORealPart.Data[i, j] + " + " + EOImPart.Data[i, j]);
                            }
                        }
                    }*/

                    // Amplitude of even & odd filter response.
                    Matrix<double> An = new Matrix<double>(rows, cols);
                    using (Matrix<double> temp = Utillity.power(EORealPart, 2) + Utillity.power(EOImPart, 2))
                    {
                        CvInvoke.Sqrt(temp, An);
                    }

                    using (Matrix<double> temp = sumAn_ThisOrient.Clone())
                        CvInvoke.Add(temp.Clone(), An.Clone(), sumAn_ThisOrient);           // Sum of amplitude responses.
                    using (Matrix<double> temp = sumE_ThisOrient.Clone())
                        CvInvoke.Add(temp.Clone(), EORealPart.Clone(), sumE_ThisOrient);    // Sum of even filter convolution results.
                    using (Matrix<double> temp = sumO_ThisOrient.Clone())
                        CvInvoke.Add(temp.Clone(), EOImPart.Clone(), sumO_ThisOrient);      // Sum of odd filter convolution results.

                    if (s == 1)
                    {
                        EM_n = CvInvoke.Sum(Utillity.power(filter, 2)).V0;   // Record mean squared filter value at smallest
                                                                             // scale. This is used for noise estimation.
                        maxAn = An.Clone();                                                 // Record the maximum An over all scales.
                    }
                    else
                    {
                        Matrix<double> tempMax = maxAn.Clone();
                        CvInvoke.Max(tempMax, An, maxAn);
                        tempMax.Dispose();
                    }
                    
                    An.Dispose(); EORealPart.Dispose(); EOImPart.Dispose();
                }                                   // ... and process the next scale
                
                // Get weighted mean filter response vector, this gives the weighted mean
                // phase angle.

                Matrix<double> xEnergy = new Matrix<double>(rows, cols);
                using (Matrix<double> sumE_ThisOrientKuadrat = new Matrix<double>(rows, cols))
                {
                    using (Matrix<double> sumO_ThisOrientKuadrat = new Matrix<double>(rows, cols))
                    {
                        CvInvoke.Pow(sumE_ThisOrient, 2, sumE_ThisOrientKuadrat);
                        CvInvoke.Pow(sumO_ThisOrient, 2, sumO_ThisOrientKuadrat);
                        CvInvoke.Sqrt(sumE_ThisOrientKuadrat + sumO_ThisOrientKuadrat, xEnergy);
                    }
                }
                xEnergy += epsilon;

                Matrix<double> meanE = new Matrix<double>(rows, cols);
                CvInvoke.Divide(sumE_ThisOrient.Clone(), xEnergy.Clone(), meanE);
                Matrix<double> meanO = new Matrix<double>(rows, cols);
                CvInvoke.Divide(sumO_ThisOrient.Clone(), xEnergy.Clone(), meanO);
                xEnergy.Dispose(); sumE_ThisOrient.Dispose(); sumO_ThisOrient.Dispose();
                // Now calculate An(cos(phase_deviation) - | sin(phase_deviation)) | by
                // using dot and cross products between the weighted mean filter response
                // vector and the individual filter response vectors at each scale.  This
                // quantity is phase congruency multiplied by An, which we call energy.

                //The real part of EO
                Matrix<double> E = new Matrix<double>(rows, cols);
                //The imaginary part of EO
                Matrix<double> O = new Matrix<double>(rows, cols);

                for (int s = 1; s <= nScale; s++)
                {

                    // Extract even and odd convolution results.
                    using (VectorOfMat vm = new VectorOfMat())
                    {
                        vm.Push(E.Mat);
                        vm.Push(O.Mat);
                        CvInvoke.Split(EO[s - 1][o - 1], vm);
                    }
                    using (Matrix<double> abs = new Matrix<double>(rows, cols))
                    {
                        CvInvoke.AbsDiff(Utillity.multArray(E, meanO), Utillity.multArray(O, meanE), abs);
                        Energy += Utillity.multArray(E, meanE) + Utillity.multArray(O, meanO) - abs;
                    }
                }
                meanE.Dispose(); meanO.Dispose();
                
                // Compensate for noise
                // We estimate the noise power from the energy squared response at the
                // smallest scale.If the noise is Gaussian the energy squared will have a
                // Chi - squared 2DOF pdf.We calculate the median energy squared response
                // as this is a robust statistic.From this we estimate the mean.
                // The estimate of noise power is obtained by dividing the mean squared
                // energy value by the mean squared filter value
                double medianE2n;
                using (Matrix<double> realPart = new Matrix<double>(rows, cols))
                {
                    using (Matrix<double> imaginaryPart = new Matrix<double>(rows, cols))
                    {
                        using (VectorOfMat vm = new VectorOfMat())
                        {
                            vm.Push(realPart.Mat);
                            vm.Push(imaginaryPart.Mat);
                            CvInvoke.Split(EO[0][o - 1], vm);
                        }
                        using (Matrix<double> temp = Utillity.power(realPart, 2) + Utillity.power(imaginaryPart, 2))
                        {
                            medianE2n = Utillity.median(temp.Clone());
                        }
                    }
                }
                double meanE2n = -medianE2n / Math.Log(0.5);
                estMeanE2n[o - 1] = meanE2n;
                double noisePower = meanE2n / EM_n;     // Estimate of noise power.
                Matrix<double> EstSumAn2 = new Matrix<double>(rows, cols);
                
                for (int s = 0; s < nScale; s++)
                {
                    using (Matrix<double> temp = EstSumAn2.Clone())
                        CvInvoke.Add(temp, Utillity.power(ifftFilterArray[s], 2), EstSumAn2);
                    //EstSumAn2 += Utillity.power(ifftFilterArray[s], 2);
                }
                Matrix<double> EstSumAiAj = new Matrix<double>(rows, cols);
                for (int si = 0; si < nScale - 1; si++)
                {
                    for (int sj = si + 1; sj < nScale; sj++)
                    {
                        EstSumAiAj += Utillity.multArray(ifftFilterArray[si], ifftFilterArray[sj]);
                    }
                }
                
                double sumEstSumAn2 = CvInvoke.Sum(EstSumAn2).V0;
                double sumEstSumAiAj = CvInvoke.Sum(EstSumAiAj).V0;
                EstSumAn2.Dispose(); EstSumAiAj.Dispose();
                double EstNoiseEnergy2 = 2 * noisePower * sumEstSumAn2 + 4 * noisePower * sumEstSumAiAj;
                double tau = Math.Sqrt(EstNoiseEnergy2 / 2);                              // Rayleigh parameter
                double EstNoiseEnergy = tau * Math.Sqrt(Math.PI / 2);                     // Expected value of noise energy
                double EstNoiseEnergySigma = Math.Sqrt((4 - Math.PI / 2) * Math.Pow(tau, 2));

                double T = EstNoiseEnergy + k * EstNoiseEnergySigma;                    // Noise threshold
                // The estimated noise effect calculated above is only valid for the PC_1 measure.
                // The PC_2 measure does not lend itself readily to the same analysis.However
                // empirically it seems that the noise effect is overestimated roughly by a factor
                // of 1.7 for the filter parameters used here.
                T = T / 1.7;    // Empirical rescaling of the estimated noise effect to 
                                // suit the PC_2 phase congruency measure
                
                using (Matrix<double> energyTemp = Energy.Clone())
                    CvInvoke.Max(energyTemp - T, zeros, Energy);        // Apply noise threshold
                
                // Form weighting that penalizes frequency distributions that are
                // particularly narrow.Calculate fractional 'width' of the frequencies
                // present by taking the sum of the filter response amplitudes and dividing
                // by the maximum amplitude at each point on the image.

                Matrix<double> width = new Matrix<double>(rows, cols);
                CvInvoke.Divide(sumAn_ThisOrient, (maxAn + epsilon), width);
                width /= nScale;

                // Now calculate the sigmoidal weighting function for this orientation.
                Matrix<double> weight = Utillity.power(1 + Utillity.exp((cutOff - width) * g), -1);
                width.Dispose();
                // Apply weighting to energy and then calculate phase congruency
                Matrix<double> tempDivPC = new Matrix<double>(rows, cols);
                using (Matrix<double> tempMultPC = new Matrix<double>(rows, cols))
                {
                    CvInvoke.Multiply(weight, Energy, tempMultPC);
                    CvInvoke.Divide(tempMultPC, sumAn_ThisOrient, tempDivPC);
                    PC.Insert(o - 1, tempDivPC.Clone());                        // Phase congruency for this orientation
                }
                tempDivPC.Dispose();
                
                Matrix<double> featTemp = new Matrix<double>(rows, cols, 2);
                using (VectorOfMat mv = new VectorOfMat(new Mat[] { E.Mat, O.Mat }))
                    CvInvoke.Merge(mv, featTemp);
                featType.Insert(o - 1, featTemp.Clone());
                E.Dispose(); O.Dispose(); featTemp.Dispose();

                //Build up covariance data for every point
                Matrix<double> covx = PC[o - 1] * Math.Cos(angl);
                Matrix<double> covy = PC[o - 1] * Math.Sin(angl);
                using (Matrix<double> tempCovx2 = covx2.Clone())
                    CvInvoke.Add(tempCovx2, Utillity.power(covx, 2), covx2);
                using (Matrix<double> tempCovy2 = covy2.Clone())
                    CvInvoke.Add(tempCovy2, Utillity.power(covy, 2), covy2);
                Matrix<double> tempCovxyMul = new Matrix<double>(rows, cols);
                CvInvoke.Multiply(covx, covy, tempCovxyMul);
                using (Matrix<double> tempCovxy = covxy.Clone())
                    CvInvoke.Add(tempCovxy, tempCovxyMul, covxy);
            }   // For each orientation
                
            result.Add("PC", PC);
            result.Add("featType", featType);
            result.Add("EO", EO);

            // Edge and Corner calculations
            // The following is optimised code to calculate principal vector
            // of the phase congruency covariance data and to calculate
            // the minimumum and maximum moments -these correspond to
            // the singular values.

            // First normalise covariance values by the number of orientations/ 2
            covx2 = Utillity.div(covx2, (nOrient / 2));
            covy2 = Utillity.div(covy2, (nOrient / 2));
            covxy = Utillity.div(covxy, nOrient);
            Matrix<double> denom = new Matrix<double>(rows, cols);
            using (Matrix<double> tempSqrtDenom = new Matrix<double>(rows, cols))
            {
                using (Matrix<double> tempMinDenom = new Matrix<double>(rows, cols))
                {
                    CvInvoke.Subtract(covx2, covy2, tempMinDenom);
                    CvInvoke.Add(Utillity.power(covxy, 2), Utillity.power(tempMinDenom.Clone(), 2), tempSqrtDenom);
                    CvInvoke.Sqrt(tempSqrtDenom.Clone(), denom);
                    denom += epsilon;
                }
            }
            
            Matrix<double> sin2theta = new Matrix<double>(rows, cols);
            Matrix<double> cos2theta = new Matrix<double>(rows, cols);
           
            CvInvoke.Divide(covxy, denom, sin2theta);
            using (Matrix<double> tempMinCovx2Covy2 = new Matrix<double>(rows, cols))
            {
                CvInvoke.Subtract(covx2, covy2, tempMinCovx2Covy2);
                CvInvoke.Divide(tempMinCovx2Covy2, denom, cos2theta);
            }
            or = Utillity.div(Utillity.atan(sin2theta, cos2theta), 2);                   // Orientation perpendicular to edge.
            or = Utillity.round(or.Clone() * (180 / Math.PI));                             //Return result rounded to integer
                                                                                         // degrees.
            Matrix<double> neg = Utillity.neg(or.Clone());
            using (Matrix<double> tempAddOr = new Matrix<double>(rows, cols))
            {
                Matrix<double> tempMultNegOr = new Matrix<double>(rows, cols);
                Matrix<double> tempMultNegOr2 = new Matrix<double>(rows, cols);
                CvInvoke.Multiply(Utillity.negasi(neg), or, tempMultNegOr);
                CvInvoke.Multiply(neg, (or + 180), tempMultNegOr2);
                CvInvoke.Add(tempMultNegOr, tempMultNegOr2, or);        // Adjust range from -90 to 90
                                                                        // to 0 to 180.
            }
            Matrix<double> M = new Matrix<double>(rows, cols);
            Matrix<double> m = new Matrix<double>(rows, cols);
            using (Matrix<double> tempAddCovy2Covx2 = new Matrix<double>(rows, cols))
            {
                CvInvoke.Add(covy2, covx2, tempAddCovy2Covx2);
                CvInvoke.Add(tempAddCovy2Covx2, denom, M);
                M /= 2;                                                     // Maximum moment
                CvInvoke.Subtract(tempAddCovy2Covx2, denom, m);
                m /= 2;                                                     // Minimum moment
            }

            result.Add("or", or);
            result.Add("M", M);
            result.Add("m", m);
            result.Add("Energy", Energy);

            return result;
        }

        public Hashtable checkargs(params object[] list)
        {
            Hashtable result = new Hashtable();
            int nargs = list.Length;

            // Setup default value and then overwrite them with supplied value
            int nScale = 5;             // Number of wavelet scales.
            int nOrient = 8;            // Number of filter orientations.
            int minWaveLength = 3;      // Wavelength of smallest scale filter.
            double mult = 2.1D;         // Scaling factor between successive filters.
            double sigmaOnf = 0.55D;     /* Ratio of the standard deviation of the
                                           Gaussian describing the log Gabor filter's
                                           transfer function in the frequency domain
                                           to the filter center frequency.*/
            double dThetaOnSigma = 1.2D; /* Ratio of angular interval between filter orientations    
                                           and the standard deviation of the angular Gaussian
                                           function used to construct filters in the
                                           freq. plane. */
            double k = 2.0D;             /* No of standard deviations of the noise
                                           energy beyond the mean at which we set the
                                           noise threshold point. */
            double cutOff = 0.5D;        /* The fractional measure of frequency spread
                                           below which phase congruency values get penalized. */
            int g = 10;                 /* Controls the sharpness of the transition in
                                           the sigmoid function used to weight phase
                                           congruency for frequency spread. */

            // Allowed argument reading states
            int allnumeric = 1;     // Numeric argument values in predefined order

            int readState = allnumeric;

            if (readState == allnumeric)
            {
                for (int n = 0; n < nargs; n++)
                {
                    if (n == 0) nScale = (int)list[n];
                    else if (n == 1) nOrient = (int)list[n];
                    else if (n == 2) minWaveLength = (int)list[n];
                    else if (n == 3) mult = (double)list[n];
                    else if (n == 4) sigmaOnf = (double)list[n];
                    else if (n == 5) dThetaOnSigma = (double)list[n];
                    else if (n == 6) k = (double)list[n];
                    else if (n == 7) cutOff = (double)list[n];
                    else if (n == 8) g = (int)list[n];
                }
            }

            if (nScale < 1)
            {
                Console.WriteLine("nscale must be an integer >= 1");
                return result;
            }

            if (nOrient < 1)
            {
                Console.WriteLine("norient must be an integer >= 1");
                return result;
            }

            if (minWaveLength < 2)
            {
                Console.WriteLine("It makes little sense to have a wavelength < 2");
                return result;
            }

            if (cutOff < 0 || cutOff > 1)
            {
                Console.WriteLine("Cut off value must be between 0 and 1");
                return result;
            }


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
