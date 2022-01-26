# BasicEnvWithGRPC
 
* Rough implementation of the paper "Using Unity to Help Solve Intelligence" https://arxiv.org/abs/2011.09294
* Blog post: https://medium.com/aureliantactics/paper-implementation-using-unity-to-help-solve-intelligence-dcb4f1193c7e
* Includes code, assets, and environment from Unity ML Agents: https://github.com/Unity-Technologies/ml-agents

# Usage
* Install dependencies: 
``` pip install docker grpcio grpcio-tools dm_env_rpc ```
* Build the docker image from the app/ directory:
``` docker build -t basic_example . ```
* Launch the container from the docker image and run the Python client by running the file:

``` python dm_env_rpc_example_code/basic_environment_test_unity_with_dm_env_rpc.py ```

# This Repo Includes
* The Unity code if you want to edit the environment
* A linux build of the Unity environment (in the /app folder)
* If you want to edit the Unity code from the editor, you'll need to install the plug ins generated from the gRPC: go to https://packages.grpc.io/archive/2019/11/6950e15882f28e43685e948a7e5227bfcef398cd-6d642d6c-a6fc-4897-a612-62b0a3c9026b/index.xml go to the grpc_unity_package, unzip and drag the plug ins directory into Unity project's Plugins folder

# To Do (Pull Requests/Collaboration Accepted)
![using_unity_to_help_solve_intelligence](https://user-images.githubusercontent.com/25700742/151165654-98509e20-a185-4819-8d8f-24608203105a.png)
Picture is an outline from the paper: https://arxiv.org/abs/2011.09294

## Agent/Learning Interface (Outside the Black Box)
* End-to-end example training a model and solving an env using an existing RL library like Acme (currently just an env that does 100 timesteps).
* Be able to use a trained model to perform inference. Outside the env and maybe embedded in the env.

## The Black Box
* More black boxes to test algorithms/RL libraries on
* Better Docker documentation, better Docker practices, and versioning. My Docker is pretty rough.
* Streamline the headless Unity mode used? The DeepMind paper uses a different rendering solution but I'm not sure what is optimal.

## The communication layer: gRPC and dm_env_rpc improvements
* OpenAI Gym adaptor similar to dm_env adaptor for RL learners that only work with gym type outputs
* Better packing and unpacking of C# code (similar to this dm_env_rpc Python file)
* Figure out if current version of Unity works with gRPC and what steps need to be taken (this issue)
* Debug the dm_env_rpc proto import bug
* Multi-agent support
* Confirm I'm handling the gRPC process the correct way
* Dynamic port settings

## The session layer
* Synchronization
* Handling multiple connections to black box. Synchronization and RequestQueue concerns
* WorldTimeManager managers world time
* RequestQueue is more robust. RequestQueue is fully thought through. Should it push or pull
* Abstract part of the AgentSession class, inherit from base for specific examples
* Fully implement the requests and response fields. ie on a ResetRequest with the settings field sent over the gRPC there's no action taken by the env.
* Error handling
* Exception handling
* Figure out what the SessionFactory is and does
* Dynamic configuration of settings
* Fill out the TensorUtilities.cs file for other requests and responses

## The interface layer
* Abstract and inherit from base class for then do specific implementations for Avatar and Task
* Figure out best Avatar and Task set up. The paper has Tasks spawning Avatars while I instantiate one of each and don't have them linked explicitly.
* Abstract and dynamically define obs and action specs, obs, actions, rewards, and episode parameters. Maybe through GameObject editing.
* Make Sensors more robust so a variety of obs can be returned.
* Make Avatar scan for Sensors and Actuators.
* Allow Avatars to handle a variety of actions.
* Sequence Avatar better so JoinWorld and Reset requests can alter the settings
