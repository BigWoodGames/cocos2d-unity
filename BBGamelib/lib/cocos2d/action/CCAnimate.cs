using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BBGamelib{
	/** Animates a sprite given the name of an Animation */
    public class CCAnimate : CCActionInterval
	{
		/*******************/
		/** Notifications **/
		/*******************/
		/** @def CCAnimationFrameDisplayedNotification
		 Notification name when a CCSpriteFrame is displayed
		 */
		public const string CCAnimationFrameDisplayedNotification = "CCAnimationFrameDisplayedNotification";

		List<float>		_splitTimes;
		int			_nextFrame;
		CCAnimation			_animation;
		CCSpriteFrame					_origFrame;
		uint			_executedLoops;

		
		// Animation used for the sprite.
		public CCAnimation animation{set{_animation=value;} get{return _animation;}}
		
		
		/** creates the action with an Animation and will restore the original frame when the animation is over */
		public CCAnimate(CCAnimation animation){
			initWithAnimation (animation);		
		}
		
		/** initializes the action with an Animation and will restore the original frame when the animation is over */
		public void initWithAnimation(CCAnimation anim){
			NSUtils.Assert( anim!=null, "Animate: argument Animation must be non-nil");
			
			float singleDuration = anim.duration;
			base.initWithDuration (singleDuration * anim.loops);
			_nextFrame = 0;
			this.animation = anim;
			_origFrame = null;
			_executedLoops = 0;

			_splitTimes = new List<float> (anim.frames.Count);
			float accumUnitsOfTime = 0;
			float newUnitOfTimeValue = singleDuration / anim.totalDelayUnits;

			
			var enumerator = anim.frames.GetEnumerator();
			while (enumerator.MoveNext()) {
				var frame = enumerator.Current;
				float value =  (accumUnitsOfTime * newUnitOfTimeValue) / singleDuration;
				accumUnitsOfTime += frame.delayUnits;
				
				_splitTimes.Add(value);
			}	
		}

		protected override CCAction copyImpl ()
		{
			CCAnimate animate = new CCAnimate(_animation.copy());
			return animate;
		}

		public override void startWithTarget (object aTarget)
		{
			base.startWithTarget (aTarget);
			CCSprite sprite = _target as CCSprite;
			
			_origFrame = null;
			if( _animation.restoreOriginalFrame )
				_origFrame = sprite.displayedFrame;
			
			_nextFrame = 0;
			_executedLoops = 0;
		}

		public override void stop ()
		{
			if( _animation.restoreOriginalFrame ) {
				CCSprite sprite = _target as CCSprite;
				sprite.displayedFrame = _origFrame;
			}
			base.stop ();
		}
		public override void update (float t)
		{
			// if t==1, ignore. Animation should finish with t==1
			if( FloatUtils.Small( t , 1.0f) ) {
				t *= _animation.loops;
				
				// new loop?  If so, reset frame counter
				uint loopNumber = (uint)t;
				if( loopNumber > _executedLoops ) {
					_nextFrame = 0;
					_executedLoops++;
				}

				// new t for animations
				t = (t % 1.0f);
			}
			
			List<CCAnimationFrame> frames = _animation.frames;
			uint numberOfFrames = (uint)frames.Count;
			CCSpriteFrame  frameToDisplay = null;
			
			for( int i=_nextFrame; i < numberOfFrames; i++ ) {
				float splitTime = _splitTimes[i];
				
				if(FloatUtils.ES( splitTime , t ) ) {
					CCAnimationFrame frame = frames[i];
					frameToDisplay = frame.spriteFrame;
					((CCSprite)_target).displayedFrame = frameToDisplay;

					NSDictionary dict = frame.userInfo;
					if( dict != null)
//						NSNotificationCenter.defaultCenter.postNotification(CCAnimationFrameDisplayedNotification, _target, dict);
					
					_nextFrame = i+1;
				}
				// Issue 1438. Could be more than one frame per tick, due to low frame rate or frame delta < 1/FPS
				else
					break;
			}
		}

		protected override CCAction reverseImpl ()
		{
			List<CCAnimationFrame> oldArray = _animation.frames;
			List<CCAnimationFrame> newArray = new List<CCAnimationFrame> (oldArray.Count);
			
			var enumerator = oldArray.GetEnumerator();
			while (enumerator.MoveNext()) {
				var frame = enumerator.Current;
				newArray.Add(frame.copy());			
			}
			newArray.Reverse ();
			
			
			CCAnimation newAnim = new CCAnimation (newArray, _animation.delayPerUnit, _animation.loops);
			newAnim.restoreOriginalFrame = _animation.restoreOriginalFrame;
			CCAnimate animate = new CCAnimate(animation);
			return animate;
		}
	}
}

