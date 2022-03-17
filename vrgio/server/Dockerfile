## copy python base image
FROM python:3.8

## set working directory
WORKDIR /

## copy code to working directory in the image
COPY . .

## expose the fastapi port
EXPOSE 8000

## install dependencies
RUN pip install --no-cache-dir --upgrade -r requirements.txt

## start fastapi server
CMD ["uvicorn", "server:app", "--host", "0.0.0.0", "--port", "8000", "--ws", "websockets", "--ws-ping-interval", "1", "--ws-ping-timeout", "3", "--log-level", "info"]