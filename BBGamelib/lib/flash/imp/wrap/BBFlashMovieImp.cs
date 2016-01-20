using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BBGamelib.flash.imp;
using System;

namespace BBGamelib.flash.wrap{
	public class BBFlashMovieImp : MovieImp, BBFlashMovie
	{
		public class FrameListener{
			public System.Object target;
			public Action<BBFlashMovie> callback;
		}
		List<FrameListener> _frameListeners;

		kFrameEventMode _frameEventMode;
		public kFrameEventMode frameEventMode{get{return _frameEventMode;} set{_frameEventMode = value;}}


		public BBFlashMovieImp(DefineMovie define):base(define){
			_frameEventMode = kFrameEventMode.LabelFrame;
		}

		public BBFlashMovie getChildByClassName(string name){
			if(name==null)
				return null;
			for(int i=0; i<_depth_displays.Length; i++){
				DisplayObject child = _depth_displays[i];
				if(child!=null && child.className == name){
					return child as BBFlashMovie;
				}
			}
			return null;			
		}
		public BBFlashMovie  getChildByInstanceName(string name){
			if(name==null)
				return null;
			for(int i=0; i<_depth_displays.Length; i++){
				DisplayObject child = _depth_displays[i];
				if(child!=null && child.instanceName == name){
					return child as BBFlashMovie;
				}
			}
			return null;			
		}
		public List<BBFlashMovie> getChildrenByClassName(string name){
			if(name==null)
				return null;
			List<BBFlashMovie> children = new List<BBFlashMovie>();
			for(int i=0; i<_depth_displays.Length; i++){
				DisplayObject child = _depth_displays[i];
				if(child!=null && child.className == name){
					children.Add(child  as BBFlashMovie);
				}
			}
			return children.Count>0?children:null;
		}

		public List<BBFlashMovie> GetMoviesUnderPointInWorld(Vector2 worldPoint){
			Vector2 nodePoint = view.convertToNodeSpace (worldPoint);
			Rect bd = bounds;
			if(bd.Contains(nodePoint)){
				List<BBFlashMovie> children = new List<BBFlashMovie>();
				children.Add(this);
				for(int i=0; i<_depth_displays.Length; i++){
					DisplayObject child = _depth_displays[i];
					if(child==null)
						continue;
					BBFlashMovie childMovImp = child as BBFlashMovie;
					if(childMovImp == null)
						continue;
					List<BBFlashMovie> childrenOfChild = childMovImp.GetMoviesUnderPointInWorld(worldPoint);
					if(childrenOfChild != null){
						children.AddRange(childrenOfChild);
					}
				}
				return children.Count>0?children:null;
			}	
			return null;	
		}

		public List<BBFlashMovie> GetMoviesUnderTouch(UITouch touch){
			Vector2 point = view.convertTouchToNodeSpace (touch);
			point = view.convertToWorldSpace (point);
			return GetMoviesUnderPointInWorld (point);
		}
		
		string[] _labels;
		public string[] labels{ 
			get{
				if(_labels == null){
					_labels = new List<string>(_define.label_indexs.Keys).ToArray();
				}
				return _labels;
			}
		}

		public bool hasLabel(string label){
			return _define.label_indexs.ContainsKey(label);
		}

		public void gotoAndPlay(int start, int end){
			Stop();
			startFrame = start;
			endFrame = end;
			Play();
		}

		public void gotoAndPlay(int start, int end, Action<BBFlashMovie> callback){
			Stop();
			startFrame = start;
			endFrame = end;
			Play(delegate(Movie movie) {
				callback(movie as BBFlashMovie);
			});
		}

		public void gotoAndPlay(string start, string end){
			int startFrame;
			if(!_define.label_indexs.TryGetValue(start, out startFrame)){
				NSUtils.Assert(false, "BBFlashMovie:gotoAndPlay: Label {0} not found.", start);
			}
			int endFrame;
			if(!_define.label_indexs.TryGetValue(end, out endFrame)){
				NSUtils.Assert(false, "BBFlashMovie:gotoAndPlay: Label {0} not found.", end);
			}
			gotoAndPlay(startFrame, endFrame);
		}

		public void gotoAndPlay(string start, string end, Action<BBFlashMovie> callback){
			int startFrame;
			if(!_define.label_indexs.TryGetValue(start, out startFrame)){
				NSUtils.Assert(false, "BBFlashMovie:gotoAndPlay: Label {0} not found.", start);
			}
			int endFrame;
			if(!_define.label_indexs.TryGetValue(end, out endFrame)){
				NSUtils.Assert(false, "BBFlashMovie:gotoAndPlay: Label {0} not found.", end);
			}
			gotoAndPlay(startFrame, endFrame, callback);
		}

		public void gotoAndStop(int frame){
			Stop();
			GotoFrame(frame);
		}

		public void gotoAndStop(string label){
			int frame;
			if (!_define.label_indexs.TryGetValue (label, out frame)) {
				NSUtils.Assert(false, "BBFlashMovie:gotoAndStop: Label {0} not found.", label);
			}
			gotoAndStop(frame);
		}

		public void play(){
			gotoAndPlay (0, totalFrames - 1);
		}

		public void play(Action<BBFlashMovie> callback){
			gotoAndPlay (0, totalFrames - 1, callback);
		}
		
		public int getFrame(string label){
			int frame;
			if (_define.label_indexs.TryGetValue (label, out frame)) {
				return frame;
			}
			return -1;
		}

		public void addFrameEventListener(System.Object target, Action<BBFlashMovie> callback){
			if (_frameListeners == null) {
				_frameListeners = new List<FrameListener> ();
			} else {
				var enumerator = _frameListeners.GetEnumerator();
				while (enumerator.MoveNext()) {
					var tmp = enumerator.Current;
					if(tmp.target == target && tmp.callback == callback){
						CCDebug.Info("BBGamelib:flash: Frame event listener already exist.");
						return;
					}
				}
			}
			FrameListener listener = new FrameListener ();
			listener.target = target;
			listener.callback = callback;
			_frameListeners.Add(listener);
		}
		public void removeFrameEventListener(System.Object target){
			if (_frameListeners!=null){
				_frameListeners.RemoveAll(listener=>listener.target==target);
			}
		}

		public void frameEventCallback(Movie mov){
			if (_frameListeners != null) {
				var enumerator = new List<FrameListener>(_frameListeners).GetEnumerator();
				while (enumerator.MoveNext()) {
					var listener = enumerator.Current;
					listener.callback(mov as BBFlashMovie);
				}
			}
			if (_parent != null)
				((BBFlashMovieImp)_parent).frameEventCallback (mov);
			
		}

		public override void GotoFrame (int frame)
		{
			base.GotoFrame (frame);
			if( currentLabel!=null || _frameEventMode == kFrameEventMode.EveryFrame)
				frameEventCallback (this);
		}

		public void stop(){
			Stop ();
		}
		
		public void stopRecursive(){
			StopRecursive ();
		}

		public void pause(){
			_view.pauseSchedulerAndActions ();
		}

		public void resume(){
			_view.resumeSchedulerAndActions ();
		}

	}
}

