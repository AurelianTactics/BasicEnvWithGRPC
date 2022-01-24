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
	/// Example for now is just a generic 
	/// </summary>
	

	//to do:
	//I'm creating the action and obs spec prior to joinworld when joinworld should be able to chagne those
	//action and obs spec just dm env tensorspecs or should I have it still be its own custom class?
	//base actions and obs sent back and available in unity editor based on the specs
	//pick spec parts dynamically then package into specs
	//how to format the actions? Needs to be done on creation? 
		//I'm thinking define number of actions, min and max, continuous or discrete for each available action
			//can flow from some config set up for the env
			//return gym and dm_env convenient functions
		//brainstorming: discrete vs. continuous. array min/maxed vs selecting one. action size for multiple actions
		//action size static or increasing
		//which data type to store it in: list, dictionary, array, 
		//can probably format it based on some dm_env or gym prototype
	//how to handle invalid actions (maybe a default do nothing command that can be set upon creation)
	//how to describe the observation
		//where am I storing the global observation (maybe global avatar)
		//where am I storing and defining the local observation
	//do i need some sort of confirmation that the action is done so that things can progress?
	//have to set up the actual spec file for things to be loaded
	//need something that converst C# style action spec to the DM version
		//either grpc or json, see how the actual connect work


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
			Debug.Log("Avatar starting");

			
        }

        //in paper, described as being on the game object so can send actions and what not through that attachment
        //here faking it and have a combat controller attached then sending actions through that
        //I need to attach the avatar to the scene and put on the combatController

        public void ConfigAvatar(string config = "")
		{
			//to do: code to load the spec
			//to do: code to turn spec into various variables
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
			this.actionList = new List<int>(new int[3]);		
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
