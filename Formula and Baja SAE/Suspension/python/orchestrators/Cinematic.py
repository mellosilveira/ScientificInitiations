from __future__ import annotations

import math
from dataclasses import dataclass
from typing import Dict, Tuple, Any, Optional, List

from models.primitives import Point3D
from models.component_structures import ThreePointArm, TwoPointLink3D
from models.suspension import Suspension


# =============================================================================
# Helpers
# =============================================================================

def _deg(x_rad: float) -> float:
    return float(x_rad) * 180.0 / math.pi


def _atan2_deg(num: float, den: float) -> float:
    return _deg(math.atan2(num, den))


def _safe_get_hp(hp_dict: dict, key: str, default=(0.0, 0.0, 0.0)) -> Tuple[float, float, float]:
    x, y, z = hp_dict.get(key, default)
    return float(x), float(y), float(z)


def _p3(x: float, y: float, z: float) -> Point3D:
    return Point3D(float(x), float(y), float(z))




#  Veículo (FRONT/REAR, 4 cantos)


@dataclass
class VehicleSnapshot:
    front_right: Optional[Suspension]
    front_left: Optional[Suspension]
    rear_right: Optional[Suspension]
    rear_left: Optional[Suspension]


class VehicleOrchestrator:
    """
    Monta as 4 suspensões do veículo a partir do dict de hardpoints da UI.
    """

    @staticmethod
    def build_vehicle_from_ui(hp_dict: dict, tire_diam: float = 584.2) -> VehicleSnapshot:
        def try_build(corner: str) -> Optional[Suspension]:
            try:
                return SuspensionOrchestrator.build_suspension_from_ui(hp_dict, corner, tire_diam=tire_diam)
            except Exception:
                return None

        return VehicleSnapshot(
            front_right=try_build("FR"),
            front_left=try_build("FL"),
            rear_right=try_build("RR"),
            rear_left=try_build("RL"),
        )

    @staticmethod
    def vehicle_alignment_snapshot(vehicle: VehicleSnapshot) -> Dict[str, Any]:
        out: Dict[str, Any] = {"corners": {}, "axles": {}}

        corner_map = {"FR": vehicle.front_right, "FL": vehicle.front_left, "RR": vehicle.rear_right, "RL": vehicle.rear_left}
        for c, sus in corner_map.items():
            if sus is None:
                out["corners"][c] = None
            else:
                out["corners"][c] = AlignmentOrchestrator.static_alignment_from_suspension(sus)

        def axle_avg(c1: str, c2: str) -> Optional[Dict[str, float]]:
            a1, a2 = out["corners"].get(c1), out["corners"].get(c2)
            if not a1 or not a2:
                return None
            return {
                "Caster (deg)": 0.5 * (a1["Caster (deg)"] + a2["Caster (deg)"]),
                "Camber (deg)": 0.5 * (a1["Camber (deg)"] + a2["Camber (deg)"]),
                "Toe (deg)": 0.5 * (a1["Toe (deg)"] + a2["Toe (deg)"]),
            }

        out["axles"]["FRONT"] = axle_avg("FR", "FL")
        out["axles"]["REAR"] = axle_avg("RR", "RL")
        return out





