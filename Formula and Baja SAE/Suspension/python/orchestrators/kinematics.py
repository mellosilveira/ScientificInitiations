from __future__ import annotations

import math
from typing import Dict, Tuple, List

from models.primitives import Point3D
from models.component_structures import ThreePointArm, TwoPointLink3D
from models.suspension import Suspension
from orchestrators.suspension import AlignmentOrchestrator


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


# Cinemática (varreduras)


class KinematicsOrchestrator:
    """
    Varredura de curso (bump/rebound) e captura de camber/caster/toe.

    Depende do domínio Suspension ter um dos seguintes métodos:
      - sus.clone() e sus.apply_bump(mm)
      - sus.solve_kinematics(bump_mm)

    Se nenhum existir, levanta NotImplementedError.
    """

    @staticmethod
    def sweep_alignment_vs_travel(
        sus: Suspension,
        travel_list_mm: List[float],
        *,
        clone_method: str = "clone",
        apply_method: str = "apply_bump",
        solve_method: str = "solve_kinematics",
    ) -> List[Dict[str, float]]:
        results = []

        for t in travel_list_mm:
            # 1) clone
            if hasattr(sus, clone_method) and callable(getattr(sus, clone_method)):
                s = getattr(sus, clone_method)()
            else:
                s = sus

            # 2) aplicar curso
            if hasattr(s, apply_method) and callable(getattr(s, apply_method)):
                getattr(s, apply_method)(float(t))
            elif hasattr(s, solve_method) and callable(getattr(s, solve_method)):
                getattr(s, solve_method)(float(t))
            else:
                raise NotImplementedError(
                    "Seu domínio Suspension não tem métodos de cinemática.\n"
                    f"Implemente um destes:\n"
                    f"  - {clone_method}() + {apply_method}(travel_mm)\n"
                    f"  - {solve_method}(travel_mm)\n"
                )

            # 3) ler alinhamento
            al = AlignmentOrchestrator.static_alignment_from_suspension(s)
            results.append({
                "travel_mm": float(t),
                "camber_deg": float(al.get("Camber (deg)", 0.0)),
                "caster_deg": float(al.get("Caster (deg)", 0.0)),
                "toe_deg": float(al.get("Toe (deg)", 0.0)),
            })

        return results
