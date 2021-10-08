# VRGiO

## TODO:
## Add Arduino Docs
## VRGiO Server
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