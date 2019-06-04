using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ccontrol
{
    public partial class RichTreeView : ScrollableControl
    {
        private RichTreeViewItem _root = new RichTreeViewItem();
        private RichTreeViewColumnCollection _columns = new RichTreeViewColumnCollection();

        private Point _richTreeViewLocation;

        private const int _treeNodesOffsetX = 20;
        private int _treeNodeMaxOffsetX = 0;

        public RichTreeViewItem Root => _root;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(RichTreeViewColumnCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public RichTreeViewColumnCollection Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }
        
        public RichTreeView()
        {
            InitializeComponent();

            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.AutoScroll = true;

            _columns.OnColumnAdded += (sender, e) =>
            {
                ((RichTreeViewColumn)sender).WidthChanged += Column_WidthChanged;
                this.Invalidate();
            };
            _columns.OnColumnRemoved += (sender, e) => { this.Invalidate(); };

            Reset();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);
            base.OnPaint(e);
            int offset = _treeNodeMaxOffsetX;
            
            foreach (RichTreeViewColumn column in _columns)
            {
                column.OffsetX = offset;
                SizeF size = e.Graphics.MeasureString(column.Name, this.Font);

                size.Width = column.Width;

                if (column.Width <= 0)
                    size.Width = 1;

                column.DrawColumnItem = DrawString;
                column.CreateColumnEditor = EditItem;

                column.DrawColumnItem?.Invoke(column.Name, e, new RectangleF(new Point(offset, 5), size));

                e.Graphics.DrawLine(Pens.Black, new Point(column.OffsetX - 1, 0), new Point(column.OffsetX - 1, this.Height));
                offset += column.Width;
            }

            foreach (var child in _root.Children)
                DisplayRichTreeView(child, e);
            
            Reset();
        }

        protected virtual void Column_WidthChanged(object sender, EventArgs e) => this.Invalidate();

        private void Reset()
        {
                if (_columns.Count > 0)
                {
                    if (!string.IsNullOrEmpty(_columns[0].Name))
                    {
                        int height = TextRenderer.MeasureText(_columns[0].Name.ToString(), this.Font).Height;
                        _richTreeViewLocation = new Point(5, height + 10);
                    }
                }
                else
                    _richTreeViewLocation = new Point(5, 30);
        }

        private void DisplayRichTreeView(RichTreeViewItem node, PaintEventArgs e)
        {
            if (node.Icon != null)
            {
                e.Graphics.DrawImage(node.Icon, new Rectangle(_richTreeViewLocation, new Size(14, 14)));
                _richTreeViewLocation.X += 20;
            }
            
            node.ItemsChanged += (sender, eventArgs) => { this.Invalidate(); };
            node.VisibilityChanged += (sender, eventArgs) => { this.Invalidate(); }; 

            Size stringSize = TextRenderer.MeasureText(node.Text, this.Font);
            DrawString(node.Text, e, new Rectangle(_richTreeViewLocation, stringSize));

            int offset = _richTreeViewLocation.X + stringSize.Width;
            if (offset > _treeNodeMaxOffsetX)
            {
                _treeNodeMaxOffsetX = offset;
                this.Invalidate();
            }

            node.Location = _richTreeViewLocation;

            if (node.Values != null)
            {
                int valueOffset = _treeNodeMaxOffsetX;

                for (int i = 0; i < node.Values.Length; i++)
                {
                    string item = "";
                    if (node.Values[i] != null)
                    {
                        if (node.Values[i] is IList)
                        {
                            var currItem = ((IList)node.Values[i]);

                            if (currItem.Count > 0)
                            {
                                if (node.SelectedItems != null)
                                {
                                    foreach (var selectedItem in node.SelectedItems)
                                        if (selectedItem.Key == i)
                                            item = currItem[selectedItem.Value].ToString();
                                }
                                else
                                {
                                    item = currItem[0].ToString();
                                    node.SelectedItems = new System.Collections.Generic.Dictionary<int, int>();
                                    node.SelectedItems.Add(i, 0);
                                }
                            }
                            else
                                item = "Empty list";
                        }
                        else
                            item = node.Values[i].ToString();
                    }

                    SizeF size = e.Graphics.MeasureString(item, this.Font);
                    if (_columns.Count > i)
                    {
                        size.Width = _columns[i].Width <= 0 ? 1 : _columns[i].Width;
                        _columns[i].DrawColumnItem?.Invoke(item, e, new RectangleF(valueOffset, _richTreeViewLocation.Y, size.Width, size.Height));
                        valueOffset += _columns[i].Width;
                    }
                }
            }

            int nodesOffsetY = TextRenderer.MeasureText(node.Text, this.Font).Height + 5;
            _richTreeViewLocation.Y += nodesOffsetY;

            _richTreeViewLocation.X += _treeNodesOffsetX;

            if (!node.IsHidden)
                foreach (var subnode in node.Children)
                    DisplayRichTreeView(subnode, e);

            _richTreeViewLocation.X -= _treeNodesOffsetX;
        }

        private void DrawString(string text, PaintEventArgs e, RectangleF bounds) =>
            e.Graphics.DrawString(text, this.Font, Brushes.Black, bounds);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            IsNodeContains(Root, e.Location);
        }

        private void IsNodeContains(RichTreeViewItem node, Point mouseLocation)
        {
            if (node.Values != null)
                for (int i = 0; i < _columns.Count; i++)
                    if (node.Values.Length > i)
                        if (node.Values[i] != null)
                        {
                            var size = TextRenderer.MeasureText(node.Values[i].ToString(), this.Font);
                            size.Width = _columns[i].Width - 1;

                            var rect = new Rectangle(new Point(_columns[i].OffsetX, node.Location.Y), size);
                            if (rect.Contains(mouseLocation))
                            {
                                if (rect.Width > _columns[i].Width)
                                    rect.Width = _columns[i].Width;
                                _columns[i]?.CreateColumnEditor.Invoke(node, i, rect);
                            }
                        }

            foreach (RichTreeViewItem subnode in node.Children)
                IsNodeContains(subnode, mouseLocation);
        }

        private void EditItem(RichTreeViewItem node, int index, Rectangle bounds)
        {
            var item = node.Values[index];
            if (item is string)
            {
                var editor = new RichTreeViewTextEditor(node, index);
                editor.ValueUpdated += EditorValueUpdated;
                editor.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                this.Controls.Add(editor);
            }
            if (item is bool)
            {
                var editor = new RichTreeViewBoolEditor(node, index);
                editor.ValueUpdated += EditorValueUpdated;
                editor.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                this.Controls.Add(editor);
            }
            if (item is IList)
            {
                if (((IList)item).Count > 0)
                {
                    var editor = new RichTreeViewListEditor(node, index);
                    editor.ValueUpdated += EditorValueUpdated;
                    editor.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                    this.Controls.Add(editor);
                }
            }
        }

        private void EditorValueUpdated(object sender, EventArgs e)
        {
            if (sender != null)
                (sender as Control).Dispose();
            this.Controls.Clear();
        }
    }
}
