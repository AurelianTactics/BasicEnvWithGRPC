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
	/// Handles processed instructions from communication layer to manipulate agent in env
	/// Writes responses to go to the communication layer to be sent back over gRPC
	/// setups up the Avatars and BlackBoxRL tasks to get the main information from the env
	/// </summary>

	//to do:
	//no session factory
	//no synchronicity
	//lot of error handling
	//implement all request/response types for dmenvrpc
	//lots needs to be abstracted and then inherit specific examples:
		//agent session/action/obs/reward specs/avatars/tasks/world name/start env options/reset env options/join world options
		//action checking

	

	//i'm thinking i instantiate/look for the avatars and tasks and attach them to agentsession in an iterable object
	//then can do shit on them and be able to do stuff on them
	//i'm assuming possible multiples of avatars but only one task per scene.
	//task is supposed to specify the avatar(s) (maybe plural, not sure on this)
	//the agent controls a single scalar reward
	//i think i want action masking here 
	// make obs, action, reward dynamic
	//checks to make sure reponses actually went through?
	//checks to make sure I'm syncing stuff properly?


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
		/// StepReponse asks for state description.
		/// when state is RUNNING env can step with actions as normal
		/// when TERMINATED or INTERRUPTED the next step action is ignored but state moves to running
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
		const int UID_OBSERVATION_REWARD = 2004;
		/// <summary>
		/// Server sets some UID keys for passing tensor maps over gRPC
		/// </summary>
		const int UID_OBSERVATION_AGENT = 2003;

		/// <summary>
		/// Action spec gives info to the learner for valid actions. Gotten from avatar at start.
		/// </summary>
		TensorSpec actionSpec;
		/// <summary>
		/// Obs spec gives info to the learner for obs shapes and type. Gotten from avatar at start.
		/// </summary>
		TensorSpec observationSpec;
		TensorSpec rewardSpec;


		private int priorAction; //to do: change based on action shape
		private float priorObs; //to do: change based on obs shape
		private float priorReward;

		bool isReadyForRequestQueue = false; //if ready to read from request queue

		public AgentSession(WorldTimeManager wtm, string worldName = "basic_env_world")
		{
			this.worldTimeManager = wtm;
			this.priorObs = 10;
			this.priorReward = 0.0f;
			this.priorAction = 0;
			this.isReadyForRequestQueue = true;
			this.envStateType = EnvironmentStateType.Terminated; //set to running when reset

			// load the prefab scene
			UnityEngine.Object pPrefab = Resources.Load("Prefabs/Basic");
			basicAgent = (GameObject)GameObject.Instantiate(pPrefab, Vector3.zero, Quaternion.identity);

			// set up the avatar and the task
			avatar = (Avatar)basicAgent.GetComponentInChildren(typeof(Avatar));
			//Debug.Log(avatar);
			blackBoxRLTask = (BlackBoxRLTask)basicAgent.GetComponentInChildren(typeof(BlackBoxRLTask));
			//Debug.Log(blackBoxRLTask);

			this.worldName = worldName;

			// Debug.Log("TEST action and obs sepcs are");
			this.avatar.ConfigAvatar();
			this.actionSpec = avatar.GetActionSpec();
			this.observationSpec = avatar.GetObservationSpec();
			this.rewardSpec = avatar.GetRewardSpec();
			// Debug.Log(this.actionSpec);
			// Debug.Log(this.observationSpec);
		}

        async Task<string> EpisodeReset()
        {
			//Debug.Log("TEST in episode reset");
            var resetMessage= await this.blackBoxRLTask.StartEpisode();
			GetInfoFromTaskAndAvatar();
			//Debug.Log("To do: might need to reset all the stuff that gets sent on a reset to the communcation layers ie zero out the obs and what not ");
			return resetMessage;
		}

		// get all available actions agent can take
		TensorSpec GetActionSpec()
		{
			return this.avatar.GetActionSpec();
		}

		// get observation spec
		TensorSpec GetObservationSpec()
		{
			return this.avatar.GetObservationSpec();
		}

		public void GetAvailableActions()
		{
			this.availableActions = this.avatar.GetAvailableActions();
		}

		/// <summary>
		/// Takes an action from a step request and returns a response
		/// gets available actions, sends it to avatar, avatar performs the action
		/// after action, gets updated info on the env and maybe does a reset
		/// </summary>
		async Task<List<DmEnvRpc.V1.EnvironmentResponse>> TakeAction(int rqAction)
		{
			//Debug.Log("TEST: top of take action");
			List<EnvironmentResponse> eroList = new List<EnvironmentResponse>();
			//Debug.Log("TEST: top of take action, after creating ubeList");

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
			//Debug.Log("TEST: TakeAction awaiting action to be taken");
			var stepMessage = await this.avatar.TakeAction(rqAction);
			//Debug.Log("TEST: TakeAction after action taken");
			//to do: handle error message

			// update prior obs, reward, and action. Some RL algs use these
			UpdatePrior();
			//Debug.Log("TEST: TakeAction after update prior");

			// get Obs back from avatar
			this.observation = (float) this.avatar.GetObservation();
            // TO DO: get info (or dm_env equivalent) back from avatar)

            // get done and reward from Task
			this.done = this.blackBoxRLTask.GetDone();
			this.reward = this.blackBoxRLTask.GetReward();

            if(this.done)
            {
				this.envStateType = EnvironmentStateType.Terminated;
				var erObject = MakeEnvironmentResponseObject(EnvironmentResponse.StepFieldNumber);
				eroList.Add(erObject);

				//Debug.Log("TEST: TakeAction is done, sending reset");
				var resetMessage = await EpisodeReset();
				//todo error handling
				// I'm pretty sure I don't send back a reset request here, simply change the state
				
				// erObject = MakeEnvironmentResponseObject(EnvironmentResponse.ResetFieldNumber);
				// eroList.Add(erObject);
				//Debug.Log("TEST: TakeAction is done, after reset");
			}
			else {
				var erObject = MakeEnvironmentResponseObject(EnvironmentResponse.StepFieldNumber);
				eroList.Add(erObject);
			}
			this.isReadyForRequestQueue = true;

			//Debug.Log("TEST: TakeAction bottom");
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
			//Debug.Log("TEST starting env with initial start position of " + configValue);
			this.priorObs = configValue;
			//// send info to task and reset through task
			this.blackBoxRLTask.pos = configValue;
			var resetMessage = await EpisodeReset();
			
			//this.isReadyForRequestQueue = true;
			//Debug.Log("TEST end starting env");
			return resetMessage;
		}

		/// <summary>
		/// Get obs info from avatar
		/// get done/reward info from task
		/// </summary>
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

		/// <summary>
		/// Make response object per request type. Sent back by server to client over grpc
		/// </summary>
		DmEnvRpc.V1.EnvironmentResponse MakeEnvironmentResponseObject(int responseType)
		{
			
			//Debug.Log("making new EnvironmentResponseObject");
			var erObject = new DmEnvRpc.V1.EnvironmentResponse();

			if( responseType == DmEnvRpc.V1.EnvironmentRequest.StepFieldNumber){
				DmEnvRpc.V1.StepResponse response = new StepResponse();
				//Debug.Log("TEST sending back response done, obs, reward: " + this.envStateType + ", " + this.observation + ", " + this.reward);
				response.State = this.envStateType;

				Tensor agentTensor = new Tensor();
				agentTensor.Floats = new Tensor.Types.FloatArray();
				agentTensor.Floats.Array.Add(this.observation);
				response.Observations[UID_OBSERVATION_AGENT] = agentTensor;

				Tensor rewardTensor = new Tensor();
				rewardTensor.Floats = new Tensor.Types.FloatArray();
				rewardTensor.Floats.Array.Add(this.reward);
				response.Observations[UID_OBSERVATION_REWARD] = rewardTensor;
				
				erObject.Step = response;
			}
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.ResetFieldNumber){
				//Debug.Log("TEST: AS making env response object sending back reset");
				DmEnvRpc.V1.ResetResponse response = new ResetResponse();
				response.Specs = new ActionObservationSpecs();
				response.Specs.Actions.Add(UID_ACTIONS, this.actionSpec);
				response.Specs.Observations.Add(UID_OBSERVATION_AGENT, this.observationSpec);
				response.Specs.Observations.Add(UID_OBSERVATION_REWARD, this.rewardSpec);
				erObject.Reset = response;
			}
			// else if( responseType == DmEnvRpc.V1.EnvironmentRequest.ResetWorldFieldNumber){
			// 	DmEnvRpc.V1.ResetWorldResponse response = new ResetWorldResponse();
            // 	Debug.Log("To Do: missing part of the response");
			// 	erObject.ResetWorld = response;
			// }
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.JoinWorldFieldNumber){
				DmEnvRpc.V1.JoinWorldResponse response = new JoinWorldResponse();
            	//Debug.Log("TEST: joining world");
				// proto example
				// message JoinWorldResponse {
				// 	ActionObservationSpecs specs = 1;
				// }
				response.Specs = new ActionObservationSpecs();
				// response.Specs.Actions.Add(DmEnvRpc.V1.ActionObservationSpecs.ActionsFieldNumber, this.actionSpec);
				// response.Specs.Observations.Add(DmEnvRpc.V1.ActionObservationSpecs.ObservationsFieldNumber, this.observationSpec);
				response.Specs.Actions.Add(UID_ACTIONS, this.actionSpec);
				response.Specs.Observations.Add(UID_OBSERVATION_AGENT, this.observationSpec);
				response.Specs.Observations.Add(UID_OBSERVATION_REWARD, this.rewardSpec);
				// Debug.Log("TEST: joining world adding obs spec");
				erObject.JoinWorld = response;
			}
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.LeaveWorldFieldNumber){
				DmEnvRpc.V1.LeaveWorldResponse response = new LeaveWorldResponse();
				erObject.LeaveWorld = response;
			}
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.CreateWorldFieldNumber){
				DmEnvRpc.V1.CreateWorldResponse response = new CreateWorldResponse();
            	response.WorldName = this.worldName;
				erObject.CreateWorld = response;
			}
			else if( responseType == DmEnvRpc.V1.EnvironmentRequest.DestroyWorldFieldNumber){
				DmEnvRpc.V1.DestroyWorldResponse response = new DestroyWorldResponse();
				erObject.DestroyWorld = response;
				// no message back on destroyWorld
			}
			// else if( responseType == DmEnvRpc.V1.EnvironmentRequest.ExtensionFieldNumber){
			// 	DmEnvRpc.V1 response = new Response();
            // 	Debug.Log("To Do: missing part of the response");
			// 	erObject.Extension = response;
			// }
			//to do: extension
			//to do: error

			//Debug.Log("TEST returning new EnvironmentResponseObject");
			return erObject;
		}

		/// <summary>
		/// client sends EnvironmentRequests to server
		/// server packages the requests into a requestQueue object in WTM
		/// server then asks for a response here
		/// </summary>
		public async Task<List<DmEnvRpc.V1.EnvironmentResponse>> HandleEnvironmentRequest()
		{
			//Debug.Log("TEST: AS in HandleEnvironmentRequest 0");
			List<DmEnvRpc.V1.EnvironmentResponse> eroList = new List<DmEnvRpc.V1.EnvironmentResponse>();
			RequestQueueObject rqo = worldTimeManager.ProcessRequests();
			//to do: rqo actual message
			if (rqo != null)
			{
				//Debug.Log("TEST: AS in HandleEnvironmentRequest 1 " + rqo.requestType);
				if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.StepFieldNumber){
					//Debug.Log("TEST: AS in HandleEnvironmentRequest 2 step " + rqo.requestType);
					//reset on a terminated keeps it at terminated. reset on interrupted/running goes to interrupted
					//first step moves from terminated/interuppted into running
					if(this.envStateType == EnvironmentStateType.Running){
						int action = 0;
						//to do: error handling for actions not being set in during step
						action = rqo.unpackedTensorDict["actions"][0];
						//to do: handle requested_observations setting. just sending back the standard
						eroList = await TakeAction(action);
					}
					else if( this.envStateType == EnvironmentStateType.Interrupted || this.envStateType == EnvironmentStateType.Terminated){
						this.envStateType = EnvironmentStateType.Running;
						var erObject = MakeEnvironmentResponseObject(rqo.requestType);
						eroList.Add(erObject);
					} else{
						//to do error handling
					}
					
				}
				else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.ResetFieldNumber){
					//Debug.Log("TEST: AS in HandleEnvironmentRequest RESET 3 " + rqo.requestType);
					//reset on a terminated keeps it at terminated. reset on interrupted/running goes to interrupted
					//first step moves from terminated/interrupted into running

					//to do: maybe warning if reset doesn't come at end of episode? eh maybe not necessary
					//to do: handle reset settings
					//to do: how to handle other state types and send an error
					if( this.envStateType == EnvironmentStateType.Terminated){
						this.envStateType = EnvironmentStateType.Terminated;
					}
					else if( this.envStateType == EnvironmentStateType.Interrupted || this.envStateType == EnvironmentStateType.Running){
						this.envStateType = EnvironmentStateType.Interrupted;
					}
					
					var erObject = MakeEnvironmentResponseObject(rqo.requestType);
					eroList.Add(erObject);
				}
				else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.JoinWorldFieldNumber){
					//to do later
					//join the world based on teh world_name and settings from the join world request
					// message JoinWorldRequest {
					// 	// The name of the world to join.
					// 	string world_name = 1;

					// 	// Agent-specific settings which define how to join the world, such as agent
					// 	// name and class in an RPG.
					// 	map<string, Tensor> settings = 2;
					// }
					var erObject = MakeEnvironmentResponseObject(rqo.requestType);
					eroList.Add(erObject);
				}
				else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.LeaveWorldFieldNumber){
					//no messages sent on leave world request or response
					var erObject = MakeEnvironmentResponseObject(rqo.requestType);
					eroList.Add(erObject);
				}
				// else if( rqo.requestType == DmEnvRpc.V1.EnvironmentRequest.ResetWorldFieldNumber){
					//to do
				// }
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
					//to do: 
					//add actual logic for destroying the world
					//add the worldName to the rqo and destroy that world
					// message DestroyWorldRequest {
					// 	string world_name = 1;
					// }
					var erObject = MakeEnvironmentResponseObject(rqo.requestType);
					eroList.Add(erObject);
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
