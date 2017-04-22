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
		protected OpacityTransform	_displayedOpacity, _realOpacity;
		protected ColorTransform	_displayedColor, _realColor;
		bool		_cascadeOpacityEnabled, _cascadeColorEnabled;
		
		
        public CCLayerRGBA()
        {
			_displayedOpacity = _realOpacity = OpacityTransform.Default;
			_displayedColor = _realColor = ColorTransform.Default;

			this.cascadeOpacityEnabled = false;
			this.cascadeColorEnabled = false;
		}
		
		public bool cascadeOpacityEnabled{get{return _cascadeOpacityEnabled;} set{_cascadeOpacityEnabled=value;}}
		public bool cascadeColorEnabled{get{return _cascadeColorEnabled;} set{_cascadeColorEnabled=value;}}
		public bool opacityModifyRGB{get{return false;} set{}}
		
        public virtual byte opacity{
			get{return _realOpacity.tint;}
			
			/** Override synthesized setOpacity to recurse items */
			set{
				_displayedOpacity.tint = _realOpacity.tint = value;
				if( _cascadeOpacityEnabled && _parent!=null) {
					OpacityTransform parentOpacity = OpacityTransform.Default;
					if( _parent is CCNodeRGBA && ((CCRGBAProtocol)_parent).cascadeOpacityEnabled)
						parentOpacity = ((CCRGBAProtocol)_parent).displayedOpacity;
					updateDisplayedOpacity(parentOpacity);
				}
			}
		}
		public virtual OpacityTransform opacityTransform
		{
			get{ return _realOpacity;}
			set{
				_displayedOpacity = _realOpacity = value;
				if( _cascadeOpacityEnabled && _parent!=null) {
					OpacityTransform parentOpacity = OpacityTransform.Default;
					if( _parent is CCNodeRGBA && ((CCRGBAProtocol)_parent).cascadeOpacityEnabled)
						parentOpacity = ((CCRGBAProtocol)_parent).displayedOpacity;
					updateDisplayedOpacity(parentOpacity);
				}
			}
		}

		public virtual OpacityTransform displayedOpacity{
			get{return _displayedOpacity;}
		}

		
		public virtual void updateDisplayedOpacity(OpacityTransform parentOpacity)
		{
			_displayedOpacity.tint = (byte)(_realOpacity.tint * parentOpacity.tint/255.0f);
			_displayedOpacity.add = (byte)(_realOpacity.add * parentOpacity.tint + parentOpacity.add);
			
			if (_cascadeOpacityEnabled && _children!=null) {
				var enumerator = _children.GetEnumerator();
				while (enumerator.MoveNext()) {
					CCNode item = enumerator.Current;
					if (item is CCRGBAProtocol) {
						((CCRGBAProtocol)item).updateDisplayedOpacity(_displayedOpacity);
					}
				}
			}
		}

        public virtual Color32 color{
			get{return _realColor.tint;}
			set{
				_displayedColor.tint = _realColor.tint = value;
				if(_cascadeColorEnabled && _parent!=null){
					ColorTransform parentColor = ColorTransform.Default;
					if( _parent is CCRGBAProtocol && ((CCRGBAProtocol)_parent).cascadeColorEnabled)
						parentColor = ((CCRGBAProtocol)_parent).displayedColor;
					updateDisplayedColor(parentColor);
				}
			}
		} 
		
		public virtual ColorTransform colorTransform{
			get{ return _realColor;}
			set{
				_displayedColor = _realColor = value;
				if(_cascadeColorEnabled && _parent!=null){
					ColorTransform parentColor = ColorTransform.Default;
					if( _parent is CCRGBAProtocol && ((CCRGBAProtocol)_parent).cascadeColorEnabled)
						parentColor = ((CCRGBAProtocol)_parent).displayedColor;
					updateDisplayedColor(parentColor);
				}
			}
		}
		
		public virtual ColorTransform displayedColor{
			get{return _displayedColor;}
		}

		public virtual void updateDisplayedColor (ColorTransform parentColor)
		{
			Color displayedTint = _displayedColor.tint;
			Color displayedAdd = _displayedColor.add;
			
			displayedTint.r = (byte)(_realColor.tint.r * parentColor.tint.r/255.0f);
			displayedTint.g = (byte)(_realColor.tint.g * parentColor.tint.g/255.0f);
			displayedTint.b = (byte)(_realColor.tint.b * parentColor.tint.b/255.0f);
			
			displayedAdd.r = (byte)(_realColor.add.r * parentColor.tint.r + parentColor.add.r);
			displayedAdd.g = (byte)(_realColor.add.g * parentColor.tint.g + parentColor.add.g);
			displayedAdd.b = (byte)(_realColor.add.b * parentColor.tint.b + parentColor.add.b);
			
			_displayedColor.tint = displayedTint;
			_displayedColor.add = displayedAdd;
			
			if (_cascadeColorEnabled && _children != null) {
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
