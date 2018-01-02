using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using SoundDirectionViiewer.Properties;
using ZedGraph;

namespace SoundDirectionViiewer
{
    public partial class Form1 : Form, IDisposable
    {
        const int DATA_LENGTH = 256;                                // Размер буфера
        private const float ADC_REF_V = 3.3f;                       // Опорное напряжение
        private const float SAMPLE_RATE = 72000000 / 1800.0f;      // Частота дискретизации
        private double D = 0.01;                                    // Расстояние между двумя микрофонами

        private SerialPort _com;
        private PackageBuilder _builder;

        private float[] data1, data2;

        public Form1()
        {
            InitializeComponent();

            data1 = new float[DATA_LENGTH];
            data2 = new float[DATA_LENGTH];

            _com = new SerialPort();
            _builder = new PackageBuilder(new byte[] { 0x32, 0xFA, 0x12 }, DATA_LENGTH * 2 * sizeof(UInt16));
            _com.DataReceived += _com_DataReceived;
            _builder.PackageReceived += _builder_PackageReceived;

            sgraphAdc.AddChannel("ADC1", Color.Red);
            sgraphAdc.AddChannel("ADC2", Color.Green);
            sgraphShift.AddChannel("Сдвиг", Color.Red);
            sgraphSpectrum.AddChannel("Spectrum1", Color.Red);
            sgraphSpectrum.AddChannel("Spectrum2", Color.Green);

            sgraphAdc.WindowSize = DATA_LENGTH;
            sgraphAdc.YMaxValue = 1.1;
            sgraphAdc.YMinValue = -0.1;
            sgraphAdc.XMinValue = 0;
            sgraphAdc.XMaxValue = DATA_LENGTH;

            UpdateComs();
        }


        private void btnRefreshComs_Click(object sender, EventArgs e)
        {
            UpdateComs();
        }

        private void btnOpenCom_Click(object sender, EventArgs e)
        {
            if (_com.IsOpen)
            {
                _com.Close();
                btnOpenCom.Image = new Bitmap(Properties.Resources.icon_disconnected);
                btnOpenCom.ToolTipText = "Подключиться";
            }
            else
            {
                if (cboxComs.SelectedText == null)
                    return;

                if (cboxComs.SelectedItem == null)
                    return;

                _com.PortName = cboxComs.SelectedText;
                _com.BaudRate = 57600;

                try
                {
                    _com.Open();
                    btnOpenCom.Image = new Bitmap(Resources.icon_connected);
                    btnOpenCom.ToolTipText = "Отключиться";
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Не удается открыть COM-порт", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void _com_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var buffer = new byte[_com.BytesToRead];
            _com.Read(buffer, 0, buffer.Length);
            _builder.ProcessPart(buffer);
        }

        private void _builder_PackageReceived(object sender, byte[] package)
        {
            try
            {
                Invoke((MethodInvoker)(() => packageReceived(package)));
            }
            catch (ObjectDisposedException) { }
        }


        private void packageReceived(byte[] package)
        {
            GetDataFromPackage(package, ref data1, ref data2);


            //DataUtils.MovingAverage(data1, 5);
            //DataUtils.MovingAverage(data2, 20);
            DataUtils.Normalize(data1);
            DataUtils.Normalize(data2);

            var a = new Complex32[data1.Length];
            var b = new Complex32[data1.Length];
            var sp1 = new Complex32[data1.Length];
            var sp2 = new Complex32[data1.Length];
            var window = Window.Hamming(data1.Length);

            // Составляем комплексный массив и умножаем на окно Хэмминга
            for (int i = 0; i < data1.Length; i++)
            {
                a[i] = new Complex32(data1[i] * (float)window[i], 0);
                b[i] = new Complex32(data2[i] * (float)window[i], 0);
            }

            // Преобразование фурье
            Fourier.Forward(a);
            Fourier.Forward(b);
            Array.Copy(a, sp1, sp1.Length);
            Array.Copy(b, sp2, sp2.Length);


            // Поэлементно умножаем первое на сопряженное ко второму
            for (int i = 0; i < a.Length; i++)
                a[i] = a[i] * b[i].Conjugate();

            // Поэлементно делим на модуль самого себя
            //for (int i = 0; i < a.Length; i++)
            //    a[i] = Complex32.Divide(a[i], a[i].Magnitude);

            // Обратное преобразование Фурье
            //Fourier.Inverse(a);

            int maxI = 0;
            float max = a[0].Imaginary;
            for (int i = 0; i < a.Length; i++)
            {
                if (Math.Abs(a[i].Imaginary) > max)
                {
                    maxI = i;
                    max = Math.Abs(a[i].Imaginary);
                }
            }

            sgraphAdc.Clear();
            sgraphSpectrum.Clear();
            sgraphShift.Clear();

            // Отображение спектра
            for (int i = 0; i < sp1.Length / 2; i++)
            {
                float hz = i * SAMPLE_RATE / data1.Length;
                // sgraphSpectrum.AddData(hz, spectrum1[i].Magnitude, spectrum2[i].Magnitude);
                sgraphSpectrum.AddData(hz, 0, a[i].Imaginary);
            }

            // Отображение сигнала
            for (int i = 0; i < data1.Length; i++)
            {
                sgraphAdc.AddData(i, data1[i], data2[i]);
            }

            sgraphShift.AddData(0, a[maxI].Imaginary);

            sgraphShift.UpdateGraph();
            sgraphSpectrum.UpdateGraph();
            sgraphAdc.UpdateGraph();
        }


        // Извлекает отдельные данные первого и второго канала из пакета
        private void GetDataFromPackage(byte[] buffer, ref float[] data1, ref float[] data2)
        {
            for (int i = 0; i < DATA_LENGTH - 1; i++)
            {
                data1[i] = BitConverter.ToUInt16(buffer, i * 4);
                data2[i] = BitConverter.ToUInt16(buffer, i * 4 + 2);
            }
        }

        private void UpdateComs()
        {
            cboxComs.Items.Clear();

            var names = SerialPort.GetPortNames();
            foreach (var name in names)
                cboxComs.Items.Add(name);
        }


    }

    struct CorrelationResult
    {
        public int MaxShift { get; set; }
        public double MaxCorrelation { get; set; }
    }
}
