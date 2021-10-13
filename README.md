# VRGiO

## Arduino
Following are the main functions supported by the server.

* Register cube with unique IP address with the server<br/>
* Listen to requests from other cubes and perform any actuation/action based on request data <br/>

## VRGiO Server
Following are the main functions supported by the server.

* Register cube with unique IP address <br/>
* Attach two components (cubes) to each other<br/>
* Remove bi-directional connection in Graph between two nodes representing the physical components that got attached to each other <br/>
* Total count of components registered <br/>
* Actuates the physical component or performs message passing from one component to another <br/>
* Visualizes the entire Graph with all components shown <br/>
* The details for each feature can be accessed using the following URL <br/>
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
