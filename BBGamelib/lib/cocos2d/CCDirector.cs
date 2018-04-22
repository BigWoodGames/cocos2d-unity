using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BBGamelib{
	public class CCDirector : UIViewController
	{
		#region properties
		const float kDefaultFPS	=60.0f;	// 60 frames per second

		// internal timer
		float _animationInterval;
		float _oldAnimationInterval;

		/* stats */
		bool	_displayStats;
		bool	_displayError;
		Exception _lastError;

		int _frames;
//		int _totalFrames;
	//		float _secondsPerFrame;
		float		_accumDt;
		float		_frameRate;
		uint _ccNumberOfDraws;
		uint _ccNumberOfDrawsToShow;

		/* delta time since last tick to main loop */
		float _dt;

		/* is the running scene paused */
		bool _isPaused;

		/* whether or not the next delta time will be zero */
		bool _nextDeltaTimeZero;

		/* Is the director running */
		bool _isAnimating;

		/* The running scene */
		CCScene _runningScene;

		/* will be the next 'runningScene' in the next frame */
		CCScene _nextScene;

		/* If YES, then "old" scene will receive the cleanup message */
		bool	_sendCleanupToScene;
		
		/* scheduled scenes */
		Stack<CCScene> _scenesStack;

		/* window size in pixels */
		Vector2	_winSizeInPixels;

		/* scheduler associated with this director */
		CCScheduler _scheduler;

		/* action manager associated with this director */
		CCActionManager _actionManager;

		/*display link, different from ios version*/
		CADisplayLink _displayLink;
		
		/*  OpenGLView , just a simulation of cocos2d-iPhone*/
		CCGLView _view;

		/*content scale*/
//		float _ccContentScaleFactor;
		
		/* contentScaleFactor could be simulated */
//		bool _isContentScaleSupported;

		
		CCTouchDispatcher _touchDispatcher;


		int _globolRendererSortingOrder;
		int _startGlbolRendererSortingOrder;

		#endregion

		#region accessor
		
		/** The current running Scene. Director can only run one Scene at the time */
		public CCScene runningScene{
			get{return _runningScene;}
		}
		/** Whether or not to display director statistics */
		public bool displayStats{
			get{ return _displayStats;}
			set{ _displayStats = value;}
		}

		/** Whether or not to display error on screen */
		public bool displayError{
			get{ return _displayError;}
			set{ _displayError = value;}
		}
		/** Whether or not to display director statistics */
//		@property (nonatomic, readwrite, assign) BOOL displayStats;
		/** whether or not the next delta time will be zero */
		public bool nextDeltaTimeZero{
			get{return _nextDeltaTimeZero;}
			set{_nextDeltaTimeZero = value;}
		}
		/** Whether or not the Director is paused */
		public bool isPaused{
			get{return _isPaused;}
		}
		/** Whether or not the Director is active (animating) */
		public bool isAnimating{
			get{return _isAnimating;}
		}
		/** Sets an OpenGL projection */
//		@property (nonatomic,readwrite) ccDirectorProjection projection;

		/** How many frames were called since the director started */
//		public int totalFrames{
//			get{return _totalFrames;}
//		}
		/** seconds per frame */
//		public float secondsPerFrame{
//			get{return _secondsPerFrame;}
//		}
		/** Whether or not the replaced scene will receive the cleanup message.
		 If the new scene is pushed, then the old scene won't receive the "cleanup" message.
		 If the new scene replaces the old one, the it will receive the "cleanup" message.
		 @since v0.99.0
		 */
		public bool sendCleanupToScene{
			get{return _sendCleanupToScene;}
		}

		/** This object will be visited after the main scene is visited.
		 This object MUST implement the "visit" selector.
		 Useful to hook a notification object, like CCNotifications (http://github.com/manucorporat/CCNotifications)
		 @since v0.99.5
		 */
//		@property (nonatomic, readwrite, retain) id	notificationNode;

		/** CCDirector delegate. It shall implement the CCDirectorDelegate protocol*/
//		@property (nonatomic, readwrite, assign) id<CCDirectorDelegate> delegate;

		/** CCScheduler associated with this director*/
		public CCScheduler scheduler{
			get{ return _scheduler;}
		}

		/*DisplayLink is a ticker that synchronizes timers with the refresh rate of the display.*/
		public CADisplayLink displayLink
		{
			get{ return _displayLink;}
			set{ _displayLink = value;}
		}

		/** CCActionManager associated with this director*/
		public CCActionManager actionManager{
			get{ return _actionManager;}
		}

		public uint ccNumberOfDraws{
			get{ return _ccNumberOfDraws;}
			set{_ccNumberOfDraws = value;}
		}

		public int globolRendererSortingOrder{
			get{return _globolRendererSortingOrder;}
			set{ _globolRendererSortingOrder = value;}
		}

		public int startGlobolRendererSortingOrder{
			get{return _startGlbolRendererSortingOrder;}
			set{ _startGlbolRendererSortingOrder = value;}
		}

		#endregion

		//
		// singleton stuff
		//
		static CCDirector _sharedDirector = null;
		public static CCDirector sharedDirector{
			get{
				if(_sharedDirector == null){
					#if UNITY_STANDALONE || UNITY_WEBGL
					_sharedDirector = new CCDirectorMac();
					#else
					_sharedDirector = new CCDirector();
					#endif
                }
                return _sharedDirector;
            }
        }
		public static void Reset(){
			_sharedDirector = null;
		}
		protected CCDirector()
		{
			CCDebug.Log("cocos2d: cocos2d-iphone v2.1");
			// scenes
			_runningScene = null;
			_nextScene = null;
			
//			_notificationNode = nil;

			_oldAnimationInterval = _animationInterval = 1.0f / kDefaultFPS;
			_scenesStack = new Stack<CCScene> (10);
			
			// Set default projection (3D)
//			_projection = kCCDirectorProjectionDefault;
			
			// projection delegate if "Custom" projection is used
//			_delegate = nil;
			
			// FPS
			_displayStats = false;

			_displayError = false;

//			_totalFrames = _frames = 0;
			
			// paused ?
			_isPaused = false;
			
			// running thread
//			_runningThread = null;
			
			// scheduler
			_scheduler = new CCScheduler();
			
			// action manager
			_actionManager = new CCActionManager ();
			_scheduler.scheduleUpdate (_actionManager, CCScheduler.kCCPrioritySystem, false);
			
			_winSizeInPixels = Vector2.zero;


			//CCDirectorIOS
//			_ccContentScaleFactor = 1;
//			_isContentScaleSupported = false;
			_touchDispatcher = new CCTouchDispatcher ();
		}
		
		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | Size: {2:0.0} x {3:0.0}, view = {4}>", GetType().Name, this.GetHashCode(), _winSizeInPixels.x, _winSizeInPixels.y, _view);
		}
		~CCDirector(){
			_sharedDirector = null;
//			CCDebug.Info("cocos2d: deallocing {0}", this.ToString());
		}

		//
		// Draw the Scene
		//
		public void drawScene()
		{
			
			if (_displayError && _lastError != null) {
				return;
			}
			try{
				/* calculate "global" dt */
				calculateDeltaTime ();
				
				/* tick before glClear: issue #533 */
				if( ! _isPaused )
					_scheduler.update(_dt);

				/* to avoid flickr, nextScene MUST be here: after tick and before draw.
				 XXX: Which bug is this one. It seems that it can't be reproduced with v0.9 */
				if( _nextScene != null)
					setNextScene();

				
	//			DateTime t3 = DateTime.Now;
				_globolRendererSortingOrder = _startGlbolRendererSortingOrder;
				if(_runningScene!=null)
					_runningScene.visit();
			}catch(Exception e){
				CCDebug.Log(e.ToString());
				if(_displayError){
					_lastError = e;
					throw e;
				}else{
					Application.Quit();
				}
			}
			showState ();

//			_totalFrames++;
		}
		void calculateDeltaTime()
		{
			if( _nextDeltaTimeZero ) {
				_dt = 0;
				_nextDeltaTimeZero = false;
			} else {
                //Time.smoothDeltaTime looks better than Time.deltaTime
                _dt = Time.smoothDeltaTime;
//              _dt = Time.deltaTime;
//				_dt = _animationInterval;
			}
		}
		#region Director - Memory Helper
		public void purgeCachedData()
		{
//			[CCLabelBMFont purgeCachedData];
//			if ([_sharedDirector view])
//				[[CCTextureCache sharedTextureCache] removeUnusedTextures];
//			[[CCFileUtils sharedFileUtils] purgeCachedEntries];
		}
		#endregion
		#region  Director Integration with a UIKit view
		public override UIView view
		{
			get{return _view;}
			set{
				if( value != _view) {
					
					_view = value as CCGLView;
					
					// set size
                    resetWinSize();

					((CCGLView)view).touchDelegate = _touchDispatcher;
					_touchDispatcher.dispatchEvents = true;
				}
			}
		}
		#endregion
		
		#region Director Scene Landscape
		[Obsolete("Deprecated. Use touch.location instead.")]
		public Vector2 convertTouchToGL(UITouch touch)
		{
			Vector2 uiPoint = touch.location;
			return uiPoint;
		}
		public Vector2 winSize
		{
			get{return _winSizeInPixels;}
        }

        //reset winsize when resolution changed
        public void resetWinSize()
        {
            if (_view != null)
            {
                _winSizeInPixels = _view.bounds.size;
                CCDebug.Log("cocos2d: window size: {0}", winSize);
            }
        }
		#endregion
		
		#region Director Scene Management 
		public void runWithScene(CCScene scene)
		{
			NSUtils.Assert( scene != null, "Argument must be non-nil");
			NSUtils.Assert(_runningScene == null, "This command can only be used to start the CCDirector. There is already a scene present.");
			
			pushScene (scene);
			startAnimation();
		}
		
		public void pushScene(CCScene scene)
		{
			NSUtils.Assert( scene != null, "Argument must be non-nil");
			
			_sendCleanupToScene = false;
			if (_runningScene != null){
				_runningScene.gameObject.SetActive(false);
			}

			_scenesStack.Push(scene);
			_nextScene = scene;	// _nextScene is a weak ref
		}

		public void popScene()
		{	
			NSUtils.Assert( _runningScene != null, "A running Scene is needed");
			
			_scenesStack.Pop();
			int c = _scenesStack.Count;
			
			if( c == 0 )
				this.end();
			else {
				_sendCleanupToScene = true;
				_nextScene = _scenesStack.Peek();
				_runningScene.gameObject.SetActive(_runningScene.visible);
			}
		}
		
		public void replaceScene(CCScene scene)
		{
			NSUtils.Assert( scene != null, "Argument must be non-nil");
			
			if (_runningScene != null)
			{
				_sendCleanupToScene = true;
				_scenesStack.Pop();
				_scenesStack.Push(scene);
				_nextScene = scene;	// _nextScene is a weak ref
			}
			else
			{
				pushScene (scene);
				startAnimation();
			}
		}
		public void presentScene(CCScene scene)
		{
			if (_runningScene != null)
				replaceScene (scene);
			else
				runWithScene (scene);
		}

		
		public void end()
		{
			_runningScene.onExitTransitionDidStart();
			_runningScene.onExit();
			_runningScene.cleanup();
			
			_runningScene = null;
			_nextScene = null;
			
			// remove all objects, but don't release it.
			// runWithScene might be executed after 'end'.
			_scenesStack.Clear();
			
			this.stopAnimation();

			this.view = null;

			// Purge all managers / caches
			CCAnimationCache.PurgeSharedAnimationCache();
			CCSpriteFrameCache.PurgeSharedSpriteFrameCache();
		}
		
		void setNextScene()
		{
			bool runningIsTransition = false;//[_runningScene isKindOfClass:transClass];
			bool newIsTransition = false;//[_nextScene isKindOfClass:transClass];
			
			// If it is not a transition, call onExit/cleanup
			if( ! newIsTransition ) {
				if(_runningScene!=null){
					_runningScene.onExitTransitionDidStart();
					_runningScene.onExit();
					// issue #709. the root node (scene) should receive the cleanup message too
					// otherwise it might be leaked.
					if( _sendCleanupToScene)
						_runningScene.cleanup();
				}
			}
			_runningScene = _nextScene;
			_nextScene = null;
			_runningScene.transform.parent = _view.transform;
			
			if( ! runningIsTransition ) {
				_runningScene.onEnter();
				_runningScene.onEnterTransitionDidFinish();
			}
		}
		public void pause(){
			if (_isPaused)
				return;
			_oldAnimationInterval = _animationInterval;
			
			// when paused, don't consume CPU
//			this.animationInterval=1/4.0f;
			
//			[self willChangeValueForKey:@"isPaused"];
			_isPaused = true;
//			[self didChangeValueForKey:@"isPaused"];
		}

		public void resume(){
			if( ! _isPaused )
				return;
			
			this.animationInterval=_oldAnimationInterval;
			
//			if( gettimeofday( &_lastUpdate, NULL) != 0 ) {
//				CCLOG(@"cocos2d: Director: Error in gettimeofday");
//			}
			
//			[self willChangeValueForKey:@"isPaused"];
			_isPaused = false;
//			[self didChangeValueForKey:@"isPaused"];
			
			_dt = 0;
		}
		
		void startAnimation()
		{
			_nextDeltaTimeZero = true;
			
			if(_isAnimating)
				return;
			
			
			// approximate frame rate
			// assumes device refreshes at 60 fps
			int frameInterval = Mathf.FloorToInt(_animationInterval * 60.0f);
			

			CCDebug.Log("cocos2d: animation started with frame interval: {0:0.00}", 60.0f/frameInterval);
			
			_displayLink.registWithTarget (this, mainLoop, OnGUI);
			
			_isAnimating = true;
		}
		
		void stopAnimation()
		{
			if(!_isAnimating)
				return;
			
			CCDebug.Log("cocos2d: animation stopped");
			
			_displayLink.invalidate(this);
			_isAnimating = false;
		}
		
		/** The FPS value */
		public float animationInterval
		{
			get{return _animationInterval;}
			set{
				_animationInterval = value;

                //调低全局帧数会影响所有FPS，比如GUI
//				Application.targetFrameRate = Mathf.RoundToInt(1.0f / _animationInterval);
                Application.targetFrameRate = 60;
				if(_displayLink!=null){
					stopAnimation();
					startAnimation();
				}
			}
		}

		void showState(){
			if (_displayStats) {
				_frames++;
				_accumDt += _dt;
				if(FloatUtils.Big(_accumDt,ccConfig.CC_DIRECTOR_STATS_INTERVAL)){
					_frameRate = _frames / _accumDt;
					_frames = 0;
					_accumDt = 0;
				}
				_ccNumberOfDrawsToShow = _ccNumberOfDraws;
			}
			_ccNumberOfDraws = 0;
		}

		
		// shows the statistics
		void OnGUI(CADisplayLink sender){
			if (_displayStats) {
				int w = Screen.width, h = Screen.height;
				GUIStyle style = new GUIStyle();
				style.alignment = TextAnchor.LowerLeft;
				style.fontSize = h * 4 / 100;
				style.normal.textColor = Color.white;
				
				int rH = h  / 100;
				Rect rect = new Rect(0, h - rH * 8, w, rH * 8);
				string text = string.Format("{0}\n{1:0.000}\n{2:0.0}", _ccNumberOfDrawsToShow, 1/_frameRate, _frameRate);
				GUI.Label(rect, text, style);
			}
			if (_displayError && _lastError!=null) {
				int w = Screen.width, h = Screen.height;
				GUIStyle style = new GUIStyle();
				style.alignment = TextAnchor.UpperLeft;
				style.fontSize = Mathf.RoundToInt(h * 1.8f/ 100);
				style.normal.textColor = Color.white;

				Rect rect = new Rect(0, 0, w, h);
				string text = string.Format("{0}", _lastError);

                drawQuad(new Rect(0, 0, w, h/2), new Color32(0, 0, 0, 128) ); 
				GUI.Label(rect, text, style);	
			}
		}
		void drawQuad(Rect position, Color color) {
			Texture2D texture = new Texture2D(1, 1);
			texture.SetPixel(0,0,color);
			texture.Apply();
			GUI.skin.box.normal.background = texture;
			GUI.Box(position, GUIContent.none);
		}
		#endregion

		#region mark Director - TouchDispatcher
		
		public CCTouchDispatcher touchDispatcher
		{
			get{ return _touchDispatcher; }
			set{
				if( value != _touchDispatcher ) {
					_touchDispatcher = value;
				}		
			}
		}
		#endregion

		void mainLoop(CADisplayLink sender){
			drawScene ();
		}
	}
}
