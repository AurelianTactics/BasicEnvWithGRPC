using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using DmEnvRpc.V1;

namespace AurelianTactics.BlackBoxRL
{
	/// <summary>
	/// Receives information from AgentSession to Actuators (sort of like actions)
	/// Sends information to AgentSession from Sensors (sort of like observations)
	/// DeepMind paper way is to attach an an Avatar to a game object and then have Acuators and Sensors be C# components
	/// </summary>
	

	//to do:
	//need to rethink how I'm designing this, in particular in relation to BBRL Task and multiple avatars
	//abstract, inherit, make more customizable
	//sending back/getting obs more dynamic
	//be able to send back a variety of obs like the paper does
	//reward spec: how I want to do things
	//verifying the action is valid

	//Bad sequencing: I'm creating the action and obs spec prior to joinworld when joinworld should be able to chagne those
	//action and obs spec just dm env tensorspecs or should I have it still be its own custom class?
	//fully implement all action and obs specs fields (ie the BlackBoxSense shit)
	//how to format the action and specs
	//how to handle invalid actions (maybe a default do nothing command that can be set upon creation)
	//action masking (maybe here or agent session)

	//able to handle a more generic env and send mroe generic commands get more generic info from that env


	public class Avatar : MonoBehaviour
    {
        [ScalarActuator("ACTION_LENGTH")]
        public int ActionLength;

        [BlackBoxSensor("OBS_SHAPE", new int[] {20})]
        public double[] OBSShape = new double[20];

        public BasicController basicController;

        TensorSpec actionSpec;
		TensorSpec observationSpec;
		TensorSpec rewardSpec;
		Dictionary<string, string> observation;
		NextActionState nextActionState;
		public List<int> actionList;


        void Start()
        {
			//Debug.Log("Avatar starting");
			//this.actionList = new List<int>(new int[3]);
		}

        //in paper, described as being on the game object so can send actions and what not through that attachment
        //here faking it and have a combat controller attached then sending actions through that
        //I need to attach the avatar to the scene and put on the combatController

		/// <summary>
		/// Settings can be sent in with Create/join/resetWorld and reset requests and
		/// Specs can be changed based on those updated settings
		/// </summary>
		/// <param name="config"></param>
        public void UpdateSpecs(string config = "")
		{
			//to do: code to load the spec
			//to do: code to turn spec into various variables
			//to do: get specs from env

			this.actionSpec = new TensorSpec{
				Name="agent_actions",
				Dtype=DmEnvRpc.V1.DataType.Int32
			};
			this.observationSpec = new TensorSpec{
				Name="agent_observations",
				Dtype=DmEnvRpc.V1.DataType.Float
			};
			this.rewardSpec = new TensorSpec{
				Name="reward",
				Dtype=DmEnvRpc.V1.DataType.Float
			};
			this.nextActionState = NextActionState.Waiting;
				
		}

		// Action sent in from AgentSession (which gets action from WTM, which gets action from rpc and so on)
		public async Task<string> TakeAction(int action)
		{
            var stepMessage = await basicController.ConvertActionToDirectionAndMove(action);
			this.nextActionState = NextActionState.Ready;

			//to do: error handly
			return stepMessage;
		}


		//I'm imagining something like the AgentSession wants to know available actions
		//to do: add mask
		public List<int> GetAvailableActions()
		{
			return new List<int>(new int[this.actionList.Count]);
		}

		void SendSensorInformation()
		{

		}

		//imagining something like sending over the number of actions and the observation size
		void SendActionObservationSpecs()
		{

		}

		public TensorSpec GetObservationSpec()
		{
			return this.observationSpec;
		}

		public TensorSpec GetActionSpec()
		{
			return this.actionSpec;
		}

		public TensorSpec GetRewardSpec()
		{
			return this.rewardSpec;
		}

        // to do:
        // obs can be many types of things. need more flexible GetObservation interface
        // shape shouldn't be hardcoded in like this if i'm allowing an attribute to modify shape
        /// <summary>
        /// For now just returning position of the agent. can flesh this out at some point with an actual obs
		/// ie array of all zeros and then a 1 for where the position is
        /// </summary>
        /// <returns></returns>
		public int GetObservation()
		{
            return basicController.position;
        }

		public NextActionState GetNextActionState()
		{
			return this.nextActionState;
		}

		public void SetNextActionState(NextActionState nas)
		{
			this.nextActionState = nas;
		}

		public void TestPrint(string msg)
		{
			Debug.Log("TEST in avatar, printing a message " + msg);
		}
	}

	

}
