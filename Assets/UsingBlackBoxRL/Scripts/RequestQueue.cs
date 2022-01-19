using System;
using System.Collections;
using System.Collections.Generic;
using DmEnvRpc.V1;

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

		// public void AddRequestQueueObject(string message, int actionInt)
		// {
		// 	var rqo = new RequestQueueObject(message, actionInt);
		// 	this.rqoLinkedList.AddLast(rqo);
		// }

		public void AddRequestQueueObject(EnvironmentRequest ero)
		{
			var rqo = new RequestQueueObject(ero);
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
		public int requestType;
		public Dictionary<string,List<int>> createWorldSettings;

		public Dictionary<string, List<int>> unpackedTensorDict;

		public RequestQueueObject(EnvironmentRequest ero){
			this.requestType = (int)ero.PayloadCase;

			unpackedTensorDict = TensorUtilities.UnpackRequestTensor(ero);
			
		}


		// old way
		// public string message;
		// public int actionInt;

		// public RequestQueueObject(string msg, int action)
		// {
		// 	this.message = msg;
		// 	this.actionInt = action;
		// }


	}

}



    // CreateWorldRequest create_world = 1;
    // JoinWorldRequest join_world = 2;
    // StepRequest step = 3;
    // ResetRequest reset = 4;
    // ResetWorldRequest reset_world = 5;
    // LeaveWorldRequest leave_world = 6;
    // DestroyWorldRequest destroy_world = 7;

    // // If the environment supports a specialized request not covered above it
    // // can be sent this way.
    // //
    // // Slot 15 is the last slot which can be encoded with one byte.  See
    // // https://developers.google.com/protocol-buffers/docs/proto3#assigning-field-numbers
    // google.protobuf.Any extension = 15;

