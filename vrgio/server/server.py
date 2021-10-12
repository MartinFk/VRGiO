from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from structure_manager import StructureManager

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
    return {"status": True, "component_id": component[0][0], "event": "add"}


@app.get("/connect/components")
async def connect_shape(node_one_ip: str, node_two_ip: str, side: str):
    """
    Etasblishes bi-directional connection in Graph between two nodes representing
    the physical components that got attached to each other.


    Args:
        node_one_ip (str): Node's IP which got a new node attached to it.
        node_two_ip (str): The new node's IP that got itself attached.
        side (str): Side at which cube got connected {left, right, up, down, front, back}

    Returns:
        response (Dict): Status about the operation
    """
    response = {"status": True, "event": "connect"}
    try:
        structure_manager.add_connection(node_one_ip, node_two_ip, side)
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


@app.get("/visualize/graph")
async def visualize_graph():
    """
    Visualizes the entire Graph with all components shown
    """
    structure_manager.visualize_graph()
    return {"status": True, "event": "export"}
