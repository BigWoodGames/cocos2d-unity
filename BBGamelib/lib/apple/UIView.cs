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

		#if UNITY_STANDALONE || UNITY_WEBGL
		// Mouse
		public abstract void mouseDown(NSEvent theEvent);
		public abstract void mouseUp(NSEvent theEvent);
		public abstract void mouseMoved(NSEvent theEvent);
		public abstract void mouseDragged(NSEvent theEvent);
		public abstract void rightMouseDown(NSEvent theEvent);
		public abstract void rightMouseDragged(NSEvent theEvent);
		public abstract void rightMouseUp(NSEvent theEvent);
		public abstract void otherMouseDown(NSEvent theEvent);
		public abstract void otherMouseDragged(NSEvent theEvent);
		public abstract void otherMouseUp(NSEvent theEvent);
		public abstract void scrollWheel(NSEvent theEvent);
		public abstract void mouseEntered(NSEvent theEvent);
		public abstract void mouseExited(NSEvent theEvent);
		
		
		// Keyboard
		public abstract void keyDown(NSEvent theEvent);
		public abstract void keyUp(NSEvent theEvent);
		#endif
	}
}
