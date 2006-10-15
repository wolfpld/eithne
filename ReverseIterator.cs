using System;
using System.Collections;

namespace Eithne
{
	public class ReverseIterator : IEnumerable
	{
		IEnumerable _enumerable;

		public ReverseIterator(IEnumerable enumerable)
		{
			_enumerable = enumerable;
		}

		public IEnumerator GetEnumerator()
		{
			return new ReverseIteratorEnumerator(_enumerable.GetEnumerator());
		}

		internal class ReverseIteratorEnumerator : IEnumerator
		{
			private ArrayList _list;
			private int       _index;

			internal ReverseIteratorEnumerator( IEnumerator enumerator )
			{
				_list = new ArrayList();

				while(enumerator.MoveNext())
				{
					_list.Add( enumerator.Current );
				}

				_index = _list.Count;
			}

			public void Reset()
			{
				_index = _list.Count;
			}

			public object Current
			{
				get
				{
					if((_index < 0) || (_index == _list.Count))
						throw new InvalidOperationException();
					return _list[_index];
				}
			}

			public bool MoveNext()
			{
				if(_index >= 0)
					--_index;

				return _index >= 0;
			}
		}
	}
}
