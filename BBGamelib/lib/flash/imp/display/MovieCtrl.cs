using UnityEngine;
using System.Collections;

namespace BBGamelib.flash.imp
{
	public class MovieCtrl
	{
		public delegate void Callback(Movie movie);
		Movie _movie;
		int _startFrame;
		int _endFrame;
		bool _loop;
		float _fps;
		bool _isPlaying;
		float _elapsed;
		bool _paused;
		Callback _callback;
		
		public bool isPlaying{ get { return _isPlaying; } }
		public int startFrame{ get { return _startFrame; } set { _startFrame = value; } }
		public int endFrame{ get { return _endFrame; } set { _endFrame = value; } }
		public bool loop { get { return _loop; } set { _loop = value; } }
		public float fps{ get { return _fps; } set { _fps = value; } }
		public Callback callback{ get { return _callback; } set { _callback = value; } }

		public MovieCtrl(Movie movie)
		{
			_movie = movie;
			_startFrame = 0;
			_endFrame = _movie.movieDefine.frames.Length - 1;
			_fps = _movie.define.flash.frameRate;
			_loop = true;
			_isPlaying = false;
			_paused = false;
			_elapsed = 0;
			_callback = null;
		}

		public void start(){
			if(_isPlaying)
				stop ();
			_movie.gotoFrame(_startFrame);
			if (_startFrame != _endFrame) {
				_isPlaying = true;
				_paused = false;
				_movie.schedule(this.update);
			}
		}
		
		public void stop(){
			if(_isPlaying)
				_movie.unschedule (this.update);
			_isPlaying = false;
		}
		
		public void stopRecursive(){
			stop ();
			for(int i=0; i<_movie.depthDisplays.Length; i++){
				Display child = _movie.depthDisplays[i];
				Movie childMovie = child as Movie;
				if(childMovie != null)
					childMovie.movieCtrl.stopRecursive();
			}
		}

		public void pause(){
			_paused = true;
			_movie.unschedule (this.update);
		}

		public void resume(){
			_paused = false;
			if(_isPlaying)
				_movie.schedule (this.update);
		}

		public void reset(){
			stop ();
			_startFrame = 0;
			_endFrame = _movie.movieDefine.frames.Length - 1;
			_fps = _movie.define.flash.frameRate;
			_loop = true;
			_isPlaying = false;
			_paused = false;
			_elapsed = 0;
			_callback = null;
			for(int i=0; i<_movie.depthDisplays.Length; i++){
				Display child = _movie.depthDisplays[i];
				child.removeFromParent();
				_movie.depthDisplays[i] = null;
			}
		}

		void update(float dt){
			if (_paused)
				return;
			_elapsed += dt;
			//Make sure endFrame not equals to startFrame when init
			if (_endFrame > _startFrame) {
				int toFrame = _startFrame + Mathf.FloorToInt (_elapsed * _fps);
				if(toFrame > _endFrame){
					_elapsed -= Mathf.Abs(_endFrame - _startFrame + 1)/_fps;
					toFrame = _startFrame + Mathf.FloorToInt (_elapsed * _fps);
				}
				toFrame = Mathf.Min (toFrame, _endFrame);
				
				int nextFrame = _movie.curFrame + 1;
				if(toFrame > nextFrame){
					for (int i=nextFrame; i < toFrame; i++) {
						if(_movie.tweenMode == kTweenMode.SkipFrames || (_movie.tweenMode==kTweenMode.SkipNoLabelFrames && _movie.movieDefine.frames[i]==null))
							_movie.skipFrame(i);
						else
							_movie.gotoFrame(i);
					}
				}
				if(toFrame != _movie.curFrame){
					_movie.gotoFrame(toFrame);
					if (_movie.curFrame == _endFrame) {
						if(!_loop)
							stop ();
						if(_callback != null)
							_callback(_movie);
					}
				}
			} else {
				int toFrame = _startFrame + Mathf.FloorToInt (_elapsed * _fps);
				if(toFrame < _endFrame){
					_elapsed -= Mathf.Abs(_endFrame - _startFrame + 1)/_fps;
					toFrame = _startFrame + Mathf.FloorToInt (_elapsed * _fps);
				}
				toFrame = Mathf.Max (toFrame, _endFrame);
				int nextFrame = _movie.curFrame - 1;
				if(toFrame < nextFrame){
					for (int i=nextFrame; i > toFrame; i--) {
						if(_movie.tweenMode == kTweenMode.SkipFrames || (_movie.tweenMode==kTweenMode.SkipNoLabelFrames && _movie.movieDefine.frames[i]==null))
							_movie.skipFrame(i);
						else
							_movie.gotoFrame(i);
					}
				}
				if(toFrame != _movie.curFrame){
					_movie.gotoFrame(toFrame);
					if (_movie.curFrame == _endFrame) {
						if(!_loop)
							stop ();
						if(_callback != null)
							_callback(_movie);
					}
				}
			}
		}
	}
}
