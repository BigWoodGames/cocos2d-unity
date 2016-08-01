using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BBGamelib{
	class tHashElement{
		public List<CCAction> actions;
		public int actionIndex;
		public bool paused;
		public bool currentActionSalvaged;
		
		public System.Object target;
		public CCAction currentAction;

		public int hashKey()
		{
			return target.GetHashCode ();		
		}
	}

	public class CCActionManager
	{
		utHash<int, tHashElement> _targets;
		tHashElement _currentTarget;
		bool _currentTargetSavlvaged;

		public CCActionManager(){
			_targets = new utHash<int, tHashElement>();
		}

		public override string ToString ()
		{
			return string.Format ("<{0} = {1}>", this.GetType().Name, this.GetHashCode());
		}

		~CCActionManager(){
			//removeall actions
		}

		#region ActionManager - Private
		void deleteHashElement(tHashElement element){
			element.actions = null;
			_targets.HASH_DEL (element.target.GetHashCode());
			element.target = null;
		}
		
		void actionAllocWithHashElement(tHashElement element){
			if (element.actions == null)
				element.actions = new List<CCAction> (4);
			else if (element.actions.Count == element.actions.Capacity)
				element.actions.Capacity *= 2;
		}
		void removeActionAtIndex(int index, tHashElement element){
			CCAction action = element.actions [index];
			if (action == element.currentAction && !element.currentActionSalvaged) {
				element.currentActionSalvaged = true;			
			}
			element.actions.RemoveAt (index);
			if (element.actionIndex == index)
				element.actionIndex --;
			if (element.actions.Count == 0) {
				if(_currentTarget == element)
					_currentTargetSavlvaged = true;
				else
					deleteHashElement(element);
			}
		}
		#endregion

		
		#region ActionManager - Pause / Resume
		public void pauseTarget(System.Object target){
			tHashElement element = _targets.HASH_FIND_INT (target.GetHashCode());
			if (element != null)
				element.paused = true;
		}
		public void resumeTarget(System.Object target){
			tHashElement element = _targets.HASH_FIND_INT (target.GetHashCode());
			if (element != null)
				element.paused = false;
		}
//		-(NSSet *) pauseAllRunningActions
//		{
//			NSMutableSet* idsWithActions = [NSMutableSet setWithCapacity:50];
//			
//			for(tHashElement *element=targets; element != NULL; element=element->hh.next) {
//				if( !element->paused ) {
//					element->paused = YES;
//					[idsWithActions addObject:element->target];
//				}
//			}
//			return idsWithActions;
//		}
//		
//		-(void) resumeTargets:(NSSet *)targetsToResume
//		{
//			for(id target in targetsToResume) {
//				[self resumeTarget:target];
//			}
//		}

		#endregion

		
		
		#region ActionManager - run
		public void addAction(CCAction action, System.Object target, bool paused){
			NSUtils.Assert( action != null, "Argument action must be non-nil");
			NSUtils.Assert( target != null, "Argument target must be non-nil");
			
			tHashElement element = _targets.HASH_FIND_INT(target.GetHashCode());
			if (element == null) {
				element = new tHashElement();
				element.paused = paused;
				element.target = target;
				_targets.HASH_ADD_INT(target.GetHashCode(), element);
			}
			actionAllocWithHashElement (element);
			
			NSUtils.Assert (!element.actions.Contains (action), "runAction: Action already running");
			element.actions.Add (action);
			
			action.startWithTarget (target);
		}
		#endregion
		
		#region ActionManager - remove
		public void removeAllActions(){
			for (int i=_targets.Count - 1; i>=0; i--) {
				KeyValuePair<int, tHashElement> keyValue = _targets.ElementAt(i);
				removeAllActionsFromTarget(keyValue.Value.target);
			}
		}
		public void removeAllActionsFromTarget (System.Object target){
			if (target == null)
				return;
			tHashElement element = _targets.HASH_FIND_INT (target.GetHashCode());
			if (element != null) {
				if(element.actions.Contains(element.currentAction) && !element.currentActionSalvaged){
					element.currentActionSalvaged = true;
				}
				element.actions.Clear();
				if(_currentTarget == element)
					_currentTargetSavlvaged = true;
				else
					deleteHashElement(element);
			}
		}
		public void removeAction(CCAction action){
			if (action == null)
				return;
			System.Object target = action.originalTarget;
			tHashElement element = _targets.HASH_FIND_INT(target.GetHashCode());
			if (element != null) {
				int i = element.actions.IndexOf(action);
				if(i != -1){
					removeActionAtIndex(i, element);
				}
			}
		}
		public void removeActionByTag(int aTag, System.Object target){
			NSUtils.Assert (aTag != CCAction.kCCActionTagInvalid, "Invalid tag");	
			NSUtils.Assert (target != null, "Target should be null !");
			
			tHashElement element = _targets.HASH_FIND_INT (target.GetHashCode());
			if (element != null) {
				if(element.actions!=null){
					int limit = element.actions.Count;
					for(int i=0; i<limit; i++){
						CCAction a = element.actions[i];
						if(a.tag == aTag && a.originalTarget == target){
							removeActionAtIndex(i, element);
							break;
						}
					}
				}
			}
		}
		#endregion

		#region ActionManager - get
		public CCAction getActionByTag(int aTag, System.Object target){
			NSUtils.Assert (aTag != CCAction.kCCActionTagInvalid, "Invalid tag");	
			tHashElement element = _targets.HASH_FIND_INT (target.GetHashCode());
			if (element != null) {
				if(element.actions!=null){
					int limit = element.actions.Count;
					for(int i=0; i<limit; i++){
						CCAction a = element.actions[i];
						if(a.tag == aTag){
							return a;
						}
					}
				}
			}
			return null;
		}
		
		public uint numberOfRunningActionsInTarget(System.Object target){
			tHashElement element = _targets.HASH_FIND_INT (target.GetHashCode());
			if (element != null && element.actions != null)
				return (uint)(element.actions.Count);
			return 0;
		}
		#endregion

		#region ActionManager - main loop
		
		public void update(float dt){
			if (!_targets.Any ()) {
				return;		
			}
			var enumerator = new Dictionary<int, tHashElement>(_targets).GetEnumerator();
			while (enumerator.MoveNext()) {
				var elt = enumerator.Current.Value;
				_currentTarget = elt;
				_currentTargetSavlvaged = false;
				
				if(elt.target !=null && elt.actions != null && !elt.paused){
					for( elt.actionIndex = 0; elt.actions!=null && elt.actionIndex < elt.actions.Count; elt.actionIndex++) {
						elt.currentAction = elt.actions[elt.actionIndex];
						elt.currentActionSalvaged = false;
						elt.currentAction.step(dt);
						
						if(!elt.currentActionSalvaged && elt.currentAction!=null && elt.currentAction.isDone()){
							elt.currentAction.stop();
							
							CCAction a = elt.currentAction;
							elt.currentAction = null;
							removeAction(a);
						}
						elt.currentAction = null;
					}
				}
				if(_currentTargetSavlvaged && (elt.actions.Count ==0 || elt.target==null)){
					deleteHashElement(elt);
				}
			}
			_currentTarget = null;
		}

		#endregion
	}
}

