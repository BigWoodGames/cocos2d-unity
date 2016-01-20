using UnityEngine;
using System.Collections;

namespace BBGamelib{
	/** A CCMenuItemFont
	 Helper class that creates a CCMenuItemLabel class with a Label
	 */
	public class CCMenuItemFont : CCMenuItemLabel<CCLabelTTF>
	{
		public static int _globalFontSize = kCCItemSize;
		public static string _globalFontName = "Arial";
		/** default font size */
		public static int FontSize{get{return _globalFontSize;}set{_globalFontSize=value;}}
		
		/** default font name */
		public static string FontName{get{return _globalFontName;}set{_globalFontName=value;}}


		int _fontSize;
		string _fontName;


		/** creates a menu item from a string without target/selector. To be used with CCMenuItemToggle */
		public CCMenuItemFont(string value)
		{
			initWithString(value, null);
		}
		/** creates a menu item from a string with the specified block.
		 The block will be "copied".
		 */
		public CCMenuItemFont(string value,CCMenuItemDelegate block)
		{
			initWithString(value, block);
		}
		//
		// Designated initializer
		//
		protected void initWithString(string value, CCMenuItemDelegate block)
		{
			NSUtils.Assert( value.Length > 0, "Value length must be greater than 0");
			
			_fontName = _globalFontName;
			_fontSize = _globalFontSize;
			
			CCLabelTTF label = new CCLabelTTF(value, _fontName, _fontSize);
			initWithLabel (label, block);
		}
		
		public void recreateLabel()
		{
			CCLabelTTF label = new CCLabelTTF(_label.text, _fontName, _fontSize);
			this.label = label;
		}


		public int fontSize{get{return _fontSize;}
			set{
				_fontSize=value;
				recreateLabel();
			}
		}

		public string fontName{get{return _globalFontName;}
			set{
				_globalFontName=value;
				recreateLabel();
			}
		}
	}
}

