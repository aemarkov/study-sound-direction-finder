using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ZedGraph;

namespace SoundDirectionViiewer.Components
{
    public partial class ScrollingGraph : UserControl
    {
        private int _windowSize;
        private double _minValue;
        private double _maxValue;

        [Description("Windows Size")]
        public int WindowSize { get { return _windowSize; } set { _windowSize = value; } }

        [Description("Min Value")]
        public double MinValue { get { return _minValue; } set { _minValue = value; } }

        [Description("Max Value")]
        public double MaxValue { get { return _maxValue; } set { _maxValue = value; } }

        private Dictionary<string, PointPairList> _channels;
        private int index;
        private LineItem curve;

        public ScrollingGraph()
        {
            _windowSize = 1000;
            _maxValue = 100;
            _minValue = 0;

            InitializeComponent();

            _channels = new Dictionary<string, PointPairList>();
        }

        public void AddChannel(string name, Color color)
        {
            var list = new PointPairList();
            _channels.Add(name, list);

            var pane = graph.GraphPane;
            pane.AddCurve(name, list, color, SymbolType.None);

            graph.AxisChange();
            graph.Invalidate();
        }

        public void AddData(string name, float y)
        {
            //if(!_channels.ContainsKey(name))
            //    throw new ArgumentException("Channel doesn't exists in graph", nameof(name));

            var dataList = _channels[name];

            dataList.Add(index, y);
            index++;

            if (dataList.Count > WindowSize)
                dataList.RemoveAt(0);

            graph.GraphPane.XAxis.Scale.Min = dataList.First().X;

            if (dataList.Count < WindowSize)
                graph.GraphPane.XAxis.Scale.Max = WindowSize;
            else
                graph.GraphPane.XAxis.Scale.Max = dataList.Last().X;

            graph.GraphPane.YAxis.Scale.Min = MinValue;
            graph.GraphPane.YAxis.Scale.Max = MaxValue;

            graph.AxisChange();
            graph.Invalidate();
        }
    }
}
