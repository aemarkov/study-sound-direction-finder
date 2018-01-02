using System;
using System.Linq;

namespace SoundDirectionViiewer
{
    public static class TimeAnalysis
    {

        // Поиск сдвига, при котором взаимная корреляция максимальна   
        public static CorrelationResult FindMaxShift(float[] a, float[] b)
        {
            int maxShift = 0;
            float maxCorrelation = 0;

            for (int shift = 0; shift < a.Length / 4; shift++)
            {
                float c = CalcCorrelation(a, b, shift);
                if (c > maxCorrelation)
                {
                    maxCorrelation = c;
                    maxShift = shift;
                }

                c = CalcCorrelation(a, b, -shift);
                if (c > maxCorrelation)
                {
                    maxCorrelation = c;
                    maxShift = -shift;
                }
            }

            return new CorrelationResult() { MaxShift = maxShift, MaxCorrelation = maxCorrelation };
        }

        // Находит корреляцию двух сигналов
        public static float CalcCorrelation(float[] a, float[] b, int shift)
        {
            float correlation = 0;
            int length = a.Length - Math.Abs(shift);

            for (int i = 0; i < length; i++)
            {
                if (shift > 0)
                    correlation += a[i + shift] * b[i];
                else
                    correlation += a[i] * b[i - shift];
            }

            return correlation / length;
        }

        public struct CorrelationResult
        {
            public int MaxShift { get; set; }
            public float MaxCorrelation { get; set; }
        }
    }
}