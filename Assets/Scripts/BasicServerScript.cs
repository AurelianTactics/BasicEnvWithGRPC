using UnityEngine;
using System.Threading.Tasks;
using System;
using Grpc.Core;
using Unitybasicenv;
using System.Collections.Generic;
using AurelianTactics.BlackBoxRL;

/// <summary>
/// Launch server from here
/// Functionality in UnityBasicEnvServer, this file provides the Monobehavior
/// </summary>
/// 

//to do:
//how to handle server stopping

public class BasicServerScript : MonoBehaviour
{
	//RequestQueue requestQueue = new RequestQueue();
	[SerializeField] private int gRPCPort = 30051;
	WorldTimeManager worldTimeManager;
	AgentSession agentSession;
	UnityBasicEnvImpl ubeImpl;
    // Start is called before the first frame update
    void Start()
    {
		//yes this can obviously be cleaned up
		worldTimeManager = new WorldTimeManager();
		agentSession = new AgentSession(worldTimeManager);
		ubeImpl = new UnityBasicEnvImpl();
		ubeImpl.AssignAgentSessionAndWTM(agentSession, worldTimeManager);

		Server server = new Server
		{
			//Services = { UnityBasicEnv.BindService(new UnityBasicEnvImpl()) },
			Services = { UnityBasicEnv.BindService(ubeImpl) },
			Ports = { new ServerPort("localhost", gRPCPort, ServerCredentials.Insecure) }
		};
		server.Start();

		
    }

    // Update is called once per frame
    void Update()
    {
		//agentSession.TestAvatarPrint("Asdfasdf");
    }

	public void SendInfoToServer(string msg, int obs, float rew, bool done)
	{
		Debug.Log("NOT IMPLEMENTED sending info to the server");
		//ubeServer.UnityBasicEnvChat(msg, obs, rew, done);
		//ubeImpl.AddToResponseList(msg, obs, rew, done);
	}
}
