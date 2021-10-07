from component_schema import Component
from typing import Tuple
import networkx as nx
from networkx import DiGraph
from networkx.drawing.nx_agraph import write_dot, graphviz_layout
import matplotlib.pyplot as plt


class StructureManager:
    def __init__(self):
        self.structure: DiGraph = DiGraph()
        self.last_node_id = None

    def add_component(self, component: Tuple):
        self.structure.add_nodes_from(component)
        self.last_node_id = component[0][0]

    def add_connection(self, src_ip: str, side: str):
        self.structure.add_edge(src_ip, self.last_node_id, side=side)
        self.last_node_id = None

    def get_neighbors(self, src_ip: str):
        return (
            self.structure[src_ip],
            nx.get_node_attributes(self.structure, "component_class")[src_ip],
            nx.get_node_attributes(self.structure, "type")[src_ip],
        )

    def save_graph(self):
        # same layout using matplotlib with no labels
        plt.title("VRGiO Shape")
        nx.draw_networkx(self.structure)
        plt.show()
