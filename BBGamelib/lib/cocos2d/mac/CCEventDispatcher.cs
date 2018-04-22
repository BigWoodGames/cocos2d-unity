
#if UNITY_STANDALONE || UNITY_WEBGL
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BBGamelib{
	public class CCEventDispatcher : CCEventDelegate
	{
		public abstract class tDelegateEntry{
			public System.Object					aDelegate;
		}
		public class tListEntry : tDelegateEntry
		{
			public int			priority;
		}
		
		public class tListDeletedEntry : tDelegateEntry
		{
			public utList<tListEntry>			listToBeDeleted;
			
		}
		
		public class tListAddedEntry : tDelegateEntry
		{
			public int				priority;
			public utList<tListEntry>		listToBeAdded;
		}

		bool					_dispatchEvents;
		bool					_locked;
		
		utList<tListEntry>	_keyboardDelegates;
		utList<tListEntry>	_mouseDelegates;
		
		utList<tListAddedEntry> _delegatesToBeAdded;
		utList<tListDeletedEntry> _delegatesToBeRemoved;

		public bool dispatchEvents{set{_dispatchEvents=value;}get{return _dispatchEvents;}}

		public CCEventDispatcher(){
			// events enabled by default
			_dispatchEvents = true;
			
			// delegates
			_keyboardDelegates = new utList<tListEntry>();
			_mouseDelegates = new utList<tListEntry>();
			
			_delegatesToBeAdded = new utList<tListAddedEntry>();
			_delegatesToBeRemoved = new utList<tListDeletedEntry>();
			
			_locked = false;
		}

		#region CCEventDispatcher - add / remove delegates

		public void addLaterDelegate(System.Object aDelegate, int priority, utList<tListEntry> list)
		{
			// XXX: Since, "remove" is "executed" after "add", it is not needed to check if the delegate was already added for removal.
			// In fact, if you remove it now, it could be a bug, since EventDispatcher doesn't support updated priority.
			// And the only way to update the priority is by deleting, re-adding the delegate with a new priority
			tListAddedEntry listEntry = new tListAddedEntry();
			
			listEntry.aDelegate = aDelegate;
			listEntry.priority = priority;
			listEntry.listToBeAdded = list;

			utNode<tListAddedEntry> listElement = new utNode<tListAddedEntry> ();
			listElement.next = listElement.prev = null;
			listElement.obj = listEntry;

			_delegatesToBeAdded.DL_APPEND(listElement);
		}
		
		public void addDelegate(System.Object aDelegate, int priority, utList<tListEntry> list)
		{
			tListEntry listEntry = new tListEntry();
			
			listEntry.aDelegate = aDelegate;
			listEntry.priority = priority;

			utNode<tListEntry> listElement = new utNode<tListEntry> ();
			listElement.next = listElement.prev = null;
			listElement.obj = listEntry;

			bool added = false;
			
			for( utNode<tListEntry> elem = list.head; elem != null ; elem = elem.next ) {
				if( priority <= elem.obj.priority ) {
					
					if( elem == list.head )
						list.DL_PREPEND(listElement);
					else {
						listElement.next = elem;
						listElement.prev = elem.prev;
						
						elem.prev.next = listElement;
						elem.prev = listElement;
					}
					
					added = true;
					break;
				}
			}
			
			// Not added? priority has the higher value. Append it.
			if( !added )
				list.DL_APPEND(listElement);

		}
		
		
		public void removeLaterDelegate(System.Object aDelegate, utList<tListEntry> list)
		{
			// Only add it if it was not already added for deletion
			if( ! removeDelegate<tListAddedEntry>(aDelegate, _delegatesToBeAdded)) {
				
				tListDeletedEntry listEntry = new tListDeletedEntry();
				
				listEntry.aDelegate = aDelegate;
				listEntry.listToBeDeleted = list;

				
				utNode<tListDeletedEntry> listElement = new utNode<tListDeletedEntry> ();
				listElement.next = listElement.prev = null;
				listElement.obj = listEntry;
				
				_delegatesToBeRemoved.DL_APPEND(listElement );
			}
		}
		
		public bool removeDelegate<T>(System.Object aDelegate, utList<T> list) where T : tDelegateEntry
		{
			utNode<T> entry;
			for((entry)=(list.head); (entry !=null ); (entry) = entry.next){
				if(entry.obj.aDelegate == aDelegate){
					list.DL_DELETE(entry);
					return true;
				}
			}
			return false;
		}
		
		public void removeAllDelegatesFromList(utList<tListEntry> list)
		{
			NSUtils.Assert( ! _locked, "BUG. Open a ticket. Can't call this function when processing events.");
			
			lock(this) {
				utNode<tListEntry> entry, tmp;
				entry = list.head;
				while(entry != null){
					tmp = entry.next;
					list.DL_DELETE(entry);
					entry = tmp;
					tmp = null;
				}
			}
		}

		public void addMouseDelegate(CCMouseEventDelegate aDelegate, int priority)
		{
			if( _locked )
				addLaterDelegate(aDelegate, priority, _mouseDelegates);
			else
				addDelegate(aDelegate, priority, _mouseDelegates);
			
		}
		
		public void removeMouseDelegate(CCMouseEventDelegate aDelegate)
		{
			if( _locked )
				removeLaterDelegate(aDelegate, _mouseDelegates);
			else
				removeDelegate(aDelegate, _mouseDelegates);
		}

		
		public void removeAllMouseDelegates()
		{
			removeAllDelegatesFromList(_mouseDelegates);
		}

		public void addKeyboardDelegate(CCKeyboardEventDelegate aDelegate, int priority)
		{
			if( _locked )
				addLaterDelegate(aDelegate, priority, _keyboardDelegates);
			else
				addDelegate(aDelegate, priority, _keyboardDelegates);
		}
		
		
		public void removeKeyboardDelegate(System.Object aDelegate)
		{
			if( _locked )
				removeLaterDelegate(aDelegate, _keyboardDelegates);
			else
				removeDelegate(aDelegate, _keyboardDelegates);
		}

		public void removeAllKeyboardDelegates()
		{
			removeAllDelegatesFromList(_keyboardDelegates);
		}
		#endregion
		
		#region mark CCEventDispatcher - Mouse events
		//
		// Mouse events
		//
		
		//
		// Left
		//
		public void mouseDown(NSEvent evt)
		{
			if( _dispatchEvents) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccMouseDown(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		
		public void mouseMoved(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccMouseMoved(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}

		public void mouseDragged(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccMouseDragged(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}

		public void mouseUp(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccMouseUp(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		
		//
		// Mouse Right
		//
		public void rightMouseDown(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccRightMouseDown(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		public void rightMouseDragged(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccRightMouseDragged(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		public void rightMouseUp(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccRightMouseUp(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		
		//
		// Mouse Other
		//
		public void otherMouseDown(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccOtherMouseDown(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		public void otherMouseDragged(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccOtherMouseDragged(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		public void otherMouseUp(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccOtherMouseUp(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		
		//
		// Scroll Wheel
		//
		public void scrollWheel(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccScrollWheel(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		
		//
		// Mouse enter / exit
		public void mouseExited(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccMouseExited(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		public void mouseEntered(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _mouseDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCMouseEventDelegate mouseDelegate = entry.obj.aDelegate as CCMouseEventDelegate;
					bool swallows = mouseDelegate.ccMouseEntered(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		#endregion
		
		
		#region mark CCEventDispatcher - Keyboard events
		// Keyboard events
		public void keyDown(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _keyboardDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCKeyboardEventDelegate keyboardDelegate = entry.obj.aDelegate as CCKeyboardEventDelegate;
					bool swallows = keyboardDelegate.ccKeyDown(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		public void keyUp(NSEvent evt)
		{
			if( _dispatchEvents ) {
				utNode<tListEntry> entry, tmp;
				entry = _keyboardDelegates.head;
				while(entry != null){
					tmp = entry.next;
					CCKeyboardEventDelegate keyboardDelegate = entry.obj.aDelegate as CCKeyboardEventDelegate;
					bool swallows = keyboardDelegate.ccKeyUp(evt);
					if(swallows)
						break;
					entry = tmp;
				}
			}
		}
		#endregion

		#region mark CCEventDispatcher - Dispatch
		public void dispatchEvent(CCEventObject e)
		{
			lock(this)
			{
				NSEvent evt = e.evt;
				Action<NSEvent> selector = e.selector;
				
				// Dispatch events
				if( _dispatchEvents ) {
					_locked = true;
					selector(evt);
					_locked = false;
				}

				
				// FIRST: Remove possible delegates
				utNode<tListDeletedEntry> dEntry, tTmp;
				dEntry = _delegatesToBeRemoved.head;
				while(dEntry != null){
					tTmp = dEntry.next;
					removeDelegate(dEntry.obj.aDelegate, dEntry.obj.listToBeDeleted);
					_delegatesToBeRemoved.DL_DELETE(dEntry);
					dEntry = tTmp;
				}

				
				// LATER: Add possible delegates
				utNode<tListAddedEntry> entry, tmp;
				entry = _delegatesToBeAdded.head;
				while(entry != null){
					tmp = entry.next;
					
					addDelegate(entry.obj.aDelegate, entry.obj.priority, entry.obj.listToBeAdded);
					_delegatesToBeAdded.DL_DELETE(entry);

					entry = tmp;
				}
			}
		}
		#endregion
		/*
		// Touches same with touches on mobile
		- (void)touchesBeganWithEvent:(NSEvent *)event;
		- (void)touchesMovedWithEvent:(NSEvent *)event;
		- (void)touchesEndedWithEvent:(NSEvent *)event;
		- (void)touchesCancelledWithEvent:(NSEvent *)event;
		
		// Gestures is not support yet
		- (void)beginGestureWithEvent:(NSEvent *)event;
		- (void)magnifyWithEvent:(NSEvent *)event;
		- (void)smartMagnifyWithEvent:(NSEvent *)event;
		- (void)rotateWithEvent:(NSEvent *)event;
		- (void)swipeWithEvent:(NSEvent *)event;
		- (void)endGestureWithEvent:(NSEvent *)event;
		*/
	}
}
#endif

