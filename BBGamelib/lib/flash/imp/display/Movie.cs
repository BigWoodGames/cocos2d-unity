using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


namespace BBGamelib.flash.imp{

	public delegate void MovieCallback(Movie movie);
	#region Movie
	public abstract class Movie : DisplayObjectImp{
		protected DefineMovie _define;
		protected CCNodeRGBA _view;
		protected DisplayObject[] _depth_displays;
		protected Rect _bounds;
		protected bool _isBoundsDirty;
		protected int _currentFrame;
		protected List<DisplayObject>[] _depth_displayCaches;
		protected kTweenMode _tweenMode;

		public Movie(DefineMovie define){
			_view = new CCNodeRGBA ();
			_view.cascadeColorEnabled = true;
			_view.cascadeOpacityEnabled = true;
			_view.gameObject.name = define.characterId.ToString();
			_depth_displays = new DisplayObject[define.maxDepth + 1];
			_depth_displayCaches = new List<DisplayObject>[define.maxDepth + 1];
			_define = define;
			_currentFrame = -1;
			_isBoundsDirty = true;
			_tweenMode = kTweenMode.SkipNoLabelFrames;
		}


		#region implements DisplayObject
		public override int characterId{ get{return _define.characterId;}}
		public override CCNodeRGBA view{get{return _view;}}
		public override string className{ get{return _define.className;}}
		public override kTweenMode tweenMode{get{return _tweenMode;} set{_tweenMode=value;}}
		public DefineMovie define{ get{return _define;}}

		public int currentFrame{ get{return _currentFrame;}}
		public string currentLabel{get{return _define.frames[_currentFrame].label;}}
		public override Rect bounds{ 
			get{
				if(_isBoundsDirty){
					Rect bounds = new Rect(0, 0, 0, 0);
					for(int i=0; i<_depth_displays.Length; i++){
						DisplayObject child = _depth_displays[i];
						if(child != null){
							Rect childBounds = child.bounds;
                            
                            //CHANGE CHILD BOUNDS TO PARENT
                            childBounds.position += child.view.anchorPointInPixels;
                            CGAffineTransform childTransform = child.view.nodeToParentTransform();
                            childBounds = CGAffineTransform.CGRectApplyAffineTransform(childBounds, childTransform);
                            //childBounds.position += child.view.position;

							bounds = ccUtils.RectUnion(bounds, childBounds);
						}
					}
					_bounds = bounds;
					_isBoundsDirty = false;
				}
				return _bounds;
			}
		}	

		public virtual void SkipFrame(int frame){
			if (_currentFrame != frame) {
				_currentFrame = frame;
				FrameObject frameObj = _define.frames [_currentFrame];
				removeUnusedPlaceobjs (frameObj);
			}
		}
		public virtual void GotoFrame(int frame){
			if (_currentFrame != frame) {
				_currentFrame = frame;
				FrameObject frameObj = _define.frames[_currentFrame];
				removeUnusedPlaceobjs(frameObj);

//				frameObj.reset();
//				for(int i=0; i<frameObj.placeObjectCount; i++){
//					PlaceObject placeObj = frameObj.nextPlaceObject();
				for(int i=0; i<frameObj.placeObjects.Length; i++){
					PlaceObject placeObj = frameObj.placeObjects[i];
					DisplayObject displayObj = _depth_displays[placeObj.depth];
					//no exist display, new one
					if(displayObj == null){
						displayObj = getDisplayCache(placeObj.depth, placeObj.characterId);
						if(displayObj == null){
							displayObj = createDisplayObject(placeObj.characterId);
							_view.addChild(displayObj.view);
						}
						_depth_displays[placeObj.depth] = displayObj;
					}
					//exist display obj, but the tag indicats to new one
//					else if (placeObj.hasCharacter){
//						if(displayObj.characterId != placeObj.characterId){
//							DisplayObject newDisplayObj = getDisplayCache(placeObj.depth, placeObj.characterId);
//							if(newDisplayObj == null){
//								newDisplayObj = createDisplayObject(placeObj.characterId);
//								_view.addChild(newDisplayObj.view);
//							}
//							recycleDisplayCache(placeObj.depth, displayObj);
//							displayObj = newDisplayObj;
//							_depth_displays[placeObj.depth] = displayObj;
//						}
//					} else if(displayObj.characterId != placeObj.characterId){
//						CCDebug.Log("abc");
//					}
					else if (displayObj.characterId != placeObj.characterId){
						DisplayObject newDisplayObj = getDisplayCache(placeObj.depth, placeObj.characterId);
						if(newDisplayObj == null){
							newDisplayObj = createDisplayObject(placeObj.characterId);
							_view.addChild(newDisplayObj.view);
						}
						recycleDisplayCache(placeObj.depth, displayObj);
						displayObj = newDisplayObj;
						_depth_displays[placeObj.depth] = displayObj;
					} 

					displayObj.applyPlaceObject(placeObj);
				}
				_isBoundsDirty = true;
			}
		}
		DisplayObject createDisplayObject(int characterId){
			DisplayObject displayObj = _define.flash.getDefine(characterId).createDisplay();
			displayObj.parent = this;
			Movie movie = displayObj as Movie;
			if(movie!=null){
				//default to play or just show first frame?
//				movie.GotoFrame(0);
				movie.Play();
			}
			return displayObj;
		}

		DisplayObject getDisplayCache(int depth, int characterId){
			DisplayObject displayObj = null;
			List<DisplayObject> caches = _depth_displayCaches [depth];
			if(caches!=null){
				var enumerator = caches.GetEnumerator();
				while (enumerator.MoveNext()) {
					var cache = enumerator.Current;
					if(cache.characterId == characterId)
					{
						caches.Remove(cache);
						displayObj = cache;
						break;
					}
				}
				if(displayObj!=null){
//					displayObj.view.removeFromParentAndCleanup(false);
					displayObj.view.visible = true;
					Movie movie = displayObj as Movie;
					if(movie!=null){
						//default to play or just show first frame?
//						movie.GotoFrame(0);
						movie.Play();
					}
				}
			}
			return displayObj;
		}
		void recycleDisplayCache(int depth, DisplayObject displayObj){
			displayObj.view.visible = false;
			Movie movie = displayObj as Movie;
			if(movie!=null)
				movie.StopRecursive();
			List<DisplayObject> caches = _depth_displayCaches[depth];
			if(caches==null){
				caches = new List<DisplayObject>(2);
				_depth_displayCaches[depth] = caches;
			}
			caches.Add(displayObj);
		}

		void removeUnusedPlaceobjs(FrameObject frameObj){
//			foreach(int removeObjDepth in frameObj.removedObjectDepths){
//				DisplayObject displayObj = _depth_displays[removeObjDepth];
//				if(displayObj!=null){
//					recycleDisplayCache(removeObjDepth, displayObj);
//				}
//				_depth_displays[removeObjDepth] = null;
//			}

			
			bool[] modified = new bool[_depth_displays.Length];
			for(int i=modified.Length - 1; i>=0; i--)
				modified[i] = false;

//			frameObj.reset();
//			for(int i=0; i<frameObj.placeObjectCount; i++){
//				PlaceObject placeObj = frameObj.nextPlaceObject();
			for(int i=0; i<frameObj.placeObjects.Length; i++){
				PlaceObject placeObj = frameObj.placeObjects[i];
				modified[placeObj.depth] = true;			
			}

			for(int i=modified.Length-1; i>=0; i--){
				if(!modified[i]){
					DisplayObject displayObj = _depth_displays[i];
					if(displayObj!=null){
						recycleDisplayCache(i, displayObj);
					}
					_depth_displays[i] = null;
				}
			}
		}

		//Play Movie
		public abstract void Play();
		public abstract void Play(MovieCallback callback);
		public abstract void Stop();
		public abstract void StopRecursive();

		public abstract int startFrame{ get; set;}
		public abstract int endFrame{ get; set;}
		public abstract int totalFrames{get;}
		public abstract bool loop { get; set;}
		public abstract bool isPlaying { get;}
		#endregion
	}
	#endregion
	
	#region MovieImp
	public class MovieImp : Movie{
		int _startFrame;
		int _endFrame;
		bool _loop;
		float _fps;
		CCAction _action;

		public MovieImp(DefineMovie define) : base(define){
			_startFrame = 0;
			_endFrame = _define.frames.Length - 1;
			_loop = true;
			_fps = define.flash.frameRate;
			_action = null;
		}

		#region implements Movie abstrat method
		public override int startFrame{ get{return _startFrame;} set{_startFrame = value;}}
		public override int endFrame{ get{return _endFrame;} set{_endFrame = value;}}
		public override int totalFrames{ get{return _define.frames.Length;}}
		public override bool loop { get{return _loop;} set{_loop=value;}}
		public override bool isPlaying{get{return _action != null;}}
		
		public override float fps{ 
			get{return _fps;}
			set{
				NSUtils.Assert(!isPlaying, "BBGamelib:flash: Can't set fps when playing animation.");
				_fps = value;
			}
		}
		public override void Play(){
			if (_startFrame == _endFrame) {
				GotoFrame(_startFrame);
				return;
			}
			Stop ();
			GotoFrame (_startFrame);
			MovieImpAction movieAction = new MovieImpAction (this, _startFrame, _endFrame);
			if (_loop) {
				_action = new CCRepeatForever(movieAction);
			} else {
				_action = movieAction;
			}
			_view.runAction (_action);
		}
		public override void Play(MovieCallback callback){
			if (callback == null) {
				Play();
				return;
			}
			if (_startFrame == _endFrame) {
				GotoFrame(_startFrame);
				callback(this);	
				return;
			}
			Stop ();
			
			GotoFrame (_startFrame);
			MovieImpAction action = new MovieImpAction (this, _startFrame, _endFrame);
			CCActionFiniteTime callbackAction = new CCCallBlock (delegate {
				callback(this);			
			});
			CCActionInterval seq  = CCSequence.Actions (action, callbackAction) as CCActionInterval;
			if (_loop) {
				_action = new CCRepeatForever (seq);
			} else {
				_action = seq;
			}
			_view.runAction (_action);
		}
		public override void Stop(){
			if (_action != null) {
				_view.stopAction(_action);
				_action = null;
			}
		}
		public override void StopRecursive(){
			Stop ();
			for(int i=0; i<_depth_displays.Length; i++){
				DisplayObject child = _depth_displays[i];
				Movie childMovie = child as Movie;
				if(childMovie != null)
					childMovie.StopRecursive();
			}
		}
		#endregion


		public bool hasLabel(int frame){
			return _define.frames [frame].label != null;
		}
	}
	#endregion
	
	#region MovieImpAction
	public class MovieImpAction : CCActionInterval{
		MovieImp _movie;
		int _startFrame;
		int _endFrame;
		bool _stoped;

		public MovieImpAction(MovieImp movie, int startFrame, int endFrame){
			if (startFrame == endFrame)
				CCDebug.Warning ("MovieAction-MovieID:{0}: startFrame should not equals to endFrame.", movie.characterId);
			
			_movie = movie;
			_startFrame = startFrame;
			_endFrame = endFrame;
			_stoped = false;
			
			float duration = (Mathf.Abs(_endFrame - _startFrame) + 1) / movie.fps;
			base.initWithDuration (duration);
		}
		
		
		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			if(_movie.currentFrame!= _startFrame)
				_movie.GotoFrame (_startFrame);
			_stoped = false;
		}
		
		public override void update (float t)
		{
			//Make sure endFrame not equals to startFrame when init
			if (_endFrame > _startFrame) {
				int toFrame = _startFrame + (Mathf.FloorToInt ((_endFrame - _startFrame + 1) * t));
				toFrame = Mathf.Min (toFrame, _endFrame);

				int nextFrame = _movie.currentFrame + 1;
				if(toFrame > nextFrame){
					for (int i=nextFrame; i < toFrame; i++) {
						if(_movie.tweenMode == kTweenMode.SkipFrames || (_movie.tweenMode==kTweenMode.SkipNoLabelFrames && !_movie.hasLabel(i)))
							_movie.SkipFrame(i);
						else
							_movie.GotoFrame(i);

						if(_stoped){
							return;
						}
					}
				}
				if(toFrame != _movie.currentFrame){
					_movie.GotoFrame(toFrame);
				}
			} else {
				int toFrame = _startFrame + (Mathf.CeilToInt ((_endFrame - _startFrame - 1) * t));
				toFrame = Mathf.Max (toFrame, _endFrame);
				int nextFrame = _movie.currentFrame - 1;
				if(toFrame < nextFrame){
					for (int i=nextFrame; i > toFrame; i--) {
						if(_movie.tweenMode == kTweenMode.SkipFrames || (_movie.tweenMode==kTweenMode.SkipNoLabelFrames && !_movie.hasLabel(i)))
							_movie.SkipFrame(i);
						else
							_movie.GotoFrame(i);
							
						if(_stoped){
							return;
						}
					}
				}
				if(toFrame != _movie.currentFrame){
					_movie.GotoFrame(toFrame);
				}
			}
		}
		
		public override void stop ()
		{
			base.stop ();
			_stoped = true;
		}

		protected override CCAction reverseImpl ()
		{
			return new MovieImpAction (_movie, _endFrame, _startFrame);
		}

		protected override CCAction copyImpl ()
		{
			return new MovieImpAction (_movie, _startFrame, _endFrame);
		}
	}
	#endregion

}
