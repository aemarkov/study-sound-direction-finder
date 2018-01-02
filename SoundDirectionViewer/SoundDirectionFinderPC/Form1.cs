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
            sgraphWave.Clear();
            //DataUtils.Normalize(e.Left.Buffer);
            //DataUtils.Normalize(e.Right.Buffer);

            for (int i = 0; i < e.Left.Buffer.Length; i++)
            {
                sgraphWave.AddData(i, 0, e.Right.Buffer[i]);
            }


            sgraphWave.UpdateGraph();
        }

       
    }
}
