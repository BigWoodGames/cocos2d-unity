using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib.flash.imp{
	public class TagDefineMovie: TagDefineDisplay
	{
		ITag[] _tags;
		public ITag[] tags{ get { return _tags; } }

		int _maxDepth;
		public int maxDepth{get{return _maxDepth;} }
		
		Frame[] _frames;
		public Frame[] frames{get{return _frames;} }
		
		public TagDefineMovie(Flash flash, byte[] data, Cursor cursor):base(flash, data, cursor){
			int tagsCount = Utils.ReadInt32 (data, cursor);
			_tags = new ITag[tagsCount];
			for (int i=0; i<tagsCount; i++) {
				byte type = Utils.ReadByte(data, cursor);
				if(type == TagPlaceObject.TYPE){
					ITag tag = new TagPlaceObject(this.flash, data, cursor);
					_tags[i] = tag;
				}else if(type == TagRemoveObject.TYPE){
					ITag tag = new TagRemoveObject(this.flash, data, cursor);
					_tags[i] = tag;
				}
			}

			_maxDepth = Utils.ReadInt32 (data, cursor);

			int framesCount = Utils.ReadInt32 (data, cursor); 
			_frames = new Frame[framesCount];
			for(int i = 0; i<framesCount; i++){
				Frame frame = new Frame(i, data, cursor);
				_frames[i] = frame;
			}
		}

		public override string trace (int indent){
			string indent0 = Utils.RepeatString(indent);
			string s = base.trace (indent) + " MaxDepth: " + _maxDepth + "\n";
			if (_frames.Length > 0) {
				s += indent0 + "FrameCount(" + _frames.Length +"):\n";
				for(int i=0; i<_frames.Length; i++){
					Frame frame = _frames[i];
					s += frame.trace(indent + 2) + "\n";
				}
			}
			return s;
		}
	}
}
