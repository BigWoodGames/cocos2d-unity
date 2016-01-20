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

	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class CCGLView : UIView
	{
		Rect _bounds;
		CCTouchDelegate _touchDelegate;

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
				DontDestroyOnLoad (this.gameObject);
			} 
			if (firstPassFlag) {
				gameObject.transform.position = Vector3.zero;
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

		public CCTouchDelegate touchDelegate{
			set{ _touchDelegate = value;}
			get{ return _touchDelegate;}
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
		#endregion


	}
}

