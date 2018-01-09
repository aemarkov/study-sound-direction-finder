using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;

namespace Common.Components
{
    public partial class ScrollingGraph : UserControl
    {

        public int WindowSize { get; set; }
        public double YMinValue { get; set; }
        public double YMaxValue { get; set; }
        public double XMinValue { get; set; }
        public double XMaxValue { get; set; }
        public bool IsRolling { get; set; }
        public bool IsXAutoScale { get; set; }
        public bool IsYAutoScale { get; set; }

        public string Title
        {
            get => graph.GraphPane.Title.Text;
            set
            {
                graph.GraphPane.Title.Text = value; 
                graph.Invalidate();
            }
        }

        public string XTitle
        {
            get => graph.GraphPane.XAxis.Title.Text;
            set
            {
                graph.GraphPane.XAxis.Title.Text = value; 
                graph.Invalidate();
            }
        }

        public string YTitle
        {
            get => graph.GraphPane.YAxis.Title.Text;
            set
            {
                graph.GraphPane.YAxis.Title.Text = value;
                graph.Invalidate();
            }
        }

        private List<RollingPointPairList> _rollingChannels;
        private List<PointPairList> _channels;
        private int x;
        

        public ScrollingGraph()
        {
            WindowSize = 1000;
            YMaxValue = 100;
            IsRolling = true;
            YMinValue = 0;

            InitializeComponent();

            _rollingChannels = new List<RollingPointPairList>();
            _channels = new List<PointPairList>();
        }

        public void AddChannel(string name, Color color)
        {
            IPointList list;

            if (IsRolling)
            {
                list = new RollingPointPairList(WindowSize);
                _rollingChannels.Add((RollingPointPairList)list);
            }
            else
            {
                list = new PointPairList();
                _channels.Add((PointPairList)list);
            }

            var pane = graph.GraphPane;
            pane.AddCurve(name, list, color, SymbolType.None);

            graph.AxisChange();
            graph.Invalidate();
        }

        public void AddData(double x, params double[] y)
        {
            var channelsCount = IsRolling ? _rollingChannels.Count : _channels.Count;

            if(y.Length != channelsCount)
                throw new ArgumentException("Неверное количество каналов");

            double xVal = IsRolling ? this.x : x;

            if (IsRolling)
            {
                for (int i = 0; i < y.Length; i++)
                    _rollingChannels[i].Add(this.x, y[i]);
            }
            else
            {
                for (int i = 0; i < y.Length; i++)
                    _channels[i].Add(x, y[i]);
            }
        }

        public void UpdateGraph()
        {
            var pane = graph.GraphPane;

            if (IsRolling)
            {
                x++;
                pane.XAxis.Scale.Min = x - WindowSize;
                pane.XAxis.Scale.Max = x;
            }
            else if (!IsXAutoScale)
            {
                pane.XAxis.Scale.Min = XMinValue;
                pane.XAxis.Scale.Max = XMaxValue;
            }

            if (!IsYAutoScale)
            {
                pane.YAxis.Scale.Min = YMinValue;
                pane.YAxis.Scale.Max = YMaxValue;
            }

            graph.AxisChange();
            graph.Invalidate();
            graph.Update();
        }

        public void Clear()
        {
            if (IsRolling)
            {
                foreach (var channel in _rollingChannels)
                    channel.Clear();

                x = 0;
            }
            else
            {
                foreach (var channel in _channels)
                    channel.Clear();
            }


        }
    }
}
