using UnityEngine;
using System.Collections;
using BBGamelib;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BBGamelib{
	public class CC3Sprite : CC3Prefab
	{
		public enum kFrameEventMode{
			LabelFrame,
			EveryFrame,
		}
		public delegate void Callback(CC3Sprite spt);
		float _fpsScale;
		Animation _fbxAnimation;
		AnimationState _currentAnimationState;
		int _currentFrame;
		List<string> _currentLabels;
		Dictionary<int, List<string>> _labels;
		float _elapsed;
		bool _loop;
		int _startFrame;
		int _endFrame;
		Callback _endCall;
		Callback _frameEventCall;
		kFrameEventMode _frameEventMode;
		bool _isRuningAnimation;

		Dictionary<string, AnimationClip> _clips;
		Dictionary<string, Dictionary<int, List<string>>> _allLabels;

		public bool loop{get{return _loop;} set{_loop=value;}}
		public string currentAnimeName{get{return _currentAnimationState==null?null:_currentAnimationState.name;}}
		public int currentFrame{get{return _currentFrame;}}
		public List<string> currentLabels{get{return _currentLabels;}}
		public string currentLabel{get{return (_currentLabels!=null && _currentLabels.Any ())?_currentLabels[0]:null;}}
		public Dictionary<int, List<string>> labels{get{return _labels;}}
		public Callback frameEventCall{get{return _frameEventCall;}set{_frameEventCall=value;}}
		public kFrameEventMode frameEventMode{get{return _frameEventMode;} set{_frameEventMode = value;}}

		public CC3Sprite(string path) : base(path)
		{
			_fbxAnimation = _prefabObj.GetComponent<Animation>();
			_fpsScale = 1;
			_loop = false;
			schedule (updateFrame);
			_currentLabels = null;
			_frameEventMode = kFrameEventMode.LabelFrame;
			_isRuningAnimation = false;
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

		public bool hasAnimation(string name){
			return _fbxAnimation.GetClip (name) != null;
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
		protected virtual void updateFrame (float dt)
		{
			if (!_isRuningAnimation)
				return;

			if (_currentAnimationState == null) {
				return;
			}
			_elapsed += dt;
			bool visible = this.visible;
			for (var p = this.parent; p!=null; p=p.parent) {
				if(!p.visible){
					visible = false;
					break;
				}
			}
			if (!visible) {
				return;
			}
			if (_fbxAnimation.clip == null || _fbxAnimation.clip.name != _currentAnimationState.clip.name) {
				_fbxAnimation.Play(_currentAnimationState.clip.name);
			}
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
	}
}
