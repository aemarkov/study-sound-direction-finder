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

        bool IS_ROLLING = false;
        private RollingPointPairList adc1List, adc2List;
        private int currentGraphX;
        private int GRAPH_CAPACITY = 1024;
        private const double Y_MIN_VALUE = -0.1 * ADC_REF_V;
        private const double Y_MAX_VALUE = 1.2 * ADC_REF_V;
        private const double X_MIN_VALUE = 0;
        private const double X_MAX_VALUE = DATA_LENGTH;

        private RollingPointPairList angleList;
        private int currentAngleX;
        private int ANGLE_GRAPH_CAPACITY = 512;
        private const double ANGLE_Y_MIN_VALUE = -100;
        private const double ANGLE_Y_MAX_VALUE = 100;

        public Form1()
        {
            InitializeComponent();

            adc1List = new RollingPointPairList(GRAPH_CAPACITY);
            adc2List = new RollingPointPairList(GRAPH_CAPACITY);
            zedGraphControl1.GraphPane.AddCurve("ADC1", adc1List, Color.Red, SymbolType.None);
            zedGraphControl1.GraphPane.AddCurve("ADC2", adc2List, Color.Green, SymbolType.None);

            var pane = zedGraphControl1.GraphPane;
            pane.YAxis.Scale.Min = Y_MIN_VALUE;
            pane.YAxis.Scale.Max = Y_MAX_VALUE;
            pane.XAxis.Scale.Min = X_MIN_VALUE;
            pane.XAxis.Scale.Max = X_MAX_VALUE;
            pane.XAxis.Title.Text = "Отсчеты";
            pane.YAxis.Title.Text = "Напряжение, В";
            pane.Title.Text = "";

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

            angleList = new RollingPointPairList(ANGLE_GRAPH_CAPACITY);
            pane = zedGraphControl2.GraphPane;
            pane.AddCurve("Shift", angleList, Color.Red, SymbolType.None);

            pane.YAxis.Scale.Min = ANGLE_Y_MIN_VALUE;
            pane.YAxis.Scale.Max = ANGLE_Y_MAX_VALUE;
            pane.XAxis.Title.Text = "Индекс";
            pane.YAxis.Title.Text = "Сдвиг";
            pane.Title.Text = "";

            zedGraphControl2.AxisChange();
            zedGraphControl2.Invalidate();

            _com = new SerialPort();
            _builder = new PackageBuilder(new byte[]{0x32, 0xFA, 0x12}, DATA_LENGTH * 2 * sizeof(UInt16));            
            _com.DataReceived += _com_DataReceived;
            _builder.PackageReceived += _builder_PackageReceived;

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
            double[]  adcVal1 = new double[DATA_LENGTH];
            double[] adcVal2 = new double[DATA_LENGTH];

            if (!IS_ROLLING)
            {
                adc1List.Clear();
                adc2List.Clear();
            }
            
            for (int i = 0; i < DATA_LENGTH-1; i++)
            {
                int x = IS_ROLLING ? currentGraphX : i;

                adcVal1[i] = BitConverter.ToUInt16(package, i*4) / 4096.0 * ADC_REF_V;
                adcVal2[i] = BitConverter.ToUInt16(package, i * 4 + 2) / 4096.0 * ADC_REF_V;
                adc1List.Add(x, adcVal1[i]);
                adc2List.Add(x, adcVal2[i]);

                currentGraphX++;
            }
            
            var corr = FindMaxShift(adcVal1, adcVal2);
            double shift = corr.MaxShift * T_SAMPL;
            double angle = Math.Sign(shift) * Math.Asin(shift * 343.0 / D);

            

            lblMaxShift.Text = corr.MaxShift.ToString();
            lblMaxCorrelation.Text = angle.ToString("000.000");

            if (IS_ROLLING)
            {
                zedGraphControl1.GraphPane.XAxis.Scale.Min = currentGraphX - GRAPH_CAPACITY;
                zedGraphControl1.GraphPane.XAxis.Scale.Max = currentGraphX;
                zedGraphControl1.AxisChange();
            }
            zedGraphControl1.Invalidate();

            angleList.Add(currentAngleX++, corr.MaxShift);
            zedGraphControl2.GraphPane.XAxis.Scale.Min = currentAngleX - ANGLE_GRAPH_CAPACITY;
            zedGraphControl2.GraphPane.XAxis.Scale.Max = currentAngleX;
            zedGraphControl2.AxisChange();
            zedGraphControl2.Invalidate();


        }


        private void UpdateComs()
        {
            cboxComs.Items.Clear();

            var names = SerialPort.GetPortNames();
            foreach (var name in names)
                cboxComs.Items.Add(name);
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
       
    }

    struct CorrelationResult
    {
        public int MaxShift { get; set; }
        public double MaxCorrelation { get; set;}
    }
}
