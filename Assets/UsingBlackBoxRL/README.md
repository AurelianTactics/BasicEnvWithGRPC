Package to turn allow Unity env to be used as a black box for purposes of Reinforcement Learning (RL)

Based on concepts and ideas outlined by DeepMind paper: https://arxiv.org/abs/2011.09294

Broad overview:
* Relies on an external communication layer and external RL agent not supplied by this package
	* External RL agent communicates with gRPC through dm_env_rpc protocols https://github.com/deepmind/dm_env_rpc
	* The dm_env_rpc communicates with a dockerized Unity env (the black box) allowing:
		* customized configuration of Unity env and parameters for the env at start up
		* communication of typical RL information between the agent and the black box such as passing in actions and receiving observations
* Internally this package translates external configurations into env specific commands and packages env specific information to the session layer.
	* Contains a session layer and an interface layer
	* Session layer processes outside requests and sends out internal data to the communication layer
	* interface layer takes session layers information and translates it to the env and sends out information from the env to the session layer


Usage:
TBD