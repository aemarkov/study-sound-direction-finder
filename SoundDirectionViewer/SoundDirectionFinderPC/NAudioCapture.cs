using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SoundDirectionFinderPC
{
    /// <summary>
    /// Захват звука с двух системных источников
    /// при помощи NAudio
    /// </summary>
    public class NAudioCapture  : IDisposable
    {
        private bool _isRecording;      
        private Channel _left, _right;
        private object _lock = new object();

        public bool IsRecording => _isRecording;
        public event EventHandler<AudioCapturedEventArgs> AudioCaptured;
        

        public NAudioCapture(int bufferSize)
        {
            _left =new Channel(bufferSize, _left_DataAvailable);
            _right = new Channel(bufferSize, _right_DataAvailable);
        }


        public SelectDeviceResult SelectDevices(MMDevice left, MMDevice right)
        {
            _left.UpdateDevice(left);
            _right.UpdateDevice(right);

            Console.WriteLine($"Left device: {left.FriendlyName}, sample rate: {_left.Source.WaveFormat.SampleRate}");
            Console.WriteLine($"Right device: {right.FriendlyName}, sample rate: {_right.Source.WaveFormat.SampleRate}");

            if (_left.Source.WaveFormat.SampleRate != _right.Source.WaveFormat.SampleRate)
            {
                Console.WriteLine("Warning! Devices has different sample rate!");
                return SelectDeviceResult.WARNING_DIFFERENT_SAMPLERATE;
            }

            return SelectDeviceResult.OK;
        }


        public void Start()
        {
            if (_left.Source == null || _right.Source == null)
                return;

            _left.Source.StartRecording();
            _right.Source.StartRecording();
            _isRecording = true;

            Console.WriteLine("Capture started");
        }

        public void Stop()
        {
            if (_left.Source == null || _right.Source == null)
                return;

            _left.Source.StopRecording();
            _right.Source.StopRecording();
            _isRecording = false;

            Console.WriteLine("Capture stopped");
        }


        private void _right_DataAvailable(object sender, WaveInEventArgs e)
        {

            lock (_lock)
            {
                SaveToChannel(_right, e.Buffer);
                FireEvent();
            }
        }

        private void _left_DataAvailable(object sender, WaveInEventArgs e)
        {
            lock (_lock)
            {
                SaveToChannel(_left, e.Buffer);
                FireEvent();
            }
        }


        // Берет пришедшие данные, преобразует и помещает в буфер соответствующего канала
        private void SaveToChannel(Channel channel, byte[] buffer)
        {
            int i = 0;
            int align = channel.Source.WaveFormat.BlockAlign;
            while (i < buffer.Length)
            {
                float v = CovnertBuffer(buffer, i, align);
                if (channel.AddValue(v))
                    return; //Игнорируем остаток

                i+=align;
            }
        }


        // Преобрузет несколько байт в float, в зависимости от типа кодирования
        private float CovnertBuffer(byte[] buffer, int startIndex, int align)
        {
            switch (align)
            {
                case 2:
                    return BitConverter.ToInt16(buffer, startIndex);
                case 4:
                    return BitConverter.ToSingle(buffer, startIndex);
                case 8:
                    return (float)BitConverter.ToDouble(buffer, startIndex);
                default:
                    throw new NotSupportedException();
            }
        }

        // Проверяет, что если оба буфера заполнены, то шлет событие
        private void FireEvent()
        {
            // Ага, разбежался
            // А если эта функция из разных потоков вызывается?

            if (_left.IsBufferFull && _right.IsBufferFull)
            {
                AudioCaptured?.Invoke(this, new AudioCapturedEventArgs() {Left = _left, Right = _right});

                _left.Restart();
                _right.Restart();
            }
        }

        

        /// <summary>
        /// Возвращает список записывающих устройств
        /// </summary>
        /// <returns></returns>
        public static List<MMDevice> GetDevices()
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            return enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).OrderBy(x=>x.ID).ToList();
        }

        public void Dispose()
        {
            _left?.Dispose();
            _right?.Dispose();
        }
    }


    /// <summary>
    /// Хранит всякие объекты, связанные с каналом
    /// </summary>
    public class Channel : IDisposable
    {
        public IWaveIn Source { get; private set; }        
        public float[] Buffer { get; private set; }
        public bool IsBufferFull => _bufferIndex == Buffer.Length;


        private MMDevice _device;
        private int _bufferIndex;

        private readonly EventHandler<WaveInEventArgs> _dataReceivedHandler;

        public Channel(int bufferSize, EventHandler<WaveInEventArgs> dataReceivedHandler)
        {
            Buffer = new float[bufferSize];
            _dataReceivedHandler = dataReceivedHandler;
        }

        /// <summary>
        /// Обновляет ассоциированное устройство
        /// </summary>
        /// <param name="newDevice"></param>
        public void UpdateDevice(MMDevice newDevice)
        {
            if (newDevice != _device)
            {
                if (Source != null)
                {
                    Source.DataAvailable -= _dataReceivedHandler;
                    Source.Dispose();
                }

                newDevice.AudioEndpointVolume.MasterVolumeLevelScalar = 2;
                
                
                _device = newDevice;
                Source = new WasapiCapture(_device);
                Source.DataAvailable += _dataReceivedHandler;
            }
        }

        /// <summary>
        /// Добавляет очередное значение в буфер
        /// </summary>
        /// <param name="v">Значение</param>
        /// <returns>Заполнен ли буфер полностью или нет</returns>
        public bool AddValue(float v)
        {
            if (_bufferIndex >= Buffer.Length)
                return true;

            Buffer[_bufferIndex++] = v;
            if (_bufferIndex == Buffer.Length)
                return true;

            return false;
        }

        /// <summary>
        /// Начинаем заполнение буфера сначала
        /// </summary>
        public void Restart()
        {
            _bufferIndex = 0;
        }

        public void Dispose()
        {
            Source?.Dispose();
            _device?.Dispose();
        }
    }

    public class AudioCapturedEventArgs
    {
        public Channel Left { get; set; }
        public Channel Right { get; set; }
    }

    public enum SelectDeviceResult
    {
        OK,
        WARNING_DIFFERENT_SAMPLERATE
    }
}