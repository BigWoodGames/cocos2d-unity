using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace BBGamelib
{
	public class CCLayerBase : CCNode, CCTouchAllAtOnceDelegate, CCTouchOneByOneDelegate, CCKeyboardEventDelegate, CCMouseEventDelegate
	{
		public enum kCCTouchesMode{
			AllAtOnce,
			OneByOne,
		}

		protected bool _touchEnabled;
		protected int _touchPriority;
		protected kCCTouchesMode _touchMode;
		protected bool _touchSwallow;

		//NOT SUPPORTED YET
//		BOOL _accelerometerEnabled;

		protected override void init()
		{
            base.init ();
            this.nameInHierarchy = "layer";

			Vector2 s = CCDirector.sharedDirector.winSize;
			_anchorPoint = new Vector2 (0.5f, 0.5f);
			this.contentSize = s;
			this.ignoreAnchorPointForPosition = true;

			_touchEnabled = false;
			_touchPriority = 0;
			_touchMode = kCCTouchesMode.AllAtOnce;
			_touchSwallow = true;
//			_accelerometerEnabled = NO;
		}

		
		#region mark Layer - iOS - Touch and Accelerometer related
		protected virtual void registerWithTouchDispatcher()
		{
			CCDirector director = CCDirector.sharedDirector;
			
			if( _touchMode == kCCTouchesMode.AllAtOnce )
				director.touchDispatcher.addStandardDelegate(this, _touchPriority);
			else /* one by one */
				director.touchDispatcher.addTargetedDelegate(this, _touchPriority,_touchSwallow);
		}
		public virtual bool isTouchEnabled{
			get{ return _touchEnabled;}
			set{
				if( _touchEnabled != value ) {
					_touchEnabled = value;
					if( _isRunning) {
						if( value )
							registerWithTouchDispatcher();
						else {
							CCDirector director = CCDirector.sharedDirector;
							director.touchDispatcher.removeDelegate(this);
						}
					}
				}	
			}
		}
		public virtual int touchPriority{
			get{ return _touchPriority;}
			set{
				if( _touchPriority != value ) {
					_touchPriority = value;
					
					if( _touchEnabled) {
						this.isTouchEnabled = false;
						this.isTouchEnabled = true;
					}
				}			
			}
		}

		public virtual kCCTouchesMode touchMode{
			get{ return _touchMode;}
			set{
				if( _touchMode != value ) {
					_touchMode = value;
					if( _touchEnabled) {
						this.isTouchEnabled = false;
						this.isTouchEnabled = true;
					}
				}	
			}
		}

		public virtual bool touchSwallow{
			get{return _touchSwallow;}
			set{
				if( _touchSwallow != value ) {
					_touchSwallow = value;
					if( _touchEnabled) {
						this.isTouchEnabled = false;
						this.isTouchEnabled = true;
					}
				}			
			}
		}


		#endregion

		
		#region mark Layer - Callbacks
		public override void onEnter ()
		{		
			// register 'parent' nodes first
			// since events are propagated in reverse order
			if (_touchEnabled)
				registerWithTouchDispatcher();
			
			
			// then iterate over all the children
			base.onEnter ();
		}
		
		// issue #624.
		// Can't register mouse, touches here because of #issue #1018, and #1021
		public override void onEnterTransitionDidFinish ()
		{
			base.onEnterTransitionDidFinish ();
		}

		public override void onExit ()
		{
			CCDirector director = CCDirector.sharedDirector;
			if( _touchEnabled )
				director.touchDispatcher.removeDelegate(this);

			base.onExit ();
		}
		public virtual bool ccTouchBegan(UITouch touch)
		{
			NSUtils.Assert(false, "Layer#ccTouchBegan override me");
			return true;
		}
		
		public virtual void ccTouchMoved(UITouch touch){}
		public virtual void ccTouchEnded(UITouch touch){}
		public virtual void ccTouchCancelled(UITouch touch){}
		public virtual void ccTouchesBegan(HashSet<UITouch> touches){}
		public virtual void ccTouchesMoved(HashSet<UITouch> touches){}
		public virtual void ccTouchesEnded(HashSet<UITouch> touches){}
		public virtual void ccTouchesCancelled(HashSet<UITouch> touches){}
		#endregion
		#region keyboard & mouse compatible
		public virtual bool isMouseEnabled{
			get{return false;}
			set{ }
		}

		/** priority of the mouse events. Default is 0 */
		public virtual int mousePriority{
			get{ return 0;}
			set{		}
		}

		/** whether or not it will receive keyboard events.

		 Valid only on OS X. Not valid on iOS
		 */
		public virtual bool isKeyboardEnabled{
			get{ return false;}
			set{}
		}
		/** Priority of keyboard events. Default is 0 */
		//		@property (nonatomic, assign) NSInteger keyboardPriority;
		public virtual int keyboardPriority{
			get{ return 0;}
			set{		}
		}

		public virtual bool ccKeyDown(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccKeyUp(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccMouseDown(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccMouseUp(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccMouseMoved(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccMouseDragged(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccRightMouseDown(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccRightMouseDragged(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccRightMouseUp(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccOtherMouseDown(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccOtherMouseDragged(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccOtherMouseUp(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccScrollWheel(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccMouseEntered(NSEvent theEvent){
			return false; 
		}
		public virtual bool ccMouseExited(NSEvent theEvent){
			return false; 
		}
		#endregion
	}

	#if UNITY_STANDALONE || UNITY_WEBGL
	public class CCLayer : CCLayerBase
	{
		bool	_mouseEnabled;
		int		_mousePriority;

		bool	_keyboardEnabled;
		int		_keyboardPriority;

		protected override void init()
		{
			base.init ();

			_mouseEnabled = false;
			_mousePriority = 0;
			_keyboardEnabled = false;
			_keyboardPriority = 0;
		}

		public override void onEnter ()
		{
			base.onEnter ();
			CCEventDispatcher eventDispatcher = CCDirectorMac.sharedDirector.eventDispatcher;

			if( _mouseEnabled )
				eventDispatcher.addMouseDelegate(this, _mousePriority);

			if( _keyboardEnabled)
				eventDispatcher.addKeyboardDelegate(this, _keyboardPriority);
		}

		public override void onExit ()
		{
			CCEventDispatcher eventDispatcher = CCDirectorMac.sharedDirector.eventDispatcher;
			if( _mouseEnabled )
				eventDispatcher.removeMouseDelegate(this);

			if( _keyboardEnabled )
				eventDispatcher.removeKeyboardDelegate(this);
			base.onExit ();
		}

		/** whether or not it will receive mouse events.

		 Valid only on OS X. Not valid on iOS
		 */
		public override bool isMouseEnabled{
			get{ return _mouseEnabled;}
			set{
				if( _mouseEnabled != value ) {
					_mouseEnabled = value;
					if( _isRunning) {
						CCDirectorMac director = (CCDirectorMac)CCDirector.sharedDirector;
						if( value )
							director.eventDispatcher.addMouseDelegate(this, _mousePriority);
						else {
							director.eventDispatcher.removeMouseDelegate(this);
						}
					}
				}	
			}
		}

		/** priority of the mouse events. Default is 0 */
		public override int mousePriority{
			get{ return _mousePriority;}
			set{
				if( _mousePriority != value ) {
					_mousePriority = value;

					if( _mouseEnabled) {
						this.isMouseEnabled = false;
						this.isMouseEnabled = true;
					}
				}			
			}
		}


		/** whether or not it will receive keyboard events.

		 Valid only on OS X. Not valid on iOS
		 */
		public override bool isKeyboardEnabled{
			get{ return _keyboardEnabled;}
			set{
				if( _keyboardEnabled != value ) {
					_keyboardEnabled = value;
					if( _isRunning) {
						CCDirectorMac director = (CCDirectorMac)CCDirector.sharedDirector;
						if( value )
							director.eventDispatcher.addKeyboardDelegate(this, _keyboardPriority);
						else {
							director.eventDispatcher.removeKeyboardDelegate(this);
						}
					}
				}	
			}
		}
		/** Priority of keyboard events. Default is 0 */
		//		@property (nonatomic, assign) NSInteger keyboardPriority;
		public override int keyboardPriority{
			get{ return _keyboardPriority;}
			set{
				if( _keyboardPriority != value ) {
					_keyboardPriority = value;

					if( _keyboardEnabled) {
						this.isKeyboardEnabled = false;
						this.isKeyboardEnabled = true;
					}
				}			
			}
		}
	}
	#else
	public class CCLayer : CCLayerBase{}
	#endif
}

