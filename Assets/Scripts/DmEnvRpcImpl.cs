using UnityEngine;
using System.Threading.Tasks;
using System;
using Grpc.Core;
using DmEnvRpc.V1;
using System.Collections.Generic;
using AurelianTactics.BlackBoxRL;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Implements the GRPC functionality the compiled proto files in DmEnvRPC and DmEnvRPCGrpc
/// </summary>

/*
To do:
be able to handle multiple agent connects
ie implement synchronization code like locking etc
The balance between Session, WorldTimeManager, and session factory described in the paper is not really ideal
I'm not quite sure I'm handling the grpc request response better
some things are hard coded in that whoudl be dynamic configurations like the UID and WORLD_NAME

*/

public class DmEnvRpcImpl : DmEnvRpc.V1.Environment.EnvironmentBase
{
    readonly object myLock = new object();
	public RequestQueue requestQueue;

	public AgentSession agentSession;
    const string WORLD_NAME = "basic_env_world";

    const int UID_ACTIONS = 2000;
    const int UID_OBSERVATIONS = 2001;
    const int UID_REQUESTED_OBSERVATIONS = 2002;

	public DmEnvRpcImpl()
	{

	}

	public void AssignAgentSession(AgentSession aSession)
	{
		//Debug.Log("assigned agent session");
		this.agentSession = aSession;
	}

	public void AssignAgentSessionAndWTM(AgentSession aSession, WorldTimeManager wtm)
	{
		//Debug.Log("assigned agent session");
		this.agentSession = aSession;
		this.requestQueue = wtm.requestQueue;
	}
    

      /// <summary>
      /// Process incoming environment requests.
      /// </summary>
      /// <param name="requestStream">Used for reading requests from the client.</param>
      /// <param name="responseStream">Used for sending responses back to the client.</param>
      /// <param name="context">The context of the server-side call handler being invoked.</param>
      /// <returns>A task indicating completion of the handler.</returns>
	public override async Task Process(IAsyncStreamReader<DmEnvRpc.V1.EnvironmentRequest> requestStream, IServerStreamWriter<DmEnvRpc.V1.EnvironmentResponse> responseStream, 
		ServerCallContext context)
	{
		//Debug.Log("handling incoming chat before while");
		while (await requestStream.MoveNext())
		{
			EnvironmentRequest envRequest = requestStream.Current;
			//Debug.Log("TEST handling incoming request in while loop, up next is the request");
			//Debug.Log(envRequest); //{ "createWorld": { "settings": { "seed": { "int64s": { "array": [ "1234" ] } } } } }

            //working code
            //Debug.Log("TEST writing envRequest to request queue");
			this.requestQueue.AddRequestQueueObject(envRequest);
            //now testing with agentsession class and as class does some monobehavior
			//Debug.Log("TEST waiting for envRespose from agentSession");
            var envResponseList = await this.agentSession.HandleEnvironmentRequest();
			foreach (DmEnvRpc.V1.EnvironmentResponse ero in envResponseList)
			{
				//Debug.Log("TEST new envRespose write is ");
				//Debug.Log(ero);
				await responseStream.WriteAsync(ero);
			}
		}
	}
}

/*
scrap code for testing grpc requests, responses, and handling unpacking tensors


            //  THIS WORKS testing step
            // StepRequest rt = (StepRequest) envRequest.Step;
            // Debug.Log(rt.Actions);
            // Debug.Log(rt.Actions[UID_ACTIONS]);
            // Debug.Log(rt.Actions[UID_ACTIONS].Int64S.Array);
            // foreach( var item in rt.Actions[UID_ACTIONS].Int64S.Array){
            //     Debug.Log(item);
            // }
            // var erObject = new EnvironmentResponse();
            // DmEnvRpc.V1.StepResponse sr = new StepResponse();
            // sr.State = EnvironmentStateType.Running;
            // erObject.Step = sr;
            // await responseStream.WriteAsync(erObject);
            // Debug.Log("=== ENDING STEP TEST ===");


            // this works for createWOrldRequest
            // Debug.Log("before making new CreateWorldResponseObject");
            // //python example: 
            // //https://github.com/deepmind/dm_env_rpc/blob/master/examples/catch_environment.py
            // //environment_response = dm_env_rpc_pb2.EnvironmentResponse()
            // //response = dm_env_rpc_pb2.CreateWorldResponse(world_name=_WORLD_NAME)
            // //getattr(environment_response, message_type).CopyFrom(response)
            // var erObject = new EnvironmentResponse();
            // DmEnvRpc.V1.CreateWorldResponse cwr = new CreateWorldResponse();
            // cwr.WorldName = WORLD_NAME;
            // erObject.CreateWorld = cwr;
            // //me: can only return an environmentresponse but not sure how to convert it
            
            // await responseStream.WriteAsync(erObject);
            // Debug.Log("after making new CreateWorldResponseObject");



            //this works for CreateWorldRequest
            // Debug.Log(envRequest.CreateWorld); //{ "settings": { "seed": { "int64s": { "array": [ "1234" ] } } } }
            // Debug.Log(envRequest.PayloadCase); //CreateWorld
            // Debug.Log(envRequest.ToString());

            // var cwrTest = (CreateWorldRequest) envRequest.CreateWorld;
            // Debug.Log(cwrTest.Settings);
            // Debug.Log(cwrTest.Settings["seed"]);
            // Debug.Log(cwrTest.Settings["seed"]);
            // Tensor seedResult = cwrTest.Settings["seed"];
            // //actually decent way to unpack
            // foreach( var item in seedResult.Int64S.Array){
            //     Debug.Log(item);
            // }



            //bunch of nonsense before I found a way to unpack
            // Debug.Log("seeing methods for EnvironmentRequest");
            // foreach (var item in typeof(EnvironmentRequest).GetMethods())
            // {
            //         Debug.Log(item.Name);
            // } 
            // Debug.Log("seeing methods for CreateWorldRequest");
            // foreach (var item in typeof(CreateWorldRequest).GetMethods())
            // {
            //         Debug.Log(item.Name);
            // } 
            // Debug.Log("seeing methods for seedResult");
            // foreach (var item in typeof(Tensor).GetMethods())
            // {
            //         Debug.Log(item.Name);
            // } 
            // Debug.Log("seeing methods for Int64S");
            // foreach (var item in typeof(Tensor.Types.Int64Array).GetMethods())
            // {
            //         Debug.Log(item.Name);
            // } 
            // Debug.Log("seeing methods for Int64S.Array");
            // foreach (var item in typeof(Tensor.Types.Int64Array).GetMethods())
            // {
            //         Debug.Log(item.Name);
            // } 
            // Debug.Log(seedResult.PayloadCase); //Int64S
            //Tensor.Types.Uint64Array.
            //Debug.Log(seedResult.Int64S.Array); //[ "1234" ]
            //Debug.Log(seedResult.Int64S.Array.ToString()); //[ "1234" ]
            //Debug.Log(seedResult.Int64S.Array.ToString()[0]);
            //var testInt = Int32.Parse(seedResult.Int64S.Array.ToString());
            //Debug.Log("" + testInt);
            //string result = Regex.Replace(subject, "[^0-9]", "");

            //FUCKING HELL took way to long to get to this fucking result
            // string testRegex = Regex.Replace(seedResult.Int64S.Array.ToString(), "[^0-9]", "");
            // Debug.Log(testRegex);
            // var testInt2 = Int32.Parse(testRegex);
            // Debug.Log("" + testInt2);

            //var stringResult = new String(seedResult.Int64S.Array.ToString().Where(c => char.IsDigit(c)).ToArray());
            //new String(phone.Where(c => char.IsDigit(c)).ToArray())
            //var newVar = unchecked((int) seedResult.Int64S.Array);
            //Debug.Log(seedResult.Int64S.array);
            //Debug.Log(seedResult.Int64S.ToString());
            //Debug.Log(seedResult.Int64S); //{ "array": [ "1234" ] }
            
            //Debug.Log("=== END TESTING === ");



			//this example works. sends shit back to client which reacts
			//var new_note = MakeUnityBasicEnvNote("step", 1, 0.23f, false);
			//Debug.Log("new note is ");
			//Debug.Log(new_note);
			//await responseStream.WriteAsync(new_note);

			//await this.agentSession.TestMono("after note sent back");
*/
