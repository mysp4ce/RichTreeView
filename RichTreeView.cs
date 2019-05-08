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
        private RichTreeViewItem _rootNode;

        private bool _checkBoxes = false;

        private const int _treeOffsetX = 35;
        private const int _treeOffsetY = 10;
        private int _tableOffset = 70;
        private int _labelOffset = 0;

        private string _textBoxText;
        private int _valuesOffsetX = 150;
        private int _nodeOffsetY = 20;

        private List<TableLine> _tableLines;
        private List<RichTreeViewItemImage> _icons;
        private Point _location = new Point(_treeOffsetX, _treeOffsetY);
        private Rectangle _highlight;
        
        public RichTreeViewItem Root => _rootNode;

        public RichTreeView()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            _rootNode = new RichTreeViewItem();
            _tableLines = new List<TableLine>();
            _icons = new List<RichTreeViewItemImage>();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (IsHandleCreated)
            {
                foreach (TableLine line in _tableLines)
                    line.EndPoint.Y = base.Height;
                _highlight.Width = base.Width;
                Invalidate();
            }
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();

            if (IsHandleCreated)
            {
                this.BeginInvoke(new Action(() => DisplayTree()));
            }

            _highlight = new Rectangle();
            _highlight.Width = base.Width;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            using (var pen = new Pen(Color.Gray, 0.1f))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, base.Size.Width - 1, base.Size.Height - 1));
                e.Graphics.DrawRectangle(pen, _highlight);
            }

            using (var brush = new SolidBrush(Color.FromArgb(10, Color.DeepSkyBlue)))
                e.Graphics.FillRectangle(brush, _highlight);

            if (_tableLines.Count > 0)
                using (var pen = new Pen(Color.Black, 0.1f))
                    foreach (TableLine line in _tableLines)
                        line.DrawLine(pen, e);

            if (_icons.Count > 0)
                foreach (var icon in _icons)
                    e.Graphics.DrawImage(icon.Icon, icon.location.X, icon.location.Y, 15, 15);
        }

        public bool CheckBoxes
        {
            get { return _checkBoxes; }

            set
            {
                _checkBoxes = value;
                this.Invalidate();
            }
        }

        private void DisplayTree()
        {
            foreach (RichTreeViewItem node in _rootNode.Children)
                RecursiveDisplayTree(node);
            
            this.Invalidate();
        }

        private void Reset()
        {
            foreach (RichTreeViewItem node in _rootNode.Children)
                RemoveEvents(node);

            _icons = new List<RichTreeViewItemImage>();
            _location = new Point(_treeOffsetX, _treeOffsetY);
            _nodeOffsetY = 20;
            _valuesOffsetX = 150;
        }

        private void RecursiveDisplayTree(RichTreeViewItem node)
        {
            if (!node.IsHidden)
            {
                AddLabel(node.Text, _location, node, true, this);
                node.ItemsChanged += new EventHandler(CollectionChanged);

                if (node.Icon != null)
                    _icons.Add(new RichTreeViewItemImage()
                    {
                        Icon = node.Icon,
                        location = new Point(_location.X - 15, _location.Y)
                    });

                if (_checkBoxes)
                    if (node.Icon == null)
                        AddCheckBox(new Point(_location.X - 15, _location.Y - 4), node, node.Checked);
                    else
                        AddCheckBox(new Point(_location.X - 30, _location.Y - 4), node, node.Checked);

                if (node.Values != null)
                    DisplayValues(_location, node);

                _location.Y += _nodeOffsetY;

                foreach (RichTreeViewItem item in node.Children)
                {
                    if (node.CanExpand)
                    {
                        int offset = 15;

                        if (CheckBoxes)
                            offset = 30;

                        _location.X += offset;
                        RecursiveDisplayTree(item);
                        _location.X -= offset;
                    }
                }
            }
        }

        private void DisplayValues(Point location, RichTreeViewItem node)
        {
            int offset = _valuesOffsetX;

            for (int i = 0; i < node.Values.Length; i++)
            {
                RichTreeViewItemValue nodeWithValueIndex = new RichTreeViewItemValue() { Node = node, Index = i };

                if (node.Values[i] is bool)
                    AddCheckBox(new Point(offset + 25, location.Y - 4), nodeWithValueIndex, (bool)nodeWithValueIndex.Node.Values[nodeWithValueIndex.Index]);
                else
                    AddLabel(
                        node.Values[i] == null ? null : ((node.Values[i] is IList) ? (node.Values[i] as IList)[0].ToString() : node.Values[i].ToString()),
                        new Point(offset, location.Y),
                        nodeWithValueIndex,
                        true,
                        this
                        );

                TableLine tableLine = new TableLine()
                {
                    StartPoint = new Point(offset - 3, 0),
                    EndPoint = new Point(offset - 3, base.Height)
                };

                if (_tableLines.Count < node.Values.Count())
                    if (!_tableLines.Any(line => line.StartPoint == tableLine.StartPoint && line.EndPoint == tableLine.EndPoint))
                        _tableLines.Add(tableLine);

                offset += _tableOffset;
            }
        }

        private void RemoveEvents(RichTreeViewItem node)
        {
            node.ItemsChanged -= new EventHandler(CollectionChanged);

            foreach (RichTreeViewItem item in node.Children)
                if (node.CanExpand)
                    RemoveEvents(item);
        }

        private void Hide(RichTreeViewItem node, bool hide)
        {
            foreach (RichTreeViewItem child in node.Children)
            {
                child.IsHidden = hide;
                if (child.CanExpand)
                    Hide(child, hide);
            }
        }

        private void AddLabel(string text, Point location, object tag, bool autoSize, Control parent)
        {
            Label label = new Label();
            if (tag is RichTreeViewItem)
                foreach (Control panel in this.Controls)
                    if (panel is Panel)
                        if ((int)panel.Tag == 1)
                            label.Parent = panel;
            label.Text = text;
            label.Location = location;
            label.MinimumSize = new Size(50, 15);
            label.Tag = tag;
            label.ForeColor = Color.Black;
            label.AutoSize = autoSize;
            label.UseCompatibleTextRendering = true;
            label.BackColor = Color.Transparent;
            label.DoubleClick += new EventHandler(Label_DoubleClick);
            label.Click += new EventHandler(Label_Click);

            this.Controls.Add(label);

            //if (_labelOffset > 0)
            //    foreach (Control control in this.Controls)
            //        if (control is Label)
            //            for (int i = 0; i < _tableLines.Count; i++)
            //                if (control.Location.X < _tableLines[i].StartPoint.X && control.Location.X + control.Width > _tableLines[i].StartPoint.X)
            //                {
            //                    foreach (Control controlForOffset in this.Controls)
            //                        if (controlForOffset.Location.X > _tableLines[i].StartPoint.X)
            //                            controlForOffset.Location = new Point(controlForOffset.Location.X + 20, controlForOffset.Location.Y);

            //                    for (int j = i; j < _tableLines.Count; j++)
            //                    {
            //                        _tableLines[j].StartPoint = new Point(_tableLines[j].StartPoint.X + 20, _tableLines[j].StartPoint.Y);
            //                        _tableLines[j].EndPoint = new Point(_tableLines[j].EndPoint.X + 20, _tableLines[j].EndPoint.Y);
            //                    }

            //                    break;
            //                }
        }

        private void AddTextBox(string text, Point location, object tag, Size size)
        {
            TextBox textBox = new TextBox();
            textBox.Text = text;
            textBox.Location = location;
            textBox.Tag = tag;
            textBox.Size = size;
            textBox.BorderStyle = BorderStyle.None;
            textBox.KeyDown += new KeyEventHandler(TextBox_KeyDown);

            this.Controls.Add(textBox);
        }

        private void AddCheckBox(Point location, object tag, bool check)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.Tag = tag;
            checkBox.Location = location;
            checkBox.BackColor = Color.Transparent;
            checkBox.Checked = check;
            checkBox.Width = 20;
            checkBox.CheckedChanged += new EventHandler(CheckBox_CheckedChanged);

            this.Controls.Add(checkBox);
        }

        private void AddComboBox(Point location, object tag, BindingSource bindingSource, int width)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Tag = tag;
            comboBox.Location = location;
            comboBox.DataSource = bindingSource.DataSource;
            comboBox.Width = width;
            comboBox.KeyDown += new KeyEventHandler(ComboBox_KeyDown);
            comboBox.AllowDrop = false;

            this.Controls.Add(comboBox);
        }
        
        private void Label_DoubleClick(object sender, EventArgs e)
        {
            Label label = sender as Label;
            if (label.Tag is RichTreeViewItemValue)
            {
                var value = label.Tag as RichTreeViewItemValue;

                if (value.Node.Values[value.Index] is IList)
                {
                    using (var bindingSource = new BindingSource())
                    {
                        bindingSource.DataSource = value.Node.Values[value.Index];
                        AddComboBox(label.Location, label.Tag, bindingSource, label.Width - 5);
                    }
                }
                else if (!(value.Node.Values[value.Index] is bool))
                {
                    AddTextBox(label.Text, label.Location, label.Tag, label.Size);
                    _textBoxText = label.Text;
                }
                label.Dispose();
            }

            if (label.Tag is RichTreeViewItem)
            {
                var node = label.Tag as RichTreeViewItem;

                if (node.CanExpand)
                {
                    if (node.Children[0].IsHidden)
                        Hide(node, false);
                    else
                        Hide(node, true);

                    this.Controls.Clear();
                    Reset();
                    DisplayTree();
                }
            }

            _labelOffset = label.Width;
        }

        private void Label_Click(object sender, EventArgs e)
        {
            Label label = sender as Label;

            _highlight.Location = new Point(0, label.Location.Y - 1);
            _highlight.Height = label.Height;
            this.Invalidate();
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var textBox = sender as TextBox;

                if (string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrEmpty(_textBoxText))
                    textBox.Text = _textBoxText;

                var value = textBox.Tag as RichTreeViewItemValue;
                value.Node.Values[value.Index] = textBox.Text;

                AddLabel(textBox.Text, textBox.Location, textBox.Tag, true, this);

                _textBoxText = null;
                textBox.Dispose();
            }
            this.Invalidate();
        }

        private void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var comboBox = sender as ComboBox;
                var value = comboBox.Tag as RichTreeViewItemValue;
                if (!((IList)value.Node.Values[value.Index]).Contains(comboBox.Text))
                    ((IList)value.Node.Values[value.Index]).Add(comboBox.Text);

                AddLabel(comboBox.Text, comboBox.Location, comboBox.Tag, true, this);
                comboBox.Dispose();
            }
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox.Tag is RichTreeViewItem)
                (checkBox.Tag as RichTreeViewItem).Checked = checkBox.Checked;

            if (checkBox.Tag is RichTreeViewItemValue)
            {
                var itemValue = checkBox.Tag as RichTreeViewItemValue;
                itemValue.Node.Values[itemValue.Index] = checkBox.Checked;
            }
        }

        private void CollectionChanged(object sender, EventArgs e)
        {
            if (e == null)
            {
                foreach (Control control in this.Controls)
                    if (control is Label)
                        if (control.Tag == sender)
                        {
                            control.Text = (sender as RichTreeViewItem).Text;
                            control.Tag = (sender as RichTreeViewItem);
                            control.Refresh();
                        }
            }
            else
            {
                if ((e as NotifyCollectionChangedEventArgs).Action == NotifyCollectionChangedAction.Add)
                    foreach (Control control in this.Controls)
                        control.Dispose();

                else if ((e as NotifyCollectionChangedEventArgs).Action == NotifyCollectionChangedAction.Remove)
                    foreach (Control control in this.Controls)
                        control.Dispose();

                this.Controls.Clear();
                Reset();
                DisplayTree();
            }
            this.Invalidate();
        }
        
        private class TableLine
        {
            public Point StartPoint;
            public Point EndPoint;
            
            public bool IsPointOnLine(Point point, int cushion) 
                => (point.X >= StartPoint.X - cushion && point.X <= StartPoint.X + cushion);

            public void DrawLine(Pen pen, PaintEventArgs e) 
                => e.Graphics.DrawLine(pen, StartPoint, EndPoint);
        }

        private class RichTreeViewItemValue
        {
            public int Index;
            public RichTreeViewItem Node;
        }

        private class RichTreeViewItemImage
        {
            public Image Icon;
            public Point location;
        }
    }
}
