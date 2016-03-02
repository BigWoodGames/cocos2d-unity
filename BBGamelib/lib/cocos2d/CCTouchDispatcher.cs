using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BBGamelib{
	public enum kCCTouchSelectorFlag
	{
		BeganBit = 1 << 0,
		MovedBit = 1 << 1,
		EndedBit = 1 << 2,
		CancelledBit = 1 << 3,
		AllBits = ( BeganBit | MovedBit | EndedBit | CancelledBit),
	};
	public enum kCCTouch{
		Began=0,
		Moved=1,
		Ended=2,
		Cancelled=3,
		
		Max=4,
	};

	struct ccTouchHandlerHelperData {
		public string		touchesSel;
		public string		touchSel;
		public kCCTouchSelectorFlag  type;
		public ccTouchHandlerHelperData(string touchesSel, string touchSel, kCCTouchSelectorFlag type){
			this.touchesSel = touchesSel;
			this.touchSel = touchSel;
			this.type = type;
		}
	};
	public class CCTouchDispatcher : CCTouchDelegate
	{
		List<CCTouchHandler> targetedHandlers;
		List<CCTouchHandler> standardHandlers;
		
		bool			locked;
		bool			toAdd;
		bool			toRemove;
		ArrayList handlersToAdd;
		ArrayList handlersToRemove;
		bool			toQuit;
		
		/** Whether or not the events are going to be dispatched. Default: YES */
		public bool	dispatchEvents;
		
		// 4, 1 for each type of event
		ccTouchHandlerHelperData[] handlerHelperData;

		public CCTouchDispatcher(){
			dispatchEvents = true;
			targetedHandlers = new List<CCTouchHandler> (8);
			standardHandlers = new List<CCTouchHandler> (4);
			
			handlersToAdd = new ArrayList (8);
			handlersToRemove = new ArrayList (8);
			
			toRemove = false;
			toAdd = false;
			toQuit = false;
			locked = false;

			handlerHelperData = new ccTouchHandlerHelperData[(int)kCCTouch.Max];
			handlerHelperData[(int)kCCTouch.Began] = new ccTouchHandlerHelperData("ccTouchesBegan", "ccTouchBegan",kCCTouchSelectorFlag.BeganBit);
			handlerHelperData[(int)kCCTouch.Moved] = new ccTouchHandlerHelperData("ccTouchesMoved", "ccTouchMoved",kCCTouchSelectorFlag.MovedBit);
			handlerHelperData[(int)kCCTouch.Ended] = new ccTouchHandlerHelperData("ccTouchesEnded", "ccTouchEnded",kCCTouchSelectorFlag.EndedBit);
			handlerHelperData[(int)kCCTouch.Cancelled] = new ccTouchHandlerHelperData("ccTouchesCancelled", "ccTouchCancelled",kCCTouchSelectorFlag.CancelledBit);
		}

		
		//
		// handlers management
		//
		#region mark TouchDispatcher - Helpers
		bool removeDelegate(System.Object del, ArrayList fromQueue)
		{
			System.Object handlerToRemove = null;
			
			var enumerator = fromQueue.GetEnumerator();
			while (enumerator.MoveNext()) {
				System.Object handlerOrDelegate = enumerator.Current;
				
				if( handlerOrDelegate is CCTouchHandler ) {
					// it is a handler
					if (del == ((CCTouchHandler)handlerOrDelegate).delegate_) {
						handlerToRemove = handlerOrDelegate;
						break;
					}
				} else {
					// it is a delegate
					if (del == handlerOrDelegate) {
						handlerToRemove = handlerOrDelegate;
						break;
					}
				}
			}
			
			if( handlerToRemove!=null ) {
				fromQueue.Remove(handlerToRemove);
				return true;
			}
			
			return false;
		}
		#endregion

		
		#region mark TouchDispatcher - Add Hanlder
		
		void forceAddHandler(CCTouchHandler handler,List<CCTouchHandler> array)
		{
			int i = 0;
			
			var enumerator = array.GetEnumerator();
			while (enumerator.MoveNext()) {
				CCTouchHandler h = enumerator.Current;
				if( h.priority < handler.priority )
					i++;
				
				NSUtils.Assert( h.delegate_ != handler.delegate_, "Delegate already added to touch dispatcher.");
			}
			array.Insert (i, handler);
		}
		
		/** Adds a standard touch delegate to the dispatcher's list.
		 See StandardTouchDelegate description.
		 IMPORTANT: The delegate will be retained.
		 */
		public void addStandardDelegate(CCTouchAllAtOnceDelegate aDelegate, int priority)
		{
			CCTouchHandler handler = new CCStandardTouchHandler(aDelegate, priority);
			if( ! locked ) {
				forceAddHandler(handler, standardHandlers);
			} else {
				if(!removeDelegate(aDelegate, handlersToRemove)){
					handlersToAdd.Add(handler);
					toAdd = true;
				}
			}
		}
		
		public void addTargetedDelegate(CCTouchOneByOneDelegate aDelegate, int priority, bool swallowsTouches)
		{
			CCTouchHandler handler = new CCTargetedTouchHandler(aDelegate, priority, swallowsTouches);
			if( ! locked ) {
				forceAddHandler(handler, targetedHandlers);
			} else {
				if(!removeDelegate(aDelegate, handlersToRemove)){
					handlersToAdd.Add(handler);
					toAdd = true;
				}
			}
		}
		#endregion

		
		#region mark TouchDispatcher - removeDelegate
		
		void forceRemoveDelegate(System.Object aDelegate)
		{
			// XXX: remove it from both handlers ???
			{
				var enumerator = targetedHandlers.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCTouchHandler handler = enumerator.Current;
					if( handler.delegate_ == aDelegate ) {
						targetedHandlers.Remove(handler);
						break;
					}
				}
			}
			{
				var enumerator = standardHandlers.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCTouchHandler handler = enumerator.Current;
					if( handler.delegate_ == aDelegate ) {
						standardHandlers.Remove(handler);
						break;
					}
				}
			}
		}
		/** Removes a touch delegate.
		 The delegate will be released
		 */
		public void removeDelegate(System.Object aDelegate)
		{
			if( aDelegate == null )
				return;
			
			if( ! locked ) {
				forceRemoveDelegate(aDelegate);
			} else {
				if( ! removeDelegate(aDelegate, handlersToAdd) ) {
					handlersToRemove.Add(aDelegate);
					toRemove = true;
				}
			}
		}
		#endregion

		
		#region mark TouchDispatcher  - removeAllDelegates
		
		void forceRemoveAllDelegates()
		{
			standardHandlers.Clear ();
			targetedHandlers.Clear ();
		}
		/** Removes all touch delegates, releasing all the delegates */
		public void removeAllDelegates()
		{
			if (! locked)
				forceRemoveAllDelegates ();
			else
				toQuit = true;
		}
		#endregion

		
		#region mark Changing priority of added handlers
		
		CCTouchHandler findHandler(System.Object aDelegate)
		{
			{
				var enumerator = targetedHandlers.GetEnumerator();
				while (enumerator.MoveNext()) {
					var handler = enumerator.Current;
					if( handler.delegate_ == aDelegate ) {
						return handler;
					}
				}
			}
			{
				var enumerator = standardHandlers.GetEnumerator();
				while (enumerator.MoveNext()) {
					var handler = enumerator.Current;
					if( handler.delegate_ == aDelegate ) {
						return handler;
					}
				}
			}
			
			if (toAdd) {
				var enumerator = handlersToAdd.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCTouchHandler handler = (CCTouchHandler)enumerator.Current;
					if (handler.delegate_ == aDelegate) {
						return handler;
					}
				}
			}

			return null;
		}

		int sortByPriority(CCTouchHandler first, CCTouchHandler second)
		{
			if (first.priority < second.priority)
				return -1;
			else if (first.priority > second.priority)
				return 1;
			else
				return 0;
		}

		void rearrangeHandlers(List<CCTouchHandler> array)
		{
			array.Sort (sortByPriority);
		}

		/** Changes the priority of a previously added delegate. The lower the number,
		 the higher the priority */
		public void setPriority(int priority, System.Object aDelegate)
		{
			NSUtils.Assert(aDelegate != null, "Got nil touch delegate!");
			
			CCTouchHandler handler = null;
			handler = findHandler(aDelegate);
			
			NSUtils.Assert(handler != null, "Delegate not found!");
			
			handler.priority = priority;
			
			rearrangeHandlers(targetedHandlers);
			rearrangeHandlers(standardHandlers);
		}
		
		//
		// dispatch events
		//
		void touches(HashSet<UITouch> touches, kCCTouch touchType)
		{
			NSUtils.Assert((int)touchType < 4, "Invalid idx value");
			
			HashSet<UITouch> mutableTouches;
			locked = true;
			
			// optimization to prevent a mutable copy when it is not necessary
			int targetedHandlersCount = targetedHandlers.Count;
			int standardHandlersCount = standardHandlers.Count;
			bool needsMutableSet = (targetedHandlersCount!=0 && standardHandlersCount!=0);
			
			mutableTouches = needsMutableSet?(new HashSet<UITouch>(touches)):touches;
			
			ccTouchHandlerHelperData helper = handlerHelperData[(int)touchType];
			//
			// process the target handlers 1st
			//
			if( targetedHandlersCount > 0 ) {
				var enumerator = touches.GetEnumerator();
				while (enumerator.MoveNext()) {
					UITouch touch = enumerator.Current;

					var targetedHandlersEnumerator = targetedHandlers.GetEnumerator();
					while (targetedHandlersEnumerator.MoveNext()) {
						CCTargetedTouchHandler handler = (CCTargetedTouchHandler)targetedHandlersEnumerator.Current;						
						bool claimed = false;
						if( touchType == kCCTouch.Began ) {
							claimed = ((CCTouchOneByOneDelegate)handler.delegate_).ccTouchBegan(touch);
							if( claimed )
								handler.claimedTouches.Add(touch);
						}
						
						// else (moved, ended, cancelled)
						else if( handler.claimedTouches.Contains(touch) ) {
							claimed = true;
							if( ((int)handler.enabledSelectors & (int)(helper.type)) != 0){
								Type thisType = handler.delegate_.GetType();
								MethodInfo theMethod = thisType.GetMethod(helper.touchSel);
								theMethod.Invoke(handler.delegate_, new object[1]{touch});
							}
							if( ((int)helper.type & ((int)kCCTouchSelectorFlag.CancelledBit | (int)kCCTouchSelectorFlag.EndedBit)) != 0)
								handler.claimedTouches.Remove(touch);
						}
						
						if( claimed && handler.swallowsTouches ) {
							if( needsMutableSet )
								mutableTouches.Remove(touch);
							break;
						}
					}
				}
			}
			
			//
			// process standard handlers 2nd
			//
			if( standardHandlersCount > 0 && mutableTouches.Count>0 ) {
				
				var enumerator = standardHandlers.GetEnumerator();
				while (enumerator.MoveNext()) {
					var handler = enumerator.Current;
					if( ((int)handler.enabledSelectors & ((int)(helper.type))) != 0){
						Type thisType = handler.delegate_.GetType();
						MethodInfo theMethod = thisType.GetMethod(helper.touchesSel);
						theMethod.Invoke(handler.delegate_, new object[1]{mutableTouches});
					}
				}
			}
			if( needsMutableSet )
				mutableTouches = null;
			
			//
			// Optimization. To prevent a [handlers copy] which is expensive
			// the add/removes/quit is done after the iterations
			//
			locked = false;
			
			//issue 1084, 1139 first add then remove
			if( toAdd ) {
				toAdd = false;
				
				var enumerator = handlersToAdd.GetEnumerator();
				while (enumerator.MoveNext()) {
					var handler = enumerator.Current;
					if( handler is CCTargetedTouchHandler )
						forceAddHandler((CCTouchHandler)handler, targetedHandlers);
					else
						forceAddHandler((CCTouchHandler)handler, standardHandlers);
				}
				handlersToAdd.Clear();
			}
			
			if( toRemove ) {
				toRemove = false;

				
				var enumerator = handlersToRemove.GetEnumerator();
				while (enumerator.MoveNext()) {
					var aDelegate = enumerator.Current;
					forceRemoveDelegate(aDelegate);
				}
				handlersToRemove.Clear();
			}
			
			if( toQuit ) {
				toQuit = false;
				forceRemoveAllDelegates();
			}
		}
		public void touchesBegan(HashSet<UITouch> aTouches)
		{
			if (dispatchEvents)
				touches (aTouches, kCCTouch.Began);
		}
		public void touchesMoved(HashSet<UITouch> aTouches)
		{
			if( dispatchEvents )
				touches (aTouches, kCCTouch.Moved);
		}
		
		public void touchesEnded(HashSet<UITouch> aTouches)
		{
			if( dispatchEvents )
				touches (aTouches, kCCTouch.Ended);
		}
		
		public void touchesCancelled(HashSet<UITouch> aTouches)
		{
			if( dispatchEvents )
				touches (aTouches, kCCTouch.Cancelled);
		}

		#endregion

		
		// don't call if you don't know it well
		public List<CCTouchHandler> getTargetedHandlers(){
			return targetedHandlers;
		}
		// don't call if you don't know it well
		public List<CCTouchHandler> getStandardHandlers(){
			return standardHandlers;
		}
	}
}
