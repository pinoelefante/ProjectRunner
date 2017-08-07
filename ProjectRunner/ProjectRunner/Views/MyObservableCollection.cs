using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MyObservableCollection<T> : ObservableCollection<T>
{
    public void AddRange(IEnumerable<T> list, bool clearBefore = false)
    {
        if (clearBefore)
            this.Clear();

        if (list != null && list.Any())
        {
            foreach (var item in list)
                this.Add(item);
        }
    }
    public int Remove(IEnumerable<T> list)
    {
        if (list == null)
            return 0;
        else
        {
            int removed = 0;
            foreach (var item in list)
                removed += Remove(item) ? 1 : 0;
            return removed;
        }
    }
    public new bool Remove(T o)
    {
        return o == null ? false : base.Remove(o);
    }
}

