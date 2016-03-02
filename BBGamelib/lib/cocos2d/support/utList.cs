using UnityEngine;
using System.Collections;

namespace BBGamelib{
	public class utNode<T>{
		public utNode<T>	prev, next;
		public T obj;
	}
	public class utList<T>
	{
		utNode<T> _head;
		public utNode<T> head{get{return _head;}}
		public void DL_APPEND(utNode<T> add)
		{
			if (_head != null) {
				add.prev = _head.prev;
				_head.prev.next = add;
				_head.prev = add;
				add.next = null; 
			} else {
				_head = add;
				_head.prev = _head;
				_head.next = null;
			}
		}
		public void DL_PREPEND(utNode<T> add){
			(add).next = _head;                                                                           
			if (_head != null) {                                                                                   
				(add).prev = (_head).prev;                                                                 
				(_head).prev = (add);                                                                       
			} else {                                                                                      
				(add).prev = (add);                                                                        
			} 
			(_head) = (add);   
		}
		public void DL_DELETE(utNode<T> del){
			NSUtils.Assert((del).prev != null, "del.prev should not be null.");                                                                 
			if ((del).prev == (del)) {                                                                  
				(_head)=null;                                                                             
			} else if ((del)==(_head)) {                                                                  
				(del).next.prev = (del).prev;                                                         
				(_head) = (del).next;                                                                    
			} else {                                                                                     
				(del).prev.next = (del).next;                                                         
				if ((del).next != null) {                                                                       
					(del).next.prev = (del).prev;                                                     
				} else {                                                                                 
					(_head).prev = (del).prev;                                                          
				}                                                                                        
			}  
		}

	}
}

