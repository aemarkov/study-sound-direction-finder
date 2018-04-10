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
using System.Diagnostics;

namespace SoundDirectionViiewer
{
    public partial class Form1 : Form, IDisposable
    {
        const int ADC_WINDOW_SIZE = 500;                                // Количество элементов по одному каналу
        const double ADC_WINDOW_TIME = 0.01;                            // Длительность окна (сек)
        const double SAMPLE_RATE = ADC_WINDOW_SIZE / ADC_WINDOW_TIME;
        private const float ADC_REF_V = 3.3f;                           // Опорное напряжение
        private double D = 0.01;                                        // Расстояние между двумя микрофонами

        private SerialPort _com;
        private PackageBuilder _builder;
        private PointPairList _leftAdc, _rightAdc;
        private float[] data1, data2;

        Stopwatch watch = new Stopwatch();

        public Form1()
        {
            InitializeComponent();

            data1 = new float[ADC_WINDOW_SIZE];
            data2 = new float[ADC_WINDOW_SIZE];

            _com = new SerialPort();
            _builder = new PackageBuilder(new byte[] { 0x32, 0xFA, 0x12 }, ADC_WINDOW_SIZE * 2 * sizeof(UInt16));
            _com.DataReceived += _com_DataReceived;
            _builder.PackageReceived += _builder_PackageReceived;

            InitGraph();

            UpdateComs();

            //watch.Start();
        }


        void InitGraph()
        {
            _leftAdc = new PointPairList();
            _rightAdc = new PointPairList();

            for (int i = 0; i < ADC_WINDOW_SIZE; i++)
            {
                double x = i / SAMPLE_RATE;
                _leftAdc.Add(x, 0);
                _rightAdc.Add(x, 0);
            }

            adcGraph.GraphPane.AddCurve("Левый", _leftAdc, Color.Red, SymbolType.None);
            adcGraph.GraphPane.AddCurve("Правый", _rightAdc, Color.Green, SymbolType.None);

            adcGraph.GraphPane.YAxis.Scale.Min = -0.1;
            //adcGraph.GraphPane.YAxis.Scale.Max = ADC_REF_V * 1.1;
            adcGraph.GraphPane.XAxis.Scale.Min = 0;
            adcGraph.GraphPane.XAxis.Scale.Max = ADC_WINDOW_SIZE / SAMPLE_RATE;

            adcGraph.AxisChange();

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
                    watch.Start();
                    
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
            //Console.WriteLine("Package received");
            Console.WriteLine(watch.ElapsedMilliseconds);
            watch.Restart();
            try
            {
                Invoke((MethodInvoker)(() => packageReceived(package)));
            }
            catch (ObjectDisposedException) { }
        }


        private void packageReceived(byte[] package)
        {
            GetDataFromPackage(package, ref data1, ref data2);

            // Отображение сигнала
            for (int i = 0; i < data1.Length; i++)
            {
                _leftAdc[i].Y = data1[i];
                _rightAdc[i].Y = data2[i];
            }
;
            adcGraph.AxisChange();
            adcGraph.Invalidate();
            adcGraph.Update();
        }


        // Извлекает отдельные данные первого и второго канала из пакета
        private void GetDataFromPackage(byte[] buffer, ref float[] data1, ref float[] data2)
        {
            int index = 0;
            for (int i = 0; i < buffer.Length; i+=4)
            {
                data1[index] = BitConverter.ToUInt16(buffer, i) / 4096.0f * ADC_REF_V;
                data2[index] = BitConverter.ToUInt16(buffer, i + 2) / 4096.0f * ADC_REF_V;
                index++;
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
}
