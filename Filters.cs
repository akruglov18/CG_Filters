using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.ComponentModel;
using System.Windows;

namespace Task_1
{
    abstract class Filters
    {
        public virtual Bitmap processImage(Bitmap image, BackgroundWorker worker)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);
            for (int i = 0; i < result.Width; i++)
            {
                worker.ReportProgress((int)((float)i / image.Width * 100));
                if (worker.CancellationPending) return null;
                for (int j = 0; j < result.Height; j++)
                {
                    result.SetPixel(i, j, calculateNewPixelColor(image, i, j));
                }
            }
            return result;
        }

        protected abstract Color calculateNewPixelColor(Bitmap image, int i, int j);
        public int Clamb(int val, int min, int max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }
    }
    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap image, int x, int y)
        {
            Color source = image.GetPixel(x, y);
            Color result = Color.FromArgb(255 - source.R, 255 - source.G, 255 - source.B);
            return result;
        }
    }
    class IntensityFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap image, int x, int y)
        {
            Color source = image.GetPixel(x, y);
            int k = 25;
            Color result = Color.FromArgb(Clamb(source.R+k,0,255), Clamb(source.G + k, 0, 255), Clamb(source.B + k, 0, 255));
            return result;
        }
    }
    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap image, int x, int y)
        {
            int radX = kernel.GetLength(0) / 2;
            int radY = kernel.GetLength(1) / 2;
            float resR = 0, resG = 0, resB = 0;
            for (int l = -radY; l <= radY; l++)
            {
                for (int k = -radX; k <= radX; k++)
                {
                    int idX = Clamb(x + k, 0, image.Width - 1);
                    int idY = Clamb(y + l, 0, image.Height - 1);
                    Color neighborcolor = image.GetPixel(idX, idY);
                    resR += neighborcolor.R * kernel[k + radX, l + radY];
                    resG += neighborcolor.G * kernel[k + radX, l + radY];
                    resB += neighborcolor.B * kernel[k + radX, l + radY];
                }
            }
            return Color.FromArgb(Clamb((int)resR, 0, 255),
                                  Clamb((int)resG, 0, 255),
                                  Clamb((int)resB, 0, 255));
        }
    }
    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3, sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
        }
    }
    class GaussianFilter : MatrixFilter
    {
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }

        public void createGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (2 * sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
    }
    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap image, int x, int y)
        {
            int R = (int)(0.299 * image.GetPixel(x, y).R);
            int G = (int)(0.587 * image.GetPixel(x, y).G);
            int B = (int)(0.114 * image.GetPixel(x, y).B);
            int result = Clamb(R + G + B, 0, 255);
            return Color.FromArgb(result, result, result);
        }
    }
    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap image, int x, int y)
        {
            int R = (int)(0.299 * image.GetPixel(x, y).R);
            int G = (int)(0.587 * image.GetPixel(x, y).G);
            int B = (int)(0.114 * image.GetPixel(x, y).B);
            int intensity = Clamb(R + G + B, 0, 255);
            int k = 35;
            return Color.FromArgb(Clamb(intensity + 2 * k, 0, 255), Clamb((int)(intensity + 0.5 * k), 0, 255), Clamb(intensity - k, 0, 255));
        }
    }
    class AcutanceFilter : MatrixFilter
    {
        public AcutanceFilter()
        {
            kernel = new float[,] { { 0, -1, 0 }, { -1, 5, -1 }, { 0, -1, 0 } };
        }
    }
    class SobelFilter : MatrixFilter
    {
        float[,] kernely = null;
        float[,] kernelx = null;
        public SobelFilter()
        {
            kernelx = new float[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            kernel = kernelx;
            kernely = new float[,] { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };
        }
        public override Bitmap processImage(Bitmap image, BackgroundWorker worker)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);
            for (int i = 0; i < result.Width; i++)
            {
                worker.ReportProgress((int)((float)i / (image.Width * 2) * 100));
                if (worker.CancellationPending) return null;
                for (int j = 0; j < result.Height; j++)
                {
                    result.SetPixel(i, j, calculateNewPixelColor(image, i, j));
                }
            }
            kernel = kernely;
            for (int i = 0; i < result.Width; i++)
            {
                worker.ReportProgress((int)((float)(i + result.Width) / (image.Width * 2) * 100));
                if (worker.CancellationPending) return null;
                for (int j = 0; j < result.Height; j++)
                {
                    int r1, g1, b1;
                    r1 = result.GetPixel(i, j).R;
                    g1 = result.GetPixel(i, j).G;
                    b1 = result.GetPixel(i, j).B;
                    Color res2 = calculateNewPixelColor(image, i, j);
                    int r2, g2, b2;
                    r2 = res2.R;
                    g2 = res2.G;
                    b2 = res2.B;
                    r1 *= r1; r2 *= r2;
                    g1 *= g1;
                    g2 *= g2;
                    b1 *= b1;
                    b2 *= b2;
                    int resr = (int)Math.Sqrt(r1 + r2), resg = (int)Math.Sqrt(g1 + g2),
                        resb = (int)Math.Sqrt(b1 + b2);
                    result.SetPixel(i, j, Color.FromArgb(Clamb((int)resr, 0, 255),
                                  Clamb((int)resg, 0, 255),
                                  Clamb((int)resb, 0, 255)));
                }
            }
            return result;
        }

    }

    class GlassFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap image, int k, int l)
        {
            Random rand = new Random();
            int x = (int)(k + (rand.Next(2) - 0.5) * 10);
            int y = (int)(l + (rand.Next(2) - 0.5) * 10);
            x = Clamb(x, 0, image.Width - 1);
            y = Clamb(y, 0, image.Height - 1);
            return image.GetPixel(x, y);
        }
    }
    class WavesFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap image, int k, int l)
        {
            int x = k + (int)(20 * Math.Sin(2 * Math.PI * l / 60));
            int y = l;
            x = Clamb(x, 0, image.Width - 1);
            y = Clamb(y, 0, image.Height - 1);
            return image.GetPixel(x, y);
        }
    }
    class GreyWorldFilter : Filters
    {
        double Avg;
        double R, G, B;
        public GreyWorldFilter() { Avg = -1; }
        protected void findMidColor(Bitmap image)
        {
            long sumR = 0;
            long sumG = 0;
            long sumB = 0;
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    sumR += image.GetPixel(i, j).R;
                    sumG += image.GetPixel(i, j).G;
                    sumB += image.GetPixel(i, j).B;
                }
            }
            R = (sumR / (image.Width * image.Height));
            G = (sumG / (image.Width * image.Height));
            B = (sumB / (image.Width * image.Height));
            Avg = (R + G + B) / 3;
        }
        protected override Color calculateNewPixelColor(Bitmap image, int i, int j)
        {
            if (Avg < 0) findMidColor(image);
            int resR = (int)(image.GetPixel(i, j).R * (Avg / R));
            int resG = (int)(image.GetPixel(i, j).G * (Avg / G));
            int resB = (int)(image.GetPixel(i, j).B * (Avg / B));
            return Color.FromArgb(Clamb(resR, 0, 255),
                 Clamb(resG, 0, 255),
                 Clamb(resB, 0, 255));
        }
    }
    class LinearHistogramFilter : Filters
    {
        Color min = Color.FromArgb(255, 255, 255), max = Color.FromArgb(0, 0, 0);
        bool calculated = false;

        protected Color CalculateMax(Color res, Color cur)
        {
            int R = Math.Max(res.R, cur.R);
            int G = Math.Max(res.G, cur.G);
            int B = Math.Max(res.B, cur.B);
            return Color.FromArgb(Clamb(R, 0, 255), Clamb(G, 0, 255), Clamb(B, 0, 255));
        }

        protected Color CalculateMin(Color res, Color cur)
        {
            int R = Math.Min(res.R, cur.R);
            int G = Math.Min(res.G, cur.G);
            int B = Math.Min(res.B, cur.B);
            return Color.FromArgb(Clamb(R, 0, 255), Clamb(G, 0, 255), Clamb(B, 0, 255));
        }
        protected void findColor(Bitmap image)
        {
            calculated = true;
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    min = CalculateMin(min, image.GetPixel(i, j));
                    max = CalculateMax(max, image.GetPixel(i, j));
                }
            }
        }
        protected override Color calculateNewPixelColor(Bitmap image, int i, int j)
        {
            if (!calculated) findColor(image);
            double R = image.GetPixel(i, j).R;
            double B = image.GetPixel(i, j).B;
            double G = image.GetPixel(i, j).G;
            R = (R - min.R) * 255 / (max.R - min.R);
            G = (G - min.G) * 255 / (max.G - min.G);
            B = (B - min.B) * 255 / (max.B - min.B);
            return Color.FromArgb(Clamb((int)R, 0, 255), Clamb((int)G, 0, 255), Clamb((int)B, 0, 255));

        }
    }
    class MedianFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap image, int i, int j)
        {
            List<double> r = new List<double>(), g = new List<double>(), b = new List<double>();
            for (int k = -2; k <= 2; k++)
            {
                for (int c = -2; c <= 2; c++)
                {
                    if (j + c >= 0 && j + c < image.Height && i + k >= 0 && i + k < image.Width)
                    {
                        r.Add(image.GetPixel(i + k, j + c).R);
                        g.Add(image.GetPixel(i + k, j + c).G);
                        b.Add(image.GetPixel(i + k, j + c).B);
                    }
                }
            }
            r.Sort(); g.Sort(); b.Sort();
            double[] rr = r.ToArray<double>(), gr = g.ToArray<double>(), br = b.ToArray<double>();

            int R = (int)(rr[rr.Count() / 2]);
            int G = (int)(gr[gr.Count() / 2]);
            int B = (int)(br[br.Count() / 2]);
            return Color.FromArgb(Clamb(R, 0, 255), Clamb(G, 0, 255), Clamb(B, 0, 255));
        }
    }

    abstract class MathMorfology : Filters
    {
        protected int[,] mask = null;
        protected Color standartColor;
        protected int ProcessedCols = 0, allCols = -1;

        protected MathMorfology() { }

        public void setMask(int[,] mask)
        {
            this.mask = mask;
        }

        public void setProcessedCols(int _cols)
        {
            ProcessedCols = _cols;
        }
        public void setAllCols(int _cols)
        {
            allCols = _cols;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            throw new Exception();
        }
        public override Bitmap processImage(Bitmap image, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(image.Width, image.Height);
            if (allCols == -1)
                allCols = image.Width;
            int radX = mask.GetLength(0) / 2;
            int radY = mask.GetLength(1) / 2;
            for (int x = 0; x < image.Width; x++)
            {
                worker.ReportProgress(
                    (int)((float)(x + ProcessedCols) / (allCols) * 100));

                if (worker.CancellationPending)
                    return null;

                for (int y = 0; y < image.Height; y++)
                {
                    Color res = standartColor;
                    for (int j = -radY; j <= radY; j++)
                        for (int i = -radX; i <= radX; i++)
                        {
                            if (x + i < image.Width && y + j < image.Height && x + i >= 0 && y + j >= 0)
                            {
                                Color cur = image.GetPixel(x + i, y + j);
                                res = Calculate(mask[i + radX, j + radY], res, cur);
                            }
                        }
                    resultImage.SetPixel(x, y, res);
                }
            }

            return resultImage;
        }

        protected virtual Color Calculate(int mask, Color res, Color cur)
        {
            throw new Exception();
        }

    }
    class DilationFilter : MathMorfology
    {
        public DilationFilter()
        {
            this.mask = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            standartColor = Color.FromArgb(0, 0, 0);
        }

        public DilationFilter(int[,] _mask)
        {
            this.mask = _mask;
            standartColor = Color.FromArgb(0, 0, 0);
        }

        protected override Color Calculate(int mask, Color res, Color cur)
        {
            if (mask == 1)
            {
                int R = Math.Max(res.R, cur.R);
                int G = Math.Max(res.G, cur.G);
                int B = Math.Max(res.B, cur.B);
                return Color.FromArgb(Clamb(R, 0, 255), Clamb(G, 0, 255), Clamb(B, 0, 255));
            }
            else
            {
                return res;
            }
        }
    }

    class ErosionFilter : MathMorfology
    {
        public ErosionFilter()
        {
            this.mask = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
            standartColor = Color.FromArgb(255, 255, 255);
        }

        public ErosionFilter(int[,] _mask)
        {
            this.mask = _mask;
            standartColor = Color.FromArgb(255, 255, 255);
        }

        protected override Color Calculate(int mask, Color res, Color cur)
        {
            if (mask == 1)
            {
                int R = Math.Min(res.R, cur.R);
                int G = Math.Min(res.G, cur.G);
                int B = Math.Min(res.B, cur.B);
                return Color.FromArgb(Clamb(R, 0, 255), Clamb(G, 0, 255), Clamb(B, 0, 255));
            }
            else
            {
                return res;
            }
        }
    }
    class OpeningFilter : MathMorfology
    {
        public OpeningFilter()
        {
            this.mask = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }
        public OpeningFilter(int[,] _mask)
        {
            this.mask = _mask;
        }
        public override Bitmap processImage(Bitmap image, BackgroundWorker worker)
        {
            MathMorfology er = new ErosionFilter(mask), dil = new DilationFilter(mask);
            er.setAllCols(2 * image.Width);
            dil.setAllCols(2 * image.Width);
            dil.setProcessedCols(image.Width);
            Bitmap res = er.processImage(image, worker);
            res = dil.processImage(res, worker);
            return res;
        }
    }

    class ClosingFilter : MathMorfology
    {
        public ClosingFilter()
        {
            this.mask = new int[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };
        }
        public ClosingFilter(int[,] _mask)
        {
            this.mask = _mask;
        }
        public override Bitmap processImage(Bitmap image, BackgroundWorker worker)
        {
            MathMorfology er = new ErosionFilter(mask), dil = new DilationFilter(mask);
            dil.setAllCols(2 * image.Width);
            er.setAllCols(2 * image.Width);
            er.setProcessedCols(image.Width);
            Bitmap res = dil.processImage(image, worker);
            res = er.processImage(res, worker);
            return res;
        }
    }
    class GradFilter : MathMorfology
    {
        public GradFilter()
        {
            this.mask = new int[,] { { 0, 1, 0 }, { 1, 1, 1 }, { 0, 1, 0 } };
        }
        public GradFilter(int[,] _mask)
        {
            this.mask = _mask;
        }
        public override Bitmap processImage(Bitmap image, BackgroundWorker worker)
        {
            MathMorfology er = new ErosionFilter(mask), dil = new DilationFilter(mask);
            er.setAllCols(3 * image.Width);
            Bitmap res = er.processImage(image, worker);
            dil.setAllCols(3 * image.Width);
            dil.setProcessedCols(image.Width);
            Bitmap res1 = dil.processImage(image, worker);
            ProcessedCols = 2 * image.Width;
            allCols = 3 * image.Width;
            for (int i = 0; i < res.Width; i++)
            {
                worker.ReportProgress((int)((float)(i+ProcessedCols) * 100/allCols));
                if (worker.CancellationPending) return null;
                for (int j = 0; j < res.Height; j++)
                {
                    int R = res1.GetPixel(i, j).R - res.GetPixel(i, j).R;
                    int G = res1.GetPixel(i, j).G - res.GetPixel(i, j).G;
                    int B = res1.GetPixel(i, j).B - res.GetPixel(i, j).B;
                    res.SetPixel(i, j, Color.FromArgb(Clamb(R, 0, 255), Clamb(G, 0, 255), Clamb(B, 0, 255)));
                }
            }
            return res;
        }
    }

}


