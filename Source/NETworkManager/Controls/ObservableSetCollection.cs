using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace NETworkManager.Controls
{
    public class ObservableSetCollection<T> : ObservableCollection<T>
    { 
        public ObservableSetCollection()
        {

        }

        public ObservableSetCollection(List<T> list) :  base(list)
        {

        }

        public ObservableSetCollection(IEnumerable<T> collection) : base(collection)
        {

        }

        protected override void InsertItem(int index, T item)
        {
            if (Contains(item))
                return; // Item already exists

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, T item)
        {
            int i = IndexOf(item);

            if (i >= 0 && i != index)
                return; // Item already exists

            base.SetItem(index, item);
        }
    }
}