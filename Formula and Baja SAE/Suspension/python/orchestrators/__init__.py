"""Camada pública de orquestração da aplicação."""

from .suspension import SuspensionOrchestrator, AlignmentOrchestrator
from .kinematics import KinematicsOrchestrator
from .vehicle import VehicleOrchestrator, VehicleSnapshot
from .serialization import SerializationOrchestrator


class Orchestrators:
    """Fachada compatível para acesso centralizado aos orquestradores."""

    suspension = SuspensionOrchestrator
    alignment = AlignmentOrchestrator
    kinematics = KinematicsOrchestrator
    vehicle = VehicleOrchestrator
    serialize = SerializationOrchestrator


__all__ = [
    "SuspensionOrchestrator",
    "AlignmentOrchestrator",
    "KinematicsOrchestrator",
    "VehicleOrchestrator",
    "VehicleSnapshot",
    "SerializationOrchestrator",
    "Orchestrators",
]
