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
			gotoFrame(_startFrame);
			if (_startFrame != _endFrame) {
				_isPlaying = true;
				_paused = false;
                _elapsed = 0;
				_movie.schedule(this.update);
			}
		}
		
		public void stop(){
			if(_isPlaying)
				_movie.unschedule (this.update);
			_isPlaying = false;
            _paused = false;
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
            if (_paused)
            {
                _paused = false;
                if (_isPlaying)
                    _movie.schedule(this.update);
            }
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
            int dFrame = Mathf.FloorToInt(_elapsed * _fps);
            if (dFrame > 0)
            {
                _elapsed -= dFrame / _fps;
                int toFrame = _movie.curFrame + dFrame;
                if (_loop)
                {
                    int curFrame = _movie.curFrame;
                    if (toFrame > _endFrame)
                    {

                        int modFrame = (toFrame - _endFrame - 1) % (_endFrame - _startFrame + 1);
                        toFrame = _startFrame + modFrame;

                        for (int i = _movie.curFrame + 1; i <= _endFrame; i++)
                        {
                            gotoFrame(i);
                        }
                        onEnd();
                        curFrame = _startFrame-1;
                    }

                    for (int i = curFrame + 1; i <= toFrame; i++)
                    {
                        gotoFrame(i);
                    }
                    if (toFrame == _endFrame)
                    {
                        onEnd();
                    }
                } else
                {
                    toFrame = System.Math.Min(_endFrame, toFrame);
                    if (toFrame != _movie.curFrame)
                    {
                        for (int i = _movie.curFrame + 1; i <= toFrame; i++)
                        {
                            gotoFrame(i);
                        }
                        if (toFrame == _endFrame)
                        {
                            onEnd();
                        }
                    }
                }
            }
        }

        void gotoFrame(int frameIndex){
            _movie.gotoFrame(frameIndex, frameIndex == _startFrame && _startFrame > 0);
        }

		void onEnd(){
			if(!_loop)
				stop ();
			if(_callback != null)
				_callback(_movie);
		}
	}
}
