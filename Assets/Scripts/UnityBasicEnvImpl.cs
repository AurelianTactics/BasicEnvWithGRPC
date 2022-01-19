using UnityEngine;
using System.Threading.Tasks;
using System;
using Grpc.Core;
using Unitybasicenv;
using System.Collections.Generic;
using AurelianTactics.BlackBoxRL;

//to do:
//my server response is fucking garbage. I don't really get how to do this

public class UnityBasicEnvImpl : UnityBasicEnv.UnityBasicEnvBase
{
	readonly object myLock = new object();
	public RequestQueue requestQueue;
	List<UnityBasicEnvNote> responseList;

	public AgentSession agentSession;

	public UnityBasicEnvImpl()
	{
		//this.requestQueue = new RequestQueue();
		//responseList = new List<UnityBasicEnvNote>();
	}

	public void AssignAgentSession(AgentSession aSession)
	{
		Debug.Log("assigned agent session");
		this.agentSession = aSession;
		//Debug.Log("testing agent session");
		//this.agentSession.GetNoteResponse();
		//await this.agentSession.TestPrint(" init ");
	}

	public void AssignAgentSessionAndWTM(AgentSession aSession, WorldTimeManager wtm)
	{
		Debug.Log("assigned agent session");
		this.agentSession = aSession;
		this.requestQueue = wtm.requestQueue;
		//Debug.Log("testing agent session");
		//this.agentSession.GetNoteResponse();
		//await this.agentSession.TestPrint(" init ");

		//this.agentSession.TestAvatarPrint("testing in assign");
	}

	public void AddToResponseList(string msg, int obs, float rew, bool done)
	{
		Debug.Log("adding to response list");
		responseList.Add(MakeUnityBasicEnvNote(msg, obs, rew, done));
	}

	//public override Task UnityBasicEnvChat(IAsyncStreamReader<UnityBasicEnvNote> requestStream, 
	//	IServerStreamWriter<UnityBasicEnvNote> responseStream, ServerCallContext context)
	//{
	//	return base.UnityBasicEnvChat(requestStream, responseStream, context);
	//}

	/// <summary>
	/// Receives a stream of message/location pairs, and responds with a stream of all previous
	/// messages at each of those locations.
	/// </summary>
	
	//me: this kind of works. can send and receive but only in direct response to each other
	//this might actually work but would require the call to be tightly coupled
	public override async Task UnityBasicEnvChat(IAsyncStreamReader<UnityBasicEnvNote> requestStream, IServerStreamWriter<UnityBasicEnvNote> responseStream, 
		ServerCallContext context)
	{
		

		//Debug.Log("handling incoming chat before while");
		while (await requestStream.MoveNext())
		{
			var note = requestStream.Current;
			Debug.Log("TEST handling incoming chat in while loop, up next is the note");
			Debug.Log(note);

			//await this.agentSession.TestPrint(" in request stream ");

			//this example works. sends shit back to client which reacts
			//var new_note = MakeUnityBasicEnvNote("step", 1, 0.23f, false);
			//Debug.Log("new note is ");
			//Debug.Log(new_note);
			//await responseStream.WriteAsync(new_note);

			//await this.agentSession.TestMono("after note sent back");

			//await this.agentSession.TestPrint(" pre request stream");
			//this works example works now trying outside class
			//TestNoteClass tnc = new TestNoteClass();
			//var new_note = tnc.MakeUnityBasicEnvNote("step", 2, 0.3123f, true);
			//Debug.Log("new note is ");
			//await responseStream.WriteAsync(new_note);

			//now trying with agentsession class
			//await this.agentSession.TestPrint("testing inside request stream");
			//var new_note = MakeUnityBasicEnvNote("step", 10, 1.23f, false);
			//Debug.Log("new note is ");
			//Debug.Log(new_note);
			//await responseStream.WriteAsync(new_note);



			//this is the working method
			// Debug.Log("TEST writing note to request queue");
			// this.requestQueue.AddRequestQueueObject(note.Message, note.ActionObs);
			//now getting response back from monobehavior
			// Debug.Log("TEST waiting for new note from agentSession");
			// var noteList = await this.agentSession.GetNoteResponse();
			// foreach (UnityBasicEnvNote ubeNote in noteList)
			// {
			// 	Debug.Log("TEST new note write is ");
			// 	Debug.Log(ubeNote);
			// 	await responseStream.WriteAsync(ubeNote);
			// }

			//await this.agentSession.GetNoteResponse();


			// trying shitty while loop way, I doubt this works
			//doesn't work
			//Debug.Log("shitty while loop start");
			//while (this.responseList.Count <= 0)
			//{
			//	//Debug.Log("TEST Stuck in while loop inside handling request, waiting for client to produce something to send back ");
			//}
			//Debug.Log("shitty while loop end");
			//Debug.Log("TEST finished while loop, response list count is " + this.responseList.Count);
			//foreach (UnityBasicEnvNote chat in this.responseList)
			//{
			//	Debug.Log("TEST attempting sending note is ");
			//	Debug.Log(chat);
			//	await responseStream.WriteAsync(chat);
			//	Debug.Log("TEST after attempting sending note is ");
			//}
			//this.responseList.Clear();


		}

		//untested, i dont' think this should work since no listener
		//if( this.responseList.Count > 0)
		//{
		//	var send_note = this.responseList[0];
		//	this.responseList.RemoveAt(0);
		//	Debug.Log("testing sending note, send note is ");
		//	Debug.Log(send_note);
		//	await responseStream.WriteAsync(send_note);
		//}
	}

	////i don't really think this is the proper way to use the bidirectional streamer
	//public async Task UnityBasicEnvChat(string msg, int act, float rew, bool done, 
	//	IAsyncStreamReader<UnityBasicEnvNote> requestStream, IServerStreamWriter<UnityBasicEnvNote> responseStream, ServerCallContext context)
	//{
	//	Debug.Log("testing writing a new note");
	//	var new_note = MakeUnityBasicEnvNote(msg, act, rew, done);
	//	Debug.Log("new note is ");
	//	Debug.Log(new_note);
	//	await responseStream.WriteAsync(new_note);
	//}

	//UnityBasicEnvNote HandleAction(int obs)
	//{
	//	//to do: env based code:
	//	obs = UnityEngine.Random.Range(0, 20);
	//	bool done = false;
	//	float reward = -0.1f;
	//	if (obs >= 15)
	//	{
	//		done = true;
	//		reward = 1.0f;
	//	}
	//	return MakeUnityBasicEnvNote("step", obs, reward, done);
	//}

	UnityBasicEnvNote MakeUnityBasicEnvNote(string message, int actionObs, float reward, bool done)
	{
		Debug.Log("making new unitybasicenv note");
		return new UnityBasicEnvNote
		{
			Message = message,
			ActionObs = actionObs,
			Reward = reward,
			Done = done
		};

		//return new RouteSummary
		//{
		//	PointCount = pointCount,
		//	FeatureCount = featureCount,
		//	Distance = distance,
		//	ElapsedTime = (int)(stopwatch.ElapsedMilliseconds / 1000)
		//};
	}


	//def handle_action(self, action):
	//	if action == 1:
	//	  obs = random.randint(10, 20)
	//	else:
	//	  obs = random.randint(0, 20)

	//	if obs >= 15:
	//	  done = True
	//	  reward = 1.0
	//	else:

	//	  done = False
	//	  reward = -0.1

	//	self.is_done = done
	//	print("received action, sending back step info")
	//	return make_unity_basic_env_note("step", obs, reward, done)

	///route guide example
	//public override async Task RouteChat(IAsyncStreamReader<RouteNote> requestStream, IServerStreamWriter<RouteNote> responseStream, ServerCallContext context)
	//{
	//	while (await requestStream.MoveNext())
	//	{
	//		var note = requestStream.Current;
	//		List<RouteNote> prevNotes = AddNoteForLocation(note.Location, note);
	//		foreach (var prevNote in prevNotes)
	//		{
	//			await responseStream.WriteAsync(prevNote);
	//		}
	//	}
	//}

	//helloworld example
	//class GreeterImpl : Greeter.GreeterBase
	//{
	//  // Server side handler of the SayHello RPC
	//  public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
	//  {
	//    return Task.FromResult(new HelloReply { Message = "Hello " + request.Name });
	//  }
	//}
}

class TestNoteClass
{
	public UnityBasicEnvNote MakeUnityBasicEnvNote(string message, int actionObs, float reward, bool done)
	{
		Debug.Log("making new unitybasicenv note");
		return new UnityBasicEnvNote
		{
			Message = message,
			ActionObs = actionObs,
			Reward = reward,
			Done = done
		};

		//return new RouteSummary
		//{
		//	PointCount = pointCount,
		//	FeatureCount = featureCount,
		//	Distance = distance,
		//	ElapsedTime = (int)(stopwatch.ElapsedMilliseconds / 1000)
		//};
	}

}

