using UnityEngine;
using System.Collections;

namespace BBGamelib
{
	#region mark - CCRGBAProtocol
	public interface CCRGBAProtocol{
		/** sets and returns the color (tint)*/
		Color32 color{ get; set;}
		
		/** returns the displayed color */
		Color32 displayedColor{ get;}

		/** whether or not color should be propagated to its children */
		bool cascadeColorEnabled{ get; set;}
		
		/** recursive method that updates display color */
		void updateDisplayedColor(Color32 color);
		
		/** sets and returns the opacity.
		 @warning If the the texture has premultiplied alpha then, the R, G and B channels will be modified.
		 Values goes from 0 to 255, where 255 means fully opaque.
		 */
		byte opacity{ get; set;}
		/** returns the displayed opacity */
		byte displayedOpacity{ get;}
		/** whether or not opacity should be propagated to its children */
		bool cascadeOpacityEnabled{get;set;}
		
		/** recursive method that updates the displayed opacity */
		void updateDisplayedOpacity(byte opacity);

		/** sets the premultipliedAlphaOpacity property.
		 If set to NO then opacity will be applied as: glColor(R,G,B,opacity);
		 If set to YES then opacity will be applied as: glColor(opacity, opacity, opacity, opacity );
		 Textures with premultiplied alpha will have this property by default on YES. Otherwise the default value is NO
		 */
		bool opacityModifyRGB{get;set;}
	}
	#endregion
	
	#region mark - CCSpriteProtocol
	/** CCNode objects that uses a Sprite to render the images.
	 The texture can have a blending function.
	 If the texture has alpha premultiplied the default blending function is:
	    src=GL_ONE dst= GL_ONE_MINUS_SRC_ALPHA
	 else
		src=GL_SRC_ALPHA dst= GL_ONE_MINUS_SRC_ALPHA
	 But you can change the blending function at any time.
	 @since v0.8.0
	 */
	public interface  CCSpriteProtocol{
		CCSpriteFrame displayFrame{get;set;}
	}
	#endregion

	
	#region mark - CCLabelProtocol
	/** Common interface for Labels */
	public interface CCLabelProtocol{
		/** sets a new label using an NSString.
		 The string will be copied.
		 */
		string text{get;set;}
	}
	#endregion
}

