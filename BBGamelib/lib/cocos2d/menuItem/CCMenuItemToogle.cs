using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	/** A CCMenuItemToggle
	 A simple container class that "toggles" its inner items
	 The inner itmes can be any MenuItem
	 */
	public class CCMenuItemToggle : CCMenuItem, CCRGBAProtocol{
		int	_selectedIndex;
		List<CCMenuItem> _subItems;
		CCMenuItem	_currentItem;
		
		/** NSMutableArray that contains the subitems. You can add/remove items in runtime, and you can replace the array with a new one.
		 @since v0.7.2
		 */
		public List<CCMenuItem> subItems{get{return _subItems;} set{_subItems = value;}}
		/**
		 Reference to the current display item.
		 */
		CCMenuItem currentItem{
			get{return _currentItem;}
			set{_currentItem = value;}
		}
		
		public CCMenuItemToggle(CCMenuItemDelegate block, params CCMenuItem[] items){
			initWithItems (items, block);
		}
		
		void initWithItems(CCMenuItem[] arrayOfItems, CCMenuItemDelegate block)
		{
			base.initWithBlock (block);
			this.subItems = new List<CCMenuItem>(new List<CCMenuItem>(arrayOfItems));
			
			_currentItem = null;
			_selectedIndex = int.MaxValue;
			this.selectedIndex = 0;
			
			this.cascadeColorEnabled = true;
			this.cascadeOpacityEnabled = true;
		}
		/** returns the selected item */
		public int selectedIndex{get{return _selectedIndex;} 
			set{
				if( value != _selectedIndex ) {
					_selectedIndex=value;
					
					if( _currentItem!=null )
						_currentItem.removeFromParentAndCleanup(false);
					
					CCMenuItem item = _subItems[_selectedIndex];
					addChild(item, 0);
					this.currentItem = item;
					
					Vector2 s = item.contentSize;
					this.contentSize = s;
					item.position = s / 2;
				}
			}
		}
		
		public override void selected ()
		{
			base.selected ();
			_subItems [_selectedIndex].selected ();
		}
		
		public override void unselected ()
		{
			base.unselected ();
			_subItems [_selectedIndex].unselected ();
		}
		public override void activate ()
		{
			// update index
			if( _isEnabled ) {
				int newIndex = (_selectedIndex + 1) % _subItems.Count;
				this.selectedIndex = newIndex;
				
			}
			base.activate ();
		}
		
		public override bool isEnabled {
			set {
				if( _isEnabled != value ) {
					base.isEnabled = value;
					
					var enumerator = _subItems.GetEnumerator();
					while (enumerator.MoveNext()) {
						CCMenuItem item = enumerator.Current;
						item.isEnabled = value;
					}
				}
			}
		}
		
		/** return the selected item */
		public CCMenuItem selectedItem()
		{
			return _subItems[_selectedIndex];
		}
	}
}