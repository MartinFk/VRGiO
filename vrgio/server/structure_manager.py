from component_schema import Component
from typing import Tuple
import networkx as nx
from networkx import Graph
from networkx.drawing.nx_agraph import write_dot, graphviz_layout
import matplotlib.pyplot as plt


class StructureManager:
    def __init__(self):
        """
        Initializes a Bi-directional Graph and related variables
        """
        self.structure: Graph = Graph()

    def add_component(self, component: Tuple):
        """
        Adds new node i.e., cube to Graph, which gets
        connected in the shape structure physically.

        Args:
            component (Tuple): [description]
        """
        self.structure.add_nodes_from(component)

    def add_connection(self, node_one_ip: str, node_two_ip: str, side: str):
        """
        Etasblishes bi-directional connection in Graph between two nodes representing
        the physical components that got attached to each other.

        Args:
            node_one_ip (str): Node's IP which got a new node attached to it.
            node_two_ip (str): The new node's IP that got itself attached.
            side (str): Side at which cube got connected {left, right, up, down, front, back}
        """
        ## TODO: Validate if both nodes exist in the Graph
        ## TODO: Overwrite node connection for a given side
        self.structure.add_edge(node_one_ip, node_two_ip, side=side)

    def inspect_node(self, src_ip: str) -> Tuple:
        """
        Returns metadata about any given node using its
        IP address as the identifier. For inspecting any
        node for its neighbors and its own type.

        Args:
            src_ip (str): Unique IP address of that node.

        Returns:
            Tuple: Metadata associated with the given node.
        """
        return (
            self.structure[src_ip],
            nx.get_node_attributes(self.structure, "component_class")[src_ip],
            nx.get_node_attributes(self.structure, "type")[src_ip],
        )

    def visualize_graph(self):
        """
        Visualizes the entire Graph with all components shown
        """
        # same layout using matplotlib with no labels
        plt.title("VRGiO Shape")
        nx.draw_networkx(self.structure)
        plt.show()
