using UnityEngine;
using System.Collections;
using BBGamelib.flash.imp;
using System.Collections.Generic;
using System;

namespace BBGamelib.flash.wrap{
	public class BBFlashGraphic : Graphic, BBFlashMovie
	{
		public BBFlashGraphic(DefineGraphic define):base(define){
		}

		public BBFlashMovie getChildByClassName(string name){
			return null;			
		}
		public BBFlashMovie  getChildByInstanceName(string name){
			return null;			
		}
		public List<BBFlashMovie> getChildrenByClassName(string name){
			return null;	
		}
		
		public List<BBFlashMovie> GetMoviesUnderPointInWorld(Vector2 worldPoint){
			Vector2 nodePoint = view.convertToNodeSpaceAR (worldPoint);
			Rect bd = bounds;
			if (bd.Contains (nodePoint)) {
				List<BBFlashMovie> children = new List<BBFlashMovie>();
				children.Add(this);
				return children;
			}
			return null;	
		}
		
		public List<BBFlashMovie> GetMoviesUnderTouch(UITouch touch){
			Vector2 point = view.convertTouchToNodeSpace (touch);
			point = view.convertToWorldSpace (point);
			return GetMoviesUnderPointInWorld (point);
		}

		
		public bool hasLabel(string label){
			return false;
		}
		
		public void gotoAndPlay(int start, int end){
		}
		
		public void gotoAndPlay(int start, int end, Action<BBFlashMovie> callback){
		}
		
		public void gotoAndPlay(string start, string end){
		}
		
		public void gotoAndPlay(string start, string end, Action<BBFlashMovie> callback){
		}
		
		public void gotoAndStop(int frame){
		}
		
		public void gotoAndStop(string label){
		}
		
		public void play(){
		}

		public void play(Action<BBFlashMovie> callback){
		}

		public void stop(){
		}

		public void stopRecursive(){
		}
		
		public void pause(){
		}
		
		public void resume(){
		}

		public int getFrame(string label){
			return -1;
		}
		
		public void addFrameEventListener(System.Object target, Action<BBFlashMovie> callback){
		}

		public void removeFrameEventListener(System.Object target){
		}

		public void frameEventCallback(Movie mov){
		}

		
		public int currentFrame{ get{return 0;}}
		public int startFrame{get{return 0;}}
		public int endFrame{get{return 0;}}
		public string currentLabel{ get{return null;}}
		public int totalFrames{ get{return 1;}}
		public bool isPlaying{ get { return false; } }
		
		string[] _labels = new string[0];
		public string[] labels{ get{return _labels;}}
		public bool loop { get { return false; } set{ ; }}
		public kFrameEventMode frameEventMode{ get { return kFrameEventMode.LabelFrame; } set{ ; }}
	}
}

