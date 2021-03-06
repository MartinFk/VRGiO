import datetime
import os
from ast import Assert
from typing import Dict, List, Optional, Tuple

import matplotlib.pyplot as plt
import networkx as nx
import requests
from networkx import DiGraph
from networkx.algorithms.assortativity import neighbor_degree
from networkx.drawing.nx_agraph import graphviz_layout, write_dot

from component_schema import Component

"""
This map is useful when for instance a cube gets connected to left and you want
to establish a bi-directional connection, hence the reverse edge would be named
as right, this map will act as a quick lookup in that instance.
"""
TIMEOUT_THRESHOLD = 5000


class StructureManager:
    def __init__(self):
        """
        Initializes a Bi-directional Graph and related variables
        """
        self.structure: DiGraph = DiGraph()
        self.last_connected: str = None
        self.last_connected_side: str = None
        self.last_connected_timeout: str = None

    def add_component(self, component: Tuple):
        """
        Adds new node i.e., cube to Graph, which gets
        connected in the shape structure physically.

        Args:
            component (Tuple): [description]
        """
        self.structure.add_nodes_from(component)

    def add_connection(self, node_ip: str, side: str) -> None:
        """
        Etasblishes bi-directional connection in Graph between two nodes representing
        the physical components that got attached to each other.

        Args:
            node_ip (str): Node's IP which got a new node attached to it.
            node_two_ip (str): The new node's IP that got itself attached.
            side (str): Side at which cube got connected {left, right, up, down, front, back}
        """
        if (
            self.last_connected_timeout
            and datetime.datetime.now() > self.last_connected_timeout
        ):
            (
                self.last_connected,
                self.last_connected_side,
                self.last_connected_timeout,
            ) = (None, None, None)

        if not self.last_connected:
            self.last_connected = node_ip
            self.last_connected_side = side
            self.last_connected_timeout = datetime.datetime.now() + datetime.timedelta(
                milliseconds=TIMEOUT_THRESHOLD
            )

        ## constrain self loop
        elif (
            node_ip != self.last_connected
            and datetime.datetime.now() < self.last_connected_timeout
        ):
            ## check if there's a node on the `side` already present
            ## if present then remove it for both nodes
            self.connectivity_sanity_check(node_ip, side)
            self.connectivity_sanity_check(
                self.last_connected, self.last_connected_side
            )

            ## make newer connection
            self.structure.add_edge(node_ip, self.last_connected, side=side)
            self.structure.add_edge(
                self.last_connected, node_ip, side=self.last_connected_side
            )

            (
                self.last_connected,
                self.last_connected_side,
                self.last_connected_timeout,
            ) = (None, None, None)

    def connectivity_sanity_check(self, src_ip: str, side: str):
        """
        Performs a sanity check for a node whether it has a node already
        connected to the side a new node has requested to be attached to.

        Args:
            src_ip (str): IP address of the node being sanity checked.
            side (str): requested side for node attachment
        """
        ## get neighbor nodes and their IPs
        neigbor_nodes: Dict = dict(self.structure[src_ip])
        neighbor_nodes_ip: List = list(neigbor_nodes.keys())

        ## iterate over node and delete connection for the side if exists
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

    def cleanse_component_from_structure(self, src_ip: str):
        if (src_ip in self.structure.nodes):
            neigbor_nodes: Dict = dict(self.structure[src_ip])
            neighbor_nodes_ip: List = list(neigbor_nodes.keys())

            for neighbor_ip in neighbor_nodes_ip:
                self.remove_connection(src_ip, neighbor_ip)
            self.structure.remove_node(src_ip)

    def change_touch(self, src_ip: str, side: str, value: bool):
        if (src_ip in self.structure.nodes):
            node = self.structure.nodes.pop(src_ip)
            print(node)