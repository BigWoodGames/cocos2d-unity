
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	public interface CCTouchDelegate
	{
		void touchesBegan(HashSet<UITouch> touches);
		void touchesMoved(HashSet<UITouch> touches);
		void touchesEnded(HashSet<UITouch> touches);
		void touchesCancelled(HashSet<UITouch> touches);
	}
}