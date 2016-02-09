using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class utNode<T>{
		public utNode<T>	prev, next;
		public T obj;
	}
	public class utList<T>
	{
		utNode<T> _head;

		public void DL_APPEND(utNode<T> add)
		{
			if (_head != null) {
				add.prev = _head.prev;
				_head.prev.next = add;
				_head.prev = add;
				add.next = null; 
			} else {
				_head = add;
				_head.prev = _head;
				_head.next = null;
			}
		}
	}
}

