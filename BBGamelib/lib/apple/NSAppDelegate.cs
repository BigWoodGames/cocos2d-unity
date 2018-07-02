using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace BBGamelib{
	public class NSLooper{
		public System.Object target;
		public Action<CADisplayLink> selOnUpdate;
		public Action<CADisplayLink> selOnGUI;
	}
	
	[ExecuteInEditMode]
	public abstract class NSAppDelegate : MonoBehaviour, CADisplayLink
	{
		#region properties
		[SerializeField] protected UIWindow _window;
		List<NSLooper> _loopers;
		int _looperCount;
		bool _doseApplicationDidFinishLaunched;
		#endregion

		#region singleton
		[SerializeField] [HideInInspector]private bool firstPassFlag=true;
		static NSAppDelegate _appDelegate = null;
		public static NSAppDelegate AppDelegate{
			get{
				return _appDelegate;
			}
		}
		public virtual void Awake() {
			if (Application.isPlaying) {
				if (_appDelegate != null && _appDelegate != this) {
					Destroy (this.gameObject);
					return;
				} else {
					_appDelegate = this;
				}
				DontDestroyOnLoad (this.gameObject);
				_loopers = new List<NSLooper>();
				_looperCount = 0;
				registWithTarget(_window, _window.OnUpdate, _window.OnGUIUpdate);
				_doseApplicationDidFinishLaunched = false;
			} 

			if (firstPassFlag) {
				//init
				gameObject.transform.position =Vector3.zero;
				gameObject.name = "AppDelegate";

				//window
				{
					GameObject obj = new GameObject ();
					obj.transform.parent = this.transform;
					_window = obj.AddComponent<UIWindow> ();
				}

				//impl building
				applicationRunOnceOnBuilding();

				firstPassFlag = false;
			}
		}
		#endregion


		#region mono
		public virtual void Start(){
			if (Application.isPlaying) {
				applicationDidFinishLaunching ();
				_doseApplicationDidFinishLaunched = true;
			}
		}

        //3d-support: Update->LastUpdate for unity animation events
		public virtual void LateUpdate(){
			if (Application.isPlaying && _doseApplicationDidFinishLaunched && _loopers != null) {
				for(int i=0; i < _looperCount; i++)
					_loopers[i].selOnUpdate.Invoke (this);
			}
		}
		public virtual void OnGUI(){
			if (Application.isPlaying && _loopers != null) {
				for(int i=0; i < _looperCount; i++)
					_loopers[i].selOnGUI.Invoke (this);
			}
		}
		#endregion

		#region override these for application entry
		// Build is called once when AppDelegate Script attached
		public abstract void applicationRunOnceOnBuilding ();

		// Update is called once per frame
		public abstract void applicationDidFinishLaunching ();
		#endregion

		
		#region inherite from CADisplayLink
		public void registWithTarget (System.Object target, Action<CADisplayLink> selOnUpdate, Action<CADisplayLink> selOnGUI){
			NSLooper looper = new NSLooper ();
			looper.target = target;
			looper.selOnUpdate = selOnUpdate;
			looper.selOnGUI = selOnGUI;
			_loopers.Add (looper);
			_looperCount ++;
		}
		public void invalidate(System.Object target){
			_loopers.RemoveAll (looper => looper.target == target);
			_looperCount = _loopers.Count;
		}
		#endregion
	}
}
