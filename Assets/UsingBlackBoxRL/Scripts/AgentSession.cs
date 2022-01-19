using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grpc.Core;
//using Unitybasicenv;
using DmEnvRpc.V1;
using System.Threading.Tasks;

namespace AurelianTactics.BlackBoxRL
{
	/// <summary>
	/// Sends information to Communication layer
	/// Not sure how and what to balance between WorldTimeManager and this
	/// </summary>

	//to do:
	//i'm thinking i instantiate/look for the avatars and tasks and attach them to agentsession in an iterable object
	//then can do shit on them and be able to do stuff on them
	//i'm assuming possible multiples of avatars but only one task per scene.
	//task is supposed to specify the avatar(s) (maybe plural, not sure on this) the agent controls
	//a single scalar reward
	//i think i want action masking here
	// make obs, action, reward dynamic
	//some sort of sanity check to confirm action actually went through?
	//reward penalty for choosing non available action
	//I don't think I'm syncing stuff properly

	// from deepmind env (bsuite env catch example)
	//print(env.action_spec())
	//DiscreteArray(shape= (), dtype= int64, name= action, minimum= 0, maximum= 2, num_values= 3)
	//print(env.observation_spec())
	//BoundedArray(shape= (10, 5), dtype= dtype('float32'), name= 'board', minimum= 0.0, maximum= 1.0)




	public class AgentSession
    {
		//Unity related things
		GameObject basicAgent;
        WorldTimeManager worldTimeManager;
        Avatar avatar;
        BlackBoxRLTask blackBoxRLTask;
		//BasicServerScript basicServerScript;
		string worldName; //CreateWorldResponse sends this back

		private List<int> availableActions; //to do: change based on shape
		private int defaultAction;

		private int action; //to do: change based on action shape
		private float observation; //to do: change based on obs shape
		private float reward; 
		private bool done;

		/// <summary>
		/// StepReponse asks for state description. I think it's for handling resets (ie terminated, running, etc)
		/// </summary>
		private EnvironmentStateType envStateType;

		/// <summary>
		/// Server sets some UID keys for passing tensor maps over gRPC
		/// </summary>
        const int UID_ACTIONS = 2000;
		/// <summary>
		/// Server sets some UID keys for passing tensor maps over gRPC
		/// </summary>
        const int UID_OBSERVATIONS = 2001;
		/// <summary>
		/// Server sets some UID keys for passing tensor maps over gRPC
		/// </summary>
        const int UID_REQUESTED_OBSERVATIONS = 2002;
		/// <summary>
		/// Server sets some UID keys for passing tensor maps over gRPC
		/// </summary>
		const int UID_OBSERVATION_REWARD = 2003;
		/// <summary>
		/// Server sets some UID keys for passing tensor maps over gRPC
		/// </summary>
		const int UID_OBSERVATION_AGENT = 2004;


		private int priorAction; //to do: change based on action shape
		private float priorObs; //to do: change based on obs shape
		private float priorReward;

		bool isReadyForRequestQueue = false; //if ready to read from request queue

		const string CONNECT_MESSAGE = "connect";
		const string START_MESSAGE = "start";
		const string STEP_MESSAGE = "step";

		public AgentSession(WorldTimeManager wtm, string worldName = "basic_env_world")
		{
			this.worldTimeManager = wtm;
			this.priorObs = 10;
			this.priorReward = 0.0f;
			this.priorAction = 0;
			this.isReadyForRequestQueue = true;
			this.envStateType = EnvironmentStateType.Terminated; //set to running when reset

			//UnityEngine.Object pPrefab = Resources.Load("Assets/Prefab/Items/Key_yellow"); // note: not .prefab!
			//GameObject pNewObject = (GameObject)GameObject.Instantiate(pPrefab, Vector3.zero, Quaternion.identity);
			UnityEngine.Object pPrefab = Resources.Load("Prefabs/Basic");
			basicAgent = (GameObject)GameObject.Instantiate(pPrefab, Vector3.zero, Quaternion.identity);

			//GameObject instance = Instantiate(Resources.Load("enemy", typeof(GameObject))) as GameObject;
			//basicAgent = GameObject.Instantiate(Resources.Load("Assets/Prefabs/Basic", typeof(GameObject))) as GameObject;

			//ClassName variableName = (ClassName)gameObject.GetComponent(typeof(ClassName));
			//HingeJoint hinge = GetComponentInChildren<HingeJoint>();
			avatar = (Avatar)basicAgent.GetComponentInChildren(typeof(Avatar));
			Debug.Log(avatar);
			blackBoxRLTask = (BlackBoxRLTask)basicAgent.GetComponentInChildren(typeof(BlackBoxRLTask));
			Debug.Log(blackBoxRLTask);

			this.worldName = worldName;

			//instantiate a prefab
			//Obj = MonoBehaviour.Instantiate(object);
		}

		//private void Start()
  //      {
		//	Debug.Log("agentSession is started");
		//	//TO DO:
		//	//scan for avatars and add rather than hard code in
		//	//assign tasks rather than hard code in
		//	//don't hardcode in priorObs, reward, action but do it based on the specs
		//	this.priorObs = 10;
  //          this.priorReward = 0.0f;
  //          this.priorAction = 0;
		//	this.isReadyForRequestQueue = true;

		//}


        async Task<string> EpisodeReset()
        {
			Debug.Log("TEST in episode reset");
            var resetMessage= await this.blackBoxRLTask.StartEpisode();
			GetInfoFromTaskAndAvatar();
			//SendInfoToCommunicationLayer("step");
			Debug.Log("To do: might need to reset all the stuff that gets sent on a reset to the communcation layers ie zero out the obs and what not ");
			return resetMessage;
		}

		//not implemented. I think it would be better this way but i'm not clear how to access the grpc bidirectional stream to send information
		//after every reset/step update blackBoxRLTask updates done and rewards and avatar updates obs
		// void SendInfoToCommunicationLayer(string message)
		// {
		// 	Debug.Log("testing sending stuff to communication layer");
		// 	//basicServerScript.SendInfoToServer(message, this.observation, this.reward, this.done);
		// 	////to do actually hook up and send
		// 	//Console.WriteLine(
		// 	//	"statement Obs :{0}\t reward B :{1}\t action C :{2}\t done D :{3}",
		// 	//	this.observation, this.reward, this.action, this.done);
		// }

		// get all available actions agent can take
		ActionSpec GetActionSpec()
		{
			return this.avatar.GetActionSpec();
		}

		// get observation spec
		ObservationSpec GetObservationSpec()
		{
			return this.avatar.GetObservationSpec();
		}


		public void GetAvailableActions()
		{
			this.availableActions = this.avatar.GetAvailableActions();
		}

		// take an action
		//get available actions
		//select the one setn from the manager
		//take it in the avatar
		//get result
		//async Task<List<UnityBasicEnvNote>> TakeAction(int wmAction)
		async Task<List<DmEnvRpc.V1.EnvironmentResponse>> TakeAction(int rqAction)
		{
			Debug.Log("TEST: top of take action");
			//List<UnityBasicEnvNote> ubeList = new List<UnityBasicEnvNote>();
			List<EnvironmentResponse> eroList = new List<EnvironmentResponse>();
			Debug.Log("TEST: top of take action, after creating ubeList");

			// to do: implement that a check that action can be taken
			//if (this.availableActions.Contains(action))
			//{
			//	Debug.Log("action found");
			//	this.action = this.availableActions[wmAction];
			//}
			//else
			//{
			//	Debug.Log("action not found, using default action ");
			//	this.action = this.defaultAction;
			//}

			// send action to the env
			Debug.Log("TEST: TakeAction awaiting action to be taken");
			var stepMessage = await this.avatar.TakeAction(rqAction);
			Debug.Log("TEST: TakeAction after action taken");
			//to do: handle error message

			// update prior obs, reward, and action. Some RL algs use these
			UpdatePrior();
			Debug.Log("TEST: TakeAction after update prior");

			// get Obs back from avatar
			this.observation = (float) this.avatar.GetObservation();
            // TO DO: get info (or dm_env equivalent) back from avatar)

            // get done and reward from Task
			this.done = this.blackBoxRLTask.GetDone();
			this.reward = this.blackBoxRLTask.GetReward();

			// alternative way
			//SendInfoToCommunicationLayer("step");

			//var stepNote = MakeUnityBasicEnvNote("step", this.observation, this.reward, this.done);
			//ubeList.Add(stepNote);
			var erObject = MakeEnvironmentResponseObject(EnvironmentResponse.StepFieldNumber);
			eroList.Add(erObject);
            if(this.done)
            {
				Debug.Log("TEST: TakeAction is done, sending reset");
				var resetMessage = await EpisodeReset();
				//todo error handling
				// var resetNote = MakeUnityBasicEnvNote("step", this.observation, this.reward, this.done);
				// ubeList.Add(resetNote);
				erObject = MakeEnvironmentResponseObject(EnvironmentResponse.ResetFieldNumber);
				eroList.Add(erObject);
				Debug.Log("TEST: TakeAction is done, after reset");
			}
			this.isReadyForRequestQueue = true;

			Debug.Log("TEST: TakeAction bottom");
			//return ubeList;
			return eroList;
        }

		// update prior information
		void UpdatePrior()
		{
			this.priorObs = this.observation;
			this.priorReward = this.reward;
			this.priorAction = this.action;
		}

		// to do: more config options. for now just sets the position
		async Task<string> StartEnv(int configValue)
		{
			Debug.Log("TEST starting env with initial start position of " + configValue);
			this.priorObs = configValue;
			//// send info to task and reset through task
			this.blackBoxRLTask.pos = configValue;
			var resetMessage = await EpisodeReset();
			
			//this.isReadyForRequestQueue = true;
			Debug.Log("TEST end starting env");
			return resetMessage;
		}


		//to do: abstract settings better. maybe push into the BBRLTask
		//validate the settings somewhere rather than assume proper format
		async Task<string> StartEnv(Dictionary<string,Tensor> cwSettings)
		{
			Debug.Log("TEST starting env with befor reading settings ");
			var startPositionTensor = new Tensor();
			cwSettings.TryGetValue("start_position", out startPositionTensor);
			//startPositionTensor.PayloadCase
			//int startPosition = 11;
			// var startPositionTensor = new Tensor();
			// //startPositionTensor.Int32S = new Tensor.Types.Int32Array[] ;
			// //Tensor.Types.Int8Array startPostion = new Tensor.Types.Int32Array[] {11};
			// // Tensor startPostion = new int[] {11};
			// cwSettings.TryGetValue("start_position", out startPositionTensor);
			// startPositionTensor.Strings.get
			// var startPositionArray = (int[]) startPositionTensor;
			// //int startPosition = (int) startPositionTensor.Int32S;
			// Debug.Log("TEST starting env with initial start position of " + startPosition[0]);



			// this.priorObs = startPosition[0];
			// //// send info to task and reset through task
			// this.blackBoxRLTask.pos = startPosition[0];
			var resetMessage = await EpisodeReset();
			
			//this.isReadyForRequestQueue = true;
			Debug.Log("TEST end starting env");
			return resetMessage;
		}

		void GetInfoFromTaskAndAvatar()
		{
			this.observation = (float) this.avatar.GetObservation();
			this.reward = this.blackBoxRLTask.GetReward();
			this.done = this.blackBoxRLTask.GetDone();
			if( this.done){
				this.envStateType = EnvironmentStateType.Terminated;
			}
			else {
				this.envStateType = EnvironmentStateType.Running;
			}
		}

		//to do: test in Impl, expand, add error functionality

		DmEnvRpc.V1.EnvironmentResponse MakeEnvironmentResponseObject(int responseType)
		{
			
			Debug.Log("making new EnvironmentResponseObject");
			var erObject = new DmEnvRpc.V1.EnvironmentResponse();

			if( responseType == DmEnvRpc.V1.EnvironmentRequest.StepFieldNumber){
				DmEnvRpc.V1.StepResponse response = new StepResponse();
				Debug.Log("To Do: send back obs, send back whether the env is runing or not (not running if done)");
				response.State = this.envStateType;

				Tensor rewardTensor = new Tensor();
				rewardTensor.Floats = new Tensor.Types.FloatArray();
				rewardTensor.Floats.Array.Add(this.reward);
				response.Observations[UID_OBSERVATION_REWARD] = rewardTensor;

				Tensor agentTensor = new Tensor();
				agentTensor.Floats = new Tensor.Types.FloatArray();
				agentTensor.Floats.Array.Add(this.observation);
				response.Observations[UID_OBSERVATION_REWARD] = agentTensor;
				
				erObject.Step = response;
			}
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.ResetFieldNumber){
				DmEnvRpc.V1.ResetResponse response = new ResetResponse();
            	Debug.Log("To Do: missing part of the response");
				erObject.Reset = response;
			}
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.ResetWorldFieldNumber){
				DmEnvRpc.V1.ResetWorldResponse response = new ResetWorldResponse();
            	Debug.Log("To Do: missing part of the response");
				erObject.ResetWorld = response;
			}
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.JoinWorldFieldNumber){
				DmEnvRpc.V1.JoinWorldResponse response = new JoinWorldResponse();
            	Debug.Log("To Do: missing part of the response");
				erObject.JoinWorld = response;
			}
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.LeaveWorldFieldNumber){
				DmEnvRpc.V1.LeaveWorldResponse response = new LeaveWorldResponse();
            	Debug.Log("To Do: missing part of the response");
				erObject.LeaveWorld = response;
			}
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.CreateWorldFieldNumber){
				DmEnvRpc.V1.CreateWorldResponse response = new CreateWorldResponse();
            	response.WorldName = this.worldName;
				erObject.CreateWorld = response;
			}
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.DestroyWorldFieldNumber){
				DmEnvRpc.V1.DestroyWorldResponse response = new DestroyWorldResponse();
            	Debug.Log("To Do: missing part of the response");
				erObject.DestroyWorld = response;
			}
			// else if( responseType == DmEnvRpc.V1.EnvironmentRequest.ExtensionFieldNumber){
			// 	DmEnvRpc.V1 response = new Response();
            // 	Debug.Log("To Do: missing part of the response");
			// 	erObject.Extension = response;
			// }
			//to do: extension
			//to do: error

			Debug.Log("TEST returning new EnvironmentResponseObject");
			return erObject;
			// if( payloadCase == DmEnvRpc.V1.EnvironmentRequest.CreateWorldFieldNumber){
			// 	return new DmEnvRpc.V1.EnvironmentResponse
			// 	{
			// 		DmEnvRpc.V1.CreateWorldResponse("UnityBasicEnvWorldName")
			// 	};
			// }


			// Debug.Log("Error unable to find proper EnvironmentResponseObject");
			// return new DmEnvRpc.V1.EnvironmentResponse
			// {
				
			// };
		}

		// client sends EnvironmentRequests to server
		// server packages the requests into a requestQueue object in WTM
		// server then asks for a response here
		public async Task<List<DmEnvRpc.V1.EnvironmentResponse>> GetEnvironmentResponseList()
		{
			Debug.Log("in GetEnvResponse 0");
			List<DmEnvRpc.V1.EnvironmentResponse> eroList = new List<DmEnvRpc.V1.EnvironmentResponse>();
			RequestQueueObject rqo = worldTimeManager.ProcessRequests();
			//to do: rqo actual message
			if (rqo != null)
			{
				Debug.Log("in GetEnvResponse 1 " + rqo.requestType);
				if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.StepFieldNumber){
					int action = 0;
					//to do: error handling for actions not being set in during step
					action = rqo.unpackedTensorDict["actions"][0];
					//not sure how to handle requested_observations
					eroList = await TakeAction(action);
				}
				else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.ResetFieldNumber){

				}
				else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.JoinWorldFieldNumber){

				}
				else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.LeaveWorldFieldNumber){

				}
				else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.ResetWorldFieldNumber){

				}
				else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.CreateWorldFieldNumber){
					// CreateWorld comes in with config settings
					// in this case just the reset position of the agent
					int startPosition = 10;
					if (rqo.unpackedTensorDict.ContainsKey("start_position") ){
						if(rqo.unpackedTensorDict["start_position"] != null && rqo.unpackedTensorDict["start_position"].Count > 0)
							startPosition = rqo.unpackedTensorDict["start_position"][0];
					}
					var startMessage = await StartEnv(startPosition);
					var erObject = MakeEnvironmentResponseObject(rqo.requestType);
					eroList.Add(erObject);
				}
				else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.DestroyWorldFieldNumber){

				}
				else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.ExtensionFieldNumber){

				}
			}
			else
			{
				Debug.Log("ERROR: not able to process rqo");
				// var newNote = MakeUnityBasicEnvNote("step", this.observation, this.reward, this.done);
				// ubeList.Add(newNote);
				//to do: some sort of error
			}

			return eroList;
		}


		// UnityBasicEnvNote MakeUnityBasicEnvNote(string message, int actionObs, float reward, bool done)
		// {
		// 	Debug.Log("making new unitybasicenv note");
		// 	return new UnityBasicEnvNote
		// 	{
		// 		Message = message,
		// 		ActionObs = actionObs,
		// 		Reward = reward,
		// 		Done = done
		// 	};
		// }

		// public async Task<List<UnityBasicEnvNote>> GetNoteResponse()
		// {
		// 	Debug.Log("in GetNoteResponse 0");
		// 	var ubeList = new List<UnityBasicEnvNote>();
		// 	var rqo = worldTimeManager.ProcessRequests();
		// 	if (rqo != null)
		// 	{
		// 		Debug.Log("in GetNoteResponse 1 " + rqo.message);
		// 		if (rqo.message == STEP_MESSAGE)
		// 		{
		// 			Debug.Log("in GetNoteResponse 2");
		// 			//to do: error handly that sends back an error note
		// 			ubeList = await TakeAction(rqo.actionInt);
		// 		}
		// 		else if (rqo.message == START_MESSAGE)
		// 		{
		// 			Debug.Log("in GetNoteResponse 3");
		// 			Debug.Log("rqo action int is " + rqo.actionInt);
		// 			var startMessge = await StartEnv(rqo.actionInt);
		// 			//to do: error handling that sends back an error note
		// 			var newNote = MakeUnityBasicEnvNote("step", this.observation, this.reward, this.done);
		// 			ubeList.Add(newNote);
		// 		}
		// 		//else if ( rqo.message == CONNECT_MESSAGE)
		// 		//{
		// 		//	//todo
		// 		//}
		// 	}
		// 	else
		// 	{
		// 		Debug.Log("ERROR: not able to process rqo");
		// 		var newNote = MakeUnityBasicEnvNote("step", this.observation, this.reward, this.done);
		// 		ubeList.Add(newNote);
		// 	}

		// 	return ubeList;
		// }

		// public async Task TestPrint(string msg)
		// {
		// 	Debug.Log("in agent session: " + msg);
		// }

		// public async Task TestMono(string msg)
		// {
		// 	Debug.Log("TEST: top in agent session: " + msg);
		// 	this.blackBoxRLTask.pos = 11;
		// 	this.avatar.TestPrint(" call from AS ");
		// 	Debug.Log("TEST: bottom in agent session: " + msg);
		// }

		// public void TestAvatarPrint(string msg)
		// {
		// 	Debug.Log("TEST: top in agent session: " + msg);
		// 	//this.blackBoxRLTask.pos = 11;
		// 	this.avatar.TestPrint(" call from AS ");
		// 	Debug.Log("TEST: bottom in agent session: " + msg);
		// }

	}

	
}
