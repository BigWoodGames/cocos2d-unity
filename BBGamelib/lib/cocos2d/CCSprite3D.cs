using UnityEngine;
using System.Collections;
using BBGamelib;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BBGamelib{
	[ExecuteInEditMode]
	[AddComponentMenu("")]
	public class CCSprite3DFactory : MonoBehaviour{
		#region singleton
		static CCSprite3DFactory _Instance;
		[SerializeField] [HideInInspector]private bool firstPassFlag=true;
		//---------singleton------
		public static CCSprite3DFactory Instance{
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
				gameObject.name = "CCSprite3DFactory";
				firstPassFlag = false;
			}
		}
		#endregion
		
		Dictionary<string, UnityEngine.Object> _prefabs = new Dictionary<string, UnityEngine.Object>();
		Dictionary<string, AnimationClip[]> _animationClips = new Dictionary<string, AnimationClip[]>();
		Dictionary<string, List<GameObject>> _fbxs_path_list = new Dictionary<string, List<GameObject>>();

		public void preloadPrefab(string path){
			if (!_prefabs.ContainsKey (path)) {
				UnityEngine.Object obj = Resources.Load (path);
				_prefabs[path] = obj;
			}
		}
		public void removePrefab(string path){
			_prefabs.Remove (path);
		}


		public void preloadAnimationClips(string path){
			if (!_animationClips.ContainsKey (path)) {
				AnimationClip[] obj = Resources.LoadAll<AnimationClip> (path);
				_animationClips[path] = obj;
			}
		}
		public void removeAnimationClip(string path){
			_animationClips.Remove (path);
		}

		public AnimationClip[] getAnimationClips(string path, bool createIfNeed){
			AnimationClip[] clips;
			if(_animationClips.TryGetValue(path, out clips)){
				return clips;
			}else if(createIfNeed){
				clips = Resources.LoadAll<AnimationClip> (path);
				_animationClips[path] = clips;
				return clips;
			}
			return null;
		}



		public void preloadCache(string path, int num){
			UnityEngine.Object prefab;
			if (!_prefabs.TryGetValue (path, out prefab)) {
				preloadPrefab(path);
				prefab = _prefabs[path];
			}
			List<GameObject> fbxs;
			if (!_fbxs_path_list.TryGetValue (path, out fbxs)) {
				fbxs = new List<GameObject>();
				_fbxs_path_list[path] = fbxs;
			}
			for(int i=0; i<num; i++){
				GameObject obj = Instantiate(prefab) as GameObject; 
				obj.SetActive(false);
				fbxs.Add(obj);
			}
		}
		public void removeCache(string path){
			List<GameObject> fbxs;
			if (_fbxs_path_list.TryGetValue (path, out fbxs)) {
				var fbxsEnu = fbxs.GetEnumerator();
				while(fbxsEnu.MoveNext()){
					var fbx = fbxsEnu.Current;
					Destroy(fbx);
				}
			}
			_fbxs_path_list.Remove (path);
		}
		public GameObject getFBXObject(string path, bool createIfNeed=true){
			List<GameObject> fbxs;
			if (_fbxs_path_list.TryGetValue (path, out fbxs) && fbxs.Any ()) {
				GameObject obj = fbxs [0];
				fbxs.RemoveAt(0);
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localEulerAngles = Vector3.zero;
				obj.transform.localScale = new Vector3 (1, 1, 1);
				obj.SetActive (true);
				return obj;
			} else if (createIfNeed) {
				UnityEngine.Object prefab;
				if (!_prefabs.TryGetValue (path, out prefab)) {
					preloadPrefab(path);
					prefab = _prefabs[path];
				}
				GameObject obj = Instantiate(prefab) as GameObject; 
				return obj;
			}
			return null;
		}
		public void recycleFBXObject(string path, GameObject obj){
			obj.transform.parent = this.transform;
			obj.SetActive (false);
			List<GameObject> fbxs;
			if (!_fbxs_path_list.TryGetValue (path, out fbxs)) {
				fbxs = new List<GameObject>();
				_fbxs_path_list[path] = fbxs;
			}
			fbxs.Add (obj);
		}
	}

	public class CCSprite3D : CCNodeRGBA
	{
		string _path;
		float _fpsScale;
		GameObject _fbxObject;
		Animation _fbxAnimation;
		AnimationState _currentAnimationState;
		int _currentFrame;
		float _elapsed;
		bool _isLoop;
		int _startFrame;
		int _endFrame;
		Vector3 _rotation3D;
		Vector3 _scale3D;

		Dictionary<string, AnimationClip> _clips;
		
		public bool isLoop{get{return _isLoop;} set{_isLoop=value;}}

		public CCSprite3D(string path){
			_path = path;
			_fbxObject = CCSprite3DFactory.Instance.getFBXObject (path, true);
			_fbxObject.transform.parent = this.transform;
			_fbxObject.transform.localPosition = Vector3.zero;
			_fbxObject.transform.localEulerAngles = Vector3.zero;
			_fbxObject.transform.localScale = new Vector3 (1, 1, 1);
			
			_fbxAnimation = _fbxObject.GetComponent<Animation>();
			_rotation3D = Vector3.zero;
			_scale3D = new Vector3 (1, 1, 1);
			_fpsScale = 1;
			_isLoop = false;
			scheduleUpdateWithPriority ();
		}
		
		public void addAnimationClip(string path){
			AnimationClip[] clips = CCSprite3DFactory.Instance.getAnimationClips (path, true);
			for (int i=0; i<clips.Length; i++) {
				_fbxAnimation.AddClip (clips[i], clips[i].name);
			}
		}
		public void playAnimation(string name){
			setAnimationState (name);
			
			_currentAnimationState.speed = 0;
			_startFrame = 0;
			_endFrame = Mathf.FloorToInt(_currentAnimationState.clip.length * _currentAnimationState.clip.frameRate);
			gotoFrame(_startFrame);
		}
		public void playAnimation(string name, int start, int end){
			setAnimationState (name);
			
			_currentAnimationState.speed = 0;
			_startFrame = Math.Max(start, 0);
			_endFrame = Mathf.FloorToInt(Mathf.Min(end, _currentAnimationState.clip.length * _currentAnimationState.clip.frameRate));
			gotoFrame(_startFrame);
		}
		
		public void gotoFrame(string name, int frame){
			setAnimationState (name);
			_currentAnimationState.speed = 0;
			gotoFrame(frame);
		}
		
		public Vector3 rotation3D{
			get{return _rotation3D;}
			set{
				if(_rotation3D != value){
					_rotation3D = value;
					_rotation = _rotation3D.z;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}
		public override float rotation {
			set {
				base.rotation = value;
				_rotation3D.z = this.rotation;
			}
		}
		public Vector3 scale3D{
			get{return _scale3D;} 
			set{
				if(_scale3D != value){
					_scale3D = value;
					this.scaleX = _scale3D.x;
					this.scaleY = _scale3D.y;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}

		public override float scaleX {
			set {
				base.scaleX = value;
				_scale3D.x = value;
				NSUtils.Assert(FloatUtils.EB(this.scaleX, 0), "CCSprite3D#ScaleX should not be negative.");
			}
		}
		public override float scaleY {
			set {
				base.scaleY = value;
				_scale3D.y = value;
				NSUtils.Assert(FloatUtils.EB(this.scaleY, 0), "CCSprite3D#ScaleY should not be negative.");
			}
		}
		public override void updateTransform ()
		{
			base.updateTransform ();
			Vector3 rotation = transform.localEulerAngles;
			rotation.z = -_rotation3D.z;
			rotation.y = _rotation3D.y;
			rotation.x = _rotation3D.x;
			
			transform.localEulerAngles = rotation;
			transform.localScale = _scale3D;
		}
		
		void setAnimationState(string name){
			NSUtils.Assert (_fbxAnimation != null, "animation of {0} is null.", name);
			
			_fbxAnimation.Play(name);
			_currentAnimationState = _fbxAnimation [name];
			NSUtils.Assert (_currentAnimationState != null, "{0}.{1} is null.", _fbxAnimation.name, name);
		}
		
		void gotoFrame(int frame){
			if (_currentAnimationState != null) {
				float time = frame / _currentAnimationState.clip.frameRate;
				_currentAnimationState.time = time;
			}
		}
		
		public override void update (float dt)
		{
			base.update (dt);
			_elapsed += dt;
			float realElapsed = _elapsed * _fpsScale;
			int toFrame = Mathf.FloorToInt(realElapsed * _currentAnimationState.clip.frameRate) + _startFrame;
			if (_isLoop) {
				while (toFrame > _endFrame) {
					realElapsed -= _currentAnimationState.length;
					toFrame = Mathf.FloorToInt(realElapsed * _currentAnimationState.clip.frameRate) + _startFrame;
				}
				_elapsed = realElapsed / _fpsScale;
			} else {
				toFrame = Math.Min (toFrame, _endFrame);
			}
			if (toFrame != _currentFrame) {
				_currentFrame = toFrame;
				gotoFrame(toFrame);
			}
		}

		public override void cleanup ()
		{
			base.cleanup ();
			CCSprite3DFactory.Instance.recycleFBXObject (_path, _fbxObject);
		}


		public GameObject getChild(string name){
			for(int i=0; i<_fbxObject.transform.childCount; i++){
				GameObject child = _fbxObject.transform.GetChild(i).gameObject;
				if(child.name == name){
					return child;
				}
			}
			return null;
		}

	}
}
