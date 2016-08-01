using UnityEngine;
using System.Collections;
using System;

namespace BBGamelib{
	public interface CCEventDelegate
	{			
		// Mouse
		void mouseDown(NSEvent theEvent);
		void mouseUp(NSEvent theEvent);
		void mouseMoved(NSEvent theEvent);
		void mouseDragged(NSEvent theEvent);
		void rightMouseDown(NSEvent theEvent);
		void rightMouseDragged(NSEvent theEvent);
		void rightMouseUp(NSEvent theEvent);
		void otherMouseDown(NSEvent theEvent);
		void otherMouseDragged(NSEvent theEvent);
		void otherMouseUp(NSEvent theEvent);
		void scrollWheel(NSEvent theEvent);
		void mouseEntered(NSEvent theEvent);
		void mouseExited(NSEvent theEvent);
		// Keyboard
		void keyDown(NSEvent theEvent);
		void keyUp(NSEvent theEvent);
	}
	public interface CCMouseEventDelegate{
		bool ccMouseDown(NSEvent theEvent);
		bool ccMouseUp(NSEvent theEvent);
		bool ccMouseMoved(NSEvent theEvent);
		bool ccMouseDragged(NSEvent theEvent);
		bool ccRightMouseDown(NSEvent theEvent);
		bool ccRightMouseDragged(NSEvent theEvent);
		bool ccRightMouseUp(NSEvent theEvent);
		bool ccOtherMouseDown(NSEvent theEvent);
		bool ccOtherMouseDragged(NSEvent theEvent);
		bool ccOtherMouseUp(NSEvent theEvent);
		bool ccScrollWheel(NSEvent theEvent);
		bool ccMouseEntered(NSEvent theEvent);
		bool ccMouseExited(NSEvent theEvent);
	}
	
	public interface CCKeyboardEventDelegate{
		bool ccKeyDown(NSEvent theEvent);
		bool ccKeyUp(NSEvent theEvent);
	}
	
	#region mark - CCEventObject
	public class CCEventObject{
		public NSEvent evt;
		public Action<NSEvent> selector;
	}
	#endregion
}

