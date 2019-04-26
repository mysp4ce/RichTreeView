using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace custcontrol
{
    public partial class RichTreeView : Control
    {
        private List<RichTreeViewItem> _nodes;
        private static Point _location = new Point(15, 15);
        private List<TableLine> _tableLines;

        private int _treeOffsetY = 15;
        private int _valuesOffsetX = 100;

        public List<RichTreeViewItem> Nodes => _nodes;

        public RichTreeView()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            _nodes = new List<RichTreeViewItem>();
            _tableLines = new List<TableLine>();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (IsHandleCreated)
            {
                Invoke(new Action(() => Invalidate()));
                foreach (TableLine line in _tableLines)
                    line.EndPoint.Y = base.Height;
            }
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();

            if (IsHandleCreated)
                this.BeginInvoke(new Action(() => DisplayTree()));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            base.BackColor = Color.White;
            using (Pen pen = new Pen(Color.Gray, 0.1f))
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, base.Size.Width - 1, base.Size.Height - 1));
            
                if (_tableLines.Count > 0)
                    foreach (TableLine line in _tableLines)
                        using (Pen pen = new Pen(Color.Black, 0.5f))
                            line.DrawLine(pen, e);
        }

        public override AnchorStyles Anchor
        {
            get { return base.Anchor; }

            set { base.Anchor = value; }
        }

        private void DisplayTree()
        {
            foreach (RichTreeViewItem node in _nodes)
                RecursiveDisplay(node);
        }

        private void RecursiveDisplay(RichTreeViewItem node)
        {
            AddLabel(node.Text, _location, node, true);
            node.ItemsChanged += new EventHandler(CollectionChanged);

            if (node.Values != null)
                DisplayValues(_location, node);

            _location.Y += _treeOffsetY;

            foreach (RichTreeViewItem item in node.Children)
            {
                if (node.Children.Count > 0)
                {
                    _location.X += 15;
                    RecursiveDisplay(item);
                    _location.X -= 15;
                }
            }
        }

        private void DisplayValues(Point location, RichTreeViewItem node)
        {
            _tableLines = new List<TableLine>();

            for (int i = 0; i < node.Values.Length; i++)
            {
                RichTreeViewItemValue nodeWithValueIndex = new RichTreeViewItemValue() { Node = node, Index = i };
                AddLabel(node.Values[i].ToString(), new Point(_valuesOffsetX, location.Y), nodeWithValueIndex, true);
                _tableLines.Add(new TableLine() { StartPoint = new Point(_valuesOffsetX - 1, 0),
                    EndPoint = new Point(_valuesOffsetX - 1, base.Height) });
                _valuesOffsetX += 50;
            }

            _valuesOffsetX = 100;
        }

        private void AddLabel(string text, Point location, object tag, bool autoSize)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = location;
            label.Tag = tag;
            label.ForeColor = Color.Black;
            label.AutoSize = autoSize;
            label.DoubleClick += new EventHandler(Label_DoubleClick);
            
            this.Controls.Add(label);
        }
        
        private void Label_DoubleClick(object sender, EventArgs e)
        {
            Label label = sender as Label;

            if (label.Tag is RichTreeViewItemValue)
            {
                MessageBox.Show("VALUE " + (label.Tag as RichTreeViewItemValue).Index + " " + (label.Tag as RichTreeViewItemValue).Node.Text);
            }
        }

        private void CollectionChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private class TableLine
        {
            public Point StartPoint;
            public Point EndPoint;

            public void DrawLine(Pen pen, PaintEventArgs e)
            {
                e.Graphics.DrawLine(pen, StartPoint, EndPoint);
            }
        }

        private class RichTreeViewItemValue
        {
            public int Index;
            public RichTreeViewItem Node;
        }
    }

    public static class RichTreeViewListExtensions
    {
        public static void Add(this List<RichTreeViewItem> list, string text) =>
            list.Add(new RichTreeViewItem() { Text = text });

        public static void Add(this List<RichTreeViewItem> list, string text, Image icon) =>
            list.Add(new RichTreeViewItem() { Text = text, Icon = icon });

        public static void Add(this List<RichTreeViewItem> list, string text, object[] values) =>
            list.Add(new RichTreeViewItem() { Text = text, Values = values });

        public static void Add(this List<RichTreeViewItem> list, string text, Image icon, object[] values) =>
            list.Add(new RichTreeViewItem() { Text = text, Icon = icon, Values = values });
    }
}
