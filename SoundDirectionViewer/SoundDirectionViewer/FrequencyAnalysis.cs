using System;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;


namespace SoundDirectionViiewer
{
    public static class FrequencyAnalysis
    {
        /// <summary>
        /// Поиск задержки (Time delay of arrival, TDOA) с использованием метода
        /// Generalized Cross Correlation with Phase Transform
        /// </summary>
        /// <param name="aR"></param>
        /// <param name="bR"></param>
        /// <returns></returns>
        public static int GccPhat(float[] aR, float[] bR, out Complex32[] spectrum1, out Complex32[] spectrum2, out Complex32[] gcc)
        {
            //Описание метода: http://www.xavieranguera.com/phdthesis/node92.html

            var a = new Complex32[aR.Length];
            var b = new Complex32[aR.Length];
            var window = Window.Hamming(a.Length);

            for (int i = 0; i < bR.Length; i++)
            {
                a[i] = new Complex32(aR[i] * (float)window[i], 0);
                b[i] = new Complex32(bR[i] * (float)window[i], 0);
            }

            // Преобразование Фурье
            Fourier.Forward(a);
            Fourier.Forward(b);

            // Поэлементно умножаем первое на сопряженное ко второму
            for (int i = 0; i < aR.Length; i++)
                a[i] = a[i] * b[i].Conjugate();            

            // Поэлементно делим на модуль самого себя
            //for (int i = 0; i < aR.Length; i++)            
            //    a[i] /= a[i].Magnitude;

            // Обратное преобразование Фурье
            /*Fourier.Inverse(a);

            // Поиск максимума
            float max = a[0].Real;
            int maxI = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Real > max)
                {
                    max = a[i].Magnitude;
                    maxI = i;
                }
            }*/


            spectrum1 = new Complex32[aR.Length];
            Array.Copy(a, spectrum1, a.Length);
            spectrum2 = b;
            gcc = a;

            //return maxI;
            return 0;
        }
    }
}