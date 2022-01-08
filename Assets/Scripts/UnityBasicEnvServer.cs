#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using UnityEngine;
using System.Threading.Tasks;
using System;
using Grpc.Core;
using Unitybasicenv;
using System.Collections.Generic;
using AurelianTactics.BlackBoxRL;

class UnityBasicEnvServer
{
	// Can be run from commandline.
	// Example command:
	// "/Applications/Unity/Unity.app/Contents/MacOS/Unity -quit -batchmode -nographics -executeMethod HelloWorldTest.RunHelloWorld -logfile"
	// public static void RunUnityBasicEnv()
	// {
	//   Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

	//   Debug.Log("==============================================================");
	//   Debug.Log("Starting tests");
	//   Debug.Log("==============================================================");

	//   Debug.Log("Application.platform: " + Application.platform);
	//   Debug.Log("Environment.OSVersion: " + Environment.OSVersion);

	//   RunServer();
	//   Debug.Log("after running server");

	//   Debug.Log("==============================================================");
	//   Debug.Log("Tests finished successfully.");
	//   Debug.Log("==============================================================");
	// }

	//public static void RunServer()
	//{
	//	const int Port = 30051;

	//	Server server = new Server
	//	{
	//		Services = { UnityBasicEnv.BindService(new UnityBasicEnvImpl()) },
	//		Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
	//	};
	//	server.Start();


	//	//to do (maybe?) do I need code to keep the server running or explicitly shut it down?

	//	//Console.WriteLine("UnityBasicEnv server listening on port " + Port);
	//	//Console.WriteLine("Press any key to stop the server...");
	//	//Console.ReadKey();

	//	//server.ShutdownAsync().Wait();

	//	//route guide server
	//	//const int Port = 30052;

	//	//var features = RouteGuideUtil.LoadFeatures();

	//	//Server server = new Server
	//	//{
	//	//	Services = { RouteGuide.BindService(new RouteGuideImpl(features)) },
	//	//	Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
	//	//};
	//	//server.Start();

	//	//Console.WriteLine("RouteGuide server listening on port " + Port);
	//	//Console.WriteLine("Press any key to stop the server...");
	//	//Console.ReadKey();

	//	//server.ShutdownAsync().Wait();
	//}

	//public class UnityBasicEnvImpl : UnityBasicEnv.UnityBasicEnvBase
	//{
	//	readonly object myLock = new object();
	//	RequestQueue requestQueue;

	//	public UnityBasicEnvImpl (RequestQueue rq)
	//	{
	//		this.requestQueue = rq;
	//	}

	//	//public override Task UnityBasicEnvChat(IAsyncStreamReader<UnityBasicEnvNote> requestStream, 
	//	//	IServerStreamWriter<UnityBasicEnvNote> responseStream, ServerCallContext context)
	//	//{
	//	//	return base.UnityBasicEnvChat(requestStream, responseStream, context);
	//	//}

	//	/// <summary>
	//	/// Receives a stream of message/location pairs, and responds with a stream of all previous
	//	/// messages at each of those locations.
	//	/// </summary>
	//	public override async Task UnityBasicEnvChat(IAsyncStreamReader<UnityBasicEnvNote> requestStream, IServerStreamWriter<UnityBasicEnvNote> responseStream, ServerCallContext context)
	//	{
	//		//Debug.Log("handling incoming chat before while");
	//		while (await requestStream.MoveNext())
	//		{
	//			var note = requestStream.Current;
	//			Debug.Log("handling incoming chat in while loop, up next is the note");
	//			Debug.Log(note);
	//			//await responseStream.WriteAsync(note);
	//			var new_note = MakeUnityBasicEnvNote("step", 1, 0.23f, false);
	//			Debug.Log("new note is ");
	//			Debug.Log(new_note);
	//			await responseStream.WriteAsync(new_note);
	//			//await responseStream.WriteAsync(HandleAction(note.ActionObs));
	//		}
	//	}

	//	UnityBasicEnvNote HandleAction(int obs)
	//	{
	//		//to do: env based code:
	//		obs = UnityEngine.Random.Range(0, 20);
	//		bool done = false;
	//		float reward = -0.1f;
	//		if (obs >= 15)
	//		{
	//			done = true;
	//			reward = 1.0f;
	//		}
	//		return MakeUnityBasicEnvNote("step", obs, reward, done);
	//	}

	//	UnityBasicEnvNote MakeUnityBasicEnvNote(string message, int actionObs, float reward, bool done)
	//	{
	//		Debug.Log("making new unitybasicenv note");
	//		return new UnityBasicEnvNote
	//		{
	//			Message = message,
	//			ActionObs = actionObs,
	//			Reward = reward,
	//			Done = done
	//		};

	//		//return new RouteSummary
	//		//{
	//		//	PointCount = pointCount,
	//		//	FeatureCount = featureCount,
	//		//	Distance = distance,
	//		//	ElapsedTime = (int)(stopwatch.ElapsedMilliseconds / 1000)
	//		//};
	//	}


	//	//def handle_action(self, action):
	//	//	if action == 1:
	//	//	  obs = random.randint(10, 20)
	//	//	else:
	//	//	  obs = random.randint(0, 20)

	//	//	if obs >= 15:
	//	//	  done = True
	//	//	  reward = 1.0
	//	//	else:

	//	//	  done = False
	//	//	  reward = -0.1

	//	//	self.is_done = done
	//	//	print("received action, sending back step info")
	//	//	return make_unity_basic_env_note("step", obs, reward, done)

	//}

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
