import asyncio
import json
from asyncio import get_event_loop
from http import client
from pickle import FALSE
from time import sleep
from turtle import delay
from typing import Dict, List, Optional

import uvicorn
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from h11 import ConnectionClosed
from starlette.websockets import WebSocket

from structure_manager import StructureManager


class ConnectionManager:
    def __init__(self):
        self.active_connections: Dict[str, WebSocket] = {}

    def connect(self, client_id: str, websocket: WebSocket):
        self.active_connections[client_id] = websocket

    def disconnect(self, client_id: str):
        self.active_connections.pop(client_id, None)
        app.structure_manager.cleanse_component_from_structure(client_id)

    async def broadcast_text(self, message: str):
        for connection in self.active_connections:
            await connection.send_text(message)

    def send_json(self, payload: Dict, websocket: WebSocket):
        websocket.send_json(payload)

    async def receive_json(self, websocket: WebSocket):
        return await websocket.receive_json()

    def return_websocket(self, client_id: str):
        return self.active_connections[client_id]


app = FastAPI()
origins = ["*"]
app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.structure_manager = StructureManager()
app.connection_manager = ConnectionManager()
app.additional_data = {"cubes": {}, "unity": {}}
app.unitywebsocket = WebSocket(
    scope={"type": "websocket", "path": "/unitywebsocket"}, receive=None, send=None)


# def handle_json_data(json):
#     if json["type"] == "json":
#         assert isinstance(json, dict)
#     if not(json["success"]):
#         print("Error: message returned with success = false")
#     data = json["data"]
#     for key in data:
#         if key == "touches":
#             for touch in data[key]:
#                 if not(touch in additional_data[key]):
#                     additional_data[key].append(touch)
#         if key == "actuate":
#             for src_ip in data[key]:
#                 additional_data[key][src_ip] = data[key][src_ip]
def handle_json_data(json, client_id):
    if client_id == "unity":
        app.additional_data["unity"] = json["data"]
    else:
        app.additional_data["cubes"][client_id] = json["data"]
    return json["expect_answer"]


def wrap_data(data_category, list_of_keys):  # data category is cubes or unity
    wrapped_data = {}
    if list_of_keys and not(list_of_keys == ""):
        if list_of_keys == "all":
            for key in app.additional_data[data_category]:
                wrapped_data[key] = app.additional_data[data_category][key]
        else:
            for key in list_of_keys:
                wrapped_data[key] = app.additional_data[data_category][key]
    return wrapped_data


@app.websocket("/ws/{client_id}")
async def websocket_endpoint(websocket: WebSocket, client_id: str):
    await websocket.accept()
    app.connection_manager.connect(client_id, websocket)
    # expect_answer = True
    # additional_data["actuate"][client_id] = False
    try:
        while True:
            data = await websocket.receive_text()
            try:
                jsondata = json.loads(data)
                if handle_json_data(jsondata, client_id):
                    await websocket.send_json(
                        {"type": "json",
                         "expect_answer": True,
                         "data": wrap_data("unity", "all")})
            except Exception as e:
                print(e)
                await websocket.send_json(
                    {"type": "json",
                     "expect_answer": True,
                     "error": "Invalid JSON"})
            # if (expect_answer):
            #     data = await websocket.receive_text()
            #     jsondata = json.loads(data)
            # if not(expect_answer) or handle_json_data(jsondata):
            #     await websocket.send_json(
            #         {"type": "json",
            #          "expect_answer": True,
            #          "actuate": additional_data["actuate"][client_id],
            #          "data": wrap_data("touches")})
    except Exception as e:
        print(e)
        websocket.close()
        app.connection_manager.disconnect(client_id)


@app.websocket("/unity_ws")
async def unity_websocket_endpoint(websocket: WebSocket):
    print("a")
    await websocket.accept()
    app.unitywebsocket = websocket
    print("b")
    # expect_answer = True
    try:
        while True:
            data = await websocket.receive_text()
            try:
                jsondata = json.loads(data)
                if handle_json_data(jsondata, "unity"):
                    await websocket.send_json(
                        {"type": "json",
                            "expect_answer": True,
                            "nodes": [node for node in app.structure_manager.structure.nodes],
                            "edges": [{"node1": edge[0], "node2":edge[1], "side":edge[2]["side"]} for edge in app.structure_manager.structure.edges.data()],
                            "additional_data": wrap_data("cubes", "all")})
            except Exception as e:
                print(e)
                await websocket.send_json(
                    {"type": "json",
                                "expect_answer": True,
                                "error": "Invalid JSON"})
            await asyncio.sleep(0.2)
            # if (expect_answer):
            #     data = await websocket.receive_text()
            #     jsondata = json.loads(data)
            # if not(expect_answer) or handle_json_data(jsondata):
            #     await websocket.send_json(
            #         {"type": "json",
            #          "expect_answer": expect_answer,
            #          "nodes": [node for node in structure_manager.structure.nodes],
            #          "edges": [{"node1": edge[0], "node2":edge[1], "side":edge[2]["side"]} for edge in structure_manager.structure.edges.data()],
            #          "additional_data": wrap_data(["sides_touched"])})
            # await asyncio.sleep(1)
    except Exception as e:
        print(e)
        unitywebsocket = WebSocket(
            scope={"type": "websocket", "path": "/unitywebsocket"}, receive=None, send=None)
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
    component = [(src_ip, {"type": type, "component_class": component_class, "touch": {
                  "left": False, "right": False, "top": False, "bottom": False, "front": False, "back": False}})]
    app.structure_manager.add_component(component)
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
        app.structure_manager.add_connection(node_ip, side)
    except:
        response = {"status": False, "event": "error connecting nodes"}
    return response


@app.get("/disconnect/components")
async def disconnect_shape(node_one_ip: str, node_two_ip: str):
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
        app.structure_manager.remove_connection(node_one_ip, node_two_ip)
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
        "component_count": app.structure_manager.structure.number_of_nodes(),
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
    neighbors, component_class, type = app.structure_manager.inspect_node(src_ip)
    return {
        "status": True,
        "neighbors": neighbors,
        "class": component_class,
        "type": type,
        "event": "info",
    }


@app.get("/component/ip_info")
async def get_ip_info():
    ip_list = app.structure_manager.list_nodes()
    return {
        "status": True,
        "ip_list": ip_list,
        "event": "ip_info"
    }


touch_type_to_side = {0: "left", 1: "left", 2: "left", 3: "left", 4: "left", 5: "left",
                      6: "left", 7: "left", 8: "left", 9: "left", 10: "left", 11: "left"}


@app.get("/touch/component")
async def touch(src_ip: str, type: int, value: int):
    if not("sides_touched" in app.additional_data.keys()):
        app.additional_data["sides_touched"] = []
    sides_touched = app.additional_data["sides_touched"]
    side = touch_type_to_side[type]
    if not(value == 0):
        if (type > 5):
            connect_shape(src_ip, side)
        else:
            touch_registered = False
            for touch_dict in sides_touched:
                if (touch_dict["src_ip"] == src_ip):
                    touch_dict["sides"].append(side)
                    touch_registered = True
                    break
            if not(touch_registered):
                sides_touched.append(
                    {"src_ip": src_ip, "sides": [side]})
    else:
        if (type > 5):
            other_src_ip = app.structure_manager  # TODO find other src_ip in structure_manager
            disconnect_shape(src_ip, other_src_ip)
        else:
            for touch_dict in sides_touched:
                if (touch_dict["src_ip"] == src_ip):
                    while True:
                        touch_dict["sides"].remove(side)
                        if (not(side in sides_touched[src_ip])):
                            break
                    break
# async def get_value(shape_class: str, type: int, value: int):
#     await unitywebsocket.send_json(
#         {"type": "json_touch",
#          "ip": shape_class,
#          "side": type,
#          "value": value})


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
        websocket = app.connection_manager.active_connections[src_ip]
        app.connection_manager.send_json(payload, websocket)
        json_answer = await app.connection_manager.receive_json(websocket)

        response = {
            "status": json_answer["status"],
            "event": "actuate",
            "message": json_answer["message"]
        }
    except:
        response = {"status": False, "event": "error connecting nodes"}

    return response


if __name__ == '__main__':
    uvicorn.run(app, host="localhost", port=8000,
                ws_ping_interval=5, ws_ping_timeout=10)
