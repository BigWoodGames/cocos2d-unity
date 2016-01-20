using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace BBGamelib{
	public enum kCCTouchesMode{
		AllAtOnce,
		OneByOne,
	}
	
	#region mark Layer
	public class CCLayer : CCNode, CCTouchAllAtOnceDelegate, CCTouchOneByOneDelegate
	{
		protected bool _touchEnabled;
		protected int _touchPriority;
		protected kCCTouchesMode _touchMode;
		protected bool _touchSwallow;
//		BOOL _accelerometerEnabled;

		protected override void init()
		{
			base.init ();

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
	}
	#endregion
	
	#region mark CCLayerRGBA
	/** CCLayerRGBA is a subclass of CCLayer that implements the CCRGBAProtocol protocol using a solid color as the background.

	 All features from CCLayer are valid, plus the following new features that propagate into children that conform to the CCRGBAProtocol:
	 - opacity
	 - RGB colors
	 @since 2.1
	 */
	public class CCLayerRGBA : CCLayer, CCRGBAProtocol
	{
		byte		_displayedOpacity, _realOpacity;
		Color32	_displayedColor, _realColor;
		bool		_cascadeOpacityEnabled, _cascadeColorEnabled;
	

		protected override void init ()
		{
			base.init ();
			
			_displayedOpacity = _realOpacity = 255;
			_displayedColor = _realColor = Color.white;
			this.cascadeOpacityEnabled = false;
			this.cascadeColorEnabled = false;
		}

		public bool cascadeOpacityEnabled{get{return _cascadeOpacityEnabled;} set{_cascadeOpacityEnabled=value;}}
		public bool cascadeColorEnabled{get{return _cascadeColorEnabled;} set{_cascadeColorEnabled=value;}}
		public bool opacityModifyRGB{get{return false;} set{}}
		public byte opacity{
			get{return _realOpacity;}
			
			/** Override synthesized setOpacity to recurse items */
			set{
				_displayedOpacity = _realOpacity = value;
				
				if( _cascadeOpacityEnabled ) {
					byte parentOpacity = 255;
					if( _parent is CCRGBAProtocol && ((CCRGBAProtocol)_parent).cascadeOpacityEnabled)
						parentOpacity = ((CCRGBAProtocol)_parent).displayedOpacity;
					updateDisplayedOpacity(parentOpacity);
				}		
			}
		}

		public byte displayedOpacity{get{return _displayedOpacity;}}
		public Color32 displayedColor{get{return _displayedColor;}}

		public Color32 color{
			get{return _realColor;}
			set{
				_displayedColor = _realColor = value;
				
				if( _cascadeColorEnabled ) {
					Color32 parentColor = Color.white;
					if( _parent is CCRGBAProtocol && ((CCRGBAProtocol)_parent).cascadeColorEnabled )
						parentColor = ((CCRGBAProtocol)_parent).displayedColor;
					updateDisplayedColor(parentColor);
				}
			}
		} 

		public void updateDisplayedOpacity(byte parentOpacity){
			_displayedOpacity = (byte)(_realOpacity * parentOpacity/255.0f);
			
			if (_cascadeOpacityEnabled) {

				var enumerator = _children.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCNode item = enumerator.Current;
					if (item is CCRGBAProtocol) {
						((CCRGBAProtocol)item).updateDisplayedOpacity(_displayedOpacity);
					}
				}
			}
		}
		
		public void updateDisplayedColor(Color32 parentColor){
			_displayedColor.r = (byte)(_realColor.r * parentColor.r/255.0f);
			_displayedColor.g = (byte)(_realColor.g * parentColor.g/255.0f);
			_displayedColor.b = (byte)(_realColor.b * parentColor.b/255.0f);
			
			if (_cascadeColorEnabled) {

				var enumerator = _children.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCNode item = enumerator.Current;
					if (item is CCRGBAProtocol) {
						((CCRGBAProtocol)item).updateDisplayedColor(_displayedColor);
					}
				}
			}		
		}
	}
	#endregion
}

