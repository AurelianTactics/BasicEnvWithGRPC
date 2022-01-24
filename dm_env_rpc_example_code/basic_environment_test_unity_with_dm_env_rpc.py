from absl import app
import grpc

from dm_env_rpc.v1 import connection as dm_env_rpc_connection
from dm_env_rpc.v1 import dm_env_adaptor
from dm_env_rpc.v1 import dm_env_rpc_pb2
from dm_env_rpc.v1 import dm_env_rpc_pb2_grpc

import dm_env_rpc
import docker
import numpy as np

# runs the client code
# takes a docker image, creates container, connects
# interacts with the container with dm_env_rpc as a dm_env environment

def main(_):
  container = docker.from_env().containers.run(
    image="basic_example",
    #command='echo hello world',
    #auto_remove=False,
    detach=True,
    ports={30051:30051}
    #ports={30051:('127.0.0.1', 30051)}
    #ports={10000:10000}
    )

  connection = dm_env_rpc_connection.create_secure_channel_and_connect(
    #"localhost:30051")
    "localhost:30051")
    #"0.0.0.0:30051")
  
  env, _ = dm_env_adaptor.create_and_join_world( connection, create_world_settings={}, join_world_settings={})
  print(env)
  print(env.action_spec().items())
  time_step = env.reset()
  print(time_step)
  step_counter = 0
  #while not time_step.last():
  while True:
    action = {}
    for name, spec in env.action_spec().items():
      #action[name] = np.random.uniform(spec.minimum, spec.maximum, spec.shape)
      action[name] = 1
    time_step = env.step(action)
    print(time_step)
    step_counter += 1
    if step_counter > 100:
      break

if __name__ == '__main__':
  app.run(main)
