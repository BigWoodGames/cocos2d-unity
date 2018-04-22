using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	/*
	 * A fake utHash used to replace the real utHash in cocos2d-objc.
	 */
	public class utHash<K, T> : Dictionary<K, T>
	{
		public void HASH_DEL(K key){
			Remove (key);
		}
		
		public T HASH_FIND_INT(K key){
			T value;
			if (TryGetValue (key, out value))
				return value;
			else {
				return default(T);
			}
		}

		public void HASH_ADD_INT(K key, T value){
			Remove (key);
			Add (key, value);
		}

		public new T this[K key]{
			get{
				return HASH_FIND_INT (key);
			}
			set{
				HASH_ADD_INT (key, value);
			}
		}
	}
}

