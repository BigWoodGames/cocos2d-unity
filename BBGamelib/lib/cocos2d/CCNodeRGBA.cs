using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** CCNodeRGBA is a subclass of CCNode that implements the CCRGBAProtocol protocol.

	 All features from CCNode are valid, plus the following new features:
	 - opacity
	 - RGB colors

	 Opacity/Color propagates into children that conform to the CCRGBAProtocol if cascadeOpacity/cascadeColor is enabled.
	 */
	public class CCNodeRGBA : CCNode, CCRGBAProtocol
	{
		protected byte		_displayedOpacity, _realOpacity;
		protected Color32		_displayedColor, _realColor;
		protected bool		_cascadeColorEnabled, _cascadeOpacityEnabled;
		
		public bool cascadeColorEnabled{ get{return _cascadeColorEnabled;} set{_cascadeColorEnabled=value;}}
		public bool cascadeOpacityEnabled{ get{return _cascadeOpacityEnabled;} set{_cascadeOpacityEnabled=value;}}

		public CCNodeRGBA ()
		{
			_displayedOpacity = _realOpacity = 255;
			_displayedColor = _realColor = Color.white;
			_cascadeOpacityEnabled = false;
			_cascadeColorEnabled = false;
		}

		// XXX To make BridgeSupport happy
		public virtual byte opacity{ 
			get{return _realOpacity;}
			set{
				_displayedOpacity = _realOpacity = value;
				if( _cascadeOpacityEnabled && _parent!=null) {
					byte parentOpacity = 255;
					if( _parent is CCNodeRGBA && ((CCRGBAProtocol)_parent).cascadeOpacityEnabled)
						parentOpacity = ((CCRGBAProtocol)_parent).displayedOpacity;
					updateDisplayedOpacity(parentOpacity);
				}
			}
		}
		public virtual byte displayedOpacity{
			get{return _displayedOpacity;}
		}
		public virtual void updateDisplayedOpacity(byte parentOpacity)
		{
			_displayedOpacity = (byte)(_realOpacity * parentOpacity/255.0f);
			
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
			get{ return _realColor;}
			set{
				_displayedColor = _realColor = value;
				if(_cascadeColorEnabled && _parent!=null){
					Color parentColor = Color.white;
					if( _parent is CCRGBAProtocol && ((CCRGBAProtocol)_parent).cascadeColorEnabled)
						parentColor = ((CCRGBAProtocol)_parent).displayedColor;
					updateDisplayedColor(parentColor);
				}
			}
		}
		public virtual Color32 displayedColor{
			get{return _displayedColor;}
		}

		
		public virtual void updateDisplayedColor(Color32 parentColor)
		{
			_displayedColor.r = (byte)(_realColor.r * parentColor.r/255.0f);
			_displayedColor.g = (byte)(_realColor.g * parentColor.g/255.0f);
			_displayedColor.b = (byte)(_realColor.b * parentColor.b/255.0f);
			
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
		public virtual bool opacityModifyRGB{
			get{
//				CCDebug.Info("{0}:opacityModifyRGB not implemented", GetType().Name);
				return false;
			}
			set{
//				CCDebug.Info("{0}:opacityModifyRGB not implemented", GetType().Name);
			}
		}

	}
}

