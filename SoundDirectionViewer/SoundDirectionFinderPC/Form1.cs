using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SoundDirectionFinderPC.Properties;


namespace SoundDirectionFinderPC
{
    public partial class Form1 : Form
    {
        private List<MMDevice> _devices;
        private NAudioCapture _capture;

        public Form1()
        {
            InitializeComponent();

            sgraphWave.AddChannel("Left", Color.Red);
            sgraphWave.AddChannel("Right", Color.Green);
            sgraphSpectrum.AddChannel("Left", Color.Red);
            sgraphSpectrum.AddChannel("Right", Color.Green);
            sGraphShift.AddChannel("Сдвиг", Color.Red);



            _capture = new NAudioCapture(2048);
            _capture.AudioCaptured += _capture_AudioCaptured;

            UpdateAudioSources();
            SetDefaultSources();
        }


        #region INTERFACE

        // Обновить список устройств
        private void UpdateAudioSources()
        {
            _devices = NAudioCapture.GetDevices();

            foreach (var device in _devices)
            {
                cboxLeftDevice.Items.Add(device.FriendlyName);
                cboxRightDevice.Items.Add(device.FriendlyName);
            }
        }

        // Обновить список устройств
        private void btnUpdateDevices_Click(object sender, EventArgs e)
        {
            UpdateAudioSources();
        }

        // Выбрать устройства по-умолчанию
        private void SetDefaultSources()
        {
            if (cboxLeftDevice.Items.Count > 0)
                cboxLeftDevice.SelectedIndex = 0;

            if (cboxLeftDevice.Items.Count > 1)
                cboxRightDevice.SelectedIndex = 1;
        }

        // Начать/остановить запись
        private void btnRecord_Click(object sender, EventArgs e)
        {
            if (!_capture.IsRecording)
            {

                if (cboxLeftDevice.SelectedIndex == -1 || cboxRightDevice.SelectedIndex == -1)
                    return;

                var result = _capture.SelectDevices(_devices[cboxLeftDevice.SelectedIndex], _devices[cboxRightDevice.SelectedIndex]);
                if (result == SelectDeviceResult.WARNING_DIFFERENT_SAMPLERATE)
                    MessageBox.Show("Устройства имеют разную частоту дискретизации");

                _capture.Start();
                btnRecord.Image = Resources.icon_mic_on;
            }
            else
            {
                _capture.Stop();
                btnRecord.Image = Resources.icon_mic_off;
            }

        }

        #endregion



        private void _capture_AudioCaptured(object sender, AudioCapturedEventArgs e)
        {
            try
            {
                Invoke((MethodInvoker)(() => Capture(e)));
            }
            catch (ObjectDisposedException exception)
            {
            }
            
        }

        private void Capture(AudioCapturedEventArgs e)
        {
            
            DataUtils.Normalize(e.Left.Buffer);
            DataUtils.Normalize(e.Right.Buffer);

            var a = new Complex32[e.Left.Buffer.Length];
            var b = new Complex32[e.Right.Buffer.Length];
            var sp1 = new Complex32[a.Length];
            var sp2 = new Complex32[b.Length];
            var window = Window.Hamming(a.Length);


            // Составляем комплексный массив и умножаем на окно Хэмминга
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = new Complex32(e.Left.Buffer[i] * (float)window[i], 0);
                b[i] = new Complex32(e.Left.Buffer[i] * (float)window[i], 0);
            }

            // Преобразование фурье
            Fourier.Forward(a);
            Fourier.Forward(b);
            Array.Copy(a, sp1, a.Length);
            Array.Copy(b, sp2, b.Length);

            // Поэлементно умножаем первое на сопряженное ко второму
            for (int i = 0; i < a.Length; i++)
                a[i] = a[i] * b[i].Conjugate();


            // Поэлементно делим на модуль самого себя
            //for (int i = 0; i < a.Length; i++)
            //    a[i] = Complex32.Divide(a[i], a[i].Magnitude);

            //Fourier.Inverse(a);

            // Поиск максимума
            /*int maxI = 0;
            float max = a[0].Imaginary;
            for (int i = 0; i < a.Length; i++)
            {
                if (Math.Abs(a[i].Imaginary) > max)
                {
                    maxI = i;
                    max = Math.Abs(a[i].Imaginary);
                }
            }*/

            sgraphWave.Clear();
            sgraphSpectrum.Clear();

            for (int i = 0; i < e.Left.Buffer.Length; i++)
            {
                sgraphWave.AddData(i, e.Left.Buffer[i], e.Right.Buffer[i]);
            }

            for (int i = 0; i < a.Length / 10; i++)
            {
                float hz = i * e.Left.Source.WaveFormat.SampleRate / (float)a.Length;
                //sgraphSpectrum.AddData(hz, sp1[i].Magnitude, sp2[i].Magnitude);
                sgraphSpectrum.AddData(hz, 0, a[i].Imaginary);
            }


            sgraphWave.UpdateGraph();
            sgraphSpectrum.UpdateGraph();
        }

       
    }
}
