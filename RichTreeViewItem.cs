using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace custcontrol
{
    public class RichTreeViewItem
    {
        private string _text;
        private Image _itemIcon;
        private ObservableCollection<RichTreeViewItem> _childNodes;
        private object[] _values;

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

        public ObservableCollection<RichTreeViewItem> Children
        {
            get { return _childNodes; }

            set { _childNodes = value; }
        }
        
        public void Add(string itemName) 
            => _childNodes.Add(new RichTreeViewItem { _text = itemName });

        public void Add(string itemName, Image icon) 
            => _childNodes.Add(new RichTreeViewItem { _text = itemName, _itemIcon = icon });

        public void Add(string itemName, object[] values)
            => _childNodes.Add(new RichTreeViewItem { _text = itemName, _values = values });

        public void Add(string itemName, Image icon, object[] values)
            => _childNodes.Add(new RichTreeViewItem { _text = itemName, _itemIcon = icon, _values = values});

        public void Remove(int index) 
            => _childNodes.RemoveAt(index);

        public void Edit(int index, string itemText)
        {
            _childNodes.ElementAt(index)._text = itemText;
            ChildNodes_CollectionChanged(_childNodes.ElementAt(index), null);
        }

        public event EventHandler ItemsChanged;

        private void ChildNodes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            EventHandler handler = ItemsChanged;

            if (handler != null)
                if (e == null)
                    handler(sender, e);
                else 
                if (e.Action == NotifyCollectionChangedAction.Add)
                    handler((e.NewItems[0] as RichTreeViewItem), e);
                else 
                if (e.Action == NotifyCollectionChangedAction.Remove)
                    handler((e.OldItems[0] as RichTreeViewItem), e);
        }
    }
}
