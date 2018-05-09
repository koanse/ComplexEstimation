using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace WindowsFormsApplication1
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }


    }

    static class Estimation
    {
        static bool ReadFile(string fname)
        {
            StreamReader sr = new StreamReader(fname);
            double[,] matrVal;
            string[] names;
            List<string> lines = new List<string>();
            while (!sr.EndOfStream)
            {
                lines.Add(sr.ReadLine());
            }
            int colCount = lines[0].Split(';').Length;
            matrVal = new double[lines.Count, colCount];

            return false;
        }
    }

    public class Sample : ICloneable
    {
        protected string name = "", mark = "";
        protected double[] arrX;
        int indexMin, indexMax;
        double min, max, average, averageXPow2;
        double sum, sumXPow2, dev, devStd, sigma, sigmaStd;
        double[] arrHistY, arrHistX;    // частоты и середины интервалов
        double var;                  // коэф. вариации
        double mc1, mc2, mc3, mc4;   // центр. моменты
        double mb1, mb2, mb3, mb4;   // нач. моменты
        double asym, exc;            // асим. и эксцесс
        double asymTheor, excTheor, sigmaAsym, sigmaExc;

        public Sample() { }
        public Sample(string name, string mark, double[] arrX)
        {
            this.name = name;
            this.mark = mark;
            this.arrX = arrX;
            Calculate();
        }
        public Sample(double[] arrX)
        {
            this.arrX = arrX;
            Calculate();
        }
        public double this[int index]
        {
            get { return arrX[index]; }
            set { arrX[index] = value; }
        }
        public void Calculate()
        {
            min = double.MaxValue;
            max = double.MinValue;
            sum = 0;
            sumXPow2 = 0;
            for (int i = 0; i < arrX.Length; i++)
            {
                if (arrX[i] < min)
                {
                    min = arrX[i];
                    indexMin = i;
                }
                if (arrX[i] > max)
                {
                    max = arrX[i];
                    indexMax = i;
                }
                sum += arrX[i];
                sumXPow2 += arrX[i] * arrX[i];
            }
            average = sum / arrX.Length;
            averageXPow2 = sumXPow2 / arrX.Length;
            dev = averageXPow2 - (average * average);
            devStd = dev / arrX.Length * (arrX.Length - 1);
            sigma = (double)Math.Sqrt(dev);
            sigmaStd = (double)Math.Sqrt(devStd);

            // моменты
            mb1 = average;
            mb2 = sumXPow2 / arrX.Length;
            mc2 = dev;
            mc1 = mc3 = mc4 = 0;
            mb1 = mb2 = mb3 = mb4 = 0;
            for (int i = 0; i < arrX.Length; i++)
            {
                mb3 += arrX[i] * arrX[i] * arrX[i];
                mb4 += arrX[i] * arrX[i] * arrX[i] * arrX[i];
                mc1 += arrX[i] - mb1;
                mc3 += (arrX[i] - mb1) * (arrX[i] - mb1) * (arrX[i] - mb1);
                mc4 += (arrX[i] - mb1) * (arrX[i] - mb1) * (arrX[i] - mb1) * (arrX[i] - mb1);
            }
            mb3 /= arrX.Length;
            mb4 /= arrX.Length;
            mc1 /= arrX.Length;
            mc3 /= arrX.Length;
            mc4 /= arrX.Length;

            // асим. и эксцесс
            var = sigmaStd / average;
            asym = mc3 / (sigmaStd * sigmaStd * sigmaStd);
            exc = mc4 / (sigmaStd * sigmaStd * sigmaStd * sigmaStd) - 3;

            double n = arrX.Length;
            double g1 = mc3 / (double)Math.Pow(mc2, 1.5);
            asymTheor = (double)Math.Sqrt(n * (n - 1)) / (n - 2) * g1;
            sigmaAsym = (double)Math.Sqrt((6 * n * (n - 1)) /
                ((n - 2) * (n + 1) * (n + 3)));
            double g2 = mc4 / (mc2 * mc2) - 3;
            excTheor = (n - 1) / ((n - 2) * (n - 3)) * ((n + 1) * g2 + 6);
            sigmaExc = (double)Math.Sqrt(24 * n * (n - 2) * (n - 3) /
                ((n + 1) * (n + 1) * (n + 3) * (n + 5)));
        }
        public double[] CloneArray()
        {
            return (double[])arrX.Clone();
        }
        public double[] CloneHistY()
        {
            return (double[])arrHistY.Clone();
        }
        public double[] CloneHistX()
        {
            return (double[])arrHistX.Clone();
        }
        public int[] DropoutErrorsTau(double alpha)
        {
            ArrayList listX = new ArrayList(arrX);
            ArrayList listIndexIgnore = new ArrayList();
            while (true)
            {
                double min = double.MaxValue, max = double.MinValue;
                double sum = 0, sumXPow2 = 0;
                int indexMin = -1, indexMax = -1;
                for (int i = 0; i < listX.Count; i++)
                {
                    if (listIndexIgnore.BinarySearch(i) >= 0)
                        continue;
                    double x = (double)listX[i];
                    if (x < min)
                    {
                        min = x;
                        indexMin = i;
                    }
                    if (x > max)
                    {
                        max = x;
                        indexMax = i;
                    }
                    sum += x;
                    sumXPow2 += x * x;
                }
                int n = listX.Count - listIndexIgnore.Count;
                double average = sum / n;
                double devStd = sumXPow2 / (n - 1) - (sum / (n - 1)) * (sum / (n - 1));
                double sigmaStd = (double)Math.Sqrt(devStd);
                double tauXMin, tauXMax;
                tauXMin = (double)Math.Abs(min - average) / sigmaStd;
                tauXMax = (double)Math.Abs(max - average) / sigmaStd;
                int index;
                double tauMax;
                if (tauXMin >= tauXMax)
                {
                    index = indexMin;
                    tauMax = tauXMin;
                }
                else
                {
                    index = indexMax;
                    tauMax = tauXMax;
                }
                //if (tauMax > StatTables.GetTau(alpha, n))
                //{
                //    listIndexIgnore.Add(index);
                //    listIndexIgnore.Sort();
                //}
                //else
                //    break;
            }
            return (int[])listIndexIgnore.ToArray(typeof(int));
        }
        public int[] DropoutErrorsStud(double alphaMin, double alphaMax)
        {
            if (alphaMin == -1)
                alphaMin = 0.001f;
            if (alphaMax == -1)
                alphaMax = 0.05f;
            ArrayList listX = new ArrayList(arrX);
            ArrayList listIndexIgnore = new ArrayList();
            while (true)
            {
                double min = double.MaxValue, max = double.MinValue;
                double sum = 0, sumXPow2 = 0;
                int indexMin = -1, indexMax = -1;
                for (int i = 0; i < listX.Count; i++)
                {
                    if (listIndexIgnore.BinarySearch(i) >= 0)
                        continue;
                    double x = (double)listX[i];
                    if (x < min)
                    {
                        min = x;
                        indexMin = i;
                    }
                    if (x > max)
                    {
                        max = x;
                        indexMax = i;
                    }
                    sum += x;
                    sumXPow2 += x * x;
                }
                int n = listX.Count - listIndexIgnore.Count;
                double average = sum / n;
                double devStd = sumXPow2 / (n - 1) - (sum / (n - 1)) * (sum / (n - 1));
                double sigmaStd = (double)Math.Sqrt(devStd);
                double tauXMin, tauXMax;
                tauXMin = (double)Math.Abs(min - average) / sigmaStd;
                tauXMax = (double)Math.Abs(max - average) / sigmaStd;
                int index;
                double tauMax;
                if (tauXMin >= tauXMax)
                {
                    index = indexMin;
                    tauMax = tauXMin;
                }
                else
                {
                    index = indexMax;
                    tauMax = tauXMax;
                }

                //double tMin = StatTables.GetStudDistrInv(listX.Count - 2, alphaMin);
                //double tMax = StatTables.GetStudDistrInv(listX.Count - 2, alphaMax);
                //double critMin = tMin * (double)Math.Sqrt(listX.Count - 1) /
                //    (double)Math.Sqrt(listX.Count - 2 + tMin * tMin);
                //double critMax = tMax * (double)Math.Sqrt(listX.Count - 1) /
                //    (double)Math.Sqrt(listX.Count - 2 + tMin * tMin);
                //if (tauMax > critMin)
                //{
                //    listIndexIgnore.Add(index);
                //    listIndexIgnore.Sort();
                //}
                //else
                //    break;
            }
            return (int[])listIndexIgnore.ToArray(typeof(int));
        }
        public void DoHistogram(bool useSturgess)
        {
            int k;
            if (useSturgess)
                k = 1 + (int)(3.32 * Math.Log10(arrX.Length));
            else
                k = (int)Math.Sqrt(arrX.Length);
            if (k < 6 && arrX.Length >= 6)
                k = 6;
            else if (k > 20)
                k = 20;

            arrHistY = new double[k];
            double h = (max - min) / k;
            for (int i = 0; i < arrX.Length; i++)
            {
                int j;
                for (j = 0; j < arrHistY.Length - 1; j++)
                    if (arrX[i] < min + h * (j + 1))
                        break;
                arrHistY[j]++;
            }

            arrHistX = new double[k];
            for (int i = 0; i < arrHistX.Length; i++)
                arrHistX[i] = min + h * (i + 0.5);
        }
        public void AddValue(double value)
        {
            double[] arrXNew = new double[arrX.Length + 1];
            arrX.CopyTo(arrXNew, 0);
            arrXNew[arrX.Length] = value;
            arrX = arrXNew;
        }
        public void RemoveValues(int[] arrIndex)
        {
            double[] arrXNew = new double[arrX.Length - arrIndex.Length];
            ArrayList listIndex = new ArrayList(arrIndex);
            listIndex.Sort();
            for (int i = 0, j = 0; i < arrX.Length; i++)
            {
                if (listIndex.BinarySearch(i) >= 0)
                    continue;
                arrXNew[j++] = arrX[i];
            }
            arrX = arrXNew;
        }
        public string GetName()
        {
            return name;
        }
        public virtual string GetMark()
        {
            return mark;
        }
        public double GetAverage()
        {
            return average;
        }
        public double GetSigma()
        {
            return sigma;
        }
        public double GetDevStd()
        {
            return devStd;
        }
        public int GetLength()
        {
            return arrX.Length;
        }
        public string GetReport()
        {
            int d = 3;
            string s = string.Format("<P>ВЫБОРОЧНЫЕ ХАРАКТЕРИСТИКИ {0} [={1}]<BR>" +
                "Минимум: {1}<SUB>min</SUB> = {2}<BR>" +
                "Максимум: {1}<SUB>max</SUB> = {3}<BR>" +
                "Размах выборки: w = {4}<BR>" +
                "Среднее: {1}<SUB>ср</SUB> = {5}<BR>" +
                "Средний квадрат: {1}<SUP>2</SUP><SUB>ср</SUB> = {6}<BR>" +
                "Дисперсия: s<SUP>2</SUP> = {7}<BR>" +
                "Среднее квадр. откл.: s = {8}<BR>" +
                "Испр. дисперсия: s<SUP>2</SUP><SUB>испр</SUB> = {9}<BR>" +
                "Испр. среднее квадр. откл.: s<SUB>испр</SUB> = {10}<BR>" +
                "Асимметрия: A = {11}<BR>" +
                "Эксцесс: E = {12}<BR>" +
                "Коэффициент вариации: v = {13}<BR></P>",
                name, mark, Math.Round(min, d), Math.Round(max, d),
                Math.Round(min - max, d), Math.Round(average, d),
                Math.Round(averageXPow2, d), Math.Round(dev, d), Math.Round(sigma, d),
                Math.Round(devStd, d), Math.Round(sigmaStd, d), Math.Round(asym, d),
                Math.Round(exc, d), Math.Round(var, d));
            return s;
        }
        public string GetHypReport()
        {
            int d = 3;
            string asymRepr = "|A| < 3s<SUB>A</SUB> => " +
                "гипотеза о нормальности не отвергается";
            string excRepr = "|E| < 5s<SUB>E</SUB> => " +
                "гипотеза о нормальности не отвергается";
            string varRepr = "v < 0.33 => гипотеза о нормальности не отвергается";
            if (Math.Abs(asym) >= 3 * sigmaAsym)
                asymRepr = "|A| ≥ 3s<SUB>A</SUB> => " +
                    "гипотеза о нормальности отвергается";
            if (Math.Abs(exc) >= 5 * sigmaExc)
                excRepr = "|E| ≥ 5s<SUB>E</SUB> => " +
                    "гипотеза о нормальности отвергается";
            if (var >= 0.33)
                varRepr = "v ≥ 0.33 => гипотеза о нормальности отвергается";

            string s = string.Format("<P>ПРИЗНАК: {0} [={1}]<BR>", name, mark);
            s += string.Format("<P>Показатель асимметрии:<BR>" +
            "наблюдаемый: A = {0}<BR>" +
            "ошибка: s<SUB>A</SUB> = {1}<BR>{2}</P>",
            Math.Round(asym, 3), Math.Round(sigmaAsym, d), asymRepr);
            s += string.Format("<P>Показатель эксцесса:<BR>" +
                "наблюдаемый: E = {0}<BR>" +
                "ошибка: s<SUB>E</SUB> = {1}<BR>{2}</P>",
                Math.Round(exc, d), Math.Round(sigmaExc, d), excRepr);
            s += string.Format("<P>Коэффициент вариации:<BR>" +
                "наблюдаемый: v = {0}<BR>{1}</P>",
                Math.Round(var, d), varRepr);
            return s;
        }
        public void SetName(string name)
        {
            this.name = name;
        }
        public object Clone()
        {
            return new Sample(name, mark, arrX);
        }
    }
}
