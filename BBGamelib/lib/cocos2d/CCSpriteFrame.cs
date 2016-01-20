using UnityEngine;
using System.Collections;
using System;
using System.IO;

namespace BBGamelib{
	[Serializable]
	public class CCSpriteFrame
	{
		Sprite _sprite;
		[SerializeField] Vector2 _originalSize;
		[SerializeField] Vector2 _offset;
		[SerializeField] bool _rotated;
		[SerializeField] string _textureFilename;

		public CCSpriteFrame(string file){
			_textureFilename = file;
			string ext = Path.GetExtension (file);
			if(ext!=null && ext.Length>0)
				file = file.Replace (ext, "");
			Sprite s = Resources.Load<Sprite> (file);
			NSUtils.Assert (s!=null, "Sprite {0} not found!", file);
			init(s);
		}

		public CCSpriteFrame(Sprite spt){
			init (spt);
		}

		void init(Sprite spt){
			Vector2 size = spt.bounds.size;	
			size = ccUtils.UnitsToPixels (size);
			init (spt, size, Vector2.zero, false);
		}

		
		public CCSpriteFrame(Sprite spt, Vector2 originalSize, Vector2 offset, bool isRoated){
			init (spt, originalSize, offset, isRoated);
		}

		void init(Sprite spt, Vector2 originalSize, Vector2 offset, bool isRoated){
			_sprite = spt;
			_originalSize = originalSize;
			_offset = offset;
			_rotated = isRoated;
		}

		/** sprite of the frame */
		public Sprite sprite{get{return _sprite;} set{_sprite = value;}}

		/** offset of the frame in points */
		public Vector2 offset{get{return _offset;} set{_offset=value;}}
		
		/** original size of the trimmed image in points */
		public Vector2 originalSize{get{return _originalSize;} set{_originalSize=value;}}

		/** whether or not the rect of the frame is rotated ( x = x+width, y = y+height, width = height, height = width ) */
		public bool rotated{get{return _rotated;} set{_rotated=value;}}

		public string textureFilename{get{return _textureFilename;} set{_textureFilename=value;}}

	}
}

