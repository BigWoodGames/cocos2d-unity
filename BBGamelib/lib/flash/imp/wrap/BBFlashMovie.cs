using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


namespace BBGamelib{
	public enum kFrameEventMode{
		LabelFrame,
		EveryFrame,
	}
	public interface BBFlashMovie{
		/*id*/
		int characterId{ get;}
		
		/*View for renderer*/
		CCNodeRGBA view{get;}
		
		/*Returns the child display object.*/
		BBFlashMovie getChildByClassName(string name);
		BBFlashMovie getChildByInstanceName(string name);
		List<BBFlashMovie> getChildrenByClassName(string name);
		
		/*Returns an array of objects that lie under the specified point and are children (or grandchildren, and so on) of this DisplayObjectContainer instance.*/
		List<BBFlashMovie> GetMoviesUnderPointInWorld(Vector2 point);
		List<BBFlashMovie> GetMoviesUnderTouch(UITouch touch);
		
		/*Returns a rectangle that defines the area of the display object relative to the coordinate system of the targetCoordinateSpace object.*/
		Rect bounds{ get;}

		/*[read-only] Specifies the number of the frame in which the playhead is located in the timeline of the MovieClip instance.*/
		int currentFrame{ get;}
		
		/*[read-only] Specifies the number of the frame in which the playhead is started in the timeline of the MovieClip instance.*/
		int startFrame{ get;}

		/*[read-only] Specifies the number of the frame in which the playhead is ended in the timeline of the MovieClip instance.*/
		int endFrame{ get;}
		
		/*[read-only] The label at the current frame in the timeline of the MovieClip instance.*/
		string currentLabel{ get;}
		
		/*[read-only] Returns an array of FrameLabel objects from the current scene.*/
		string[] labels{ get;}
		
		/*[read-only] The total number of frames in the MovieClip instance.*/
		int totalFrames{get;}
		
		/*[read-only] A Boolean value that indicates whether a movie clip is curently playing.*/
		bool isPlaying{get;}
		
		/*Class name */ 
		string className{ get;}
		
		/*Class name */ 
		string instanceName{ get;}
		
		/*Indicates whether the movie clip play with loop mode*/
		bool loop { get; set;}
		
		/*fps*/
		float fps{ get; set;}
		
		/*has label*/
		bool hasLabel(string label);
		
		/*Starts playing the SWF file at the specified frame.*/
		void gotoAndPlay(int start, int end);
		void gotoAndPlay(int frame, int end, Action<BBFlashMovie> callback);
		void gotoAndPlay(string start, string end);
		void gotoAndPlay(string start, string end, Action<BBFlashMovie> callback);
		void play();
		void play(Action<BBFlashMovie> callback);

		/*Brings the playhead to the specified frame of the movie clip and stops it there.*/
		void gotoAndStop(int frame);
		void gotoAndStop(string frameLabel);

		/*Stops the playhead in the movie clip.*/
		void stop();
		void stopRecursive();
		void pause();
		void resume ();

		int getFrame(string label);

		void addFrameEventListener(System.Object target, Action<BBFlashMovie> callback);
		void removeFrameEventListener(System.Object target);

		/*Set the frame event callback mode. The default mode is kFrameEventMode.LabelFrame, only frames with label will be callback*/
		kFrameEventMode frameEventMode{ get; set;}

		/*Set the tween mode when fps is low. The default mode is kTweenMode.SkipNoLabelFrames*/
		kTweenMode tweenMode{ get; set;}
	}
}
