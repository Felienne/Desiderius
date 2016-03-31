using System.Collections;
using System.Collections.Generic;

namespace Sodes.Base
{
	public class PriorityQueue<T> : IEnumerable<T>
	{
		public PriorityQueue()
		{
		}
		public PriorityQueue(IComparer<T> icomparer)
		{
			specialComparer = icomparer;
		}


		private List<T> internalQueue = new List<T>();
		protected IComparer<T> specialComparer = null;
		//protected List<T> InternalQueue
		//{
		//  get
		//  {
		//    return internalQueue;
		//  }
		//}

		public int Count
		{
			get
			{
				return (internalQueue.Count);
			}
		}

		public void Clear()
		{
			internalQueue.Clear();
		}

		public object Clone()
		{
			// Make a new PQ and give it the same comparer.
			PriorityQueue<T> newPQ = new PriorityQueue<T>(specialComparer);
			newPQ.CopyTo(internalQueue.ToArray(), 0);
			return newPQ;
		}

		public int IndexOf(T item)
		{
			return (internalQueue.IndexOf(item));
		}

		public bool Contains(T item)
		{
			return (internalQueue.Contains(item));
		}

		public int BinarySearch(T item)
		{
			return (internalQueue.BinarySearch(item, specialComparer));
		}

		public bool Contains(T item, IComparer<T> comparer)
		{
			return (internalQueue.BinarySearch(item, comparer) >= 0);
		}

		public void CopyTo(T[] array, int index)
		{
			internalQueue.CopyTo(array, index);
		}

		public T[] ToArray()
		{
			return (internalQueue.ToArray());
		}

		public void TrimToSizeTrimExcess()
		{
			internalQueue.TrimExcess();
		}

		public void Enqueue(T item)
		{
			internalQueue.Add(item);
			internalQueue.Sort(specialComparer);
		}

		public T Dequeue()
		{
			T item = internalQueue[internalQueue.Count - 1];
			internalQueue.RemoveAt(internalQueue.Count - 1);

			return (item);
		}

		public T Peek()
		{
			return (internalQueue[internalQueue.Count - 1]);
		}

		public IEnumerator GetEnumerator()
		{
			return (internalQueue.GetEnumerator());
		}

		IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
		{
			return (internalQueue.GetEnumerator());
		}
	}
}