using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;

namespace ccontrol
{
    public class RichTreeViewItem
    {
        private ObservableCollection<RichTreeViewItem> _childNodes;

        private string _text;
        private bool _hidden;
        private object[] _values;
        private Image _itemIcon;
        private Point _position;

        private Dictionary<int, int> _selectedItems;

        private EventHandler _itemsChanged;
        private EventHandler _visibilityChanged;

        public RichTreeViewItem()
        {
            _childNodes = new ObservableCollection<RichTreeViewItem>();
            _childNodes.CollectionChanged += ChildNodes_CollectionChanged;
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public Image Icon
        {
            get { return _itemIcon; }
            set { _itemIcon = value; }
        }

        public object[] Values
        {
            get { return _values; }
            set { _values = value; }
        }

        public Dictionary<int, int> SelectedItems
        {
            get { return _selectedItems; }
            set { _selectedItems = value; }
        }

        public ObservableCollection<RichTreeViewItem> Children
        {
            get { return _childNodes; }
            set { _childNodes = value; }
        }

        public Point Location
        {
            get { return _position; }
            set { _position = value; }
        }

        public bool IsHidden
        {
            get { return _hidden; }
            set
            {
                _hidden = value;
                _visibilityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ItemsChanged
        {
            add
            {
                if (_itemsChanged == null || !_itemsChanged.GetInvocationList().Contains(value))
                    _itemsChanged += value;
            }
            remove { _itemsChanged -= value; }
        }

        public event EventHandler VisibilityChanged
        {
            add
            {
                if (_visibilityChanged == null || !_visibilityChanged.GetInvocationList().Contains(value))
                    _visibilityChanged += value;
            }
            remove { _visibilityChanged -= value; }
        }

        public bool CanExpand => (_childNodes.Count > 0);

        public void Add(string itemName)
            => _childNodes.Add(new RichTreeViewItem { _text = itemName });

        public void Add(string itemName, Image icon)
            => _childNodes.Add(new RichTreeViewItem { _text = itemName, _itemIcon = icon });

        public void Add(string itemName, object[] values)
            => _childNodes.Add(new RichTreeViewItem { _text = itemName, _values = values });

        public void Add(string itemName, Image icon, object[] values)
            => _childNodes.Add(new RichTreeViewItem { _text = itemName, _itemIcon = icon, _values = values });

        public void Remove(int index)
            => _childNodes.RemoveAt(index);

        public void Edit(int index, string itemText)
        {
            _childNodes.ElementAt(index)._text = itemText;
            ChildNodes_CollectionChanged(_childNodes.ElementAt(index), null);
        }

        private void ChildNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e == null)
                _itemsChanged?.Invoke(sender, e);
            else
            if (e.Action == NotifyCollectionChangedAction.Add)
                _itemsChanged?.Invoke((e.NewItems[0] as RichTreeViewItem), e);
            else
            if (e.Action == NotifyCollectionChangedAction.Remove)
                _itemsChanged?.Invoke((e.OldItems[0] as RichTreeViewItem), e);
        }
    }
}
