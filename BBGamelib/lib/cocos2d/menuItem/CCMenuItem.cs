using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace BBGamelib{
    public delegate void CCMenuItemDelegate(CCMenuItem obj);
	/** CCMenuItem base class
	 *
	 *  Subclass CCMenuItem (or any subclass) to create your custom CCMenuItem objects.
	 */
	public class CCMenuItem : CCNodeRGBA
	{
		protected const int kCCItemSize = 32;
		protected const int	kCCCurrentItemTag = int.MaxValue - 1;
		protected const int	kCCZoomActionTag = int.MaxValue;

		// used for menu items using a block
		protected CCMenuItemDelegate _block;
		protected bool _isEnabled;
		protected bool _isSelected;
		
		protected bool _releaseBlockAtCleanup;
		protected Rect _activeArea;

		public CCMenuItem(){}

		public CCMenuItem(CCMenuItemDelegate selector){
			initWithBlock (selector);
		}

		protected override void init ()
		{
			base.init ();
            initWithBlock (null);
            this.nameInHierarchy = "menuItem";
		}

		protected virtual void initWithBlock(CCMenuItemDelegate block){
			if(block!=null)
				_block = block;
			this.anchorPoint = new Vector2 (0.5f, 0.5f);
			_isEnabled = true;
			_isSelected = false;

			// WARNING: Will be disabled in v2.2
			_releaseBlockAtCleanup = true;
		}

		public override Vector2 contentSize {
			set {
				base.contentSize = value;
				_activeArea = new Rect(0, 0, contentSize.x, contentSize.y);
			}
		}
		/** The item was selected (not activated), similar to "mouse-over" */
		public virtual void selected(){
			_isSelected = true;
		}
		/** The item was unselected */
		public virtual void unselected(){
			_isSelected = false;
		}
		
		/** Activate the item */
		public virtual void activate(){
			if(_isEnabled && _block!=null )
				_block.Invoke(this);
		}
		/** Enable or disabled the CCMenuItem */
		public virtual bool isEnabled{
			get{ return _isEnabled;}
			set{ _isEnabled = value;}
		}
		/** Sets the block that is called when the item is tapped.
		 The block will be "copied".
		 */
		public void setBlock(CCMenuItemDelegate block){
			_block = block;
		}

		/** returns whether or not the item is selected
		@since v0.8.2
		*/
		public bool isSelected{get{return _isSelected;}}

		/** the active area (relative to the current position) */
		public Rect activeArea{get{return _activeArea;} set{_activeArea=value;}}
	
		/** If enabled, it releases the block at cleanup time.
		 @since v2.1
		 */
		public bool releaseBlockAtCleanup{get{return _releaseBlockAtCleanup;} set{_releaseBlockAtCleanup = value;}}

		public override void cleanup ()
		{
			if( _releaseBlockAtCleanup ) {
				_block = null;
			}
			base.cleanup ();
		}

	}
}
