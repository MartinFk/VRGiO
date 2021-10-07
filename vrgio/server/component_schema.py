from datetime import datetime
from typing import Dict
import uuid
from pydantic import BaseModel


class Component(BaseModel):
    id: str = str(uuid.uuid4())
    type = "base"
    component_class: str = "cube"
    connected_to: Dict = {
        "left": None,
        "right": None,
        "front": None,
        "back": None,
        "top": None,
        "bottom": None,
    }
