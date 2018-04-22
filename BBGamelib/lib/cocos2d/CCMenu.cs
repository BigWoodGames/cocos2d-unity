using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	public enum kCCMenuState{
		Waiting,
		TrackingTouch
	}

	/** A CCMenu
	 *
	 * Features and Limitation:
	 *  - You can add MenuItem objects in runtime using addChild:
	 *  - But the only accecpted children are MenuItem objects
	 */
	public class CCMenu : CCLayerRGBA
	{
		const int kDefaultPadding = 5;
		const int kCCMenuHandlerPriority = -128;

		kCCMenuState _state;
		CCMenuItem	_selectedItem;
		bool		_enabled;
		
		/** whether or not the menu will receive events */
		public bool enabled{get{return _enabled;} set{_enabled=value;}}

		public CCMenu(params CCMenuItem[] arrayOfItems){
			initWithArray (new List<CCMenuItem>(arrayOfItems));
		}

		public CCMenu(List<CCMenuItem> arrayOfItems){
			initWithArray (arrayOfItems);
		}
		protected override void init ()
		{
			base.init ();
			initWithArray (null);
		}
        protected virtual void initWithArray(List<CCMenuItem> arrayOfItems){
            this.nameInHierarchy = "menu";
			this.touchPriority = kCCMenuHandlerPriority;
			this.touchMode = kCCTouchesMode.OneByOne;
			this.isTouchEnabled = true;
            this.isMouseEnabled = true;
            this.mousePriority = this.touchPriority;
			_enabled = true;
			
			// by default, menu in the center of the screen
			Vector2 s = CCDirector.sharedDirector.winSize;
			
			this.ignoreAnchorPointForPosition = true;
			_anchorPoint = new Vector2(0.5f, 0.5f);
			this.contentSize = s;


			this.position = s / 2;
			
			int z=0;

			if(arrayOfItems!=null){
				var enumerator = arrayOfItems.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCMenuItem item = enumerator.Current;
					addChild(item, z);
					z++;
				}
			}
			_selectedItem = null;
			_state = kCCMenuState.Waiting;
			
			// enable cascade color and opacity on menus
			this.cascadeColorEnabled = true;
			this.cascadeOpacityEnabled = true;
		}
		/*
		 * override add:
		 */
		public override void addChild (CCNode child, int z, string tag)
		{
			NSUtils.Assert( child is CCMenuItem, "Menu only supports MenuItem objects as children");
			base.addChild (child, z, tag);
		}

		public override void onExit ()
		{
			if(_state == kCCMenuState.TrackingTouch)
			{
				_selectedItem.unselected();
				_state = kCCMenuState.Waiting;
				_selectedItem = null;
			}
			base.onExit ();
		}
	
		#region mark Menu - Events
		/** set event handler priority. By default it is: kCCMenuTouchPriority */
		public void setHandlerPriority(int newPriority)
		{
			CCTouchDispatcher dispatcher = CCDirector.sharedDirector.touchDispatcher;
			dispatcher.setPriority (newPriority, this);
		}
		#endregion

		#region mark Menu - Events Touches
        CCMenuItem  itemForTouch(Vector2  touchLocation)
		{
//			touchLocation = CCDirector.sharedDirector.convertToGL(touchLocation);

			var enumerator = _children.GetEnumerator();
			while (enumerator.MoveNext()) {
				CCMenuItem item = (CCMenuItem)enumerator.Current;
				// ignore invisible and disabled items: issue #779, #866
				if ( item.visible && item.isEnabled ) {
					
					Vector2 local = item.convertToNodeSpace(touchLocation);
					Rect r = item.activeArea;
					
					if( r.Contains(local ) )
						return item;
				}
			}
			return null;
		}

        private bool touchBegan(CCMenuItem touchedItem)
        {
            if( _state != kCCMenuState.Waiting || !_visible || ! _enabled)
                return false;

            for( CCNode c = this.parent; c != null; c = c.parent )
                if( c.visible == false )
                    return false;

            _selectedItem = touchedItem;

            if( _selectedItem!=null ) {
                _selectedItem.selected();
                _state = kCCMenuState.TrackingTouch;
                return true;
            }
            return false;
        }

        private void touchEnded()
        {
//            NSUtils.Assert(_state == kCCMenuState.TrackingTouch, "[Menu ccTouchEnded] -- invalid state");
            if (_state != kCCMenuState.TrackingTouch)
                return;
            _selectedItem.unselected();
            _selectedItem.activate();
            _state = kCCMenuState.Waiting;
        }

        private void touchCancelled()
        {
//            NSUtils.Assert(_state == kCCMenuState.TrackingTouch, "[Menu ccTouchCancelled] -- invalid state");
            if (_state != kCCMenuState.TrackingTouch)
                return;
            _selectedItem.unselected();
            _state = kCCMenuState.Waiting;
        }

        private void touchMoved(CCMenuItem touchedItem)
        {
//            NSUtils.Assert(_state == kCCMenuState.TrackingTouch, "[Menu ccTouchMoved] -- invalid state");
            if (_state != kCCMenuState.TrackingTouch)
                return;

            if (touchedItem != _selectedItem) {
                _selectedItem.unselected();
                _selectedItem = touchedItem;
                _selectedItem.selected();
            }
        }

		public override bool ccTouchBegan (UITouch touch)
        {
            Vector2 touchLocation = touch.location;
            CCMenuItem touchedItem = itemForTouch(touchLocation);
            return touchBegan(touchedItem);
		}

		public override void ccTouchEnded (UITouch touch)
		{
            touchEnded();
		}

		public override void ccTouchCancelled (UITouch touch)
		{
            touchCancelled();
		}
		
        public override void ccTouchMoved (UITouch touch)
        {
            Vector2 touchLocation = touch.location;
            CCMenuItem touchedItem = itemForTouch(touchLocation);
            touchMoved(touchedItem);
			
		}

        public override bool ccMouseDown(NSEvent theEvent)
        {
            Vector2 touchLocation = theEvent.mouseLocation;
            CCMenuItem touchedItem = itemForTouch(touchLocation);
            return touchBegan(touchedItem);
        }

        public override bool ccMouseUp(NSEvent theEvent)
        {
            touchEnded();
            return false;
        }

        public override bool ccMouseDragged(NSEvent theEvent)
        {
            Vector2 touchLocation = theEvent.mouseLocation;
            CCMenuItem touchedItem = itemForTouch(touchLocation);
            touchMoved(touchedItem);
            return false;
        }

		#endregion

		
		#region mark Menu - Alignment
		/** align items vertically */
		public void alignItemsVertically()
		{
			alignItemsVerticallyWithPadding(kDefaultPadding);
		}
		/** align items vertically with padding
		 @since v0.7.2
		 */
		public void alignItemsVerticallyWithPadding(float padding){
			float height = -padding;

			var enumerator = _children.GetEnumerator();
			while (enumerator.MoveNext()) {
				CCNode child = enumerator.Current;
				CCMenuItem item = child as CCMenuItem;
				if(item!=null)
					height += item.contentSize.y * item.scaleY + padding;
			}
			
			float y = height / 2.0f;

			enumerator = _children.GetEnumerator();
			while (enumerator.MoveNext()) {
				CCNode child = enumerator.Current;
				CCMenuItem item = child as CCMenuItem;
				if(item!=null){
					Vector2 itemSize = item.contentSize;
					item.position=new Vector2(0, y - itemSize.y * item.scaleY / 2.0f);
					y -= itemSize.y * item.scaleY + padding;
				}
			}
		}
		
		/** align items horizontally */
		public void alignItemsHorizontally()
		{
			alignItemsHorizontallyWithPadding(kDefaultPadding);
		}
		/** align items horizontally with padding
		 @since v0.7.2
		 */
		public void alignItemsHorizontallyWithPadding(float padding)
		{	
			float width = -padding;

			var enumerator = _children.GetEnumerator();
			while (enumerator.MoveNext()) {
				CCNode child = enumerator.Current;
				CCMenuItem item = child as CCMenuItem;
				if (item != null)
					width += item.contentSize.x * item.scaleX + padding;
			}
			float x = -width / 2.0f;

			enumerator = _children.GetEnumerator();
			while (enumerator.MoveNext()) {
				CCNode child = enumerator.Current;
				CCMenuItem item = child as CCMenuItem;
				if(item!=null){
					Vector2 itemSize = item.contentSize;
					item.position=new Vector2(x + itemSize.x * item.scaleX / 2.0f, 0);
					x += itemSize.x * item.scaleX + padding;
				}
			}

		}
		/** align items in rows of columns */
		public void alignItemsInColumns(params int[] columns){
			alignItemsInColumnsWithArray (columns);
        }
		public void alignItemsInColumnsWithArray(int[] rows)
		{	int height = -5;
			int row = 0, rowHeight = 0, columnsOccupied = 0, rowColumns;

			var enumerator = _children.GetEnumerator();
			while (enumerator.MoveNext()) {
				CCMenuItem item = (CCMenuItem)enumerator.Current;
				NSUtils.Assert( row < rows.Length, "Too many menu items for the amount of rows/columns.");
				
				rowColumns = rows[row];
				NSUtils.Assert( rowColumns!=0, "Can't have zero columns on a row");
				
				rowHeight = Mathf.RoundToInt(Mathf.Max(rowHeight, item.contentSize.y));
				++columnsOccupied;
				
				if(columnsOccupied >= rowColumns) {
					height += rowHeight + 5;
					
					columnsOccupied = 0;
					rowHeight = 0;
					++row;
				}
			}
			NSUtils.Assert( columnsOccupied==0, "Too many rows/columns for available menu items." );
			
			Vector2 winSize = CCDirector.sharedDirector.winSize;
			
			row = 0; rowHeight = 0; rowColumns = 0;
			float w=0, x=0, y = height / 2;

			enumerator = _children.GetEnumerator();
			while (enumerator.MoveNext()) {
				CCMenuItem item = (CCMenuItem)enumerator.Current;
				if(rowColumns == 0) {
					rowColumns = rows[row];
					w = winSize.x / (1 + rowColumns);
					x = w;
				}
				
				Vector2 itemSize = item.contentSize;
				rowHeight = Mathf.RoundToInt(Mathf.Max(rowHeight, itemSize.y));
				item.position= new Vector2(x - winSize.x / 2,
				                      y - itemSize.y / 2);
				
				x += w;
				++columnsOccupied;
				
				if(columnsOccupied >= rowColumns) {
					y -= rowHeight + 5;
					
					columnsOccupied = 0;
					rowColumns = 0;
					rowHeight = 0;
					++row;
				}
			}
		}
		
		/** align items in columns of rows */
		public void alignItemsInRows(params int[] rows){
			alignItemsInRowsWithArray (rows);
        }
		
		void alignItemsInRowsWithArray(int[]  columns){
			List<int> columnWidths = new List<int> ();
			List<int> columnHeights = new List<int> ();
			
			int width = -10, columnHeight = -5;
			int column = 0, columnWidth = 0, rowsOccupied = 0, columnRows;

			var enumerator = _children.GetEnumerator();
			while (enumerator.MoveNext()) {
				CCMenuItem item = (CCMenuItem)enumerator.Current;
				NSUtils.Assert( column < columns.Length, "Too many menu items for the amount of rows/columns.");
				
				columnRows = columns[column];
				NSUtils.Assert( columnRows!=0, "Can't have zero rows on a column");
				
				Vector2 itemSize = item.contentSize;
				columnWidth = Mathf.RoundToInt(Mathf.Max(columnWidth, itemSize.x));
				columnHeight += Mathf.RoundToInt(itemSize.y + 5);
				++rowsOccupied;
				
				if(rowsOccupied >= columnRows) {
					columnWidths.Add(columnWidth);
					columnHeights.Add(columnHeight);
					width += columnWidth + 10;
					
					rowsOccupied = 0;
					columnWidth = 0;
					columnHeight = -5;
					++column;
				}
			}
			NSUtils.Assert( rowsOccupied==0, "Too many rows/columns for available menu items.");
			
			Vector2 winSize = CCDirector.sharedDirector.winSize;
			
			column = 0; columnWidth = 0; columnRows = 0;
			float x = -width / 2, y=0;

			enumerator = _children.GetEnumerator();
			while (enumerator.MoveNext()) {
				CCMenuItem item = (CCMenuItem)enumerator.Current;
				if(columnRows == 0) {
					columnRows = columns[column];
					y = (columnHeights[column] + winSize.y) / 2;
                }
                
                Vector2 itemSize = item.contentSize;
				columnWidth = Mathf.RoundToInt(Mathf.Max(columnWidth, itemSize.x));
				item.position=new Vector2(x + columnWidths[column] / 2,
                                               y - winSize.y / 2);
                
                y -= itemSize.y + 10;
                ++rowsOccupied;
                
                if(rowsOccupied >= columnRows) {
					x += columnWidth + 5;
					
					rowsOccupied = 0;
					columnRows = 0;
					columnWidth = 0;
					++column;
				}
			}
		}
		#endregion

//		
//		/** set event handler priority. By default it is: kCCMenuTouchPriority */
//		-(void) setHandlerPriority:(NSInteger)newPriority;
	}
}
