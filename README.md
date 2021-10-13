# VRGiO

## TODO:
## Add Arduino Docs
## VRGiO Server
Following are the main functions supported by the server.

* Register cube with unique IP address <br/>
* Attach two components to each other<br/>
The details for each feature can be accessed using the following URL <br/>
[http://localhost:8000/docs](http://localhost:8000/docs)
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
