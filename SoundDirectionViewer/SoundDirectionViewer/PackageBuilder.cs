using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundDirectionViiewer
{
    public delegate void PackageReceivedDelegate(object sender, byte[] package);

    /// <summary>
    /// Собирает приходящие по UART пакеты (обнаруживая заголовок).
    /// Может собрать пакет из кусков, разделенный на несколько приемов
    /// </summary>
    public class PackageBuilder
    {
        public event PackageReceivedDelegate PackageReceived;

        enum ReceiveState
        {
            RECEIVING_HEADER,
            RECEVING_BODY
        };

        //Автомат приема
        private ReceiveState _state;
        private List<byte> _receivedQueue;
        private int _currentHeaderByte;

        private int _packageLength;
        private byte[] _packageHeder;

        public PackageBuilder(byte[] header, int packageLength)
        {
            _packageHeder = new byte[header.Length];
            Array.Copy(header, _packageHeder, header.Length);

            _packageLength = packageLength;

            _receivedQueue = new List<byte>();
            _currentHeaderByte = 0;
            _state = ReceiveState.RECEIVING_HEADER;
        }

        /// <summary>
        /// Обработать очередной кусок принятых данных
        /// </summary>
        /// <param name="buffer"></param>
        public void ProcessPart(byte[] buffer)
        {
            int i = 0;
            while (i < buffer.Length)
            {
                if (_state == ReceiveState.RECEIVING_HEADER)
                {
                    //Ждем заголовка

                    if (buffer[i] == _packageHeder[_currentHeaderByte])
                    {
                        //Читаем заголовок
                        _state = ReceiveState.RECEIVING_HEADER;
                        _currentHeaderByte++;
                    }
                    else
                    {
                        //Это не заголовок
                        _currentHeaderByte = 0;
                        _state = ReceiveState.RECEIVING_HEADER;
                    }

                    if (_currentHeaderByte == _packageHeder.Length)
                    {
                        //Заголовок полностью считан
                        _state = ReceiveState.RECEVING_BODY;
                    }
                }
                else if (_state == ReceiveState.RECEVING_BODY)
                {
                    //Читаем пакет
                    _receivedQueue.Add(buffer[i]);

                    if (_receivedQueue.Count == _packageLength)
                    {
                        //Пакет полностью считан
                        _state = ReceiveState.RECEIVING_HEADER;
                        _currentHeaderByte = 0;

                        if (PackageReceived != null)
                            PackageReceived(this, _receivedQueue.ToArray());

                        _receivedQueue.Clear();
                    }
                }

                i++;
            }
        }
    }
}
