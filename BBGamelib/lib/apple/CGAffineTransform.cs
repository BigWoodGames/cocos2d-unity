using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public struct CGAffineTransform
	{
		public float a, b, c, d;
		public float tx, ty;

		public static readonly CGAffineTransform Identity=CGAffineTransform.Make(1, 0, 0, 1, 0, 0);
		public static CGAffineTransform Make(float a, float b, float c, float d, float tx, float ty){
			CGAffineTransform t;
			t.a = a; t.b = b; t.c = c; t.d = d; t.tx = tx; t.ty = ty;
			return t;
		}
		public static CGAffineTransform MakeTranslation(float tx, float ty){
			CGAffineTransform t = Make (1, 0, 0, 1, tx, ty);
			return t;
		}
		public static CGAffineTransform MakeScale(float sx, float sy){
			CGAffineTransform t = Make (sx, 0, 0, sy, 0, 0);
			return t;
		}
		public static CGAffineTransform MakeRotation(float angle){
			float cosAngle = Mathf.Cos (angle);
			float sinAngle = Mathf.Sin (angle);
			CGAffineTransform t = Make (cosAngle, sinAngle, -sinAngle, cosAngle, 0, 0);
			return t;
		}
		public static bool IsIdentity(CGAffineTransform t){
			return EqualToTransform (Identity, t);
		}
		public static CGAffineTransform Translate(CGAffineTransform t, float tx, float ty){
			CGAffineTransform translate = MakeTranslation(tx,ty);
			return Concat(translate,t);
		}
		public static CGAffineTransform Scale(CGAffineTransform t, float sx, float sy){
			CGAffineTransform scale=MakeScale(sx,sy);
			return Concat(scale,t);
		}
		public static CGAffineTransform Rotate(CGAffineTransform t, float angle){
			CGAffineTransform rotate=MakeRotation(angle);
			return Concat(rotate,t);
		}
		public static CGAffineTransform Invert(CGAffineTransform xform){
			CGAffineTransform result;
			float determinant;
			
			determinant=xform.a*xform.d-xform.c*xform.b;
			if(FloatUtils.EQ(determinant, 0)){
				return xform;
			}
			
			result.a=xform.d/determinant;
			result.b=-xform.b/determinant;
			result.c=-xform.c/determinant;
			result.d=xform.a/determinant;
			result.tx=(-xform.d*xform.tx+xform.c*xform.ty)/determinant;
			result.ty=(xform.b*xform.tx-xform.a*xform.ty)/determinant;
			
			return result;
		}
		public static CGAffineTransform Concat(CGAffineTransform xform,CGAffineTransform append){
			CGAffineTransform result;
			
			result.a=xform.a*append.a+xform.b*append.c;
			result.b=xform.a*append.b+xform.b*append.d;
			result.c=xform.c*append.a+xform.d*append.c;
			result.d=xform.c*append.b+xform.d*append.d;
			result.tx=xform.tx*append.a+xform.ty*append.c+append.tx;
			result.ty=xform.tx*append.b+xform.ty*append.d+append.ty;
			
			return result;
		}
		
		public static bool EqualToTransform(CGAffineTransform t1,CGAffineTransform t2){
			return FloatUtils.EQ(t1.a, t2.a) && 
					FloatUtils.EQ(t1.b, t2.b) &&
					FloatUtils.EQ(t1.c, t2.c) && 
					FloatUtils.EQ(t1.d, t2.d) && 
					FloatUtils.EQ(t1.tx, t2.tx) && 
					FloatUtils.EQ(t1.ty, t2.ty);
		}
		public static Vector2 CGPointApplyAffineTransform(Vector2 point, CGAffineTransform xform){
			Vector2 p;
			
			p.x=xform.a*point.x+xform.c*point.y+xform.tx;
			p.y=xform.b*point.x+xform.d*point.y+xform.ty;
			
			return p;
		}
		public static Vector2 CGSizeApplyAffineTransform(Vector2 size,CGAffineTransform xform){
			Vector2 s;
			
			s.x=xform.a*size.x+xform.c*size.y;
			s.y=xform.b*size.x+xform.d*size.y;
			
			return s;
		}
		public static Rect CGRectApplyAffineTransform(Rect rect, CGAffineTransform transform)
		{
			float xMin = rect.position.x;
			float xMax = rect.position.x + rect.size.x;
			float yMin = rect.position.y;
			float yMax = rect.position.y + rect.size.y;
			
			Vector2[] points = new Vector2[4]{
				CGPointApplyAffineTransform(new Vector2(xMin, yMin), transform),
				CGPointApplyAffineTransform(new Vector2(xMin, yMax), transform),
				CGPointApplyAffineTransform(new Vector2(xMax, yMin), transform),
				CGPointApplyAffineTransform(new Vector2(xMax, yMax), transform),
			};
			
			float newXMin =  float.MaxValue;
			float newXMax =  float.MinValue;
			float newYMin =  float.MaxValue;
			float newYMax =  float.MinValue;
			
			for (int i = 0; i < 4; i++) {
				newXMax = Mathf.Max(newXMax, points[i].x);
				newYMax = Mathf.Max(newYMax, points[i].y);
				newXMin = Mathf.Min(newXMin, points[i].x);
				newYMin = Mathf.Min(newYMin, points[i].y);
			}
			
			Rect result = new Rect(newXMin, newYMin, newXMax - newXMin, newYMax - newYMin);
			
			return result;
		}
	}
}
