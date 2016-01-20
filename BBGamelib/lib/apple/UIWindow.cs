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
		[SerializeField] BoxCollider2D _collider;
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
				DontDestroyOnLoad (this.gameObject);
			} 
			if (firstPassFlag) {
				gameObject.transform.position = Vector3.zero;
				gameObject.name = "UIWindow";
				_collider = gameObject.AddComponent<BoxCollider2D>();
				firstPassFlag = false;
			}
		}
		#endregion


		#region properties
		public void setResolutionHeight(float hInPixels){
			float wInPixels = hInPixels * Camera.main.aspect;
			float h = hInPixels / PIXEL_PER_UNIT;
			float w = wInPixels / PIXEL_PER_UNIT;

			Rect boundsInUnits = new Rect (-w / 2, -h / 2, w, h);
			Rect boundsInPixels = new Rect (-wInPixels / 2, -hInPixels / 2, wInPixels, hInPixels);

			#if UNITY_4_5 || UNITY_4_6
			_collider.center = (boundsInUnits.position + boundsInUnits.size * 0.5f);
			#else
			_collider.offset = (boundsInUnits.position + boundsInUnits.size * 0.5f);
			#endif
			_collider.size = boundsInUnits.size;
			_bounds = boundsInPixels;

			Camera.main.orthographicSize = h / 2;
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
            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WP8Player) {
				int touchCount = Input.touchCount;
				if (touchCount > 0) {

					int count = Input.touches.Length;
					for(int i=0; i<count; i++){
						Touch touch = Input.touches[i];
						UITouch uiTouch = new UITouch();
						uiTouch.fingerId = touch.fingerId;
						uiTouch.phase = touch.phase;
						uiTouch.location = Camera.main.ScreenToWorldPoint(touch.position) * PIXEL_PER_UNIT;
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
                }
            } else {
				if(Input.GetMouseButtonDown(0)){
					UITouch uiTouch = new UITouch();
					uiTouch.fingerId = UITouch.SINGLE_TOUCH_ID;
					uiTouch.phase = TouchPhase.Began;
					uiTouch.location = Camera.main.ScreenToWorldPoint(Input.mousePosition) * PIXEL_PER_UNIT;
					uiTouch.tapCount = 1;
                    uiTouch.timestamp = DateTime.Now;

					touchesBegan.Add (uiTouch);
					hasTouchesBegan = true;
                }else if(Input.GetMouseButtonUp(0)){
					UITouch uiTouch = new UITouch();
					uiTouch.fingerId = UITouch.SINGLE_TOUCH_ID;
					uiTouch.phase = TouchPhase.Ended;
					uiTouch.location = Camera.main.ScreenToWorldPoint(Input.mousePosition) * PIXEL_PER_UNIT;
					uiTouch.tapCount = 1;
					uiTouch.timestamp = DateTime.Now;
					
					touchesEnded.Add (uiTouch);
					hasTouchesEnded = true;
				}else if(Input.GetMouseButton(0)){
					UITouch uiTouch = new UITouch();
					uiTouch.fingerId = UITouch.SINGLE_TOUCH_ID;
					uiTouch.phase = TouchPhase.Moved;
					uiTouch.location = Camera.main.ScreenToWorldPoint(Input.mousePosition) * PIXEL_PER_UNIT;
					uiTouch.tapCount = 1;
					uiTouch.timestamp = DateTime.Now;
					
					touchesMoved.Add (uiTouch);
					hasTouchesMoved = true;
				}
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
		}
		
		public void OnGUIUpdate(CADisplayLink sender){
		}
		#endregion
    }
}

