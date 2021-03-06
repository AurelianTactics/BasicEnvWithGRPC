using System;
using System.Collections;
using System.Collections.Generic;
using DmEnvRpc.V1;

namespace AurelianTactics.BlackBoxRL
{
	/// <summary>
	/// Handles information from the communication layer
	/// Currently takes request info from grpc server and packages it a bit before agentsession grabs it from queue
	/// </summary>


	//to do
	//think if this is really designed properly
	//synchronization: locking etc
	//how to work better with the WTM
	//generalize the RequestQueueObject better for configuring stuff and what not
	//once I know more from teh communications layer, expand RQO
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

