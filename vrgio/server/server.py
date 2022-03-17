import asyncio
import json
from asyncio import get_event_loop
from http import client
from pickle import FALSE
from time import sleep
from turtle import delay
from typing import Dict, List, Optional

import websockets
import websockets.server
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from h11 import ConnectionClosed
from starlette.websockets import WebSocket

from structure_manager import StructureManager

# current_touches = List()

class ConnectionManager:
    def __init__(self):
        self.active_connections: Dict[str, WebSocket] = {}

    def connect(self, client_id: str, websocket: WebSocket):
        self.active_connections[client_id] = websocket

    def disconnect(self, client_id: str):
        self.active_connections.pop(client_id, None)
        structure_manager.cleanse_component_from_structure(client_id)

    async def broadcast_text(self, message: str):
        for connection in self.active_connections:
            await connection.send_text(message)

app = FastAPI()
origins = ["*"]
app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)

structure_manager = StructureManager()
connection_manager = ConnectionManager()

@app.websocket("/ws/{client_id}")
async def websocket_endpoint(websocket: WebSocket, client_id: str):
    await websocket.accept()
    connection_manager.connect(client_id, websocket)
    try:
        while True:
            data = await websocket.receive_text()
            await websocket.send_text("get touch data")
    except:
        websocket.close()
        connection_manager.disconnect(client_id)

@app.websocket_route("/unity_ws")
async def unity_websocket_endpoint(websocket: WebSocket):
    await websocket.accept()
    try:
        while True:
            await websocket.receive_text()
            await websocket.send_json(
                {"type": "json", 
                "nodes": [node for node in structure_manager.structure.nodes],
                "edges": [{"node1":edge[0], "node2":edge[1], "side":edge[2]["side"]} for edge in structure_manager.structure.edges.data()]})
            await asyncio.sleep(1)
    except:
        await websocket.close()

@app.get("/register/component")
async def register_shape(
    src_ip: str, type: str = "main", component_class: str = "cube"
):
    """
    Adds new node i.e., cube to Graph, which gets
    connected in the shape structure physically.

    Args:
        src_ip (str): [description]
        type (str, optional): [description]. Defaults to "main".
        component_class (str, optional): [description]. Defaults to "cube".

    Returns:
        response (Dict): Status about the operation
    """
    component = [(src_ip, {"type": type, "component_class": component_class})]
    structure_manager.add_component(component)
    # for (int i in range(6)):
    return {"status": True, "component_id": component[0][0], "event": "add"}


@app.get("/connect/components")
async def connect_shape(node_ip: str, side: str):
    """
    Etasblishes bi-directional connection in Graph between two nodes representing
    the physical components that got attached to each other.


    Args:
        node_ip (str): Node's IP which got a new node attached to it.
        side (str): Side at which cube got connected {left, right, up, down, front, back}

    Returns:
        response (Dict): Status about the operation
    """
    response = {"status": True, "event": "connect"}
    try:
        structure_manager.add_connection(node_ip, side)
    except:
        response = {"status": False, "event": "error connecting nodes"}
    return response


@app.get("/disconnect/components")
async def connect_shape(node_one_ip: str, node_two_ip: str):
    """
    Removes bi-directional connection in Graph between two nodes representing
    the physical components that got attached to each other.

    Args:
        node_one_ip (str): Node's IP which got a new node attached to it.
        node_two_ip (str): The new node's IP that got itself attached.

    Returns:
        response (Dict): Status about the operation
    """
    response = {"status": True, "event": "disconnect"}
    try:
        structure_manager.remove_connection(node_one_ip, node_two_ip)
    except:
        response = {"status": False, "event": "error disconnecting nodes"}
    return response


@app.get("/component/count")
async def count_nodes():
    """
    Returns the total count of

    Returns:
        response (Dict): Status about the operation
    """
    return {
        "status": True,
        "component_count": structure_manager.structure.number_of_nodes(),
        "event": "count",
    }


@app.get("/component/info")
async def get_info(src_ip: str):
    """
    Returns metadata about any given node using its
    IP address as the identifier. For inspecting any
    node for its neighbors and its own type.

    Args:
        src_ip (str): [description]

    Returns:
        response (Dict): Status about the operation
    """
    neighbors, component_class, type = structure_manager.inspect_node(src_ip)
    return {
        "status": True,
        "neighbors": neighbors,
        "class": component_class,
        "type": type,
        "event": "info",
    }

@app.get("/component/ip_info")
async def get_ip_info():
    ip_list = structure_manager.list_nodes()
    return {
        "status": True,
        "ip_list": ip_list,
        "event": "ip_info"
    }


@app.post("/component/actuate")
async def actuate(src_ip: str, payload: Optional[Dict]):
    """
    Actuates the physical component or performs message passing from
    one component to another.

    Args:
        src_ip (str): IP address of the component to be actuated.

    Returns:
        response (Dict): Status about the operation
    """
    try:
        websocket = connection_manager.active_connections[src_ip]
    except:
        response = {"status": False, "event": "error connecting nodes"}

    connection_manager.send_json(payload, websocket)
    json_answer = await connection_manager.receive_json(websocket)

    response = {
        "status": json_answer["status"],
        "event": "actuate",
        "message": json_answer["message"]
    }
    return response

import uvicorn

if __name__ == '__main__':
    uvicorn.run(app, host="0.0.0.0", port=8000, ws_ping_interval=5, ws_ping_timeout=10)
