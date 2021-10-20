# VRGiO

## 1.1 VRGiO WEMOS Features
Following are the main functions currently supported by the .

* Register cube with unique IP address with the server<br/>
* Listen to requests from other cubes and perform any actuation/action based on request data <br/>

## 1.2 VRGiO Server Features
Following are the main functions supported by the server.

* Register cube with unique IP address <br/>
* Attach two components (cubes) to each other<br/>
* Remove bi-directional connection in Graph between two nodes representing the physical components that got attached to each other <br/>
* Total count of components registered <br/>
* Retrieve metadata about any given node using its IP address as the identifier. For inspecting any node for its neighbors and its own type. <br/>
* Actuates the physical component or performs message passing from one component to another <br/>
* Visualizes the entire Graph with all components shown <br/>
* The details for each feature can be accessed using the following URL after you run the server <br/>
[http://localhost:8000/docs](http://0.0.0.0:8000/docs)
## 1.3 VRGiO Server Setup Guide
### 1.3.1.1 Build the Docker Image
```bash
cd /vrgio/server/
docker build . -t vrgio_server
```
### 1.3.1.2 Start the Docker Container
```bash
docker run -p 8000:8000 vrgio_server
```
### 1.3.1.3 Access the server UI
```bash
http://0.0.0.0:8000/docs
```
### 1.3.2 Alternatively run server without Docker
```bash
cd /vrgio/server/
pip3 install -r requirements.txt
uvicorn server:app
```
