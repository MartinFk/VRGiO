# VRGiO

## TODO:
## Add Arduino Docs
## VRGiO Server
Following are the main functions supported by the server.
* Register cube with unique IP address, request format as follows:
`http://<HOST_NAME>:8000/register/component?src_ip=<CUBE_IP_ADDRESS>&type=<CUBE_TYPE>&component_class=<COMPONENT_TYPE>`
CUBE_TYPE = {vibrating, main, translational, shape}
COMPONENT_TYPE = {cube, shape}
* Attach two components to each other, request format as follows:
`http://<HOST_NAME>:8000/connect/components?node_one_ip=<CUBE_IP_ADDRESS>&node_two_ip=<CUBE_IP_ADDRESS>&side=<ATTACHED_SIDE>`
ATTACHED_SIDE = {left, right, up, down, front, back}
TODO add remaining API endpoints...
### Build the Docker Image
```bash
cd /vrgio/server/
docker build . -t vrgio_server
```
### Start the Docker Container
```bash
docker run -p 8000:8000 vrgio_server
```
### Access the server UI
```bash
http://0.0.0.0:8000/docs
```
### Alternatively run server without Docker
```bash
cd /vrgio/server/
uvicorn server:app
```