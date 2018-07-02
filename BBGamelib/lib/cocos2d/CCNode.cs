using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using BBGamelib.scheduler;

namespace BBGamelib
{
	public class CCNode
	{
		static uint globalOrderOfArrival = 1;
//		static uint globalSortingOrder = -9;

		//--------properties--------
		protected CCFactoryGear _gear;

		// rotation angle
		protected float _rotation;
		
		// scaling factors
		protected float _scaleX, _scaleY;
		
		// openGL real Z vertex
		protected float _vertexZ;
		
		// position of the node
		protected Vector2 _position;
		
		// skew angles
		protected float _skewX, _skewY;
		
		// anchor point in pixels
		protected Vector2 _anchorPointInPixels;

		// anchor point normalized (NOT in points)
		protected Vector2 _anchorPoint;
		
		// untransformed size of the node
		protected Vector2	_contentSize;
		
		// transform
		protected CGAffineTransform _transform, _inverse;
		protected bool _isTransformDirty;
		protected bool _isInverseDirty;
		protected bool _isUpdateTransformDirty;
		
		// a Camera
//		CCCamera *_camera;
		
		// a Grid
//		CCGridBase *_grid;
		
		// z-order value
		protected int _zOrder;
		
		// array of children
		protected List<CCNode> _children;
		
		// weak ref to parent
		protected CCNode _parent;
		
		// a tag. any number you want to assign to the node
		protected string _userTag;
		
		// user data field
		protected System.Object _userObject;
		
		// Shader
//		CCGLProgram	*_shaderProgram;
		
		// Server side state
//		ccGLServerState _glServerState;
		
		// used to preserve sequence while sorting children with the same zOrder
		protected uint _orderOfArrival;
		
		// scheduler used to schedule timers and updates
		protected CCScheduler		_scheduler;
		
		// ActionManager used to handle all the actions
		protected CCActionManager	_actionManager;
		
		// Is running
		protected bool _isRunning; 
		
		// is visible
		protected bool _visible;
		// If YES, the Anchor Point will be (0,0) when you position the CCNode.
		// Used by CCLayer and CCScene
		protected bool _ignoreAnchorPointForPosition;
		
		protected bool _isReorderChildDirty;

        // ------------------------------------------------------------------------------
        //  CCNode Init & cleanup
        // ------------------------------------------------------------------------------
		public CCNode()
        {
			init ();
		}
		protected virtual void init()
        {
            CCFactoryGear gear = CCFactory.Instance.takeGear (CCFactory.KEY_NODE);
            initWithGear (gear);
        }
		protected virtual void initWithGear(CCFactoryGear gear){
			_gear = gear;
			_gear.gameObject.name = "node";
			_gear.gameObject.transform.localPosition =Vector3.zero;
//			_gear.gameObject.transform.localScale = new Vector3 (1, 1, 1);
			_isRunning = false;
			
			//			_skewX = _skewY = 0.0f;
			_rotation = 0.0f;
			_scaleX = _scaleY = 1.0f;
			_position = Vector2.zero;
			_contentSize = Vector2.zero;
			_anchorPointInPixels = _anchorPoint = Vector2.zero;
			
			
			// "whole screen" objects. like Scenes and Layers, should set ignoreAnchorPointForPosition to YES
			_ignoreAnchorPointForPosition = false;
			
			_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
			
			//			_vertexZ = 0;
			
			//			_grid = nil;
			
			_visible = true;
			
			_userTag = null;
			
			_zOrder = 0;
			
			// lazy alloc
			//			_camera = nil;
			
			// children (lazy allocs)
			_children = null;
			
			// userData is always inited as nil
			_userObject = null;
			
			//initialize parent to nil
			_parent = null;
			
			//			_shaderProgram = nil;
			
			_orderOfArrival = 0;
			
			//			_glServerState = 0;
			
			// set default scheduler and actionManager
			CCDirector director = CCDirector.sharedDirector;
			this.actionManager = director.actionManager;
			this.scheduler = director.scheduler;
		}
		
		public virtual void cleanup()
		{
			// actions
			stopAllActions ();
			unscheduleAllSelectors ();

			if (_children != null) {
				var enumerator = _children.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCNode child = enumerator.Current;
					child.cleanup ();
				}
				_children.Clear ();
			}
			this.parent = null;
			recycleGear ();
		}

		protected virtual void recycleGear(){
			CCFactory.Instance.recycleGear (CCFactory.KEY_NODE, _gear);
		}

		public void forceTransformDirty(){
			_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;		
		}

		public override string ToString ()
		{
			return string.Format ("<{0} = {1} | Tag = {2}>", this.GetType().Name, this.GetHashCode(), this.userTag);
		}

        // ------------------------------------------------------------------------------
        //  CCNode Setters
        // ------------------------------------------------------------------------------
		public GameObject gameObject{
			get{ return _gear.gameObject;}
		}
		public Transform transform{
			get{ return gameObject.transform;}
		}
		/** The rotation (angle) of the node in degrees. 0 is the default rotation angle. Positive values rotate node CW. */
		public virtual float rotation{
			get{
				return _rotation;		
			}
			set{
				if(FloatUtils.NEQ(_rotation, value)){
					_rotation = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}
		
		/** The scale factor of the node. 1.0 is the default scale factor. It modifies the X and Y scale at the same time. */
		public virtual float scale{
			get{
				NSUtils.Assert( FloatUtils.EQ(_scaleX , _scaleY), "CCNode#scale. ScaleX != ScaleY. Don't know which one to return");
				return _scaleX;
			}
			set{
				
				if(FloatUtils.NEQ(_scaleX, value)||FloatUtils.NEQ(_scaleY, value)){
					scaleX = value;
					scaleY = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}

		/** The scale factor of the node. 1.0 is the default scale factor. It only modifies the X scale factor. */
		public virtual float scaleX{
			get{
				return _scaleX;
			}
			set{
				if(FloatUtils.NEQ(_scaleX, value)){
					_scaleX = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;

					//reset child position if ignoreAnchorPointForPosition
					if(_ignoreAnchorPointForPosition){
						var enumerator = _children.GetEnumerator();
						while (enumerator.MoveNext()) {
							CCNode node = enumerator.Current;
							node._isUpdateTransformDirty = true;
						}
					}
				}
			}
		}
		
		/** The scale factor of the node. 1.0 is the default scale factor. It only modifies the Y scale factor. */
		public virtual float scaleY{
			get{
				return _scaleY;
			}
			set{
				if(FloatUtils.NEQ(_scaleY, value)){
					_scaleY = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;

					//reset child position if ignoreAnchorPointForPosition
					if(_ignoreAnchorPointForPosition){
						var enumerator = _children.GetEnumerator();
						while (enumerator.MoveNext()) {
							CCNode node = enumerator.Current;
							node._isUpdateTransformDirty = true;
						}
					}
				}
			}
		}
		
		/** Position (x,y) of the node in points. (0,0) is the left-bottom corner. */
		public virtual Vector2 position{
			get{
				return _position;	
			}
			set{
				if(_position!=value){
					_position = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}
		public virtual float positionX{
			get{
				return _position.x;
			}
			set{
				if(FloatUtils.NEQ(_position.x, value)){
					_position.x = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}
		public virtual float positionY{
			get{
				return _position.y;
			}
			set{
				if(FloatUtils.NEQ(_position.y, value)){
					_position.y = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}
		/**  If YES, the Anchor Point will be (0,0) when you position the CCNode.
		 Used by CCLayer and CCScene.
		 */
		public virtual bool ignoreAnchorPointForPosition {
			get{return _ignoreAnchorPointForPosition;}
			set{
				if(_ignoreAnchorPointForPosition!=value){
					_ignoreAnchorPointForPosition = value;
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				}
			}
		}
		/** anchorPoint is the point around which all transformations and positioning manipulations take place.
		 It's like a pin in the node where it is "attached" to its parent.
		 The anchorPoint is normalized, like a percentage. (0,0) means the bottom-left corner and (1,1) means the top-right corner.
		 But you can use values higher than (1,1) and lower than (0,0) too.
		 The default anchorPoint is (0,0). It starts in the bottom-left corner. CCSprite and other subclasses have a different default anchorPoint.
		 */
		public virtual Vector2 anchorPoint{
			get{return _anchorPoint;}
			set{
				if(_anchorPoint != value){
					_anchorPoint = value;
					_anchorPointInPixels = new Vector2(_contentSize.x * _anchorPoint.x, _contentSize.y * _anchorPoint.y);
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;
				
					//Change children position when update anchorpoint 
					//It's a performance issue if open this, use forceTransformDirty to update child when reset anchorpoint
					if(_contentSize != Vector2.zero && _children != null){
                        var childrenEnu = _children.GetEnumerator();
                        while(childrenEnu.MoveNext()){
                            var child = childrenEnu.Current;
							child._isUpdateTransformDirty = true;
							child._isTransformDirty = true;
							child._isInverseDirty = true;
						}
					}			
				}
			}
		}
		
		/** The untransformed size of the node in Points
		 The contentSize remains the same no matter the node is scaled or rotated.
		 All nodes has a size. Layer and Scene has the same size of the screen.
		 */
		public virtual Vector2 contentSize{
			get{
				return _contentSize;		
			}
			set{	
				if(_contentSize != value){
					_contentSize = value;	
					_anchorPointInPixels = new Vector2( _contentSize.x * _anchorPoint.x, _contentSize.y * _anchorPoint.y );
					_isUpdateTransformDirty = _isTransformDirty = _isInverseDirty = true;

                    //Change children position when update anchorpoint 
                    //It's a performance issue if open this, use forceTransformDirty to update child when reset anchorpoint
                    if(_contentSize != Vector2.zero && _children != null){
                        var childrenEnu = _children.GetEnumerator();
                        while(childrenEnu.MoveNext()){
                            var child = childrenEnu.Current;
                            child._isUpdateTransformDirty = true;
                            child._isTransformDirty = true;
                            child._isInverseDirty = true;
                        }
                    }  
				}
			}
		}
		
		/** returns a "local" axis aligned bounding box of the node in points.
		 The returned box is relative only to its parent.
 		 The returned box is in Points.
		 */
		public Rect boundingBox{
			get{
				Rect rect = new Rect(0, 0, _contentSize.x, _contentSize.y);
				return CGAffineTransform.CGRectApplyAffineTransform(rect, nodeToParentTransform());			
			}
		}

		protected void _setZOrder(int  z)
		{
			_zOrder = z;
		}

		/** The z order of the node relative to its "siblings": children of the same parent */
		public virtual int zOrder{
			get{
				return _zOrder;		
			}
			set{
				_setZOrder(value);
				
				if (_parent!=null)
					_parent.reorderChild(this, zOrder);
			}
		}
		
		
		/** A custom user data pointer */
		public virtual System.Object userObject {
			get{return _userObject;}
			set{_userObject = value;}
		}

		// a tag. any number you want to assign to the node
		public virtual string userTag {
			get{return _userTag;}
			set{_userTag = value;}
		}

		/** Array of children */
		public virtual List<CCNode> children{
			get{
				return _children;
			}
		}
		/** Whether of not the node is visible. Default is YES */
		public virtual bool visible{
			set{
				_visible = value;
				gameObject.SetActive(_visible);
			}
			get{
				return _visible;
			}
		}
		/** The anchorPoint in absolute pixels.
		 you can only read it. If you wish to modify it, use anchorPoint instead
		 */
		public virtual Vector2 anchorPointInPixels{
			get{ return _anchorPointInPixels;}
		}
		/** whether or not the node is running */
		public virtual bool isRuning{
			get{return _isRunning;}
		}
		
		/** A reference to the parent */
		public virtual CCNode parent{
			set{
				_parent = value;
				if(value!=null)
					gameObject.transform.SetParent(value.transform, false);
				else
					gameObject.transform.parent = null;
			}
			get{
				return _parent;
			}
		}
		/** used internally for zOrder sorting, don't change this manually */
		public virtual uint orderOfArrival{
			get{return _orderOfArrival;}
			set{ _orderOfArrival = value;}
		}

        // ------------------------------------------------------------------------------
        //  CCNode Composition
        // ------------------------------------------------------------------------------
		public CCNode getChildByTag(string tag)
		{
			NSUtils.Assert(tag!=null, "tag is null.");
			if(_children!=null){
				var enumerator = _children.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCNode node = enumerator.Current;
					if(node.userTag == tag){
						return node;
					}
				}
			}
			// not found
			return null;
		}
		/* "add" logic MUST only be on this method
		 * If a class want's to extend the 'addChild' behaviour it only needs
		 * to override this method
		 */
		public virtual void addChild(CCNode child, int z, string tag){
			NSUtils.Assert( child != null, "Argument must be non-nil");
			NSUtils.Assert( child.parent == null, "child already added. It can't be added again");
			
			if (_children == null)
				_children = new List<CCNode> ();
			insertChild (child, z);
			
			child.userTag = tag;
			child.parent = this;
			child.orderOfArrival = globalOrderOfArrival ++;
			
			if (_isRunning) {
				child.onEnter();
				child.onEnterTransitionDidFinish();
			}
		}
		public void addChild(CCNode child){
			NSUtils.Assert( child != null, "Argument must be non-nil");
			addChild (child, child.zOrder, child.userTag);
		}
		public void addChild(CCNode child, int z){
			NSUtils.Assert( child != null, "Argument must be non-nil");
			addChild (child, z, child.userTag);
		}

		public void removeFromParent(){
			removeFromParentAndCleanup (true);
		}
		public void removeFromParentAndCleanup(bool cleanup){
			if(_parent!=null)
				_parent.removeChildAndCleanup (this, cleanup);
		}

		/* "remove" logic MUST only be on this method
		 * If a class wants to extend the 'removeChild' behavior it only needs
		 * to override this method
		 */
		public virtual void removeChild(CCNode child){
			removeChildAndCleanup (child, true);
		}
		public virtual void removeChildAndCleanup(CCNode child, bool cleanup){
			if (child == null)
				return;
			NSUtils.Assert(_children.Contains(child), "This node does not contain the specified child.");
			detachChild(child, cleanup);
		}

		public void removeChildByTag(string tag, bool cleanup=true){
			NSUtils.Assert(tag!=null, "Invalid tag");
			
			CCNode child = getChildByTag (tag);
			if (child == null)
				CCDebug.Log ("cocos2d: removeChildByTag: child not found!");
			else
				removeChildAndCleanup (child, cleanup);
		}
		
		public virtual void removeAllChildrenAndCleanup(bool cleanup=true){
			if (_children != null) {
				for (int i=_children.Count -1; i>=0; i--) {
					CCNode c = _children [i];
					if (_isRunning) {
						c.onExitTransitionDidStart ();
						c.onExit ();
					}
					if (cleanup)
						c.cleanup ();
					else
						c.parent = null;
				}
				_children.Clear ();
			}
		}
		void detachChild(CCNode child, bool cleanup)
		{
			// IMPORTANT:
			//  -1st do onExit
			//  -2nd cleanup
			if (_isRunning)
			{
				child.onExitTransitionDidStart();
				child.onExit();
			}
			
			child.parent = null;
			_children.Remove(child);
			
			
			// If you don't do cleanup, the child's actions will not get removed and the
			// its scheduledSelectors_ dict will not get released!
			if (cleanup)
				child.cleanup ();
			else
				child.gameObject.SetActive(false);
		}
		// helper used by reorderChild & add
		public virtual void insertChild(CCNode child, int z)
		{
			_isReorderChildDirty=true;
			
			_children.Add(child);
			child.zOrder=z;
		}
		
		public virtual void reorderChild(CCNode child, int z)
		{
			NSUtils.Assert( child != null, "Child must be non-nil");
			
			_isReorderChildDirty = true;
			
			child.orderOfArrival= globalOrderOfArrival++;
			child._setZOrder(z);
		}
		protected virtual void sortAllChildren()
		{
			if (_isReorderChildDirty)
			{
				CCNode[] x = _children.ToArray();
				int i,j,length = x.Length;
				CCNode tempItem;
				
				// insertion sort
				for(i=1; i<length; i++)
				{
					tempItem = x[i];
					j = i-1;
					
					//continue moving element downwards while zOrder is smaller or when zOrder is the same but mutatedIndex is smaller
					while(j>=0 && ( tempItem.zOrder < x[j].zOrder || ( tempItem.zOrder== x[j].zOrder && tempItem.orderOfArrival < x[j].orderOfArrival ) ) )
					{
						x[j+1] = x[j];
						j = j-1;
					}
					x[j+1] = tempItem;
				}

				_children = x.OfType<CCNode>().ToList();

				//don't need to check children recursively, that's done in visit of each child
				
				_isReorderChildDirty = false;
			}
		}

        // ------------------------------------------------------------------------------
        //  CCNode Draw
        // ------------------------------------------------------------------------------
		protected virtual void draw(){
		}
		public virtual void visit(){
			if (!_visible)
				return;
			updateTransform ();
			if(_children!=null) {
				sortAllChildren();

				int count = _children.Count;
				int i = 0;
				
				// draw children zOrder < 0
				for( ; i < count; i++ ) {
					CCNode child = _children[i];
					if ( child.zOrder < 0 )
						child.visit();
					else
						break;
				}
				
				// self draw
				draw();
				
				// draw children zOrder >= 0
				for( ; i < count; i++ ) {
					CCNode child =  _children[i];
					child.visit();
				}
				
			} else
				draw ();
			
			// reset for next frame
			_orderOfArrival = 0;
		}

        // ------------------------------------------------------------------------------
        //  CCNode Transformations
        // ------------------------------------------------------------------------------
		protected virtual void transformAncestors()
		{
			if( _parent !=null) {
				_parent.transformAncestors();
				_parent.updateTransform();
			}
		}
		
		public virtual void updateTransform(){
			if (_isUpdateTransformDirty) {
				//position
				Vector2 pInParentAR = _position;
				if(_parent!=null){
					if(_parent.ignoreAnchorPointForPosition){
						if(FloatUtils.NEQ(_parent.scaleX, 0) && FloatUtils.NEQ(_parent.scaleY, 0)){
							pInParentAR += new Vector2(_parent.anchorPointInPixels.x * (1/_parent.scaleX - 1), _parent.anchorPointInPixels.y * (1/_parent.scaleY - 1)); 
						}
					}else{
						pInParentAR -= _parent.anchorPointInPixels;
                    }
                }
                Vector2 uPInParentAR = ccUtils.PixelsToUnits (pInParentAR);
				Vector3 pos = transform.localPosition;
                pos.x = uPInParentAR.x;
                pos.y = uPInParentAR.y;
				pos.z = 0;
				transform.localPosition = pos;

				
				//rotation
                Vector3 rotation = calculateRotation();
                transform.localEulerAngles =Vector3.zero;
                transform.Rotate(rotation.x, 0, 0);
                transform.Rotate(0, rotation.y, 0);
                transform.Rotate(0, 0, rotation.z);
				
				//scale			
				transform.localScale = new Vector3 (Mathf.Abs (_scaleX), Mathf.Abs (_scaleY), transform.localScale.z);
				_isUpdateTransformDirty = false;
			}
		}
        protected virtual Vector3 calculateRotation(){
            Vector3 rotation = transform.localEulerAngles;
            rotation.x = 0;
            rotation.z = -_rotation;
            rotation.y = 0;
            bool negativeScaleX = FloatUtils.Small(_scaleX, 0);
            bool negativeScaleY = FloatUtils.Small(_scaleY, 0);
            if (negativeScaleX && negativeScaleY)
            {
                rotation.z = 180 - _rotation;
            } else if (negativeScaleX)
            {
                rotation.y = 180;
                rotation.z = _rotation;
            } else if (negativeScaleY)
            {
                rotation.y = 180;
                rotation.z = _rotation + 180;
            }
            return rotation;
        }


        // ------------------------------------------------------------------------------
        //  CCNode SceneManagement
        // ------------------------------------------------------------------------------
		//
		//onEnter & onExit
		//
		public virtual void onEnter(){
			if (_children != null) {
				int childrenCount = _children.Count;
				for(int i=0; i<childrenCount; i++){
					CCNode child = _children[i];
					child.onEnter();
				}
			}
			resumeSchedulerAndActions ();
			_isRunning = true;
			gameObject.SetActive(_visible);
		}
		public virtual void onEnterTransitionDidFinish(){
			if (_children != null) {
				int childrenCount = _children.Count;
				for(int i=0; i<childrenCount; i++){
					CCNode child = _children[i];
					child.onEnterTransitionDidFinish ();
				}
			}
		}
		public virtual void onExitTransitionDidStart(){
			if (_children != null) {
				int childrenCount = _children.Count;
				for(int i=0; i<childrenCount; i++){
					CCNode child = _children[i];
					child.onExitTransitionDidStart ();
				}
			}
		}
		public virtual void onExit(){
			pauseSchedulerAndActions ();
			_isRunning = false;
			
			if (_children != null) {
				int childrenCount = _children.Count;
				for(int i=0; i<childrenCount; i++){
					CCNode child = _children[i];
					child.onExit ();
				}
			}
		}

        // ------------------------------------------------------------------------------
        //  CCNode Actions
        // ------------------------------------------------------------------------------
		/** CCActionManager used by all the actions.
		 IMPORTANT: If you set a new CCActionManager, then previously created actions are going to be removed.
		 @since v2.0
		 */
		public CCActionManager actionManager{
			get{ return _actionManager;}
			set{
				if( value != _actionManager ) {
					if(_actionManager!=null)
						stopAllActions();
					_actionManager = value;
				}			
			}
		}
		/** Executes an action, and returns the action that is executed.
		 The node becomes the action's target.
		 @warning Starting from v0.8 actions don't retain their target anymore.
		 @since v0.7.1
		 @return An Action pointer
		 */
		public CCAction runAction(CCAction action){
			NSUtils.Assert( action != null, "Argument must be non-nil");

			_actionManager.addAction (action, this, !_isRunning);
			return action;		
		}
		/** Removes all actions from the running action list */
		public void stopAllActions(){
			_actionManager.removeAllActionsFromTarget (this);
		}
		/** Removes an action from the running action list */
		public void stopAction(CCAction action){
			_actionManager.removeAction (action);
		}
		/** Removes an action from the running action list given its tag
		 @since v0.7.1
		*/
		public void stopActionByTag(int aTag){
			NSUtils.Assert( aTag != CCAction.kCCActionTagInvalid, "Invalid tag");
			_actionManager.removeActionByTag(aTag, this);
		}
		/** Gets an action from the running action list given its tag
		 @since v0.7.1
		 @return the Action the with the given tag
		 */
		public CCAction getActionByTag(int aTag){
			NSUtils.Assert( aTag != CCAction.kCCActionTagInvalid, "Invalid tag");
			return 	_actionManager.getActionByTag(aTag, this);
		}
		/** Returns the numbers of actions that are running plus the ones that are schedule to run (actions in actionsToAdd and actions arrays).
		 * Composable actions are counted as 1 action. Example:
		 *    If you are running 1 Sequence of 7 actions, it will return 1.
		 *    If you are running 7 Sequences of 2 actions, it will return 7.
		 */
		public uint numberOfRuningActions(){
			return _actionManager.numberOfRunningActionsInTarget (this);		
		}

        // ------------------------------------------------------------------------------
        //  Scheduler
        // ------------------------------------------------------------------------------
		/** CCScheduler used to schedule all "updates" and timers.
		 IMPORTANT: If you set a new CCScheduler, then previously created timers/update are going to be removed.
		 @since v2.0
		 */
		public CCScheduler scheduler{
			get{return _scheduler;}
			set{
				if( value != _scheduler ) {
					if(_scheduler!=null)
						unscheduleAllSelectors();
					_scheduler = value;
				}			
			}
		}

		/** schedules the "update" selector with a custom priority. This selector will be called every frame.
		 Scheduled selectors with a lower priority will be called before the ones that have a higher value.
		 Only one "update" selector could be scheduled per node (You can't have 2 'update' selectors).

		 @since v0.99.3
		 */
		public void scheduleUpdateWithPriority(int priority=0){
			_scheduler.scheduleUpdate (this, priority, !_isRunning);
		}
		
		/* unschedules the "update" method.

		 @since v0.99.3
		 */
		public void unscheduleUpdate(){
			_scheduler.unscheduleUpdateForTarget (this);
		}

		/**
		 repeat will execute the action repeat + 1 times, for a continues action use kCCRepeatForever
		 delay is the amount of time the action will wait before execution
		 */
		public void schedule(TICK_IMP selector, float interval=0, uint repeat=CCScheduler.kCCRepeatForever, float delay=0){
			NSUtils.Assert( selector != null, "Argument must be non-nil");
			NSUtils.Assert( FloatUtils.EB( interval , 0), "Arguemnt must be positive");
			

			_scheduler.schedule (selector, this, interval, repeat, !_isRunning, delay);
		}
		/**
		 Schedules a selector that runs only once, with a delay of 0 or larger
		*/
		public void scheduleOnce(TICK_IMP selector, float delay){
			schedule (selector, 0, 0, delay);
		}
		 
		public void unschedule(TICK_IMP selector){
			// explicit nil handling
			if (selector == null)
				return;
			
			_scheduler.unscheduleSelector(selector, this);
		}

		public void unscheduleAllSelectors(){
			_scheduler.unscheduleAllForTarget (this);
		}
		public void resumeSchedulerAndActions(){
			_scheduler.resumeTarget (this);
			_actionManager.resumeTarget (this);
		}
		public void pauseSchedulerAndActions(){
			_scheduler.pauseTarget (this);
			_actionManager.pauseTarget (this);
		}

		/* override me*/
		public virtual void update(float dt){
		}


        // ------------------------------------------------------------------------------
        //  #region  CCNode Transform
        // ------------------------------------------------------------------------------
		/** Returns the matrix that transform the node's (local) space coordinates into the parent's space coordinates.
		 The matrix is in Pixels.
		 @since v0.7.1
		 */
		public virtual CGAffineTransform nodeToParentTransform()
		{
			if ( _isTransformDirty ) {
				
				// Translate values
				float x = _position.x;
				float y = _position.y;
				
				if ( _ignoreAnchorPointForPosition ) {
					x += _anchorPointInPixels.x;
					y += _anchorPointInPixels.y;
				}
				
				// Rotation values
				// Change rotation code to handle X and Y
				// If we skew with the exact same value for both x and y then we're simply just rotating
				float cx = 1, sx = 0, cy = 1, sy = 0;
				if(!FloatUtils.EQ(_rotation, 0)) {
					float radiansX = -ccUtils.CC_DEGREES_TO_RADIANS(_rotation);
					float radiansY = -ccUtils.CC_DEGREES_TO_RADIANS(_rotation);
					cx = Mathf.Cos(radiansX);
					sx = Mathf.Sin(radiansX);
					cy = Mathf.Cos(radiansY);
					sy = Mathf.Sin(radiansY);
				}

				// optimization:
				// inline anchor point calculation if skew is not needed
				// Adjusted transform calculation for rotational skew
				if( _anchorPointInPixels != Vector2.zero ) {
					x += cy * -_anchorPointInPixels.x * _scaleX + -sx * -_anchorPointInPixels.y * _scaleY;
					y += sy * -_anchorPointInPixels.x * _scaleX +  cx * -_anchorPointInPixels.y * _scaleY;
				}
				
				
				// Build Transform Matrix
				// Adjusted transfor m calculation for rotational skew
				_transform = CGAffineTransform.Make( cy * _scaleX, sy * _scaleX,
				                                   -sx * _scaleY, cx * _scaleY,
				                                   x, y );

				_isTransformDirty = false;
			}
			
			return _transform;
		}
		public CGAffineTransform parentToNodeTransform()
		{
			if ( _isInverseDirty ) {
				_inverse = CGAffineTransform.Invert(nodeToParentTransform());
				_isInverseDirty = false;
			}
			
			return _inverse;
		}
		public CGAffineTransform nodeToWorldTransform()
		{
			CGAffineTransform t = nodeToParentTransform();
			
			for (CCNode p = _parent; p != null; p = p.parent)
				t = CGAffineTransform.Concat(t, p.nodeToParentTransform());
			
			return t;
		}
		
		public CGAffineTransform worldToNodeTransform()
		{
			return CGAffineTransform.Invert(nodeToWorldTransform());
		}
		
		public virtual Vector2 convertToNodeSpace(Vector2 worldPoint)
		{
			Vector2 ret = CGAffineTransform.CGPointApplyAffineTransform(worldPoint, worldToNodeTransform());
			return ret;
		}
		
		public virtual Vector2 convertToWorldSpace(Vector2 nodePoint)
		{
			Vector2 ret = CGAffineTransform.CGPointApplyAffineTransform(nodePoint, nodeToWorldTransform());
			return ret;
		}

		public virtual Vector2 convertToNodeSpaceAR(Vector2 worldPoint)
		{
			Vector2 nodePoint = convertToNodeSpace(worldPoint);
			return (nodePoint - _anchorPointInPixels);
		}
		
		public virtual Vector2 convertToWorldSpaceAR(Vector2 nodePoint)
		{
			nodePoint = (nodePoint + _anchorPointInPixels);
			return convertToWorldSpace(nodePoint);
		}
		
		public virtual Vector2 convertToWindowSpace(Vector2 nodePoint)
		{
			Vector2 worldPoint = convertToWorldSpace(nodePoint);
			return worldPoint;
		}

		public virtual Vector2 convertTouchToNodeSpace(UITouch touch)
		{
			Vector2 point = touch.location;
			return convertToNodeSpace(point);
		}
		
		public virtual Vector2 convertTouchToNodeSpaceAR(UITouch touch)
		{
			Vector2 point = touch.location;
			return convertToNodeSpaceAR(point);
		}
		
		public Vector2 convertMouseEventToNodeSpace(NSEvent evt)
		{
			Vector2 point = evt.mouseLocation;
			return convertToNodeSpace(point);
		}

		public Vector2 convertMouseEventToNodeSpaceAR(NSEvent evt)
		{
			Vector2 point = evt.mouseLocation;
			return convertToNodeSpaceAR(point);
		}


        // ------------------------------------------------------------------------------
        //  unity
        // ------------------------------------------------------------------------------
		public string nameInHierarchy{
			get{return _gear.gameObject.name;}
			set{_gear.gameObject.name=value;}
		}

        //set unity layer
        public virtual void setUnityLayer(int layer){
            this.gameObject.layer = layer;
        }

        //set unity layer recursively
        public virtual void setUnityLayerRecursively(int layer){
            setUnityLayerRecursively (this.gameObject, layer);
        }

        //set unity layer
        public virtual void setUnityLayer(string layer){
            this.gameObject.layer = LayerMask.NameToLayer(layer);
        }

        //set unity layer recursively
        public virtual void setUnityLayerRecursively(string layer){
            setUnityLayerRecursively (this.gameObject, LayerMask.NameToLayer(layer));
        }

        private void setUnityLayerRecursively(GameObject obj, int newLayer )
        {
            obj.layer = newLayer;
            for(int i=obj.transform.childCount-1; i>=0; i--){
                Transform child = obj.transform.GetChild(i);
                setUnityLayerRecursively( child.gameObject, newLayer );
            }
        }
	}
}

