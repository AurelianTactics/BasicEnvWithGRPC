using UnityEngine;
using System.Threading.Tasks;
using System;
using Grpc.Core;
using DmEnvRpc.V1;
using System.Collections.Generic;
using AurelianTactics.BlackBoxRL;
using System.Reflection;

/// <summary>
/// Launch grpc server from here
/// Functionality for server in  DmEnvRpcImpl, this file provides the Monobehavior
/// </summary>


//to do:
//how to handle server stopping
//dynamically choose ports
//parts from the paper are not implemented super cleanly: world time manager and session factory

public class BasicServerScript : MonoBehaviour
{
	[SerializeField] private int gRPCPort = 30051;
	WorldTimeManager worldTimeManager;
	AgentSession agentSession;
	//UnityBasicEnvImpl ubeImpl;
	DmEnvRpcImpl dmImpl;
    
    void Start()
    {
		worldTimeManager = new WorldTimeManager();
		agentSession = new AgentSession(worldTimeManager);
		dmImpl = new DmEnvRpcImpl();
		dmImpl.AssignAgentSessionAndWTM(agentSession, worldTimeManager);
		//Debug.Log("TEST BasicServerScript starting server");
		Server server = new Server
		{
			Services = { DmEnvRpc.V1.Environment.BindService(dmImpl) },
			//Ports = { new ServerPort("localhost", gRPCPort, ServerCredentials.Insecure) }
			Ports = { new ServerPort("0.0.0.0", gRPCPort, ServerCredentials.Insecure) }
		};
		server.Start();
		//Debug.Log("=== END TEST BasicServerScript after starting the server, server is next");

		
    }

}

/*
Scrap code for testing how to pack Tensor objects to send over grpc
This took way too long

	static void PrintMethods(System.Object o) {

		Type t = o.GetType();
		MethodInfo[] methods = t.GetMethods();
		foreach(MethodInfo method in methods) {

			Debug.Log(method.Name );
		}

	}

	// // seeing how to pack tensors
		// Debug.Log(" === START testing methods JoinWorldResponse ===");
		// DmEnvRpc.V1.JoinWorldResponse jwr = new DmEnvRpc.V1.JoinWorldResponse();
		// PrintMethods(jwr);
		// Debug.Log(" === START testing methods jwr.specs ===");
		// jwr.Specs = new ActionObservationSpecs();
		// PrintMethods(jwr.Specs);
		// //jwr.Specs.ActionObservationSpecs[jwr.Specs.ActionObservationSpecs.ActionsFieldNumber] = new TensorSpec();
		// Debug.Log(jwr.Specs);
		
		// Debug.Log(" === START testing methods jwr.specs.actions ===");
		// PrintMethods(jwr.Specs.Actions);
		// Debug.Log(" === START testing methods jwr.specs ===");
		// TensorSpec ts = new TensorSpec{
		// 	Name="agent_actions",
		// 	Dtype=DmEnvRpc.V1.DataType.Int64
		// };
		// jwr.Specs.Actions.Add(1, ts); 
		// Debug.Log(jwr.Specs.Actions);
		// // Tensor tt = new Tensor();
		// // //PrintMethods(tt);
		// // tt.Floats = new Tensor.Types.FloatArray();
		// // Debug.Log(" === START testing methods Floats ===");
		// // //PrintMethods(tt.Floats);
		// // Debug.Log(" === START testing methods Floats array ===");
		// // //is a get_Array but is no set array
		// // //PrintMethods(tt.Floats.Array);
		// // tt.Floats.Array.Add(0.5f);
		// // Debug.Log("Holy shit did i fucking finally do it?" );
		// // Debug.Log( tt.Floats.Array);
		// Debug.Log(" === END testing methods ===");

		//this might actually work? idk
		// var exampleTensorSpec = new TensorSpec{
		// 	Name="tensorSpecExample",
		// 	Shape={1,0},
		// 	Dtype=DmEnvRpc.V1.DataType.Int64,
		// };
		// TensorSpec testTS = new TensorSpec();;
		// testTS.Name = "asdf";

*/