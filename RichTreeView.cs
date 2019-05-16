using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace custcontrol
{
    public partial class RichTreeView : ScrollableControl
    {
        private RichTreeViewItem _rootNode = new RichTreeViewItem();

        private bool _checkBoxes = false;

        private const int _treeStartOffsetX = 35;
        private const int _treeStartOffsetY = 25;
        private int _treeMaxOffset = 0; 
        private int _stringOffset = 0;
        private int _nodeOffsetY = 20;

        private int _maxScrollPointX = 0;
        private int _maxScrollPointY = 0;

        private string _textBoxText;

        private int _comboBoxSelectedIndex = -1;

        private HashSet<StringItem> _nodes = new HashSet<StringItem>();
        private List<TableLine> _tableLines = new List<TableLine>();
        private List<Image> _icons = new List<Image>();
        private List<RichTreeViewCheckBox> _checkBoxValues = new List<RichTreeViewCheckBox>();
        private List<RichTreeViewColumn> _columns;

        private Point _location = new Point(_treeStartOffsetX, _treeStartOffsetY);
        private Rectangle _highlight;
        
        public RichTreeViewItem Root => _rootNode;

        public event EventHandler ItemDrawn;
        public event EventHandler NodeChanged;
        public event EventHandler ValuesChanged;
        public event EventHandler NodeExpanded;
        public event EventHandler NodeHided;

        public List<RichTreeViewColumn> Columns => _columns;

        public bool CheckBoxes
        {
            get { return _checkBoxes; }

            set
            {
                _checkBoxes = value;
                base.Invalidate();
            }
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.Invalidate();
        }

        public RichTreeView()
        {
            InitializeComponent();
            base.DoubleBuffered = true;

            base.AutoScroll = true;
            base.AutoScrollMinSize = new Size(base.Height, base.Width);
        }

        public virtual void Relayout()
        {
            Reset();
            base.Controls.Clear();
            DisplayTree();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            if (IsHandleCreated)
            {
                foreach (TableLine line in _tableLines)
                    line.EndPoint.Y = base.Height;
                _highlight.Width = base.Width;
                base.Invalidate();
            }
            base.AutoScrollMinSize = new Size(_maxScrollPointX + 20, _maxScrollPointY + 20);
        }

        protected override void CreateHandle()
        {
            base.CreateHandle();

            if (IsHandleCreated)
                base.BeginInvoke(new Action(() => DisplayTree()));

            _columns = new List<RichTreeViewColumn>();

            _highlight = new Rectangle();
            _highlight.Width = base.Width;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.TranslateTransform(this.AutoScrollPosition.X,
                                 this.AutoScrollPosition.Y);

            using (var pen = new Pen(Color.Gray, 0.1f))
            {
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, this.AutoScrollPosition.X - 1, this.AutoScrollPosition.Y - 1));
                e.Graphics.DrawRectangle(pen, new Rectangle(_highlight.X, _highlight.Y - 1, _highlight.Width, _highlight.Height + 1));
            }

            foreach (var element in _nodes)
            {
                element.DrawString(base.Font, e, Brushes.Black);
                ItemDrawn?.Invoke(element.Tag, null);
            }

            foreach (var checkBox in _checkBoxValues)
                checkBox.DrawCheckBox(e);

            using (var brush = new SolidBrush(Color.FromArgb(10, Color.DeepSkyBlue)))
                e.Graphics.FillRectangle(brush, new Rectangle(_highlight.X, _highlight.Y - 1, _highlight.Width, _highlight.Height + 1));

            if (_tableLines.Count > 0)
                using (var pen = new Pen(Color.Black, 0.1f))
                    foreach (TableLine line in _tableLines)
                        line.DrawLine(pen, e);

            if (_icons.Count > 0)
                foreach (var icon in _icons)
                {
                    Point? location = icon.Tag as Point?;
                    e.Graphics.DrawImage(icon, location.Value.X, location.Value.Y, 15, 15);
                }
            e.Graphics.TranslateTransform(_maxScrollPointX, _maxScrollPointY);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            foreach (StringItem item in _nodes)
                if (item.Rectangle.Contains(e.Location))
                    if (item.Tag != null)
                        Control_Click(item, e);
            foreach (var checkBox in _checkBoxValues)
                if (new Rectangle(checkBox.Location.X, checkBox.Location.Y, 12, 12).Contains(e.Location))
                    CheckBox_CheckedChanged(checkBox, null);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            foreach (StringItem item in _nodes.ToList())
                if (item.Rectangle.Contains(e.Location))
                    Control_DoubleClick(item, e);
        }

        protected virtual void Control_DoubleClick(object sender, EventArgs e)
        {
            var item = sender as StringItem;
            if (item.Tag is RichTreeViewItemValue)
            {
                var value = item.Tag as RichTreeViewItemValue;

                if (value.Node.Values[value.Index] is IList)
                    using (var bindingSource = new BindingSource())
                    {
                        bindingSource.DataSource = value.Node.Values[value.Index];
                        AddListEditor(Point.Truncate(item.Rectangle.Location), item.Tag, bindingSource, (int)(item.Rectangle.Width - 5));

                        for (int i = 0; i < _nodes.Count; i++)
                            if (_nodes.ElementAt(i).Equals(item))
                                _nodes.Remove(item);
                    }
                else if (!(value.Node.Values[value.Index] is bool))
                    AddTextEditor(item.Text, Point.Truncate(item.Rectangle.Location), item.Tag, Size.Truncate(item.Rectangle.Size));
            }

            if (item.Tag is RichTreeViewItem)
            {
                var node = item.Tag as RichTreeViewItem;

                if (node.CanExpand)
                {
                    if (node.Children[0].IsHidden)
                        Hide(node, false);
                    else
                        Hide(node, true);

                    base.Controls.Clear();
                    Reset();
                    DisplayTree();
                }
            }

            _stringOffset = (int)item.Rectangle.Width;
        }

        protected virtual void Control_Click(object sender, EventArgs e)
        {
            _highlight.Location = new Point(0, (int)(sender as StringItem).Rectangle.Location.Y);
            _highlight.Height = (int)(sender as StringItem).Rectangle.Height;
            base.Invalidate();
        }

        protected virtual void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var textBox = sender as TextBox;

                if (string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrEmpty(_textBoxText))
                    textBox.Text = _textBoxText;

                var value = textBox.Tag as RichTreeViewItemValue;
                value.Node.Values[value.Index] = textBox.Text;

                AddString(textBox.Text, textBox.Location, textBox.Tag);

                _textBoxText = null;
                textBox.Dispose();

                base.Controls.Clear();
                Reset();
                DisplayTree();
                Invalidate();

                ValuesChanged?.Invoke(textBox.Tag, null);
            }
        }

        protected virtual void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var comboBox = sender as ComboBox;
                var value = comboBox.Tag as RichTreeViewItemValue;

                AddString(comboBox.Text, comboBox.Location, comboBox.Tag);

                _comboBoxSelectedIndex = comboBox.SelectedIndex;
                comboBox.Dispose();
            }
        }

        protected virtual void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var checkBox = sender as RichTreeViewCheckBox;
            checkBox.Checked = checkBox.Checked ? false : true;

            if (checkBox.Tag is RichTreeViewItem)
                (checkBox.Tag as RichTreeViewItem).Checked = checkBox.Checked;

            if (checkBox.Tag is RichTreeViewItemValue)
            {
                var itemValue = checkBox.Tag as RichTreeViewItemValue;
                itemValue.Node.Values[itemValue.Index] = checkBox.Checked;
            }

            Invalidate();
        }

        protected virtual void NodesCollectionChanged(object sender, EventArgs e)
        {
            NodeChanged?.Invoke(sender, e);

            Reset();
            DisplayTree();
            base.Invalidate();
        }

        protected sealed class StringItem
        {
            public RectangleF Rectangle;
            public object Tag;
            public string Text;

            public void DrawString(Font font, PaintEventArgs e, Brush brush)
            {
                if (Tag is RichTreeViewItem)
                    e.Graphics.DrawString((Tag as RichTreeViewItem).Text, font, Brushes.Black, Rectangle);
                else if (Tag is RichTreeViewItemValue)
                {
                    var value = Tag as RichTreeViewItemValue;
                    if (value.Node.Values[value.Index] != null)
                        if (value.Node.Values[value.Index] is IList)
                        {
                            if (value.Node.Values[value.Index] != null)
                                e.Graphics.DrawString(((IList)(value.Node.Values[value.Index]))[0].ToString(), font, brush, Rectangle);
                        }
                        else
                            e.Graphics.DrawString(value.Node.Values[value.Index].ToString(), font, brush, Rectangle);
                }
                else if (Tag == null)
                    e.Graphics.DrawString(Text, font, brush, Rectangle);
            }
        }

        protected sealed class RichTreeViewCheckBox
        {
            private CheckBoxState _state;

            public Point Location;
            public object Tag;

            public bool Checked
            {
                get { return _state == CheckBoxState.CheckedNormal ? true : false; }
                set { _state = value == true ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal; }
            }

            public void DrawCheckBox(PaintEventArgs e)
                => CheckBoxRenderer.DrawCheckBox(e.Graphics, Location, _state);
        }

        protected class TableLine
        {
            public Point StartPoint;
            public Point EndPoint;

            public bool IsPointOnLine(Point point, int cushion)
                => (point.X >= StartPoint.X - cushion && point.X <= StartPoint.X + cushion);

            public void DrawLine(Pen pen, PaintEventArgs e)
                => e.Graphics.DrawLine(pen, StartPoint, EndPoint);
        }
        
        protected StringItem AddString(string text, Point location, object tag)
        {
            int width = 50;
            if (!string.IsNullOrEmpty(text))
                width = (text.Length * 9) > 50 ? text.Length * 9 : 50;

            StringItem item = new StringItem { Text = text, Tag = tag, Rectangle = new RectangleF(location, new Size(width, 15)) };
            _nodes.Add(item);

            Invalidate();

            if (location.X > _maxScrollPointX)
                _maxScrollPointX = location.X;
            if (location.Y > _maxScrollPointY)
                _maxScrollPointY = location.Y;

            return item;
        }

        protected RichTreeViewCheckBox AddCheckBox(Point location, object tag, bool check)
        {
            RichTreeViewCheckBox checkBox = new RichTreeViewCheckBox();
            checkBox.Location = location;
            checkBox.Checked = check;
            checkBox.Tag = tag;

            _checkBoxValues.Add(checkBox);
            return checkBox;
        }

        protected virtual void AddTextEditor(string text, Point location, object tag, Size size)
        {
            TextBox textBox = new TextBox();
            textBox.Text = text;
            textBox.Location = new Point(location.X, location.Y + 2);
            textBox.Tag = tag;
            textBox.Size = size;
            textBox.BorderStyle = BorderStyle.None;
            textBox.KeyDown += new KeyEventHandler(TextBox_KeyDown);

            base.Controls.Add(textBox);
        }

        protected virtual void AddListEditor(Point location, object tag, BindingSource bindingSource, int width)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Tag = tag;
            comboBox.Location = location;
            comboBox.DataSource = bindingSource.DataSource;
            comboBox.Width = width;
            comboBox.KeyDown += new KeyEventHandler(ComboBox_KeyDown);

            base.Controls.Add(comboBox);
        }
        
        private void DisplayTree()
        {
            foreach (RichTreeViewItem node in _rootNode.Children)
                RecursiveDisplayTree(node);
            
            DisplayColumns(_treeMaxOffset + 5);
            
            base.Invalidate();
        }

        private void Reset()
        {
            foreach (RichTreeViewItem node in _rootNode.Children)
                RemoveEvents(node);

            _nodes = new HashSet<StringItem>();
            _checkBoxValues = new List<RichTreeViewCheckBox>();
            _icons = new List<Image>();
            _columns = new List<RichTreeViewColumn>();
            _tableLines = new List<TableLine>();
            _location = new Point(_treeStartOffsetX, _treeStartOffsetY);
            _nodeOffsetY = 20;
        }

        private void RecursiveDisplayTree(RichTreeViewItem node)
        {
            if (!node.IsHidden)
            {
                StringItem StringItem = AddString(node.Text, _location, node);
                node.ItemsChanged += new EventHandler(NodesCollectionChanged);

                if (StringItem.Rectangle.Width + StringItem.Rectangle.Location.X > _treeMaxOffset)
                    _treeMaxOffset = (int)(StringItem.Rectangle.Width + StringItem.Rectangle.Location.X);

                if (node.Icon != null)
                {
                    var image = node.Icon;
                    image.Tag = new Point(_location.X - 15, _location.Y);
                    _icons.Add(image);
                }

                if (_checkBoxes)
                {
                    Point location;
                    if (node.Icon == null)
                        location = new Point(_location.X - 15, _location.Y + 1);
                    else
                        location = new Point(_location.X - 30, _location.Y + 1);

                    AddCheckBox(location, node, node.Checked);
                }

                if (node.Values != null)
                    CreateColumns(_location.Y, node);

                _location.Y += _nodeOffsetY;

                foreach (RichTreeViewItem item in node.Children)
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

        private void CreateColumns(int nodeOffsetY, RichTreeViewItem node)
        {
            for (int i = 0; i < node.Values.Length; i++)
            {
                if (i < _columns.Count)
                    _columns[i].Values.Add(new RichTreeViewItemValue() { Index = i, Node = node, NodeOffsetY = nodeOffsetY });
                else
                {
                    RichTreeViewColumn column = new RichTreeViewColumn();
                    column.Index = i;
                    column.Values.Add(new RichTreeViewItemValue() { Index = i, Node = node, NodeOffsetY = nodeOffsetY });
                    _columns.Add(column);
                }
            }
        }

        private void DisplayColumns(int leftOffset)
        {
            foreach (RichTreeViewColumn column in _columns)
            {
                int maxPointX = 0;
                
                foreach (RichTreeViewItemValue value in column.Values)
                    if (!value.Node.IsHidden)
                    {
                        var item = value.Node.Values[value.Index];

                        if (item is bool)
                        {
                            RichTreeViewCheckBox checkBox;
                            checkBox = AddCheckBox(new Point(leftOffset + 17, value.NodeOffsetY + 1), value, (bool)item);

                            if (checkBox.Location.X + 15 > maxPointX)
                                maxPointX = checkBox.Location.X + 15;
                        }
                        else
                        {
                            StringItem StringItem;
                            int index = _comboBoxSelectedIndex >= 0 ? _comboBoxSelectedIndex : 0;
                            StringItem = AddString(
                                item == null ? null : ((item is IList) ? (item as IList)[index].ToString() : item.ToString()),
                                new Point(leftOffset, value.NodeOffsetY),
                                value
                                );

                            if (StringItem.Rectangle.Location.X + StringItem.Rectangle.Width > maxPointX)
                                maxPointX = (int)(StringItem.Rectangle.Location.X + StringItem.Rectangle.Width);
                        }
                    }

                TableLine tableLine = new TableLine()
                {
                    StartPoint = new Point(leftOffset - 2, 0),
                    EndPoint = new Point(leftOffset - 2, base.Height)
                };

                _tableLines.Add(tableLine);

                leftOffset = maxPointX + 5;
            }

            base.Invalidate();
        }

        private void RemoveEvents(RichTreeViewItem node)
        {
            node.ItemsChanged -= new EventHandler(NodesCollectionChanged);

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

            if (hide)
                NodeHided?.Invoke(node, null);
            else
                NodeExpanded?.Invoke(node, null);
        }
    }
    
    public sealed class RichTreeViewItemValue
    {
        public int Index;
        public int NodeOffsetY;
        public RichTreeViewItem Node;
    }
    
    public class RichTreeViewColumn
    {
        public int Index;
        public List<RichTreeViewItemValue> Values;
        
        public RichTreeViewColumn()
        {
            Values = new List<RichTreeViewItemValue>();
        }
    }
}
