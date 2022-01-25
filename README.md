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
