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

namespace SoundDirectionViiewer
{
    public partial class Form1 : Form
    {
        private SerialPort _com;
        private PackageBuilder _builder;


        public Form1()
        {
            InitializeComponent();

            _com = new SerialPort();
            _builder = new PackageBuilder(new byte[]{0x32, 0xFA, 0x12}, 4);

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
            Invoke((MethodInvoker)(()=>packageReceived(package)));
        }


        private void packageReceived(byte[] package)
        {
            label1.Text = BitConverter.ToInt16(package, 0).ToString();
            label2.Text = BitConverter.ToInt16(package, 2).ToString();
        }
        

        public void UpdateComs()
        {
            cboxComs.Items.Clear();

            var names = SerialPort.GetPortNames();
            foreach (var name in names)
                cboxComs.Items.Add(name);
        }

    }
}
