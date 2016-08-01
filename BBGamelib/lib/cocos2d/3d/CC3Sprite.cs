using UnityEngine;
using System.Collections;
using BBGamelib;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BBGamelib{
	public class CC3Sprite : CC3Node
	{
		public enum kFrameEventMode{
			LabelFrame,
			EveryFrame,
		}
		public delegate void Callback(CC3Sprite spt);
		string _path;
		float _fpsScale;
		GameObject _fbxObject;
		Animation _fbxAnimation;
		AnimationState _currentAnimationState;
		int _currentFrame;
		List<string> _currentLabels;
		Dictionary<int, List<string>> _labels;
		float _elapsed;
		bool _loop;
		int _startFrame;
		int _endFrame;
		Bounds _bounds;
		bool _isBoundsDirty;
		Callback _endCall;
		Callback _frameEventCall;
		kFrameEventMode _frameEventMode;

		bool _opacityModifyRGB;
		Color _quadColor;
		Renderer[] _renderers;
		Color32[] _originalColors;
		bool _isRuningAnimation;

		Dictionary<string, AnimationClip> _clips;
		bool _reused;

		Dictionary<string, Dictionary<int, List<string>>> _allLabels;

		public bool loop{get{return _loop;} set{_loop=value;}}
		public float rotationZ{get{return _rotation;} set{_rotation=value;}}
		public GameObject fbxObject{get{return _fbxObject;}}

		public string currentAnimeName{get{return _currentAnimationState==null?null:_currentAnimationState.name;}}
		public int currentFrame{get{return _currentFrame;}}
		public List<string> currentLabels{get{return _currentLabels;}}
		public string currentLabel{get{return (_currentLabels!=null && _currentLabels.Any ())?_currentLabels[0]:null;}}
		public Dictionary<int, List<string>> labels{get{return _labels;}}
		public Callback frameEventCall{get{return _frameEventCall;}set{_frameEventCall=value;}}
		public kFrameEventMode frameEventMode{get{return _frameEventMode;} set{_frameEventMode = value;}}

		public CC3Sprite(string path){
			_path = path;
			_fbxObject = CC3SpriteFactory.Instance.getPrefabObject (path, true);
			_fbxObject.transform.parent = this.transform;
			_fbxObject.transform.localPosition = Vector3.zero;
			_fbxObject.transform.localEulerAngles = Vector3.zero;
			_fbxObject.transform.localScale = new Vector3 (1, 1, 1);
			
			_fbxAnimation = _fbxObject.GetComponent<Animation>();
			_fpsScale = 1;
			_loop = false;
			scheduleUpdateWithPriority ();
			_isBoundsDirty = true;
			_bounds = new Bounds (Vector3.zero, Vector3.zero);
			_currentLabels = null;
			_opacityModifyRGB = true;
			_quadColor = new Color32 (255, 255, 255, 255);
			_frameEventMode = kFrameEventMode.LabelFrame;
			_isRuningAnimation = false;
			_reused = true;
			_allLabels = new Dictionary<string, Dictionary<int, List<string>>>  ();
		}
		
		public void addAnimationClip(string path){
			AnimationClip[] clips = CC3SpriteFactory.Instance.getAnimationClips (path, true);
			for (int i=0; i<clips.Length; i++) {
				_fbxAnimation.AddClip (clips[i], clips[i].name);
			}
		}

		public void setAnimation(string name){
			setAnimationState (name);
		}

		public void playAnimation(string name, Callback endCallback=null){
			playAnimation (name, 0, int.MaxValue, endCallback);
		}

		public void playAnimation(string name, int start, int end, Callback endCallback=null){
			NSUtils.Assert (start >= 0, "CC3Sprite#playAnimation start frame should not be {0}.", start);
			NSUtils.Assert (end >= 0, "CC3Sprite#playAnimation start frame should not be {0}.", end);
			stop ();
			_endCall = endCallback;
			setAnimationState (name);
			
			if (_currentAnimationState != null) {
				_currentAnimationState.speed = 0;
				_startFrame = Math.Max (start, 0);
				_currentFrame = _startFrame;
				_endFrame = Mathf.FloorToInt (Mathf.Min (end, totalFrames - 1));
				gotoFrame (_startFrame);
				_isRuningAnimation = true;
			}
		}

		public void playAnimation(string name, string startLabel, string endLabel, Callback endCallback=null){
			setAnimationState (name);
			int startFrame = getFrame (startLabel);
			NSUtils.Assert (startFrame != -1, "CC3Sprite#playAnimation startLabel {0} not found.", startLabel);
			int endFrame = getFrame (endLabel);
			NSUtils.Assert (endFrame != -1, "CC3Sprite#playAnimation endLabel {0} not found.", endLabel);
			playAnimation (name, startFrame, endFrame, endCallback);
		}

		public void gotoLabel(string name, string label){
			int frame = getFrame (label);
			gotoFrame (name, frame);
		}

		public void gotoFrame(string name, int frame){
			stop ();
			setAnimationState (name);
			_currentAnimationState.speed = 0;
			gotoFrame(frame);
		}

		public void stop(){
			_isRuningAnimation = false;
			_elapsed = 0;
			_currentAnimationState = null;
		}

		public float currentAnimationDruation{
			get{
				if(_currentAnimationState == null)
					return 0;
				return _currentAnimationState.length;
			}
		}

		public int getFrame(string label){
			if (_labels != null) {
				var frameLabelsEnu = _labels.GetEnumerator();
				while(frameLabelsEnu.MoveNext()){
					var kv = frameLabelsEnu.Current;
					int frame = kv.Key;
					List<string> labels = kv.Value;
					if(labels.Contains(label))
						return frame;
				}
			}
			return -1;
		}

		public float frameRate{
			get{
				if (_currentAnimationState != null && _currentAnimationState.clip != null)
					return _currentAnimationState.clip.frameRate;
				return -1;
			}
		}

		public int totalFrames{
			get{
				if (_currentAnimationState != null && _currentAnimationState.clip != null){
					int totalFrames = Mathf.FloorToInt (_currentAnimationState.clip.length * _currentAnimationState.clip.frameRate) + 1;
					return totalFrames;
				}else{
					return 0;
				}
			}
		}


		public bool resued{
			get{ return _reused;}
			set{ _reused = value;}
		}


		void setAnimationState(string name){
			NSUtils.Assert (_fbxAnimation != null, "animation of {0} is null.", name);
			if (name == currentAnimeName) {
				return;
			}
			_fbxAnimation.Play(name);
			_currentAnimationState = _fbxAnimation [name];
			NSUtils.Assert (_currentAnimationState != null, "{0}#{1} is null.", _fbxAnimation.name, name);
			
			_elapsed = 0;
			_currentFrame = -1;
			_currentLabels = null;
			_startFrame = 0;
			_endFrame = 0;

			_labels = null;
			if (_currentAnimationState.clip != null) {
				if(!_allLabels.TryGetValue(name, out _labels)){
					_labels = new Dictionary<int, List<string>>();
					_allLabels.Add(name, _labels);
					AnimationEvent[] events = _currentAnimationState.clip.events;
					for (int i=0; i<events.Length; i++) {
						AnimationEvent evt = events [i];
						int frame = Mathf.FloorToInt (evt.time * _currentAnimationState.clip.frameRate);
						List<string> labelsOfFrame = null;
						_labels.TryGetValue(frame, out labelsOfFrame);
						if(labelsOfFrame == null){
							labelsOfFrame = new List<string>();
							_labels[frame] = labelsOfFrame;
						}
						labelsOfFrame.Add(evt.functionName);
					}
				}
			}
		}
		
		void gotoFrame(int frame){
			if (_currentFrame != frame) {
				_isBoundsDirty = true;
			}
			_currentFrame = frame;
			_currentLabels = null;
			if (_labels != null) {
				_labels.TryGetValue(frame, out _currentLabels);
			}
			if (_currentAnimationState != null) {
				float time = frame / _currentAnimationState.clip.frameRate;
				//in case animation goto first frame immediatlly in loop mode
				if(_currentAnimationState.wrapMode == WrapMode.Loop)
					time = Mathf.Min (_currentAnimationState.length - 1/_currentAnimationState.clip.frameRate/2, time);
				_currentAnimationState.time = time; 
			}
			if(_frameEventCall != null){
				if( _frameEventMode == kFrameEventMode.EveryFrame || (_currentLabels!=null && _currentLabels.Any())){
					_frameEventCall(this);
				}
			}
		}
		public override void update (float dt)
		{
			base.update (dt);
			if (!_isRuningAnimation)
				return;

			if (_currentAnimationState == null) {
				return;
			}
			_elapsed += dt;
			if (!this.visible) {
				return;
			}

			_fbxAnimation.Play(_currentAnimationState.name);
			float realElapsed = _elapsed * _fpsScale;
			int toFrame = Mathf.FloorToInt(realElapsed * _currentAnimationState.clip.frameRate) + _startFrame;
			if (_loop) {
				while (toFrame > _endFrame) {
					int nextFrame = _currentFrame + 1;
					while (nextFrame<=_endFrame) {
						gotoFrame(nextFrame);
						nextFrame  ++;
					}
					gotoFrame(0);
					nextFrame=1;

					realElapsed -= (_endFrame - _startFrame + 1) / _currentAnimationState.clip.frameRate;
					toFrame = Mathf.FloorToInt(realElapsed * _currentAnimationState.clip.frameRate) + _startFrame;
				}
				_elapsed = realElapsed / _fpsScale;
			} else {
				toFrame = Math.Min (toFrame, _endFrame);
			}
			if (toFrame != _currentFrame) {
				int nextFrame = _currentFrame + 1;
				while (nextFrame<toFrame) {
					gotoFrame(nextFrame);
					nextFrame  ++;
				}

				gotoFrame(toFrame);
				if(toFrame == _endFrame && _endCall != null){
					_elapsed = 0;
					_startFrame = _endFrame;
					_endCall(this);
				}
			}
		}

		public override void cleanup ()
		{
			if (_originalColors != null) {
				for (int i=renderers.Length-1; i>=0; i--) {
					var renderer = renderers [i];
					if (renderer.material.HasProperty ("_Color")) {
						renderer.material.color = _originalColors [i];
					}
				}
			}
			//reset default layer
			for (int i=renderers.Length-1; i>=0; i--) {
				var renderer = renderers [i];
				renderer.sortingLayerName = CCFactory.LAYER_DEFAULT;
				renderer.gameObject.layer = LayerMask.NameToLayer(CCFactory.LAYER_DEFAULT);
			}
			base.cleanup ();
			if (_reused) {
				CC3SpriteFactory.Instance.recyclePrefabObject (_path, _fbxObject);
			} else {
				if(Application.isEditor)
					UnityEngine.Object.DestroyImmediate(_fbxObject, true);
				else
					UnityEngine.Object.Destroy(_fbxObject);
			}
			_fbxObject = null;
		}

		/*Get the child object with specified name under fbx object.*/
		public GameObject getChildObject(string name){
			return ccUtils.GetChildObject(_fbxObject, name);
		}

		public Bounds getLocalbounds(bool recalculateIfNeed=true){
			if (_isBoundsDirty && recalculateIfNeed) {
				var bounds = calculateLocalBounds(this.renderers);
				bounds.center *= UIWindow.PIXEL_PER_UNIT;
				bounds.size *= UIWindow.PIXEL_PER_UNIT;
				_bounds = bounds;
				_isBoundsDirty = false;
			}
			//Fixed: convert bounds to content space
			return _bounds;
		}

	
		public Bounds getLocalbounds<T>() where T:Renderer{
			T[] renderers = this.gameObject.GetComponentsInChildren<T> ();
			if (renderers.Length == 0)
				return new Bounds();
			var combinedBounds = calculateLocalBounds (renderers);
			combinedBounds.center *= UIWindow.PIXEL_PER_UNIT;
			combinedBounds.size *= UIWindow.PIXEL_PER_UNIT;
			return combinedBounds;
		}

		Bounds calculateLocalBounds(Renderer[] renderers){
			var combinedBounds = renderers [0].bounds;
			for (int i=renderers.Length-1; i>0; i--) {
				var renderer = renderers [i];
				if(renderer.gameObject.activeSelf)
					combinedBounds.Encapsulate (renderer.bounds);
			}

			combinedBounds = cc3Utils.ConvertToLocalBounds (this.transform, combinedBounds);
			return combinedBounds;
		}

		
		public Renderer[] renderers{
			get{
				if(_renderers==null){
					_renderers = this.gameObject.GetComponentsInChildren<Renderer> (true);
                }
                return _renderers;
            }
        }

		#region CCSprite - RGBA protocol
		protected override void draw ()
		{
			ccUtils.CC_INCREMENT_GL_DRAWS ();
			
			Renderer[] rs = this.renderers;
			for (int i=0; i<rs.Length; i++) {
				rs [i].sortingOrder = CCDirector.sharedDirector.globolRendererSortingOrder;
			}
			CCDirector.sharedDirector.globolRendererSortingOrder ++;
		}
		public void updateColor()
		{
			Color32 color4 = new Color32(_displayedColor.r, _displayedColor.g, _displayedColor.b, _displayedOpacity);
			
			// special opacity for premultiplied textures
			if ( _opacityModifyRGB ) {
				color4.r = (byte)(color4.r * _displayedOpacity/255.0f);
				color4.g = (byte)(color4.g * _displayedOpacity/255.0f);
				color4.b = (byte)(color4.b * _displayedOpacity/255.0f);
			}
			_quadColor = color4;
			Renderer[] renderers = this.renderers;
			
			if(_originalColors == null){
				_originalColors = new Color32[_renderers.Length];
				for(int i=_originalColors.Length-1; i>=0;i--){
					if(_renderers[i].material.HasProperty("_Color"))
						_originalColors[i] = _renderers[i].material.color;
				}
			}
			for (int i=renderers.Length-1; i>=0; i--) {
				var renderer = renderers [i];
				if(renderer.material.HasProperty("_Color")){
					renderer.material.color = _originalColors[i] * _quadColor;
				}
			}
		}
		
		public override Color32 color {
			set {
				base.color = value;
				updateColor();
			}
		}
		
		public override void updateDisplayedColor (Color32 parentColor)
		{
			base.updateDisplayedColor (parentColor);
			updateColor ();
		}
		
		public override byte opacity {
			set {
				base.opacity = value;
				updateColor();
			}
		}
		public override bool opacityModifyRGB{
			get{return _opacityModifyRGB;}
			set{
				if( _opacityModifyRGB != value ) {
					_opacityModifyRGB = value;
					updateColor();
				}
			}
		}
		
		public override void updateDisplayedOpacity (byte parentOpacity)
		{
			base.updateDisplayedOpacity (parentOpacity);
			updateColor ();
		}
		
		#endregion
	}
}
