using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class CCGLView : UIView
	{
		Rect _bounds;

		CCTouchDelegate _touchDelegate;
		public CCTouchDelegate touchDelegate{set{ _touchDelegate = value;}get{ return _touchDelegate;}}
		#if UNITY_STANDALONE || UNITY_WEBGL
		CCEventDelegate _eventDelegate;
		public CCEventDelegate eventDelegate{set{ _eventDelegate = value;}get{ return _eventDelegate;}}
		#endif

		#region singleton
		[SerializeField] [HideInInspector]private bool firstPassFlag=true;
		static CCGLView _Instance=null;
		//---------singleton------
		public static CCGLView Instance{
			get{
				return _Instance;
			}
		}
		public virtual void Awake() {
			if (Application.isPlaying) {
				if (_Instance != null && _Instance != this) {
					Destroy (this.gameObject);
					return;
				} else {
					_Instance = this;
				}
			} 
			if (firstPassFlag) {
				gameObject.transform.position =Vector3.zero;
				gameObject.name = "CCGLView";
				firstPassFlag = false;
			}
		}
		#endregion

		#region propreties
		public void setFrame(Rect bounds){
			_bounds = bounds;		
		}
		
		public Rect bounds{
			get{return _bounds;}
		}

		#endregion

		#region touch

		public override void touchesBegan(HashSet<UITouch> touches){
			if (_touchDelegate != null)
				_touchDelegate.touchesBegan (touches);
		}
		public override void touchesMoved(HashSet<UITouch> touches){
			if (_touchDelegate != null)
				_touchDelegate.touchesMoved (touches);
		}
		public override void touchesEnded(HashSet<UITouch> touches){
			if (_touchDelegate != null)
				_touchDelegate.touchesEnded (touches);
		}
		public override void touchesCancelled(HashSet<UITouch> touches){
			if (_touchDelegate != null)
				_touchDelegate.touchesCancelled (touches);
		}
		#if UNITY_STANDALONE || UNITY_WEBGL
		// Mouse
		public override void mouseDown(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.mouseDown(theEvent);
		}
		public override void mouseUp(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.mouseUp (theEvent);
		}
		public override void mouseMoved(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.mouseMoved (theEvent);
		}
		public override void mouseDragged(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.mouseDragged (theEvent);
		}
		public override void rightMouseDown(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.rightMouseDown (theEvent);
		}
		public override void rightMouseDragged(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.rightMouseDragged (theEvent);
		}
		public override void rightMouseUp(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.rightMouseUp (theEvent);
		}
		public override void otherMouseDown(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.otherMouseDown (theEvent);
		}
		public override void otherMouseDragged(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.otherMouseDragged (theEvent);
		}
		public override void otherMouseUp(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.otherMouseUp (theEvent);
		}
		public override void scrollWheel(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.scrollWheel (theEvent);
		}
		public override void mouseEntered(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.mouseEntered (theEvent);
		}
		public override void mouseExited(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.mouseExited (theEvent);
		}
		
		
		// Keyboard
		public override void keyDown(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.keyDown (theEvent);
		}
		public override void keyUp(NSEvent theEvent){
			if (_eventDelegate != null)
				_eventDelegate.keyUp (theEvent);
		}
		#endif

		#endregion


	}
}

