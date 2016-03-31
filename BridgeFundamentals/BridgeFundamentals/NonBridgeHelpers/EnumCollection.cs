using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sodes.Base
{
    [DebuggerDisplay("{values}")]
    public class EnumCollection<E, T>
    {
        private Dictionary<E, T> values = new Dictionary<E, T>();
        private E[] enumValues;

        public EnumCollection()
        {
            Type enumType = typeof(E);

            //if (!enumType.IsEnum)
            //    throw new ArgumentException("Not enum type.");

            this.enumValues = (E[])Enum.GetValues(enumType);


            foreach (var ev in this.enumValues)
            {
                this[ev] = default(T);
            }
        }

        //public EnumCollection(T[] initialValue)
        //{
        //    foreach (var ev in this.enumValues)
        //    {
        //        //this[ev] = initialValue[(int)ev];
        //    }
        //}

        public T this[E index]
        {
            get
            {
                return values[index];
            }
            set
            {
                values[index] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var s in this.enumValues)
                if (this[s] != null)
                    yield return this[s];
        }

        public void ForEach(Action<E> toDo)
        {
            foreach (var ev in this.enumValues)
            {
                toDo(ev);
            }
        }
    }
}
