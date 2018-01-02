using System.Linq;

namespace Common
{
    public static class DataUtils
    {
        // Нормализует ряд данных
        public static void Normalize(float[] data)
        {
            float max = data.Max();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] /= max;
            }
        }

        // Скользяшее среднее
        public static void MovingAverage(float[] dataIn, float[] dataOut, int size)
        {
            float sum = 0;

            for (int i = 0; i < dataIn.Length; i++)
            {
                sum += dataIn[i];
                if (i > size)
                    sum -= dataIn[i - size];

                if (i > size / 2)
                    dataOut[i - size / 2] = sum / size;
            }
        }

        // Скользяшее среднее
        public static void MovingAverage(float[] data, int size)
        {
            float sum = 0;

            for (int i = 0; i < data.Length; i++)
            {
                sum += data[i];
                if (i > size)
                    sum -= data[i - size];

                if (i > size / 2)
                    data[i - size / 2] = sum / size;
            }
        }
    }
}