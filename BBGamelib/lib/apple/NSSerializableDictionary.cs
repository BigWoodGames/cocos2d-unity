using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace BBGamelib{
	[Serializable]
	public class NSSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
	{
		[SerializeField]
		private List<TKey> keys = new List<TKey>();
		
		[SerializeField]
		private List<TValue> values = new List<TValue>();
		
		// save the dictionary to lists
		public void OnBeforeSerialize()
		{
			keys.Clear();
			values.Clear();

			var enumerator = this.GetEnumerator();
			while (enumerator.MoveNext()) {
				KeyValuePair<TKey, TValue> pair = enumerator.Current;
				keys.Add(pair.Key);
				values.Add(pair.Value);
			}
		}
		
		// load dictionary from lists
		public void OnAfterDeserialize()
		{
			this.Clear();

			NSUtils.Assert (keys.Count == values.Count, 
			                "there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable.", 
			                keys.Count,
			                values.Count
			                );
			for(int i = 0; i < keys.Count; i++)
				this.Add(keys[i], values[i]);
		}
	}
}