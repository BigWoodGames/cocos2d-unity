using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** CCLayerRGBA is a subclass of CCLayer that implements the CCRGBAProtocol protocol. *IMPORTANT: unlike cocos2d-objc, there is no solid color background in this class.

	 All features from CCLayer are valid, plus the following new features that propagate into children that conform to the CCRGBAProtocol:
	 - opacity
	 - RGB colors
	 @since 2.1
	 */
	public class CCLayerRGBA : CCLayer, CCRGBAProtocol
	{
		byte		_displayedOpacity, _realOpacity;
		Color32	_displayedColor, _realColor;
		bool		_cascadeOpacityEnabled, _cascadeColorEnabled;
		
		
		protected override void init ()
		{
			base.init ();
			
			_displayedOpacity = _realOpacity = 255;
			_displayedColor = _realColor = Color.white;
			this.cascadeOpacityEnabled = false;
			this.cascadeColorEnabled = false;
		}
		
		public bool cascadeOpacityEnabled{get{return _cascadeOpacityEnabled;} set{_cascadeOpacityEnabled=value;}}
		public bool cascadeColorEnabled{get{return _cascadeColorEnabled;} set{_cascadeColorEnabled=value;}}
		public bool opacityModifyRGB{get{return false;} set{}}
		public byte opacity{
			get{return _realOpacity;}
			
			/** Override synthesized setOpacity to recurse items */
			set{
				_displayedOpacity = _realOpacity = value;
				
				if( _cascadeOpacityEnabled ) {
					byte parentOpacity = 255;
					if( _parent is CCRGBAProtocol && ((CCRGBAProtocol)_parent).cascadeOpacityEnabled)
						parentOpacity = ((CCRGBAProtocol)_parent).displayedOpacity;
					updateDisplayedOpacity(parentOpacity);
				}		
			}
		}
		
		public byte displayedOpacity{get{return _displayedOpacity;}}
		public Color32 displayedColor{get{return _displayedColor;}}
		
		public Color32 color{
			get{return _realColor;}
			set{
				_displayedColor = _realColor = value;
				
				if( _cascadeColorEnabled ) {
					Color32 parentColor = Color.white;
					if( _parent is CCRGBAProtocol && ((CCRGBAProtocol)_parent).cascadeColorEnabled )
						parentColor = ((CCRGBAProtocol)_parent).displayedColor;
					updateDisplayedColor(parentColor);
				}
			}
		} 
		
		public void updateDisplayedOpacity(byte parentOpacity){
			_displayedOpacity = (byte)(_realOpacity * parentOpacity/255.0f);
			
			if (_cascadeOpacityEnabled) {
				
				var enumerator = _children.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCNode item = enumerator.Current;
					if (item is CCRGBAProtocol) {
						((CCRGBAProtocol)item).updateDisplayedOpacity(_displayedOpacity);
					}
				}
			}
		}
		
		public void updateDisplayedColor(Color32 parentColor){
			_displayedColor.r = (byte)(_realColor.r * parentColor.r/255.0f);
			_displayedColor.g = (byte)(_realColor.g * parentColor.g/255.0f);
			_displayedColor.b = (byte)(_realColor.b * parentColor.b/255.0f);
			
			if (_cascadeColorEnabled) {
				
				var enumerator = _children.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCNode item = enumerator.Current;
					if (item is CCRGBAProtocol) {
						((CCRGBAProtocol)item).updateDisplayedColor(_displayedColor);
					}
				}
			}		
		}
	}
}
