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
        public int WindowSize { get => _windowSize; set => _windowSize = value; }

        public double MinValue { get => _minValue; set => _minValue = value; }
     
        public double MaxValue { get => _maxValue; set => _maxValue = value; }

        public bool IsRolling { get => _isRolling; set => _isRolling = value;
        }

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

        private bool _isRolling;
        private int _windowSize;
        private double _minValue;
        private double _maxValue;

        private List<RollingPointPairList> _channels;
        private int x;
        

        public ScrollingGraph()
        {
            _windowSize = 1000;
            _maxValue = 100;
            IsRolling = true;
            _minValue = 0;

            InitializeComponent();

            _channels = new List<RollingPointPairList>();
        }

        public void AddChannel(string name, Color color)
        {
            var list = new RollingPointPairList(WindowSize);
            _channels.Add(list);

            var pane = graph.GraphPane;
            pane.AddCurve(name, list, color, SymbolType.None);

            graph.AxisChange();
            graph.Invalidate();
        }

        public void AddData(double x, params double[] y)
        {
            double xVal = IsRolling ? this.x : x;

            if(y.Length != _channels.Count)
                throw new ArgumentException("Неверное количество каналов");

            for (int i = 0; i < y.Length; i++)            
               _channels[i].Add(xVal, y[i]);
        }

        public new void Invalidate()
        {
            var pane = graph.GraphPane;

            if (IsRolling)
            {
                x++;
                pane.XAxis.Scale.Min = x - WindowSize;
                pane.XAxis.Scale.Max = x;
            }

            pane.YAxis.Scale.Min = MinValue;
            pane.YAxis.Scale.Max = MaxValue;

            graph.AxisChange();
            graph.Invalidate();
        }

        public void Clear()
        {
            if (IsRolling)
                return;

            foreach (var channel in _channels)
                channel.Clear();
        }
    }
}
