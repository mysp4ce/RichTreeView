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
        private ObservableCollection<RichTreeViewItem> _childNodes;

        private string _text;
        private bool _checked;
        private Image _itemIcon;
        private object[] _values;
        private bool _hidden;

        public event EventHandler ItemsChanged;

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

        public bool Checked
        {
            get { return _checked; }

            set { _checked = value; }
        }

        public bool IsHidden
        {
            get { return _hidden; }
            
            set { _hidden = value; }
        }

        public bool CanExpand => (_childNodes.Count > 0);
        
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
