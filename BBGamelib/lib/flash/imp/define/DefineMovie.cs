using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BBGamelib.flash.imp{
	public class DefineMovie : DefineView
	{
		int _maxDepth;
		public int maxDepth{get{return _maxDepth;} }
		
		FrameObject[] _frames;
		public FrameObject[] frames{get{return _frames;} }

		Dictionary<string ,int> _label_indexs;
		public Dictionary<string ,int> label_indexs{get{return _label_indexs;} }

		protected override void parse(byte[] data, Cursor cursor){
			base.parse (data, cursor);
			_maxDepth = Utils.ReadInt32 (data, cursor);

			int framesCount = Utils.ReadInt32 (data, cursor); 
			_label_indexs = new Dictionary<string, int> (framesCount/4);
			_frames = new FrameObject[framesCount];
			for(int i = 0; i<framesCount; i++){
				FrameObject frame = new FrameObject(i, data, cursor);
				_frames[i] = frame;

				if(frame.label!=null)
					_label_indexs[frame.label] = i;
			}
		}

		
		public override DisplayObject createDisplay (){
			Movie movie = _flash.displayFactory.createMovie (this);
			return movie;
		}

		public override string trace (int indent){
			string indent0 = Utils.RepeatString(indent);
			string indent2 = Utils.RepeatString(indent+2);
			string s = base.trace (indent) + " MaxDepth: " + _maxDepth + "\n";
			if (_label_indexs.Count > 0) {
				s += indent0 + "Labels("+_label_indexs.Count + "):\n";
				var enumerator = _label_indexs.GetEnumerator();
				while (enumerator.MoveNext()) {
					KeyValuePair<string, int> kv = enumerator.Current;
					string label = kv.Key;
					s += indent2 + label + "\n";
				}
			}
			if (_frames.Length > 0) {
				s += indent0 + "FrameCount(" + _frames.Length +"):\n";
				for(int i=0; i<_frames.Length; i++){
					FrameObject frame = _frames[i];
					s += frame.trace(indent + 2) + "\n";
				}
			}
			return s;
		}
	}
}
