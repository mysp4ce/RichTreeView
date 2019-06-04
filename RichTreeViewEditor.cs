using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace ccontrol
{
    public interface IRichTreeViewEditor
    {
        event EventHandler ValueUpdated;

        RichTreeViewItem Node { get; }
        int Index { get; }
        
        void UpdateValue(object newValue);
    }

    public class RichTreeViewTextEditor : TextBox, IRichTreeViewEditor
    {
        private RichTreeViewItem _node;
        private int _index;
        public RichTreeViewItem Node => _node;
        public int Index => _index;

        public event EventHandler ValueUpdated;

        public RichTreeViewTextEditor(RichTreeViewItem node, int valueIndex)
        {
            _node = node;
            _index = valueIndex;
        }

        public void UpdateValue(object value)
        {
            if (_node == null)
                throw new ArgumentNullException("Node is null");
            if (_index < 0)
                throw new IndexOutOfRangeException("Index < 0");
            _node.Values[_index] = value;
            ValueUpdated?.Invoke(this, EventArgs.Empty);
        }
        
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == (char)Keys.Enter)
                if (!string.IsNullOrEmpty(this.Text))
                {
                    UpdateValue(this.Text);
                }
            if (e.KeyChar == (char)Keys.Escape)
                this.Dispose();
        }
    }

    public class RichTreeViewBoolEditor : CheckBox, IRichTreeViewEditor
    {
        private RichTreeViewItem _node;
        private int _index;
        public RichTreeViewItem Node => _node;
        public int Index => _index;

        public event EventHandler ValueUpdated;

        public RichTreeViewBoolEditor(RichTreeViewItem node, int index)
        {
            _node = node;
            _index = index;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            Checked = (bool)_node.Values[_index];
        }

        public void UpdateValue(object value)
        {
            _node.Values[_index] = value;
            ValueUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            base.OnCheckedChanged(e);
            UpdateValue(this.Checked);
        }
    }

    public class RichTreeViewListEditor : ComboBox, IRichTreeViewEditor
    {
        private RichTreeViewItem _node;
        private int _index;
        public RichTreeViewItem Node => _node;
        public int Index => _index;

        public event EventHandler ValueUpdated;

        public RichTreeViewListEditor(RichTreeViewItem node, int index)
        {
            _node = node;
            _index = index;
            this.DataSource = _node.Values[_index];
        }

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            base.OnSelectedValueChanged(e);
            var currItem = (IList)_node.Values[_index];
            foreach (var selectedItem in _node.SelectedItems)
                if (selectedItem.Key == _index)
                {
                    currItem[selectedItem.Value] = this.SelectedValue;
                }
        }

        public void UpdateValue(object newValue)
        {
            if (!((IList)_node.Values[_index]).Contains(newValue))
                ((IList)_node.Values[_index]).Add(newValue);
            ValueUpdated?.Invoke(this, EventArgs.Empty);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == (char)Keys.Enter)
                if (!string.IsNullOrEmpty(this.Text))
                {
                    if (!((IList)_node.Values[_index]).Contains(this.Text))
                        UpdateValue(this.Text);
                    this.Dispose();
                }
            if (e.KeyChar == (char)Keys.Escape)
                this.Dispose();
        }
    }

    public class RichTreeViewColumnCollectionEditor : CollectionEditor
    {
        public RichTreeViewColumnCollectionEditor(Type type) : base(type)
        { }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            object result = base.EditValue(context, provider, value);
            ((RichTreeView)context.Instance).Columns = (RichTreeViewColumnCollection)result;
            return result;
        }
    }
}
