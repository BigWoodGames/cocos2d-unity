using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	#region mark TouchHandler
	public class CCTouchHandler 
	{
		protected System.Object _delegate;
		protected int _priority;
		protected kCCTouchSelectorFlag _enabledSelectors;

		/** delegate */
		public System.Object delegate_{
			set{_delegate = value;}
			get{ return _delegate;}
		}
		/** priority */
		// default 0
		public int priority{
			set{_priority = value;}
			get{ return _priority;}
		}
		/** enabled selectors */
		public kCCTouchSelectorFlag enabledSelectors{
			set{ _enabledSelectors = value;}
			get{ return _enabledSelectors;}
		}
		
		/** allocates a TouchHandler with a delegate and a priority */
		public CCTouchHandler(System.Object aDelegate, int aPriority){
			init (aDelegate, aPriority);
		}
		/** initializes a TouchHandler with a delegate and a priority */
		protected virtual void init(System.Object aDelegate, int aPriority){
			this.delegate_ = aDelegate;
			_priority = aPriority;
			_enabledSelectors = 0;
		}
	}
	#endregion

	
	#region mark StandardTouchHandler
	/** CCStandardTouchHandler
	 It forwards each event to the delegate.
	 */
	public class CCStandardTouchHandler : CCTouchHandler
	{
		public CCStandardTouchHandler(System.Object aDelegate, int aPriority):base(aDelegate, aPriority){
		}
		protected override void init(System.Object aDelegate, int aPriority){
			base.init (aDelegate, aPriority);
			if(NSUtils.hasMethod(aDelegate, "ccTouchesBegan"))
				_enabledSelectors |= kCCTouchSelectorFlag.BeganBit;
			if(NSUtils.hasMethod(aDelegate, "ccTouchesMoved"))
				_enabledSelectors |= kCCTouchSelectorFlag.MovedBit;
			if(NSUtils.hasMethod(aDelegate, "ccTouchesEnded"))
				_enabledSelectors |= kCCTouchSelectorFlag.EndedBit;
			if(NSUtils.hasMethod(aDelegate, "ccTouchesCancelled"))
				_enabledSelectors |= kCCTouchSelectorFlag.CancelledBit;
		}
	}
	#endregion

	#region mark TargetedTouchHandler
	/**
	 CCTargetedTouchHandler
	 Object than contains the claimed touches and if it swallows touches.
	 Used internally by TouchDispatcher
	 */
	public class CCTargetedTouchHandler : CCTouchHandler
	{
		bool _swallowsTouches;
		HashSet<UITouch> _claimedTouches;

		/** whether or not the touches are swallowed */
		public bool swallowsTouches{set{_swallowsTouches=value;} get{return _swallowsTouches;}} // default NO
		/** MutableSet that contains the claimed touches */
		public HashSet<UITouch> claimedTouches{set{_claimedTouches=value;} get{return _claimedTouches;}}

		/** allocates a TargetedTouchHandler with a delegate, a priority and whether or not it swallows touches or not */
		public CCTargetedTouchHandler(System.Object aDelegate,int priority, bool swallowsTouches):base(aDelegate, priority){
			init (aDelegate, priority, swallowsTouches);
		}
		/** initializes a TargetedTouchHandler with a delegate, a priority and whether or not it swallows touches or not */
		public void init(System.Object aDelegate,int priority, bool swallowsTouches){


			_claimedTouches = new HashSet<UITouch> ();
			_swallowsTouches = swallowsTouches;

			if(NSUtils.hasMethod(aDelegate, "ccTouchBegan"))
				_enabledSelectors |= kCCTouchSelectorFlag.BeganBit;
			if(NSUtils.hasMethod(aDelegate, "ccTouchMoved"))
				_enabledSelectors |= kCCTouchSelectorFlag.MovedBit;
			if(NSUtils.hasMethod(aDelegate, "ccTouchEnded"))
				_enabledSelectors |= kCCTouchSelectorFlag.EndedBit;
			if(NSUtils.hasMethod(aDelegate, "ccTouchCancelled"))
				_enabledSelectors |= kCCTouchSelectorFlag.CancelledBit;
		}
	}
	#endregion
}
