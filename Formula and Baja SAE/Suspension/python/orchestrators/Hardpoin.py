from __future__ import annotations

import math
from typing import Dict, Tuple, Any

from models.primitives import Point3D
from orchestrators.suspension import SuspensionOrchestrator, AlignmentOrchestrator
from orchestrators.Alinhament import KinematicsOrchestrator
from orchestrators.Cinematic import VehicleOrchestrator


# Helpers

def _deg(x_rad: float) -> float:
    return float(x_rad) * 180.0 / math.pi


def _atan2_deg(num: float, den: float) -> float:
    return _deg(math.atan2(num, den))


def _safe_get_hp(hp_dict: dict, key: str, default=(0.0, 0.0, 0.0)) -> Tuple[float, float, float]:
    x, y, z = hp_dict.get(key, default)
    return float(x), float(y), float(z)


def _p3(x: float, y: float, z: float) -> Point3D:
    return Point3D(float(x), float(y), float(z))


# Serialização (HP + presets)


class SerializationOrchestrator:
    """
    Organiza hardpoints/presets para salvar/carregar de forma consistente.
    (Não depende de Tkinter: é puro domínio.)
    """

    @staticmethod
    def hardpoints_to_dict(hp_dict: dict) -> Dict[str, Any]:
        out = {}
        for k, v in hp_dict.items():
            if isinstance(v, (tuple, list)) and len(v) == 3:
                out[k] = (float(v[0]), float(v[1]), float(v[2]))
            else:
                try:
                    out[k] = float(v)
                except Exception:
                    out[k] = v
        return out

    @staticmethod
    def hardpoints_from_dict(data: Dict[str, Any]) -> Dict[str, Any]:
        return dict(data)


# FACADE: hub com todos os orquestradores

class Orchestrators:
    """
    Um único ponto para importar no main.py:
        from orchestrators.Hardpoin import Orchestrators
        sus = Orchestrators.suspension.build_suspension_from_ui(...)
    """
    suspension = SuspensionOrchestrator
    alignment = AlignmentOrchestrator
    kinematics = KinematicsOrchestrator
    vehicle = VehicleOrchestrator
    serialize = SerializationOrchestrator
