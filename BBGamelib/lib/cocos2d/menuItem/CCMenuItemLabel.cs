using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** An abstract class for "label" CCMenuItemLabel items
	 Any CCNode that supports the CCLabelProtocol protocol can be added.
	 Supported nodes:
	   - CCLabelTTF
	 */
	public class CCMenuItemLabel<T> : CCMenuItem where T: CCNode, CCLabelProtocol, CCRGBAProtocol
	{
		protected T _label;
		protected Color32	_colorBackup;
		protected Color32	_disabledColor;
		protected float		_originalScale;
		
		public Color32 disabledColor{
			get{return _disabledColor;}
			set{ _disabledColor = value;}
		}
		
		protected CCMenuItemLabel(){
		}
		
		public CCMenuItemLabel(T label, CCMenuItemDelegate block=null){
			initWithLabel (label, block);
		}
		
		//
		// Designated initializer
		//
		public void initWithLabel(T label, CCMenuItemDelegate block)
		{
			base.initWithBlock (block);
			_originalScale = 1;
			_colorBackup = Color.white;
			this.disabledColor = new Color32( 126,126,126, 255);
			this.label = label;
			
			this.cascadeColorEnabled = true;
			this.cascadeOpacityEnabled = true;
		}
		
		public T label{
			get{ return _label;}
			set{
				if(_label != value){
					removeChildAndCleanup(_label, true);
					addChild(value);
					_label = value;
					_label.anchorPoint = Vector2.zero;
					this.contentSize = _label.contentSize;
				}
			}
		}
		
		public void setString(string text){
			_label.text = text;
			this.contentSize = _label.contentSize;
		}
		
		public override void activate(){
			if(_isEnabled) {
				this.stopAllActions();
				
				this.scale = _originalScale;
				
				base.activate();
			}
		}
		public override void selected(){
			// subclass to change the default action
			if(_isEnabled) {
				base.selected();
				
				CCAction action = getActionByTag(kCCZoomActionTag);
				if( action!=null )
					stopAction(action);
				else
					_originalScale = this.scale;
				
				CCAction zoomAction = new CCScaleTo(0.1f, _originalScale * 1.2f);
				zoomAction.tag = kCCZoomActionTag;
				runAction(zoomAction);
			}
		}
		
		public override void unselected ()
		{
			// subclass to change the default action
			if(_isEnabled) {
				base.unselected();
				this.stopActionByTag(kCCZoomActionTag);
				CCAction zoomAction = new CCScaleTo(0.1f, _originalScale);
				zoomAction.tag = kCCZoomActionTag;
				runAction(zoomAction);
			}
		}
		
		public override bool isEnabled {
			set {
				if( _isEnabled != value ) {
					if(value == false) {
						_colorBackup = _label.color;
						_label.color = _disabledColor;
					}
					else
						_label.color = _colorBackup;
				}
				base.isEnabled = value;
			}
		}
	}
}
