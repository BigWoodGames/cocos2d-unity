using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BBGamelib.flash.imp;
using System;

namespace BBGamelib{
	public enum kFrameEventMode{
		LabelFrame,
		EveryFrame,
	}
	public delegate void BBFlashMovieCallback(BBFlashMovie movie);
	public class BBFlashMovie : Movie
	{
		kFrameEventMode _frameEventMode;
		public kFrameEventMode frameEventMode{get{return _frameEventMode;} set{_frameEventMode = value;}}

		BBFlashMovieCallback _frameListener;
		public BBFlashMovieCallback frameListener{get{return _frameListener;} set{_frameListener = value;}}

		public BBFlashMovie(TagDefineMovie define):base(define)
		{
			_frameEventMode = kFrameEventMode.LabelFrame;
		}

		public BBFlashMovie getChildByClassName(string name){
			if(name==null)
				return null;
			for(int i=0; i<_depthDisplays.Length; i++){
				BBGamelib.flash.imp.Display child = _depthDisplays[i];
                if(child!=null && !child.removed  && child.define.className == name){
					return child as BBFlashMovie;
				}
			}
			return null;			
		}
		public BBFlashMovie  getChildByInstanceName(string name){
			if(name==null)
				return null;
			for(int i=0; i<_depthDisplays.Length; i++){
				BBGamelib.flash.imp.Display child = _depthDisplays[i];
                if(child!=null && !child.removed  && child.instanceName == name){
					return child as BBFlashMovie;
				}
			}
			return null;			
		}
		public utList<BBFlashMovie> getChildrenByClassName(string name){
			if(name==null)
				return null;
			utList<BBFlashMovie> children = new utList<BBFlashMovie>();
			for(int i=0; i<_depthDisplays.Length; i++){
				BBGamelib.flash.imp.Display child = _depthDisplays[i];
                if(child!=null && !child.removed && child.define.className == name){
					children.DL_APPEND(child  as BBFlashMovie);
				}
			}
			return children;
		}

		string[] _labels;
		public string[] labels{ 
			get{
				if(_labels == null){
					int labelsCount = 0;
					for(int i=0; i<_define.frames.Length; i++){
						if(_define.frames[i].label != null){
							labelsCount ++;
						}
					}
					_labels = new string[labelsCount];
					labelsCount = 0;
					for(int i=0; i<_define.frames.Length; i++){
						if(_define.frames[i].label != null){
							_labels[labelsCount++] = _define.frames[i].label;
						}
					}
				}
				return _labels;
			}
		}

		public int getFrame(string label){
			for(int i=0; i<_define.frames.Length; i++){
				if(_define.frames[i].label == label){
					return i;
				}
			}
			return -1;
		}
		public void gotoAndPlay(int start, int end, BBFlashMovieCallback callback = null)
        {
            NSUtils.Assert(start <= end, "BBFlashMovie:gotoAndPlay: reverse play is not supported at current version.");
            
			_movieCtrl.callback = delegate {
				if(callback != null)
					callback(this);
			};
			_movieCtrl.startFrame = start;
			_movieCtrl.endFrame = end;
			_movieCtrl.start();
		}

		public void gotoAndPlay(string start, string end, BBFlashMovieCallback callback = null)
		{
			int startFrame = getFrame (start);
			NSUtils.Assert(startFrame != -1, "BBFlashMovie:gotoAndPlay: Label {0} not found.", start);
			int endFrame = getFrame (end);
			NSUtils.Assert(endFrame != -1, "BBFlashMovie:gotoAndPlay: Label {0} not found.", end);
			gotoAndPlay(startFrame, endFrame, callback);
		}

		public void gotoAndStop(int frame){
            _movieCtrl.stop ();
            _movieCtrl.startFrame = frame;
            _movieCtrl.endFrame = frame;
            gotoFrame(frame, true);
		}

		public void gotoAndStop(string label){
			int frame = getFrame (label);
			NSUtils.Assert(frame != -1, "BBFlashMovie:gotoAndStop: Label {0} not found.", label);
			gotoAndStop(frame);
		}

		public void play(){
			gotoAndPlay (0, this.totalFrames - 1, null);
		}

		public void play(BBFlashMovieCallback callback){
			gotoAndPlay (0, this.totalFrames - 1, callback);
		}

        /** Do not call this method.*/
        public override void gotoFrame(int frameIndex, bool isCheckedPreTags)
        {
            base.gotoFrame(frameIndex, isCheckedPreTags);
            if(_frameListener!=null && (this.curLabel!=null || _frameEventMode == kFrameEventMode.EveryFrame))
                _frameListener (this);
        }

		public void gotoFrame (int frame)
		{
			gotoFrame (frame, true);
		}

		public void stop(){
			_movieCtrl.stop ();
		}
		
		public void stopRecursive(){
			_movieCtrl.stopRecursive ();
		}

		public void pause(){
			_movieCtrl.pause ();
		}

		public void resume(){
			_movieCtrl.resume ();
		}

		public void reset(){
			_movieCtrl.reset ();
		}

		public int totalFrames{
			get{ return this.movieDefine.frames.Length;}
		}

		public int startFrame {
			get{ return _movieCtrl.startFrame; }
		}

		public int endFrame {
			get{ return _movieCtrl.endFrame; }
		}

		public bool isPlaying{
			get{ return _movieCtrl.isPlaying;}
		}

		public string className{
			get{ return this.define.className;}
		}

		public bool loop{
			get{ return _movieCtrl.loop;}
			set{ _movieCtrl.loop = value;}
		}

		public float fps{
			get{ return _movieCtrl.fps;}
			set{ _movieCtrl.fps = value;}
		}
	}
}

