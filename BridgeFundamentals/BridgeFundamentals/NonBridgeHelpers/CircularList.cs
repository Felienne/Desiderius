using System;
using System.Collections.Generic;

namespace Sodes.Base
{
    /// <summary>
    /// Mix of a List and a Stack with a maximum size
    /// When item [size + 1] is added, the first item is dropped
    /// </summary>
    public class CircularList<ItemType>
    {
        private ItemType[] list;
        private int firstItem;
        private int listSize;
        private int count;

        public CircularList(int size)
        {
            if (size == 0) 
                throw new ArgumentOutOfRangeException("size", "CircularList cannot be 0 long");
            this.listSize = size;
            this.list = new ItemType[size];
            this.firstItem = -1;
            this.count = 0;
        }

        public ItemType this[int item]
        {
            get
            {
                if (item >= this.count || item < 0) 
                    throw new InvalidOperationException(string.Format("Item {0} not valid; only {1} items in list", item, this.count));
                return list[(this.firstItem - item + this.listSize) % this.listSize];
            }
        }

        public void Add(ItemType item)
        {
            this.firstItem++;
            if (this.firstItem == this.listSize) this.firstItem = 0;
            this.list[this.firstItem] = item;
            if (this.count < this.listSize) this.count++;
        }

        public void ClonePlanTo(CircularList<ItemType> copy)
        {
            if (copy == null) throw new ArgumentNullException("copy");
            for (int i = this.Length - 1; i >= 0; i--)
            {
                copy.Add(this[i]);
            }
        }

        public int Length { get { return this.count; } }
    }
}
