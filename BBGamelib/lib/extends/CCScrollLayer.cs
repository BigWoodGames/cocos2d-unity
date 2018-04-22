using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib{
	public interface CCScrollLayerDelegate{
		void scrollLayerDraging(CCScrollLayer sender);
		void scrollLayerScrollingStarted(CCScrollLayer sender, int page);
		void scrollLayerToPageNum(CCScrollLayer sender, int page);
	}

	enum kCCScrollLayerState{
		Idle,
		Sliding,
	}

	public class CCScrollLayer : CCLayerRGBA {
		
		CCScrollLayerDelegate delegate__;
		
		// Holds the current page being displayed.
		int currentScreen_;
		
		// Number of previous page being displayed.
		int prevScreen_;
		
		// The x coord of initial point the user starts their swipe.
		float startSwipe_;
		
		// For what distance user must slide finger to start scrolling menu.
		float minimumTouchLengthToSlide_;
		
		// For what distance user must slide finger to change the page.
		float minimumTouchLengthToChangePage_;
		
		// Whenever show or not gray/white dots under scrolling content.
		bool showPagesIndicator_;
		Vector2 pagesIndicatorPosition_;
		Color pagesIndicatorSelectedColor_;
		Color pagesIndicatorNormalColor_;
		
		// Internal state of scrollLayer (scrolling or idle).
		kCCScrollLayerState state_;
		
		bool stealTouches_;

		// Holds the touch that started the scroll
		UITouch scrollTouch_;
		
		// Holds pages.
		List<CCLayer> layers_;
		
		// Holds current pages width offset.
		float pagesWidthOffset_;
		
		// Holds current margin offse	
		float marginOffset_;

		bool isMoving_;

		float scrollTime_;

//		public int touchPriority{set{touchPriority_ = value;} get{return touchPriority_;}}
		public CCScrollLayerDelegate delegate_{set{delegate__ = value;} get{return delegate__;}}
		
		/** Calibration property. Minimum moving touch length that is enough
 		  * to cancel menu items and start scrolling a layer.
		  */
		public float minimumTouchLengthToSlide{set{minimumTouchLengthToSlide_=value;} get{return minimumTouchLengthToSlide_;}}
		
		/** Calibration property. Minimum moving touch length that is enough to change
 		  * the page, without snapping back to the previous selected page.
 		  */
		public float minimumTouchLengthToChangePage{set{minimumTouchLengthToChangePage_=value;} get{return minimumTouchLengthToChangePage_;}}
		
		/** Offset that can be used to let user see empty space over first or last page. */
		public float marginOffset{ set { marginOffset_ = value; } get{return marginOffset_;}}
		
		/** If YES - when starting scrolling CCScrollLayer will claim touches, that are
 		  * already claimed by others targetedTouchDelegates by calling CCTouchDispatcher#touchesCancelled
		  * Usefull to have ability to scroll with touch above menus in pages.
 		  * If NO - scrolling will start, but no touches will be cancelled.
 		  * Default is YES.
 		*/
		public bool stealTouches{set{stealTouches_=value;} get{return stealTouches_;}}

		/** Whenever show or not white/grey dots under the scroll layer.
 		  * If yes - dots will be rendered in parents transform (rendered after scroller visit).
 		  */
		public bool showPagesIndicator{set{showPagesIndicator_=value;} get{return showPagesIndicator_;}}
		
		/** Position of dots center in parent coordinates.
 		 * (Default value is screenWidth/2, screenHeight/4)
		 */
		public Vector2 pagesIndicatorPosition{set{pagesIndicatorPosition_=value;} get{return pagesIndicatorPosition_;}}
		
		/** Color of dot, that represents current selected page(only one dot). */
		public Color pagesIndicatorSelectedColor{ set { pagesIndicatorSelectedColor_ = value; } get{return pagesIndicatorSelectedColor_;}}
		
		/** Color of dots, that represents other pages. */
		public Color pagesIndicatorNormalColor{ set { pagesIndicatorNormalColor_ = value; } get { return pagesIndicatorNormalColor_; } }

		/** Total pages available in scrollLayer. */
		public int totalScreens{get{return layers_.Count;}}  
		
		/** Current page number, that is shown. Belongs to the [0, totalScreen] interval. */
		public int currentScreen{get{return currentScreen_;}}
		
		/** Offset, that can be used to let user see next/previous page. */
		public float pagesWidthOffset{set{pagesWidthOffset_=value;} get{ return pagesWidthOffset_; }}
		
		/** Returns array of pages CCLayer's  */
		public List<CCLayer> pages{get{return layers_;}}

		
		public float scrollTime{get{return scrollTime_;} set{scrollTime_ = value;}}

		public CCScrollLayer(List<CCLayer>layers, int  pagesWidthOffset, int priority){
			initWithLayers (layers, pagesWidthOffset, priority);		
		}

		public void initWithLayers(List<CCLayer>layers, int  widthOffset, int priority){
			this.isTouchEnabled = true;
			this.stealTouches = true;
			this.touchPriority = priority;

			// Set default minimum touch length to scroll.
			this.minimumTouchLengthToSlide = 30.0f;
			this.minimumTouchLengthToChangePage = 100.0f;
			
			this.marginOffset = CCDirector.sharedDirector.winSize.x;
			
			// Show indicator by default.
			this.showPagesIndicator = true;
			this.pagesIndicatorPosition = new Vector2(0.5f * this.contentSize.x, Mathf.Ceil ( this.contentSize.y / 8.0f ));
			this.pagesIndicatorNormalColor = new Color32(0x96,0x96,0x96,0xFF);
			this.pagesIndicatorSelectedColor = new Color32(0xFF,0xFF,0xFF,0xFF);
			
			// Set up the starting variables
			currentScreen_ = 0;
			
			// Save offset.
			this.pagesWidthOffset = widthOffset;
			
			// Save array of layers.
			layers_ = new List<CCLayer> (layers);

			isMoving_ = false;
			scrollTime_ = 0.3f;

			this.updatePages();
		}
		
		public void updatePages()
		{
			// Loop through the array and add the screens if needed.
			int i = 0;
			
			var enumerator = layers_.GetEnumerator();
			while (enumerator.MoveNext()) {
				var l = enumerator.Current;
				l.anchorPoint = Vector2.zero;
				l.contentSize = CCDirector.sharedDirector.winSize;
				l.position = new Vector2(  (i * (this.contentSize.x - this.pagesWidthOffset)), 0  );
				if (l.parent == null)
					this.addChild(l);
				i++;
			}
		}
		
		void moveToPageEnded()
		{
			isMoving_ = false;
			if (prevScreen_ != currentScreen_)
			{
				if(this.delegate_ != null)
					this.delegate_.scrollLayerToPageNum(this, currentScreen_);
			}
			prevScreen_ = currentScreen_ = this.pageNumberForPosition (this.position);
		}

		int pageNumberForPosition(Vector2 position)
		{
			float pageFloat = - this.position.x / (this.contentSize.x - this.pagesWidthOffset);
			int pageNumber = Mathf.CeilToInt(pageFloat);
			if ( (float)pageNumber - pageFloat  >= 0.5f)
				pageNumber--;
			
			
			pageNumber = (int)Mathf.Max(0, pageNumber);
			pageNumber = (int)Mathf.Min(layers_.Count - 1, pageNumber);
			
			return pageNumber;
		}

		Vector2 positionForPageWithNumber(int pageNumber)
		{
			return new Vector2( - pageNumber * (this.contentSize.x - this.pagesWidthOffset), 0.0f );
		}
		
		public void moveToPage(int page)
		{
			if (page < 0 || page >= layers_.Count) {
				CCDebug.Error("CCScrollLayer#moveToPage: {0} - wrong page number, out of bounds. ", page);
				return;
			}
			if(this.delegate_ != null)
				this.delegate_.scrollLayerScrollingStarted(this, page);
			isMoving_ = true;
			CCActionFiniteTime changePage = new CCMoveTo(scrollTime_, this.positionForPageWithNumber(page));
			changePage = CCSequence.Actions( changePage, new CCCallFunc(this, moveToPageEnded));
			this.runAction(changePage);
			prevScreen_ = currentScreen_;
			currentScreen_ = page;
		}
		
		public void selectPage(int page)
		{
			if (page < 0 || page >= layers_.Count) {
				CCDebug.Error(@"CCScrollLayer#selectPage: {0} - wrong page number, out of bounds. ", page);
				return;
			}
			
			this.position = this.positionForPageWithNumber(page);
			prevScreen_ = currentScreen_;
			currentScreen_ = page;
		}
		
		public void addPage(CCLayer aPage)
		{
			this.addPage(aPage,layers_.Count);
		}

		public void addPage(CCLayer aPage, int page)
		{
			int pageNumber = (int)Mathf.Min(page, layers_.Count);
			pageNumber = (int)Mathf.Max(pageNumber, 0);
			
			layers_.Insert(pageNumber,  aPage);
			
			this.updatePages();
			
			this.moveToPage(currentScreen_);
		}

		public void removePage(CCLayer aPage)
		{
			layers_.Remove(aPage);
			this.removeChildAndCleanup(aPage, true);
			
			this.updatePages();
			
			prevScreen_ = currentScreen_;
			currentScreen_ = (int)Mathf.Min(currentScreen_, layers_.Count - 1);
			this.moveToPage(currentScreen_);
		}
		public void removePageWithNumber(int page)
		{
			if (page >= 0 && page < layers_.Count)
				this.removePage(layers_[page]);
		}
		
		/** Register with more priority than CCMenu's but don't swallow touches. */
		protected override void registerWithTouchDispatcher()
		{	
			CCTouchDispatcher dispatcher = CCDirector.sharedDirector.touchDispatcher;
			int priority = _touchPriority;
			
			dispatcher.addTargetedDelegate(this, priority, false);
		}
		
		/** Hackish stuff - stole touches from other CCTouchDispatcher targeted delegates.
	   	 Used to claim touch without receiving ccTouchBegan. */
		void claimTouch(UITouch aTouch)
		{
			
			CCTouchDispatcher dispatcher = CCDirector.sharedDirector.touchDispatcher;

			// Enumerate through all targeted handlers.
			
			var enumerator = dispatcher.getTargetedHandlers().GetEnumerator();
			while (enumerator.MoveNext()) {
				CCTargetedTouchHandler handler = (CCTargetedTouchHandler)enumerator.Current;
				// Only our handler should claim the touch.
				if (handler.delegate_ == this)
				{
					if (!(handler.claimedTouches.Contains(aTouch)))
					{
						handler.claimedTouches.Add(aTouch);
					}
				}
				else
				{
					// Steal touch from other targeted delegates, if they claimed it.
					if (handler.claimedTouches.Contains(aTouch))
					{
						CCTouchOneByOneDelegate oneByOneTouchDelegate = (CCTouchOneByOneDelegate)handler.delegate_; 
						if(oneByOneTouchDelegate != null){
							oneByOneTouchDelegate.ccTouchCancelled(aTouch);
						}
						handler.claimedTouches.Remove(aTouch);
					}
				}
			}
		}

		public override void ccTouchCancelled (UITouch touch)
		{
			if( scrollTouch_ == touch ) {
				scrollTouch_ = null;
				this.selectPage(currentScreen_);
			}
		}
		public override bool ccTouchBegan (UITouch touch)
		{
			if (isMoving_)
				return false;
			if( scrollTouch_ == null ) {
				scrollTouch_ = touch;
			} else {
				return false;
			}
			
			Vector2 touchPoint = this.convertTouchToNodeSpace (touch);
			touchPoint = this.convertToWorldSpace (touchPoint);
			
			startSwipe_ = touchPoint.x;
			state_ = kCCScrollLayerState.Idle;
			return true;
		}
		public override void ccTouchMoved (UITouch touch)
		{
			if( scrollTouch_ != touch ) {
				return;
			}
			
			Vector2 touchPoint = this.convertTouchToNodeSpace (touch);
			touchPoint = this.convertToWorldSpace (touchPoint);
			
			
			// If finger is dragged for more distance then minimum - start sliding and cancel pressed buttons.
			// Of course only if we not already in sliding mode
			if ( (state_ != kCCScrollLayerState.Sliding)
			    && (Mathf.Abs(touchPoint.x-startSwipe_) >= this.minimumTouchLengthToSlide) )
			{
				state_ = kCCScrollLayerState.Sliding;
				
				// Avoid jerk after state change.
				startSwipe_ = touchPoint.x;
				
				if (this.stealTouches)
				{
					this.claimTouch(touch);
				}
				if(this.delegate_ != null)
					this.delegate_.scrollLayerDraging(this);
			}
			
			if (state_ == kCCScrollLayerState.Sliding)
			{
				float desiredX = (- currentScreen_ * (this.contentSize.x - this.pagesWidthOffset)) + touchPoint.x - startSwipe_;
				int page = this.pageNumberForPosition(new Vector2(desiredX, 0));
				float offset = desiredX - this.positionForPageWithNumber(page).x;
				if ((page == 0 && FloatUtils.Big(offset , 0)) || (page == layers_.Count - 1 && FloatUtils.Small(offset , 0)))
					offset -= marginOffset_ * offset / CCDirector.sharedDirector.winSize.x;
				else
					offset = 0;
				this.position = new Vector2(desiredX - offset, 0);
			}
		}

		public override void ccTouchEnded (UITouch touch)
		{
			if( scrollTouch_ != touch ) 
				return;
			scrollTouch_ = null;

			Vector2 touchPoint = this.convertTouchToNodeSpace (touch);
			touchPoint = this.convertToWorldSpace (touchPoint);
			
			int selectedPage = currentScreen_;
			float delta = touchPoint.x - startSwipe_;
			if (Mathf.Abs(delta) >= this.minimumTouchLengthToChangePage)
			{
				selectedPage = this.pageNumberForPosition(this.position);
				if (selectedPage == currentScreen_)
				{
					if (delta < 0.0f && selectedPage < layers_.Count - 1)
						selectedPage++;
					else if (delta > 0.0f && selectedPage > 0)
						selectedPage--;
				}
			}
			this.moveToPage(selectedPage);
		}
	}
}
