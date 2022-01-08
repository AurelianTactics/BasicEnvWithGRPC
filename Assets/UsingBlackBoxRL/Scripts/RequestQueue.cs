using System;
using System.Collections;
using System.Collections.Generic;

namespace AurelianTactics.BlackBoxRL
{
	/// <summary>
	/// Handles information from the communication layer
	/// still unknown how to implement it until I get a better sense of what info is going in and how this information is being served to WorldTimeManager
	/// </summary>

	//for now jsut a simple linked list can add to end or pop from beginning

	//to do
	//have it work with dm_env_rpc
	//generalize the RequestQueueObject better for configuring stuff and what not

	//once I know more from teh communications layer, expand RQO
	//probably need to implement some sort of locking on this
	//not sure whether i want to push from here to WorldTimeManager or pull from WorldTimeManager
	//do I want to do some sort of check so that dm_env_rpc (or whatever teh outer thing) is synced with this?

	public class RequestQueue
	{

		LinkedList<RequestQueueObject> rqoLinkedList; 

		public RequestQueue()
		{
			this.rqoLinkedList = new LinkedList<RequestQueueObject>();
		}

		//then just 

		public RequestQueueObject GetNextRequest()
		{
			if (this.rqoLinkedList.Count > 0)
			{
				var rqo = this.rqoLinkedList.First.Value;
				this.rqoLinkedList.RemoveFirst();
				return rqo;
			}
			return null;	
			
		}

		public void AddRequestQueueObject(string message, int actionInt)
		{
			var rqo = new RequestQueueObject(message, actionInt);
			this.rqoLinkedList.AddLast(rqo);
		}

		public bool IsRequestInQueue()
		{
			if (this.rqoLinkedList.Count > 0)
				return true;
			return false;
		}
	}

	
	public class RequestQueueObject
	{
		public string message;
		public int actionInt;

		public RequestQueueObject(string msg, int action)
		{
			this.message = msg;
			this.actionInt = action;
		}


	}

}

