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

        public static Dictionary<string,List<int>> UnpackRequestTensor(EnvironmentRequest erObject){
            Dictionary<string,List<int>> unpackedDict = new Dictionary<string,List<int>>();
            int requestType = (int)erObject.PayloadCase;

            if( requestType == DmEnvRpc.V1.EnvironmentRequest.StepFieldNumber){
                //to do error handling
                //Debug.Log("TEST: UnpackRequestTensor Step");
				StepRequest rtObject = (StepRequest) erObject.Step;
                if( rtObject.Actions.ContainsKey(UID_ACTIONS)){
                    //Debug.Log("TEST: UnpackRequestTensor Step contains actions key");
                    //Debug.Log(rtObject.Actions[UID_ACTIONS]);
                    //To do: action array might be handled differently if multi dimension array
                    unpackedDict["actions"] = new List<int>();
                    foreach( var item in rtObject.Actions[UID_ACTIONS].Int32S.Array){
                        
                        //Debug.Log("iterating through actions array");
                        //Debug.Log(item);
                        unpackedDict["actions"].Add((int) item);
                        //Debug.Log("end iterating through actions array");
                    }
                }
                
			}
			else if( requestType == DmEnvRpc.V1.EnvironmentRequest.ResetFieldNumber){
				
            	Debug.Log("To Do: missing part of the response");
				
			}
			else if( requestType == DmEnvRpc.V1.EnvironmentRequest.ResetWorldFieldNumber){
				
            	Debug.Log("To Do: missing part of the response");
				
			}
			else if( requestType == DmEnvRpc.V1.EnvironmentRequest.JoinWorldFieldNumber){
				
            	Debug.Log("To Do: missing part of the response");
				
			}
			else if( requestType == DmEnvRpc.V1.EnvironmentRequest.LeaveWorldFieldNumber){
				
            	Debug.Log("To Do: missing part of the response");
				
			}
			else if( requestType == DmEnvRpc.V1.EnvironmentRequest.CreateWorldFieldNumber){
				//map<string, Tensor> settings = 1;
                var rtObject = (CreateWorldRequest) erObject.CreateWorld;

                if( rtObject.Settings.ContainsKey("start_position")){
                    unpackedDict["start_position"] = new List<int>();
                    foreach( var item in rtObject.Settings["start_position"].Int64S.Array){
                        unpackedDict["start_position"].Add((int) item);
                    }
                } 
                // else {
                //     Debug.Log("ERROR: CreateWorldRequest doesn't have start position")
                // }
			}
			else if( requestType == DmEnvRpc.V1.EnvironmentRequest.DestroyWorldFieldNumber){
				
            	Debug.Log("To Do: missing part of the response");

			}

            return unpackedDict;
        }

    }
}
