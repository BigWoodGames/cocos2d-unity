using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	/**
	 CCTouchOneByOneDelegate.

	 Using this type of delegate results in two benefits:
	 1. You don't need to deal with NSSets, the dispatcher does the job of splitting
	 them. You get exactly one UITouch per call.
	 2. You can *claim* a UITouch by returning YES in ccTouchBegan. Updates of claimed
	 touches are sent only to the delegate(s) that claimed them. So if you get a move/
	 ended/cancelled update you're sure it is your touch. This frees you from doing a
	 lot of checks when doing multi-touch.

	 (The name TargetedTouchDelegate relates to updates "targeting" their specific
	 handler, without bothering the other handlers.)
	 @since v0.8
	 */
	public interface CCTouchOneByOneDelegate 
	{
		/** Return YES to claim the touch.
		 @since v0.8
		 */
		bool ccTouchBegan(UITouch touch);
		void ccTouchMoved(UITouch touch);
		void ccTouchEnded(UITouch touch);
		void ccTouchCancelled(UITouch touch);
	}
	/**
	 CCTouchAllAtOnceDelegate.

	 This type of delegate is the same one used by CocoaTouch. You will receive all the events (Began,Moved,Ended,Cancelled).
	 @since v0.8
	*/
	public interface CCTouchAllAtOnceDelegate{
		void ccTouchesBegan(HashSet<UITouch> touches);
		void ccTouchesMoved(HashSet<UITouch> touches);
		void ccTouchesEnded(HashSet<UITouch> touches);
		void ccTouchesCancelled(HashSet<UITouch> touches);
	}
}

