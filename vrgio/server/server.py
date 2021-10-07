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
    component = [(src_ip, {"type": type, "component_class": component_class})]
    structure_manager.add_component(component)
    return {"status": True, "component_id": component[0][0], "event": "add"}


@app.get("/connect/components")
async def connect_shape(src_ip: str, side: str):
    structure_manager.add_connection(src_ip, side)
    return {"status": True, "event": "connect"}


@app.get("/component/count")
async def count_nodes():
    return {
        "status": True,
        "component_count": structure_manager.structure.number_of_nodes(),
        "event": "count",
    }


@app.get("/component/info")
async def get_info(src_ip: str):
    neighbors, component_class, type = structure_manager.get_neighbors(src_ip)
    return {
        "status": True,
        "neighbors": neighbors,
        "class": component_class,
        "type": type,
        "event": "info",
    }


@app.get("/export/graph")
async def save_graph():
    structure_manager.save_graph()
