from networkx.algorithms.assortativity import neighbor_degree
from component_schema import Component
from typing import List, Tuple, Optional, Dict
import networkx as nx
from networkx import DiGraph
from networkx.drawing.nx_agraph import write_dot, graphviz_layout
import matplotlib.pyplot as plt
import requests

"""
This map is useful when for instance a cube gets connected to left and you want
to establish a bi-directional connection, hence the reverse edge would be named
as right, this map will act as a quick lookup in that instance.
"""
SIDES_MAP = {
    "left": "right",
    "right": "left",
    "up": "down",
    "down": "up",
    "front": "back",
    "back": "front",
}


class StructureManager:
    def __init__(self):
        """
        Initializes a Bi-directional Graph and related variables
        """
        self.structure: DiGraph = DiGraph()

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
        ## TODO: Check if same IP pairs have different sides connected
        ## TODO: Constrain self loop

        ## check if there's a node on the `side` already present
        ## if present then remove it for both nodes
        self.connectivity_sanity_check(node_one_ip, side)
        self.connectivity_sanity_check(node_two_ip, SIDES_MAP[side])

        ## make newer connection
        self.structure.add_edge(node_one_ip, node_two_ip, side=side)
        self.structure.add_edge(node_two_ip, node_one_ip, side=SIDES_MAP[side])

    def connectivity_sanity_check(self, src_ip, side):
        neigbor_nodes: Dict = dict(self.structure[src_ip])
        neighbor_nodes_ip: List = list(neigbor_nodes.keys())

        for neighbor_node_ip in neighbor_nodes_ip:
            if neigbor_nodes[neighbor_node_ip]["side"] == side:
                self.remove_connection(src_ip, neighbor_node_ip)

    def remove_connection(self, node_one_ip: str, node_two_ip: str) -> None:
        """
        Removes bi-directional connection in Graph between two nodes representing
        the physical components that got attached to each other.

        Args:
            node_one_ip (str): Node's IP which got a new node attached to it.
            node_two_ip (str): The new node's IP that got itself attached.
        """
        deletion_edges = [(node_one_ip, node_two_ip), (node_two_ip, node_one_ip)]
        self.structure.remove_edges_from(deletion_edges)

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

    def actuate(self, src_ip: str, payload: Optional[Dict]) -> bool:
        """
        Actuates the physical component or performs message passing from
        one component to another.

         Args:
             src_ip (str): Unique IP address of that node.
             payload (Dict): Message or actuation details.

         Returns:
             status (bool): Metadata associated with the given node.
        """
        status = True
        try:
            requests.get(f"http://{src_ip}/actuate")
        except:
            status = False
        return status

    def visualize_graph(self) -> None:
        """
        Visualizes the entire Graph with all components shown
        """
        # same layout using matplotlib with no labels
        fig = plt.figure(figsize=(12, 12))
        plt.title("VRGiO Shape")
        nx.draw_networkx(self.structure)
        ## plt.show()
        plt.savefig("vrgio_graph.png", format="PNG")
