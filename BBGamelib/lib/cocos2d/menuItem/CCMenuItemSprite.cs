using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** CCMenuItemSprite accepts CCNode<CCRGBAProtocol> objects as items.
	 The images has 3 different states:
	 - unselected image
	 - selected image
	 - disabled image

	 @since v0.8.0
	 */
	public class CCMenuItemSprite<T> : CCMenuItem, CCRGBAProtocol where  T : CCNode, CCRGBAProtocol
	{
		T _normalImage, _selectedImage, _disabledImage;
		
		public CCMenuItemSprite(){
		}
		
		public CCMenuItemSprite(T normalSprite, T selectedSprite, T disabledSprite=null, CCMenuItemDelegate block=null){
			init (normalSprite, selectedSprite, disabledSprite, block);
		}
		
		protected virtual void init(T normalSprite, T selectedSprite, T disabledSprite=null, CCMenuItemDelegate block=null){
			base.initWithBlock (block);
			this.normalImage = normalSprite;
			this.selectedImage = selectedSprite;
			this.disabledImage = disabledSprite;
			
			this.contentSize = _normalImage.contentSize;
			
			this.cascadeColorEnabled = true;
			this.cascadeOpacityEnabled = true;
		}
		/** the image used when the item is not selected */
		public T normalImage{get{return _normalImage;} 
			set{
				if( value != _normalImage ) {
					value.anchorPoint = Vector2.zero;
					
					removeChildAndCleanup(_normalImage, true);
					addChild(value);
					
					_normalImage = value;
					
					this.contentSize = _normalImage.contentSize;
					
					updateImagesVisibility();
				}
			}
		}
		/** the image used when the item is selected */
		public T selectedImage{get{return _selectedImage;} 
			set{
				if( value != _selectedImage ) {
					value.anchorPoint = Vector2.zero;
					
					removeChildAndCleanup(_selectedImage, true);
					addChild(value);
					
					_selectedImage = value;
					
					updateImagesVisibility();
				}
			}
		}
		/** the image used when the item is disabled */
		public T disabledImage{get{return _disabledImage;}
			set{
				if( value != _disabledImage ) {
					value.anchorPoint = Vector2.zero;
					
					removeChildAndCleanup(_disabledImage, true);
					addChild(value);
					
					_disabledImage = value;
					
					updateImagesVisibility();
				}
			}
		}
		
		public override void selected ()
		{
			base.selected ();
			if( _selectedImage!=null ) {
				_normalImage.visible=false;
				if(_selectedImage!=null)
					_selectedImage.visible=true;
				if(_disabledImage!=null)
					_disabledImage.visible=false;
				
			} else { // there is not selected image
				_normalImage.visible=true;
				if(_selectedImage!=null)
					_selectedImage.visible=false;
				if(_disabledImage!=null)
					_disabledImage.visible=false;
			}
		}
		
		public override void unselected ()
		{
			base.unselected ();
			_normalImage.visible=true;
			if(_selectedImage!=null)
				_selectedImage.visible=false;
			if(_disabledImage!=null)
				_disabledImage.visible=false;
		}
		
		public override bool isEnabled {
			set {
				if( _isEnabled != value ) {
					base.isEnabled=value;
					
					updateImagesVisibility();
				}
			}
		}
		
		void updateImagesVisibility(){
			if( _isEnabled ) {
				_normalImage.visible=true;
				if(_selectedImage!=null)
					_selectedImage.visible=false;
				if(_disabledImage!=null)
					_disabledImage.visible=false;
				
			} else {
				if( _disabledImage!=null ) {
					_normalImage.visible=false;
					if(_selectedImage!=null)
						_selectedImage.visible=false;
					if(_disabledImage!=null)
						_disabledImage.visible=true;
				} else {
					_normalImage.visible=true;
					if(_selectedImage!=null)
						_selectedImage.visible=false;
					if(_disabledImage!=null)
						_disabledImage.visible=false;
				}
			}
		}
	}

}