using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using BBGamelib.scheduler;
using System.Linq;

namespace BBGamelib{
	//
	// CCScheduler
	//
	/** CCScheduler is responsible of triggering the scheduled callbacks.
	 You should not use NSTimer. Instead use this class.

	 There are 2 different types of callbacks (selectors):

		- update selector: the 'update' selector will be called every frame. You can customize the priority.
		- custom selector: A custom selector will be called every frame, or with a custom interval of time

	 The 'custom selectors' should be avoided when possible. It is faster, and consumes less memory to use the 'update selector'.

	*/
	public class CCScheduler
	{
		//--------const------------
		public const uint kCCRepeatForever = uint.MaxValue - 1;
		public const int  kCCPrioritySystem = int.MinValue;
		public const int  kCCPriorityNonSystemMin =  (kCCPrioritySystem+1);

		float _timeScale;

		//
		// "updates with priority" stuff
		//
		utList<tListEntry> updatesNeg;
		utList<tListEntry> updates0;
		utList<tListEntry> updatesPos;
		utHash<int, tHashUpdateEntry> hashForUpdates;
		
		// Used for "selectors with interval"
		utHash<int, tHashTimerEntry> hashForTimers;
		tHashTimerEntry currentTarget;
		bool currentTargetSalvaged;

		// Optimization
//		TICK_IMP			impMethod;
		string			updateSelector;

		bool updateHashLocked;// If true unschedule will not remove anything from a hash. Elements will only be marked for deletion.

		bool _paused;

		//----------init------------
		public CCScheduler(){
			_timeScale = 1.0f;
			
			// used to trigger CCTimer#update
			updateSelector = "update";
//			impMethod = (TICK_IMP) [CCTimerTargetSelector instanceMethodForSelector:updateSelector];

			// updates with priority
			updates0 = new utList<tListEntry>();
			updatesNeg = new utList<tListEntry>();
			updatesPos = new utList<tListEntry>();
			hashForUpdates = new utHash<int, tHashUpdateEntry>();
			
			// selectors with interval
			currentTarget = null;
			currentTargetSalvaged = false;
			hashForTimers =  new utHash<int, tHashTimerEntry>();
			updateHashLocked = false;
			_paused = false;

		}

		
		/** Modifies the time of all scheduled callbacks.
		 You can use this property to create a 'slow motion' or 'fast forward' effect.
		 Default is 1.0. To create a 'slow motion' effect, use values below 1.0.
		 To create a 'fast forward' effect, use values higher than 1.0.
		 @since v0.8
		 @warning It will affect EVERY scheduled selector / action.
		 */
		public float timeScale{
			set{_timeScale = value;}
			get{ return _timeScale;}
		}
		
		/** Will pause / resume the CCScheduler.
		 It won't dispatch any message to any target/selector, block if it is paused.

		 The difference between `pauseAllTargets` and `pause, is that `setPaused` will pause the CCScheduler,
		 while `pauseAllTargets` will pause all the targets, one by one.
		 `setPaused` will pause the whole Scheduler, meaning that calls to `resumeTargets:`, `resumeTarget:` won't affect it.

		 @since v2.1.0
		 */
		public bool paused{
			get{return _paused;}
		}


		public override string ToString ()
		{
			return string.Format ("<%@ = {0} | timeScale ={1} >", this.GetType().Name, this.GetHashCode(), timeScale);
		}

		~CCScheduler(){
//			CCDebug.Log("cocos2d: deallocing {0}", this);
//			unscheduleAll ();
		}

		#region CCScheduler - Timers

		void removeHashElement(tHashTimerEntry element){
			hashForTimers.HASH_DEL (element.target.GetHashCode());
//			element.timers = null;
//			element.target = null;
		}


		/** The scheduled method will be called every 'interval' seconds.
		 If paused is YES, then it won't be called until it is resumed.
		 If 'interval' is 0, it will be called every frame, but if so, it recommended to use 'scheduleUpdateForTarget:' instead.
		 If the selector is already scheduled, then only the interval parameter will be updated without re-scheduling it again.
		 repeat lets the action be repeated repeat + 1 times, use kCCRepeatForever to let the action run continuously
		 delay is the amount of time the action will wait before it'll start

		 @since v0.99.3, repeat and delay added in v1.1
		 */
		public void schedule(TICK_IMP selector, System.Object target, float interval, uint repeat, bool paused, float delay=0){
			NSUtils.Assert( selector != null, "Argument selector must be non-nil");
			NSUtils.Assert( target != null, "Argument target must be non-nil");

			tHashTimerEntry element = hashForTimers.HASH_FIND_INT(target.GetHashCode());
			if (element == null) {
				element = new tHashTimerEntry ();
				element.target = target;
				hashForTimers.HASH_ADD_INT(target.GetHashCode(), element);

				// Is this the 1st element ? Then set the pause level to all the selectors of this target
				element.paused = paused;
			} else 
				NSUtils.Assert( element.paused == paused, "CCScheduler. Trying to schedule a selector with a pause value different than the target");


			if (element.timers == null)
				element.timers = new List<CCTimer> (10);
			else {
				var enumerator = element.timers.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCTimer timer = enumerator.Current;
					if(timer is CCTimerTargetSelector && selector == ((CCTimerTargetSelector)timer).selector){
						CCDebug.Log("CCScheduler#scheduleSelector. Selector already scheduled. Updating interval from: {0:0.0000} to {1:0.0000}", timer.interval, interval);
						timer.interval = interval;
						return;
					}
				}
			}
			CCTimerTargetSelector timerSelector = new CCTimerTargetSelector(target, selector, interval, repeat, delay);
			element.timers.Add(timerSelector);
		}

		public void scheduleBlockForKey(string key, System.Object owner, float interval, uint repeat, float delay, bool paused, TICK_IMP block)
		{
			NSUtils.Assert( block != null, "Argument block must be non-nil");
			NSUtils.Assert( owner != null, "Argument owner must be non-nil");
			
			tHashTimerEntry element = hashForTimers.HASH_FIND_INT(owner.GetHashCode());
			
			if (element == null) {
				element = new tHashTimerEntry ();
				element.target = owner;
				hashForTimers.HASH_ADD_INT(owner.GetHashCode(), element);
				
				// Is this the 1st element ? Then set the pause level to all the selectors of this target
				element.paused = paused;
				
			} else
				NSUtils.Assert( element.paused == paused, "CCScheduler. Trying to schedule a block with a pause value different than the target");
			
			
			if (element.timers == null)
				element.timers = new List<CCTimer> (10);
			else
			{
				var enumerator = element.timers.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCTimer timer = enumerator.Current;
					if(timer is CCTimerBlock  && key == ((CCTimerBlock)timer).key) {
						CCDebug.Log("CCScheduler#scheduleBlock. Block already scheduled. Updating interval from: {0:0.0000} to {1:0.0000}", timer.interval, interval);
						timer.interval = interval;
						return;
					}
				}
			}
			
			CCTimerBlock timerBlock = new CCTimerBlock(owner, interval, key, block, repeat, delay);
			element.timers.Add(timerBlock);
		}
		/** Unshedules a selector for a given target.
		 If you want to unschedule the "update", use unscheudleUpdateForTarget.
		 @since v0.99.3
		 */
		public void unscheduleSelector(TICK_IMP selector, System.Object target){
			if (target==null && selector == null) {
				return;			
			}
			
			NSUtils.Assert( target != null, "Target MUST not be nil");
			NSUtils.Assert( selector != null, "Selector MUST not be NULL");
			
			tHashTimerEntry element = hashForTimers.HASH_FIND_INT (target.GetHashCode());
			if (element != null) {
				int timersCount = element.timers.Count;
				for(int i=0; i< timersCount; i++ ) {
					CCTimer timer = element.timers[i];
					if(timer is CCTimerTargetSelector && selector == ((CCTimerTargetSelector)timer).selector){
						if(timer == element.currentTimer && !element.currentTimerSalvaged){
							element.currentTimerSalvaged = true;
						}
						element.timers.RemoveAt(i);
						if(element.timerIndex >= i)
							element.timerIndex --;
						if(element.timers.Count == 0){
							if(currentTarget == element)
								currentTargetSalvaged = true;
							else 
								removeHashElement(element);
						}
						return;
					}
				}
			}
			
			// Not Found
			//	NSLog(@"CCScheduler#unscheduleSelector:forTarget: selector not found: %@", selString);
		}

		
		public void unscheduleBlockForKey(string key, System.Object target)
		{
			// explicity handle nil arguments when removing an object
			if( target==null && key==null)
				return;
			
			NSUtils.Assert( target != null, "Target MUST not be nil");
			NSUtils.Assert( key != null, "key MUST not be NULL");
			
			tHashTimerEntry element = hashForTimers.HASH_FIND_INT (target.GetHashCode());
			if (element != null) {
				
				for(int i=0; i< element.timers.Count; i++ ) {
					CCTimer timer = element.timers[i];
					
					
					if( timer is CCTimerBlock &&  key == ((CCTimerBlock)timer).key ) {
						
						if( timer == element.currentTimer && !element.currentTimerSalvaged ) {
							element.currentTimerSalvaged = true;
						}
						
						element.timers.RemoveAt(i);
						
						// update timerIndex in case we are in tick:, looping over the actions
						if( element.timerIndex >= i )
							element.timerIndex--;
						
						if(element.timers.Count == 0){
							if( currentTarget == element )
								currentTargetSalvaged = true;
							else
								removeHashElement(element);
						}
						return;
					}
				}
			}
			// Not Found
			//	NSLog(@"CCScheduler#unscheduleSelector:forTarget: selector not found: %@", selString);
		}
		#endregion
		
		#region CCScheduler - Update Specific

		void priorityIn(utList<tListEntry> list, System.Object target, int priority, bool paused){
			tListEntry listEntry = new tListEntry ();
			listEntry.target = target;
			listEntry.priority = priority;
			listEntry.paused = paused;
			MethodInfo method = target.GetType ().GetMethod (updateSelector);
			listEntry.impMethod = (TICK_IMP) Delegate.CreateDelegate(typeof(TICK_IMP), target, method);
			listEntry.markedForDeletion = false;
			
			utNode<tListEntry> listElement = new utNode<tListEntry> ();
			listElement.next = listElement.prev = null;
			listElement.obj = listEntry;

			
			if (list.head == null) {
				list.DL_APPEND(listElement);
			} else {
				bool added = false;
				for( utNode<tListEntry> elem = list.head; elem != null ; elem = elem.next ) {
					if(priority < elem.obj.priority){
						
						if( elem == list.head )
							list.DL_PREPEND(listElement);
						else {
							listElement.next = elem;
							listElement.prev = elem.prev;
							
							elem.prev.next = listElement;
							elem.prev = listElement;
						}
						added = true;
						break;
					}
				}

				if(!added)
					list.DL_APPEND(listElement);
			}
			tHashUpdateEntry hashElement = new tHashUpdateEntry ();
			hashElement.target = target;
			hashElement.list = list;
			hashElement.entry = listElement;
			hashForUpdates.HASH_ADD_INT (target.GetHashCode(), hashElement);
		}
		
		void appendIn(utList<tListEntry> list, System.Object target, bool paused){
			tListEntry listEntry = new tListEntry ();
			listEntry.target = target;
			listEntry.paused = paused;
			listEntry.markedForDeletion = false;
			MethodInfo method = target.GetType ().GetMethod (updateSelector);
			listEntry.impMethod = (TICK_IMP) Delegate.CreateDelegate(typeof(TICK_IMP), target, method);
			
			utNode<tListEntry> listElement = new utNode<tListEntry> ();
			listElement.next = listElement.prev = null;
			listElement.obj = listEntry;

			list.DL_APPEND(listElement);
			
			tHashUpdateEntry hashElement = new tHashUpdateEntry ();
			hashElement.target = target;
			hashElement.list = list;
			hashElement.entry = listElement;
			hashForUpdates.HASH_ADD_INT (target.GetHashCode(), hashElement);
		}

		/** Schedules the 'update' selector for a given target with a given priority.
		 The 'update' selector will be called every frame.
		 The lower the priority, the earlier it is called.
		 @since v0.99.3
		 */
		public void scheduleUpdate(System.Object target, int priority, bool paused){
			tHashUpdateEntry hashElement = hashForUpdates.HASH_FIND_INT(target.GetHashCode());
			if (hashElement!=null) {
				if(CCDebug.COCOS2D_DEBUG>=1)
					NSUtils.Assert(hashElement.entry.obj.markedForDeletion, "CCScheduler: You can't re-schedule an 'update' selector'. Unschedule it first");			

				// TODO : check if priority has changed!
				hashElement.entry.obj.markedForDeletion = false;
				return;
			}
			
			
			// most of the updates are going to be 0, that's way there
			// is an special list for updates with priority 0
			if (priority == 0){
				appendIn (updates0, target, paused);
			}
			else if (priority < 0)
				priorityIn (updatesNeg, target, priority, paused);
			else // priority > 0
				priorityIn (updatesPos, target, priority, paused);
		}

		
		void removeUpdatesFromHash(utNode<tListEntry> entry){
			tHashUpdateEntry element = hashForUpdates.HASH_FIND_INT (entry.obj.target.GetHashCode());
			if (element != null) {
				// list entry
				element.list.DL_DELETE(element.entry);	
				element.entry = null;
				
				// hash entry
				System.Object target = element.target;
				hashForUpdates.HASH_DEL(target.GetHashCode());
			}
		}

		/** Unschedules the update selector for a given target
		 @since v0.99.3
		 */
		public void unscheduleUpdateForTarget(System.Object target){
			if(target == null)
				return;
			tHashUpdateEntry element = hashForUpdates.HASH_FIND_INT (target.GetHashCode());
			if (element != null) {
				if(updateHashLocked)
					element.entry.obj.markedForDeletion = true;
				else
					removeUpdatesFromHash(element.entry);
			}
		}

		#endregion
		
		#region CCScheduler - Common for Update selector & Custom Selectors
		
		/** Unschedules all selectors and blocks from all targets.
		 You should NEVER call this method, unless you know what you are doing.
		 @since v0.99.3
		 */
		public void unscheduleAll(){
			unscheduleAllWithMinPriority(kCCPrioritySystem);
		}
		
		
		/** Unschedules all selectors and blocks from all targets with a minimum priority.
		  You should only call this with kCCPriorityNonSystemMin or higher.
		  @since v2.0.0
		  */
		public void unscheduleAllWithMinPriority(int minPriority){
			// Custom Selectors
			for (int i=hashForTimers.Count -1; i>=0; i--) {
				KeyValuePair<int, tHashTimerEntry> keyValue = hashForTimers.ElementAt(i);
				System.Object target = keyValue.Value.target;
				unscheduleAllForTarget(target);
			}
			// Updates selectors
			if(minPriority < 0) {
				for( utNode<tListEntry> tmp = updatesNeg.head; tmp != null ; tmp = tmp.next ) {
					utNode<tListEntry> entry = tmp;
					if(entry.obj.priority >= minPriority) {
						unscheduleUpdateForTarget(entry.obj.target);
					}
				}
			}
			if(minPriority <= 0) {
				for( utNode<tListEntry> tmp = updates0.head; tmp != null ; tmp = tmp.next ) {
					utNode<tListEntry> entry = tmp;
					unscheduleUpdateForTarget(entry.obj.target);
				}
			}
			for( utNode<tListEntry> tmp = updatesPos.head; tmp != null ; tmp = tmp.next ) {
				utNode<tListEntry> entry = tmp;
				if(entry.obj.priority >= minPriority) {
					unscheduleUpdateForTarget(entry.obj.target);
				}
			}
		}

		
		/** Unschedules all selectors and blocks for a given target.
		 This also includes the "update" selector.
		 @since v0.99.3
		 */
		public void unscheduleAllForTarget(System.Object target)
		{
			// explicit nil handling
			if( target == null )
				return;
			
			// Custom Selectors
			tHashTimerEntry element = hashForTimers.HASH_FIND_INT(target.GetHashCode());
			
			if( element != null) {
				if(element.timers.Contains(element.currentTimer) && !element.currentTimerSalvaged ) {
					element.currentTimerSalvaged = true;
				}
				element.timers.Clear();
				if( currentTarget == element )
					currentTargetSalvaged = true;
				else
					removeHashElement(element);
			}
			
			// Update Selector
			unscheduleUpdateForTarget(target);
		}

		
		/** Resumes the target.
		 The 'target' will be unpaused, so all schedule selectors/update will be 'ticked' again.
		 If the target is not present, nothing happens.
		 @since v0.99.3
		 */
		public void resumeTarget(System.Object target){
			NSUtils.Assert (target != null, "target must be non nil");	
			
			// Custom Selectors
			tHashTimerEntry element = hashForTimers.HASH_FIND_INT (target.GetHashCode());
			if (element != null)
				element.paused = false;

			// Update selector
			tHashUpdateEntry elementUpdate = hashForUpdates.HASH_FIND_INT (target.GetHashCode());
			if (elementUpdate != null) {
				NSUtils.Assert(elementUpdate.entry != null, "pauseTarget: unknown error");
				elementUpdate.entry.obj.paused = false;
			}
		}

		
		
		/** Pauses the target.
		 All scheduled selectors/update for a given target won't be 'ticked' until the target is resumed.
		 If the target is not present, nothing happens.
		 @since v0.99.3
		 */
		public void pauseTarget(System.Object target){
			NSUtils.Assert (target != null, "target must be non nil");		
			
			// Custom selectors
			tHashTimerEntry element = hashForTimers.HASH_FIND_INT (target.GetHashCode());
			if (element != null)
				element.paused = true;
			
			// Update selector
			tHashUpdateEntry elementUpdate = hashForUpdates.HASH_FIND_INT (target.GetHashCode());
			if (elementUpdate != null) {
				NSUtils.Assert(elementUpdate.entry != null, "pauseTarget: unknown error");
				elementUpdate.entry.obj.paused = true;
			}
		}
		
		/** Returns whether or not the target is paused
		 @since v1.0.0
		 */
		public bool isTargetPaused(System.Object target){
			NSUtils.Assert( target != null, "target must be non nil" );
			
			// Custom selectors
			tHashTimerEntry element = hashForTimers.HASH_FIND_INT(target.GetHashCode());
			if( element != null)
				return element.paused;
			
			// We should check update selectors if target does not have custom selectors
			tHashUpdateEntry elementUpdate = hashForUpdates.HASH_FIND_INT(target.GetHashCode());
			if ( elementUpdate != null)
				return elementUpdate.entry.obj.paused;
			
			return false;  // should never get here
		}
		
		
		/** Pause all selectors and blocks from all targets.
		  You should NEVER call this method, unless you know what you are doing.
		 @since v2.0.0
		  */
		public HashSet<System.Object> pauseAllTargets(){
			return pauseAllTargetsWithMinPriority (kCCPrioritySystem);		
		}
		
		
		/** Pause all selectors and blocks from all targets with a minimum priority.
		  You should only call this with kCCPriorityNonSystemMin or higher.
		  @since v2.0.0
		  */
		public HashSet<System.Object> pauseAllTargetsWithMinPriority(int minPriority){
			HashSet<System.Object> idsWithSelectors = new HashSet<System.Object>();
			
			// Custom Selectors
			{
				var enumerator = hashForTimers.GetEnumerator();
				while (enumerator.MoveNext()) {
					KeyValuePair<int, tHashTimerEntry> kv = enumerator.Current;
					tHashTimerEntry element = kv.Value;
					element.paused = true;
					idsWithSelectors.Add(element.target);
				}
			}
			// Updates selectors
			if(minPriority < 0) {
				for( utNode<tListEntry> tmp = updatesNeg.head; tmp != null ; tmp = tmp.next ) {
					utNode<tListEntry> entry = tmp;
					if(entry.obj.priority >= minPriority) {
						entry.obj.paused = true;
						idsWithSelectors.Add(entry.obj.target);
					}
				}
			}
			if(minPriority <= 0) {
				for( utNode<tListEntry> tmp = updates0.head; tmp != null ; tmp = tmp.next ) {
					utNode<tListEntry> entry = tmp;
					entry.obj.paused = true;
					idsWithSelectors.Add(entry.obj.target);
				}
			}
			{
				
				for( utNode<tListEntry> tmp = updatesPos.head; tmp != null ; tmp = tmp.next ) {
					utNode<tListEntry> entry = tmp;
					if(entry.obj.priority >= minPriority) {
						entry.obj.paused = true;
						idsWithSelectors.Add(entry.obj.target);
					}
				}
			}
			return idsWithSelectors;
		}
		
		/** Resume selectors on a set of targets.
		 This can be useful for undoing a call to pauseAllSelectors.
		 @since v2.0.0
		  */
		public void resumeTargets(HashSet<System.Object> targetsToResume){
			var enumerator = targetsToResume.GetEnumerator();
			while (enumerator.MoveNext()) {
				var target = enumerator.Current;
				resumeTarget(target);
			}
		}

		#endregion

		
		#region mark CCScheduler - Main Loop

		//-------------update------------
		/** 'update' the scheduler.
		 You should NEVER call this method, unless you know what you are doing.
		 */
		public void update(float dt){
			if (_paused)
				return;
			updateHashLocked = true;
			if (!FloatUtils.EQ(_timeScale , 1.0f))
				dt *= _timeScale;
			
			// Iterate all over the Updates selectors
			// updates with priority < 0
			{
				
				for( utNode<tListEntry> tmp = updatesNeg.head; tmp != null ; tmp = tmp.next ) {
					utNode<tListEntry> entry = tmp;
					if(! entry.obj.paused && !entry.obj.markedForDeletion){
						entry.obj.impMethod.Invoke(dt);
					}
				}
			}
			// updates with priority == 0
			{
				for( utNode<tListEntry> tmp = updates0.head; tmp != null ; tmp = tmp.next ) {
					utNode<tListEntry> entry = tmp;
					if(! entry.obj.paused && !entry.obj.markedForDeletion)
						entry.obj.impMethod.Invoke(dt);
				}
			}
			// updates with priority > 0
			{
				
				for( utNode<tListEntry> tmp = updatesPos.head; tmp != null ; tmp = tmp.next ) {
					utNode<tListEntry> entry = tmp;
					if(! entry.obj.paused && !entry.obj.markedForDeletion)
						entry.obj.impMethod.Invoke(dt);
				}
			}

//			 Iterate all over the custom selectors (CCTimers)
			if(hashForTimers.Any()){
				var enumerator = new Dictionary<int, tHashTimerEntry>(hashForTimers).GetEnumerator();
				while (enumerator.MoveNext()) {
					var elt = enumerator.Current.Value;
					currentTarget = elt;
					currentTargetSalvaged = false;
					if( ! currentTarget.paused){
						for(elt.timerIndex = 0; elt.timerIndex < elt.timers.Count; elt.timerIndex ++ ){
							elt.currentTimer = elt.timers[elt.timerIndex];
							elt.currentTimerSalvaged = false;
							elt.currentTimer.update(dt);
							elt.currentTimer = null;
						}
					}
					if(currentTargetSalvaged && currentTarget.timers.Count == 0)
						removeHashElement(currentTarget);
				}
			}

			
			for (utNode<tListEntry> tmp = updatesNeg.head; tmp != null; tmp = tmp.next) {
				utNode<tListEntry> entry = tmp;
				if(entry.obj.markedForDeletion){
					removeUpdatesFromHash(entry);
				}
			}
			
			for (utNode<tListEntry> tmp = updates0.head; tmp != null; tmp = tmp.next) {
				utNode<tListEntry> entry = tmp;
				if(entry.obj.markedForDeletion){
					removeUpdatesFromHash(entry);
				}
			}

			
			for (utNode<tListEntry> tmp = updatesPos.head; tmp != null; tmp = tmp.next) {
				utNode<tListEntry> entry = tmp;
				if(entry.obj.markedForDeletion){
					removeUpdatesFromHash(entry);
				}
			}
			updateHashLocked = false;
			currentTarget = null;
		}
		#endregion
	}
}