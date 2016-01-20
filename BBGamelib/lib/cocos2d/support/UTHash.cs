using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	public class UTHash<K, T> : Dictionary<K, T> where T : class
	{
		public void HASH_DEL(K key){
			Remove (key);
		}
		
		public T HASH_FIND_INT(K key){
			T value;
			if (TryGetValue (key, out value))
				return value;
			else
				return null;
		}

		public void HASH_ADD_INT(K key, T value){
			Add (key, value);
		}
	}
}

