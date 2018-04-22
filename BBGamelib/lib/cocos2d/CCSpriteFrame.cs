using UnityEngine;
using System.Collections;
using System;

namespace BBGamelib{
    [Serializable]
    public class CCSpriteFrame
    {
        [SerializeField] Rect _textureRect;
        [SerializeField] Vector2 _originalSize;
        [SerializeField] Vector2 _offset;
        [SerializeField] bool _rotated;
        [SerializeField] bool _semiTransparent;
        [SerializeField] Texture2D _texture;
        [SerializeField] string _textureFilename;
        [SerializeField] string _frameFileName;

        public CCSpriteFrame(Texture2D texture, Rect rect){
            init (texture, rect);
        }

        public CCSpriteFrame(Texture2D texture, Rect textureRect, bool rotated, Vector2 offset, Vector2 originalSize, bool semiTransparent){
            init (texture, textureRect, rotated, offset, originalSize, semiTransparent);
        }

        void init(Texture2D texture, Rect textureRect){
            init(texture, textureRect, false, Vector2.zero, textureRect.size, true);
        }

        void init(Texture2D texture, Rect textureRect, bool rotated, Vector2 offset, Vector2 originalSize, bool semiTransparent){
            _texture = texture;
            _textureRect = textureRect;
            _rotated = rotated;
            _offset = offset;
            _originalSize = originalSize;
            _semiTransparent = semiTransparent;
        }

        /** rect of the frame in points. */
        public Rect textureRect{get{return _textureRect;} set{_textureRect = value;}}

        /** whether or not the rect of the frame is rotated ( x = x+width, y = y+height, width = height, height = width ) */
        public bool rotated{get{return _rotated;} set{_rotated=value;}}

        /** offset of the frame in points */
        public Vector2 offset{get{return _offset;} set{_offset=value;}}

        /** original size of the trimmed image in points */
        public Vector2 originalSize{get{return _originalSize;} set{_originalSize=value;}}

        /** whether or not the frame is semi-transparent */
        public bool semiTransparent{get{return _semiTransparent;} set{_semiTransparent=value;}}

        /** texture of the frame */
        public Texture2D texture{get{return _texture;} set{_texture=value;}}

        /** filename of the texture */
        public string textureFilename{get{return _textureFilename;} set{_textureFilename=value;}}

        /** filename of the frame */
        public string frameFileName{get{return _frameFileName;} set{_frameFileName=value;}}

    }
}

