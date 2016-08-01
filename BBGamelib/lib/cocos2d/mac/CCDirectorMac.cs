#if UNITY_STANDALONE || UNITY_WEBGL
using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class CCDirectorMac : CCDirector
	{
		
		// Event Dispatcher
		CCEventDispatcher	_eventDispatcher;	


		public CCDirectorMac(){
			_eventDispatcher = new CCEventDispatcher ();
		}

		public new static CCDirectorMac sharedDirector{
			get{
				return CCDirector.sharedDirector as CCDirectorMac;
			}
		}

		public CCEventDispatcher eventDispatcher
		{
			get{ return _eventDispatcher; }
			set{
				if( value != _eventDispatcher ) {
					_eventDispatcher = value;
				}		
			}
		}	
		public Vector2 convertEventToGL(NSEvent evt)
		{
			Vector2 uiPoint = evt.mouseLocation;
			return uiPoint;
		}

		public override UIView view {
			get {
				return base.view;
			}
			set {	
				if( value != this.view) {
					base.view = value;
					((CCGLView)view).eventDelegate = _eventDispatcher;
					_eventDispatcher.dispatchEvents = true;
				}
			}
		}

		public bool isFullScreen{
			get{return Screen.fullScreen;}
			set{
				Screen.fullScreen = value;
			}
		}
	}
}
#endif
