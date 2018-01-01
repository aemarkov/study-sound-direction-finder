using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundDirectionViiewer.Properties;
using ZedGraph;

namespace SoundDirectionViiewer
{
    public partial class Form1 : Form, IDisposable
    {
        const int DATA_LENGTH = 256;                                // Размер буфера
        private const double ADC_REF_V = 3.3;                       // Опорное напряжение
        private double T_SAMPL = 180.0 / 72000000;                  // Период одного измерения
        private double D = 0.01;

        private SerialPort _com;
        private PackageBuilder _builder;

        private double[] data1, data2;
    
        public Form1()
        {
            InitializeComponent();
            
            sgraphAdc.AddChannel("ADC1", Color.Red);
            sgraphAdc.AddChannel("ADC2", Color.Green);
            sgraphShift.AddChannel("Сдвиг", Color.Red);
            
            data1 = new double[DATA_LENGTH];
            data2 = new double[DATA_LENGTH];

            _com = new SerialPort();
            _builder = new PackageBuilder(new byte[]{0x32, 0xFA, 0x12}, DATA_LENGTH * 2 * sizeof(UInt16));            
            _com.DataReceived += _com_DataReceived;
            _builder.PackageReceived += _builder_PackageReceived;

            sgraphAdc.WindowSize = DATA_LENGTH;
            sgraphAdc.MaxValue = 1.1;
            sgraphAdc.MinValue = -0.1;

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

                if(cboxComs.SelectedItem == null)
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
            double[] dataOut1 = new double[DATA_LENGTH];
            double[] dataOut2 = new double[DATA_LENGTH];


            GetDataFromPackage(package, ref data1, ref data2);
            Normalize(data1);
            Normalize(data2); 
            
            //MovingAverage(data1, dataOut1, 10);
            //MovingAverage(data2, dataOut2, 10);

            DrawData(data1, dataOut1);
        }



        // Нормализует ряд данных
        private void Normalize(double[] data)
        {
            double max = data.Max();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] /= max;
            }
        }

        private void MovingAverage(double[] dataIn, double[] dataOut, int size)
        {
            double sum = 0;

            for (int i = 0; i < dataIn.Length; i++)
            {
                sum += dataIn[i];
                if (i > size)
                    sum -= dataIn[i - size];

                dataOut[i] = sum / size;
            }
        }


        private CorrelationResult FindMaxShift(double[] a, double[] b)
        {
            int maxShift = 0;
            double maxCorrelation = 0;

            for (int shift = 0; shift < a.Length / 4; shift++)
            {
                double c = CalcCorrelation(a, b, shift);
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

            return new CorrelationResult() { MaxShift = maxShift, MaxCorrelation = maxCorrelation } ;
        }

        private double CalcCorrelation(double[] a, double[] b, int shift)
        {
            double correlation = 0;
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



        // Извлекает отдельные данные первого и второго канала из пакета
        private void GetDataFromPackage(byte[] buffer, ref double[] data1, ref double[] data2)
        {
            for (int i = 0; i < DATA_LENGTH - 1; i++)
            {
                data1[i] = BitConverter.ToUInt16(buffer, i * 4);
                data2[i] = BitConverter.ToUInt16(buffer, i * 4 + 2);
            }
        }

        // Рисует два графика
        private void DrawData(double[] data1, double[] data2)
        {
            sgraphAdc.Clear();
            for (int i = 0; i < data1.Length; i++)
                sgraphAdc.AddData(i, data1[i], data2[i]);

            sgraphAdc.Invalidate();
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
        public double MaxCorrelation { get; set;}
    }
}
