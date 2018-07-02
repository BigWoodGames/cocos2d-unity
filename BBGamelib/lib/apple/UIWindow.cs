using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BBGamelib{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class UIWindow : MonoBehaviour
	{
		public const float PIXEL_PER_UNIT = 100.0f;
		
		Rect _bounds;
		[SerializeField] UIViewController _rootViewController;

		#region singleton
		[SerializeField] [HideInInspector]private bool firstPassFlag=true;
		static UIWindow _Instance=null;
		//---------singleton------
		public static UIWindow Instance{
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
				gameObject.name = "UIWindow";
				firstPassFlag = false;
			}
		}
		#endregion


		#region properties
		public void setResolutionHeight(float hInPixels){
			float wInPixels = hInPixels * Camera.main.aspect;
			float h = hInPixels / PIXEL_PER_UNIT;
			float w = wInPixels / PIXEL_PER_UNIT;
			Rect boundsInPixels = new Rect (-wInPixels / 2, -hInPixels / 2, wInPixels, hInPixels);
			_bounds = boundsInPixels;

			Camera.main.orthographicSize = h / 2;
			Vector3 cameraPos = Camera.main.transform.position;
			cameraPos.x = w / 2;
			cameraPos.y = h / 2;
			Camera.main.transform.position = cameraPos;
		}
		
		public Rect bounds {
			get{return _bounds;}
		}

		public UIViewController rootViewController{
			set{_rootViewController = value;}
			get{ return _rootViewController;}
		}
		#endregion

		#region touch
		
		HashSet<UITouch> touchesBegan = new HashSet<UITouch>();
		HashSet<UITouch> touchesMoved = new HashSet<UITouch>();
		HashSet<UITouch> touchesEnded = new HashSet<UITouch>();
		HashSet<UITouch> touchesCancelled = new HashSet<UITouch>();
		public void OnUpdate(CADisplayLink sender) {
			bool hasTouchesBegan = false;
			bool hasTouchesMoved = false;
			bool hasTouchesEnded = false;
			bool hasTouchesCancelled = false;
			int touchCount = Input.touchCount;
			if (touchCount > 0) {
				int count = Input.touches.Length;
				for (int i=0; i<count; i++) {
					Touch touch = Input.touches [i];
					UITouch uiTouch = new UITouch ();
					uiTouch.fingerId = touch.fingerId;
					uiTouch.phase = touch.phase;
                    Vector3 p = touch.position;
					p.z = -Camera.main.transform.position.z;
					uiTouch.location = Camera.main.ScreenToWorldPoint(p) * PIXEL_PER_UNIT;
					uiTouch.tapCount = touch.tapCount;
					uiTouch.timestamp = DateTime.Now;
					if (touch.phase == TouchPhase.Began) {
						touchesBegan.Add (uiTouch);
						hasTouchesBegan = true;
					} else if (touch.phase == TouchPhase.Moved) {
						touchesMoved.Add (uiTouch);
						hasTouchesMoved = true;
					} else if (touch.phase == TouchPhase.Ended) {
						touchesEnded.Add (uiTouch);
						hasTouchesEnded = true;
					} else if (touch.phase == TouchPhase.Canceled) {
						touchesCancelled.Add (uiTouch);
						hasTouchesCancelled = true;
					}
				} 
			} else {
				#if UNITY_EDITOR
				#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_WP8_1
				if(Input.GetMouseButtonDown(0)){
					UITouch uiTouch = new UITouch();
					uiTouch.fingerId = UITouch.SINGLE_TOUCH_ID;
					uiTouch.phase = TouchPhase.Began;
					Vector3 p = Input.mousePosition;
					p.z = -Camera.main.transform.position.z;
					uiTouch.location = Camera.main.ScreenToWorldPoint(p) * PIXEL_PER_UNIT;
					uiTouch.tapCount = 1;
					uiTouch.timestamp = DateTime.Now;
					
					touchesBegan.Add (uiTouch);
					hasTouchesBegan = true;
				}else if(Input.GetMouseButtonUp(0)){
					UITouch uiTouch = new UITouch();
					uiTouch.fingerId = UITouch.SINGLE_TOUCH_ID;
					uiTouch.phase = TouchPhase.Ended;
					Vector3 p = Input.mousePosition;
					p.z = -Camera.main.transform.position.z;
					uiTouch.location = Camera.main.ScreenToWorldPoint(p) * PIXEL_PER_UNIT;
					uiTouch.tapCount = 1;
					uiTouch.timestamp = DateTime.Now;
					
					touchesEnded.Add (uiTouch);
					hasTouchesEnded = true;
				}else if(Input.GetMouseButton(0)){
					UITouch uiTouch = new UITouch();
					uiTouch.fingerId = UITouch.SINGLE_TOUCH_ID;
					uiTouch.phase = TouchPhase.Moved;
					Vector3 p = Input.mousePosition;
					p.z = -Camera.main.transform.position.z;
					uiTouch.location = Camera.main.ScreenToWorldPoint(p) * PIXEL_PER_UNIT;
					uiTouch.tapCount = 1;
					uiTouch.timestamp = DateTime.Now;
					
					touchesMoved.Add (uiTouch);
					hasTouchesMoved = true;
				}
				#endif
				#endif
			}
			if (hasTouchesBegan)
				_rootViewController.view.touchesBegan (touchesBegan);
			if (hasTouchesMoved)
				_rootViewController.view.touchesMoved (touchesMoved);
			if (hasTouchesEnded)
				_rootViewController.view.touchesEnded (touchesEnded);
			if (hasTouchesCancelled)
				_rootViewController.view.touchesCancelled (touchesCancelled);
			touchesBegan.Clear ();
			touchesMoved.Clear ();
			touchesEnded.Clear ();
			touchesCancelled.Clear ();
			
			#if UNITY_STANDALONE || UNITY_WEBGL
			if (Input.GetMouseButtonDown (0)) {
				NSEvent nsevent = getMouseEvent ();
				_rootViewController.view.mouseDown (nsevent);
			} else if (Input.GetMouseButtonUp (0)) {
				NSEvent nsevent = getMouseEvent ();
				_rootViewController.view.mouseUp (nsevent);
			} else if (Input.GetMouseButton (0)) {
				NSEvent nsevent = getMouseEvent ();
				_rootViewController.view.mouseDragged (nsevent);
			} else if (Input.GetMouseButtonDown (1)) {
				NSEvent nsevent = getMouseEvent ();
				_rootViewController.view.rightMouseDown (nsevent);
			} else if (Input.GetMouseButtonUp (1)) {
				NSEvent nsevent = getMouseEvent ();
				_rootViewController.view.rightMouseUp (nsevent);
			} else if (Input.GetMouseButton (1)) {
				NSEvent nsevent = getMouseEvent ();
				_rootViewController.view.rightMouseDragged (nsevent);
			} else if (Input.GetMouseButtonDown (2)) {
				NSEvent nsevent = getMouseEvent ();
				_rootViewController.view.otherMouseDown (nsevent);
			} else if (Input.GetMouseButtonUp (2)) {
				NSEvent nsevent = getMouseEvent ();
				_rootViewController.view.otherMouseUp (nsevent);
			} else if (Input.GetMouseButton (2)) {
				NSEvent nsevent = getMouseEvent ();
				_rootViewController.view.otherMouseDragged (nsevent);
			}else{
				float d = Input.GetAxis("Mouse ScrollWheel");
				if(!FloatUtils.EQ(d, 0)){
					NSEvent wheelEvt = getMouseEvent();
					wheelEvt.mouseWheelDelta = d;
					_rootViewController.view.scrollWheel(wheelEvt);
				}
				float dx = Input.GetAxis("Mouse X");
				float dy = Input.GetAxis("Mouse Y");
				if(!FloatUtils.EQ(dx, 0) || !FloatUtils.EQ(dy, 0)){
					NSEvent nsevent = getMouseEvent ();
					nsevent.mouseDelta = new Vector2(dx, dy) * PIXEL_PER_UNIT;
					_rootViewController.view.mouseMoved(nsevent);
				}
			}
			//Keybaord Events
			keyboardEvent();

			#endif
		}
		#if UNITY_STANDALONE || UNITY_WEBGL
		NSEvent getMouseEvent(){
			NSEvent nsevent = new NSEvent();
			nsevent.mouseLocation = Camera.main.ScreenToWorldPoint(Input.mousePosition) * PIXEL_PER_UNIT;
			return nsevent;
		}
		void keyboardEvent(){
			var keyCodeEnu = Enum.GetValues (typeof(KeyCode)).GetEnumerator ();
			while (keyCodeEnu.MoveNext()) {
				KeyCode keyCodeTmp = (KeyCode)keyCodeEnu.Current;
				if(keyCodeTmp == KeyCode.Mouse0 || 
				   keyCodeTmp == KeyCode.Mouse1 ||
				   keyCodeTmp == KeyCode.Mouse2 || 
				   keyCodeTmp == KeyCode.Mouse3 ||
				   keyCodeTmp == KeyCode.Mouse4 || 
				   keyCodeTmp == KeyCode.Mouse5){
					continue;
				}
				if(Input.GetKeyDown(keyCodeTmp)){
					NSEvent evt = new NSEvent();
					evt.keyCode = keyCodeTmp;
					_rootViewController.view.keyDown(evt);
				}

				if(Input.GetKeyUp(keyCodeTmp)){
					NSEvent evt = new NSEvent();
					evt.keyCode = keyCodeTmp;
					_rootViewController.view.keyUp(evt);
				}
			}
		}
		#endif

		public void OnGUIUpdate(CADisplayLink sender){
		}

		
		#if UNITY_STANDALONE || UNITY_WEBGL
		void OnMouseEnter() {
			NSEvent nsevent = getMouseEvent ();
			_rootViewController.view.mouseEntered (nsevent);
		}
		void OnMouseExit() {
			NSEvent nsevent = getMouseEvent ();
			_rootViewController.view.mouseExited (nsevent);
		}
		#endif
		#endregion
    }
}

