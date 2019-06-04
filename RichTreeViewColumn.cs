using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ccontrol
{
    public class RichTreeViewColumnCollection : CollectionBase
    {
        public event EventHandler OnColumnAdded;
        public event EventHandler OnColumnRemoved;

        public RichTreeViewColumnCollection() { }

        public RichTreeViewColumn this[int index] => (RichTreeViewColumn)List[index];
        
        public void Add(RichTreeViewColumn item) => List.Add(item);

        public void Add(string value)
        {
            var column = new RichTreeViewColumn() { Name = value };
            List.Add(column);
            OnColumnAdded?.Invoke(column, EventArgs.Empty);
        }

        public void Add(string value, int width)
        {
            var column = new RichTreeViewColumn() { Name = value, Width = width };
            List.Add(column);
            OnColumnAdded?.Invoke(column, EventArgs.Empty);
        }

        public void Remove(RichTreeViewColumn item)
        {
            List.Remove(item);
            OnColumnRemoved?.Invoke(this, EventArgs.Empty);
        }
        public new void RemoveAt(int index)
        {
            List.RemoveAt(index);
            OnColumnRemoved?.Invoke(this, EventArgs.Empty);
        }

        public bool Contains(RichTreeViewColumn value) => List.Contains(value);
    }

    public class RichTreeViewColumn
    {
        private int _width;
        private string _columnName;
        private int _offsetX;

        public event EventHandler WidthChanged;

        public DrawItem DrawColumnItem;
        public CreateEditControl CreateColumnEditor;

        [Browsable(true)]
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value < 0 ? 0 : value;
                WidthChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        [Browsable(true)]
        public string Name
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        [Browsable(false)]
        public int OffsetX
        {
            get { return _offsetX; }
            set { _offsetX = value; }
        }

        public delegate void DrawItem(string text, PaintEventArgs e, RectangleF bounds);
        public delegate void CreateEditControl(RichTreeViewItem node, int index, Rectangle bounds);
    }
}
