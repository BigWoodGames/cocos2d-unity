using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	public abstract class UIView : MonoBehaviour
	{
		// do so is very likely to lead to incorrect behavior or crashes.
		public abstract void touchesBegan(HashSet<UITouch> touches);
		public abstract void touchesMoved(HashSet<UITouch> touches);
		public abstract void touchesEnded(HashSet<UITouch> touches);
		public abstract void touchesCancelled(HashSet<UITouch> touches);
	}
}
