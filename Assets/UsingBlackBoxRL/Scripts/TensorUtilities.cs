using System.Collections;
using System.Collections.Generic;
using DmEnvRpc.V1;
using UnityEngine;

namespace AurelianTactics.BlackBoxRL
{
	/// <summary>
	/// dm_env_rpc sends custom Tensor objects between server and client
	/// Utilities to help manage the packing and unpacking of Tensor types from/to local types
	/// </summary>

    //to do
    //error checking and dictionary checking
    //env specific keys checking
    //abstract functionality: start with base class then go into specifics
    //read the type from the tensor so it returns the proper type
    //python version: https://github.com/deepmind/dm_env_rpc/blob/master/dm_env_rpc/v1/tensor_utils.py#L85
    //python versioning probably had a bunch of good ideas on things to implement
    //error handling for if dictionary doesn't contain anything

    public static class TensorUtilities {
        
        // environment sets some keys
        const int UID_ACTIONS = 2000;
        const int UID_OBSERVATIONS = 2001;
        const int UID_REQUESTED_OBSERVATIONS = 2002;

		/// <summary>
		/// Send in request and return rqo to RequestQueue
		/// </summary>
		/// <param name="erObject"></param>
		/// <returns></returns>
		public static RequestQueueObject CreateRequestQueueObject(EnvironmentRequest erObject)
		{
			var rqo = new RequestQueueObject();
			rqo.requestType = (int)erObject.PayloadCase;

			if (requestType == DmEnvRpc.V1.EnvironmentRequest.StepFieldNumber)
			{
				//to do error handling
				//Debug.Log("TEST: UnpackRequestTensor Step");
				StepRequest rtObject = (StepRequest)erObject.Step;
				if (rtObject.Actions.ContainsKey(UID_ACTIONS))
				{
					//Debug.Log("TEST: UnpackRequestTensor Step contains actions key");
					//Debug.Log(rtObject.Actions[UID_ACTIONS]);
					//To do: action array might be handled differently if multi dimension array
					unpackedDict["actions"] = new List<int>();
					foreach (var item in rtObject.Actions[UID_ACTIONS].Int32S.Array)
					{

						//Debug.Log("iterating through actions array");
						//Debug.Log(item);
						unpackedDict["actions"].Add((int)item);
						//Debug.Log("end iterating through actions array");
					}
				}

			}
			else if (requestType == DmEnvRpc.V1.EnvironmentRequest.ResetFieldNumber)
			{
				rqo.rqSettings = UnpackRequestSettingsTensor(rqo, erObject);
			}
			else if (requestType == DmEnvRpc.V1.EnvironmentRequest.ResetWorldFieldNumber)
			{
				if(erObject?.ResetWorld?.world_name == null)
				{
					rqo.requestWorldName = null;
				}
				else
				{
					rqo.requestWorldName = erObject.ResetWorld.world_name;
				}
				rqo.rqSettings = UnpackRequestSettingsTensor(rqo, erObject);
			}
			else if (requestType == DmEnvRpc.V1.EnvironmentRequest.JoinWorldFieldNumber)
			{
				if (erObject?.ResetWorld?.world_name == null)
				{
					rqo.requestWorldName = null;
				}
				else
				{
					rqo.requestWorldName = erObject.ResetWorld.world_name;
				}
				rqo.rqSettings = UnpackRequestSettingsTensor(rqo, erObject);

			}
			else if (requestType == DmEnvRpc.V1.EnvironmentRequest.LeaveWorldFieldNumber)
			{
				// doesn't send in an request or have a response
			}
			else if (requestType == DmEnvRpc.V1.EnvironmentRequest.CreateWorldFieldNumber)
			{
				rqo.rqSettings = UnpackRequestSettingsTensor(rqo, erObject);
				//map<string, Tensor> settings = 1;
				//var rtObject = (CreateWorldRequest)erObject.CreateWorld;

				//if (rtObject.Settings.ContainsKey("start_position"))
				//{
				//	unpackedDict["start_position"] = new List<int>();
				//	foreach (var item in rtObject.Settings["start_position"].Int64S.Array)
				//	{
				//		unpackedDict["start_position"].Add((int)item);
				//	}
				//}
				// else {
				//     Debug.Log("ERROR: CreateWorldRequest doesn't have start position")
				// }
			}
			else if (requestType == DmEnvRpc.V1.EnvironmentRequest.DestroyWorldFieldNumber)
			{
				if (erObject?.ResetWorld?.world_name == null)
				{
					rqo.requestWorldName = null;
				}
				else
				{
					rqo.requestWorldName = erObject.ResetWorld.world_name;
				}
			}


			return rqo;
		}

   //     public static Dictionary<string,List<int>> UnpackRequestTensor(EnvironmentRequest erObject){
   //         Dictionary<string,List<int>> unpackedDict = new Dictionary<string,List<int>>();
   //         int requestType = (int)erObject.PayloadCase;

   //         if( requestType == DmEnvRpc.V1.EnvironmentRequest.StepFieldNumber){
   //             //to do error handling
   //             //Debug.Log("TEST: UnpackRequestTensor Step");
			//	StepRequest rtObject = (StepRequest) erObject.Step;
   //             if( rtObject.Actions.ContainsKey(UID_ACTIONS)){
   //                 //Debug.Log("TEST: UnpackRequestTensor Step contains actions key");
   //                 //Debug.Log(rtObject.Actions[UID_ACTIONS]);
   //                 //To do: action array might be handled differently if multi dimension array
   //                 unpackedDict["actions"] = new List<int>();
   //                 foreach( var item in rtObject.Actions[UID_ACTIONS].Int32S.Array){
                        
   //                     //Debug.Log("iterating through actions array");
   //                     //Debug.Log(item);
   //                     unpackedDict["actions"].Add((int) item);
   //                     //Debug.Log("end iterating through actions array");
   //                 }
   //             }
                
			//}
			//else if( requestType == DmEnvRpc.V1.EnvironmentRequest.ResetFieldNumber){
				
   //         	Debug.Log("To Do: missing part of the response");
				
			//}
			//else if( requestType == DmEnvRpc.V1.EnvironmentRequest.ResetWorldFieldNumber){
				
   //         	Debug.Log("To Do: missing part of the response");
				
			//}
			//else if( requestType == DmEnvRpc.V1.EnvironmentRequest.JoinWorldFieldNumber){
				
   //         	Debug.Log("To Do: missing part of the response");
				
			//}
			//else if( requestType == DmEnvRpc.V1.EnvironmentRequest.LeaveWorldFieldNumber){
				
   //         	Debug.Log("To Do: missing part of the response");
				
			//}
			//else if( requestType == DmEnvRpc.V1.EnvironmentRequest.CreateWorldFieldNumber){
			//	//map<string, Tensor> settings = 1;
   //             var rtObject = (CreateWorldRequest) erObject.CreateWorld;

   //             if( rtObject.Settings.ContainsKey("start_position")){
   //                 unpackedDict["start_position"] = new List<int>();
   //                 foreach( var item in rtObject.Settings["start_position"].Int64S.Array){
   //                     unpackedDict["start_position"].Add((int) item);
   //                 }
   //             } 
   //             // else {
   //             //     Debug.Log("ERROR: CreateWorldRequest doesn't have start position")
   //             // }
			//}
			//else if( requestType == DmEnvRpc.V1.EnvironmentRequest.DestroyWorldFieldNumber){
				
   //         	Debug.Log("To Do: missing part of the response");

			//}

   //         return unpackedDict;
   //     }


		static Dictionary<string, List<int>> UnpackRequestSettingsTensor(RequestQueueObject rqo, EnvironmentRequest erObject)
		{

			int requestType = rqo.requestType;
			RequestQueue.RequestQueueSettings rqSettings = new RequestQueue.RequestQueueSettings();
			//bool isFloatDictCreated = false;
			//bool isIntDictCreated = false;
			//bool isStringDictCreated = false;

			if (requestType == DmEnvRpc.V1.EnvironmentRequest.ResetFieldNumber)
			{
				// to do
				// check if settings exist
				// for each key, check type and add to the proper rqSettings (or create and add)
				// error handling
				var rtObject = (ResetRequest)erObject.Reset;
				foreach (var item in rtObject.Settings)
				{
					rqSettings = AddToRequestQueueSettings(item, rqSettings);
				}

				//map<string, Tensor> settings = 1;
				//var rtObject = (CreateWorldRequest)erObject.CreateWorld;

					//if (rtObject.Settings.ContainsKey("start_position"))
					//{
					//	unpackedDict["start_position"] = new List<int>();
					//	foreach (var item in rtObject.Settings["start_position"].Int64S.Array)
					//	{
					//		unpackedDict["start_position"].Add((int)item);
					//	}
					//}
					//// else {
					////     Debug.Log("ERROR: CreateWorldRequest doesn't have start position")
					//// }
			}
			else if(requestType == DmEnvRpc.V1.EnvironmentRequest.ResetWorldFieldNumber)
			{

			}
			else if (requestType == DmEnvRpc.V1.EnvironmentRequest.JoinWorldFieldNumber)
			{

			}
			else if (requestType == DmEnvRpc.V1.EnvironmentRequest.CreateWorldFieldNumber)
			{
				var rtObject = (CrewateWorldRequest)erObject.Create;
				foreach (var item in rtObject.Settings)
				{
					rqSettings = AddToRequestQueueSettings(item, rqSettings);
				}
			}

			return rqSettings;
		

		}

		RequestQueueSettings AddToRequestQueSettings(Tensor item, RequestQueueSettings rqs)
		{
			//to do: 
			//figure out the argument input type
			//figure out how to get the name of the dictionary field from the item and the type
			//figure out how to access the tensortype
			//do all the casting
			//test this

			// tensor types can be 
			// "Floats", "Doubles", "Int8S", "Int32S", "Int64S", "Uint8S", "Uint32S", "Uint64S", "Bools", "Strings", "Protos", "Shape"
			var itemType = item.Type;
			var itemName = item.Name;
			if( itemType == .Floats )
			{
				rqs.settingsDictFloat[itemName] = new List<float>();
				foreach( var a in rtObject.Settings[itemName].Floats.Array)
				{
					rqs.settingsDictFloat[itemName].Add((float)a);
				}
			}
			else if (itemType == .Doubles)
			{
				rqs.settingsDictFloat[itemName] = new List<float>();
				foreach (var a in rtObject.Settings[itemName].Doubles.Array)
				{
					rqs.settingsDictFloat[itemName].Add((float)a);
				}
			}
			else if (itemType == .Int64S)
			{
				rqs.settingsDictFloat[itemName] = new List<int>();
				foreach (var a in rtObject.Settings[itemName].Int64S.Array)
				{
					rqs.settingsDictFloat[itemName].Add((int)a);
				}
			}
			//if this works then group all ints and Bools in the int one. put strings in its own thing, error handling for Protos and Shape


			return rqs;
		}


		// never implemented. would be for create, destroy, join, resetworld requests but not sure how to null check abstractly in a function
		//instead just duplicated some junks of code
		//public static string UnpackWorldName(EnvironmentRequest erObject)
		//{
		//	int requestType = (int)erObject.PayloadCase;
		//	if (requestType == DmEnvRpc.V1.EnvironmentRequest.JoinWorldFieldNumber)
		//	{

		//	}
		//	else if (requestType == DmEnvRpc.V1.EnvironmentRequest.CreateWorldFieldNumber)
		//	{
		//		var rtObject = (CrewateWorldRequest)erObject.Create;
		//		foreach (var item in rtObject.Settings)
		//		{
		//			rqSettings = AddToRequestQueueSettings(item, rqSettings);
		//		}
		//	}
		//	return "";
		//}

	}
}
