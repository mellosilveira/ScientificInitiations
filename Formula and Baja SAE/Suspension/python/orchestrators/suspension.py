from __future__ import annotations

import math
from types import SimpleNamespace
from typing import Optional

from models.primitives import Point3D, Vector3D
from models.component_structures import ThreePointArm, TwoPointLink3D
from models.suspension import Suspension
from models.lines import LineCoefficients3D
from models.numerical import SolverParameters
from models.constants import EPSILON


# ---------------------------------------------------------------------------
# Internal helpers
# ---------------------------------------------------------------------------

def _safe_float(v, default: float = 0.0) -> float:
    try:
        return float(v)
    except (TypeError, ValueError):
        return default


def _extract_hp_tuple(ui_data: dict, key: str):
    """Return (x, y, z) floats from a hardpoint entry regardless of format."""
    v = ui_data.get(key, (0.0, 0.0, 0.0))
    if isinstance(v, dict):
        return float(v.get("x", 0)), float(v.get("y", 0)), float(v.get("z", 0))
    return float(v[0]), float(v[1]), float(v[2])


def _pt(ui_data: dict, corner: str, name: str) -> Point3D:
    x, y, z = _extract_hp_tuple(ui_data, f"{corner} {name}")
    return Point3D(x, y, z)


def _vehicle_center_line() -> LineCoefficients3D:
    """Vertical line at X=0 (vehicle symmetry plane)."""
    return LineCoefficients3D(origin=Point3D(0, 0, 0), direction=Vector3D(0, 1, 0))


# ---------------------------------------------------------------------------
# SuspensionOrchestrator
# ---------------------------------------------------------------------------

class SuspensionOrchestrator:
    """
    Bridge between the UI dictionaries and the 3D domain models.

    Hard-point dict format (from _collect_preset):
        ui_data["FR Sup In"] = (x, y, z)   # tuple of floats
        ui_data["bf"]        = "1250"       # string scalar
    """

    @staticmethod
    def build_suspension_from_ui(
        ui_data: dict,
        corner: str,
        tire_diam: float = 584.2,
    ) -> Suspension:
        """
        Build a 3D Suspension object from the UI hard-points dictionary.

        The UI stores a single inboard point per arm (e.g. "Sup In").
        A ThreePointArm requires two chassis pivots (fore / aft).  We derive
        them by offsetting the single inner point ±10 mm in Z so the inner
        axis direction is non-zero while the centroid stays at the original
        point.
        """

        def pt(name: str) -> Point3D:
            return _pt(ui_data, corner, name)

        def _arm(in_name: str, out_name: str) -> ThreePointArm:
            inner = pt(in_name)
            outer = pt(out_name)
            return ThreePointArm(
                fore_inner=Point3D(inner.x, inner.y, inner.z + 10.0),
                aft_inner=Point3D(inner.x, inner.y, inner.z - 10.0),
                outer=outer,
            )

        upper = _arm("Sup In", "Sup Out")
        lower = _arm("Inf In", "Inf Out")

        sup_out = pt("Sup Out")
        inf_out = pt("Inf Out")
        toe_in = pt("Toe In")
        toe_out = pt("Toe Out")

        # Derive camber_gap from kingpin inclination angle projected in X-Y plane.
        kp_x = sup_out.x - inf_out.x
        kp_y = sup_out.y - inf_out.y
        kpi_rad = math.atan2(kp_x, -kp_y) if abs(kp_y) > EPSILON else 0.0
        camber_gap = math.tan(kpi_rad) * tire_diam

        # Derive toe_distance from tie-rod direction projected in X-Z plane.
        tie_dx = toe_out.x - toe_in.x
        tie_dz = toe_out.z - toe_in.z
        toe_rad = math.atan2(tie_dx, tie_dz) if abs(tie_dz) > EPSILON else 0.0
        toe_distance = math.tan(toe_rad) * tire_diam

        # Tire contact patch: ground projection (Y=0) of lower ball joint.
        tire_contact = Point3D(inf_out.x, 0.0, inf_out.z)

        return Suspension(
            upper_arm=upper,
            lower_arm=lower,
            tie_rod=TwoPointLink3D(inner=toe_in, outer=toe_out),
            damper=TwoPointLink3D(
                inner=pt("Damper In"),
                outer=pt("Damper Out"),
            ),
            tire_contact=tire_contact,
            tire_diameter=tire_diam,
            camber_gap=camber_gap,
            toe_distance=toe_distance,
            acceleration_on_shaft=False,
            brake_on_shaft=True,
        )

    @staticmethod
    def get_static_alignment(sus: Suspension) -> dict:
        """Return Caster, Camber, and Toe in degrees from 3D geometry."""
        kp = sus.kingpin_vector
        kp_y = kp.y

        kpi_rad = math.atan2(kp.x, -kp_y) if abs(kp_y) > EPSILON else 0.0
        caster_rad = math.atan2(kp.z, -kp_y) if abs(kp_y) > EPSILON else 0.0

        tr = sus.tie_rod
        dx = tr.outer.x - tr.inner.x
        dz = tr.outer.z - tr.inner.z
        toe_rad = math.atan2(dx, dz) if abs(dz) > EPSILON else 0.0

        return {
            "Caster (deg)": math.degrees(caster_rad),
            "Camber (deg)": math.degrees(kpi_rad),
            "Toe (deg)": math.degrees(toe_rad),
        }

    # ------------------------------------------------------------------
    # Analysis methods called by the UI action buttons
    # ------------------------------------------------------------------

    @staticmethod
    def run_2d_analysis(ui_data: dict, axis: str) -> dict:
        """
        Compute roll center geometry for one axle (FRONT or REAR).

        Returns:
            {
                "h_ro": float,                   # average roll centre height [mm]
                "corners": {corner: RollCenterResult | None},
                "suspensions": {corner: Suspension | None},
            }
        """
        from calculators.suspension_parameters import calculate_roll_center_parameters

        bf = _safe_float(ui_data.get("bf", 1250))
        tire_diam = _safe_float(ui_data.get("tire_diam", 584.2))
        corners = ("FR", "FL") if axis.upper() == "FRONT" else ("RR", "RL")
        vcl = _vehicle_center_line()

        corner_rc: dict = {}
        corner_sus: dict = {}
        for c in corners:
            try:
                sus = SuspensionOrchestrator.build_suspension_from_ui(ui_data, c, tire_diam)
                rc = calculate_roll_center_parameters(sus, vcl, bf)
                corner_rc[c] = rc
                corner_sus[c] = sus
            except Exception:
                corner_rc[c] = None
                corner_sus[c] = None

        h_vals = [
            r.roll_center_point.y
            for r in corner_rc.values()
            if r is not None and r.roll_center_point is not None
        ]
        h_ro = sum(h_vals) / len(h_vals) if h_vals else 0.0

        return {"h_ro": h_ro, "corners": corner_rc, "suspensions": corner_sus}

    @staticmethod
    def run_full_3d_analysis(ui_data: dict, corner: str) -> dict:
        """
        Compute all suspension parameters for one corner.

        Returns a dict with keys:
            "metrics"         AlignmentMetricsResult
            "steering"        SteeringMetricsResult | None
            "camber_gain"     (static_rad, new_rad, gain_deg_per_mm) | None
            "corner"          str
            "suspension"      Suspension
        """
        from calculators.suspension_parameters import (
            calculate_alignment_metrics,
            calculate_steering_metrics,
            calculate_camber_gain,
        )

        bf = _safe_float(ui_data.get("bf", 1250))
        wb = _safe_float(ui_data.get("wb", 1550))
        hcg = _safe_float(ui_data.get("hcg", 450))
        tire_diam = 584.2

        sus = SuspensionOrchestrator.build_suspension_from_ui(ui_data, corner, tire_diam)
        vcl = _vehicle_center_line()
        cg = Point3D(0.0, hcg, 0.0)

        # Front corners carry ~70% of braking, rear 30%.
        brake_pct = 0.7 if corner.upper().startswith("F") else 0.3

        metrics = calculate_alignment_metrics(sus, vcl, cg, wb, bf, brake_pct)

        # Ackermann / steering geometry (front corners only).
        steering = None
        if corner.upper().startswith("F"):
            try:
                steering = calculate_steering_metrics(sus, wb, math.radians(15.0))
            except Exception:
                pass

        # 4-bar kinematic camber gain for 20 mm bump.
        camber_gain = None
        try:
            sp = SolverParameters(
                min_val=-0.5, max_val=0.5,
                tolerance=1e-6, max_iter=200, method="brentq",
            )
            camber_gain = calculate_camber_gain(sus, 20.0, sp)
        except Exception:
            pass

        return {
            "metrics": metrics,
            "steering": steering,
            "camber_gain": camber_gain,
            "corner": corner,
            "suspension": sus,
        }

    @staticmethod
    def run_kinematic_sweep(
        ui_data: dict,
        corner: str,
        travel_min: float = -40.0,
        travel_max: float = 40.0,
        n_steps: int = 17,
    ) -> list:
        """
        Sweep wheel travel and return camber gain at each step.

        Returns list of dicts: {travel_mm, static_camber_deg, new_camber_deg, gain_deg_per_mm}
        """
        from calculators.suspension_parameters import calculate_camber_gain

        tire_diam = 584.2
        sus = SuspensionOrchestrator.build_suspension_from_ui(ui_data, corner, tire_diam)

        step = (travel_max - travel_min) / max(1, n_steps - 1)
        sp = SolverParameters(
            min_val=-0.8, max_val=0.8,
            tolerance=1e-7, max_iter=300, method="brentq",
        )

        results = []
        t = travel_min
        for _ in range(n_steps):
            try:
                static_rad, new_rad, gain = calculate_camber_gain(sus, t, sp)
                results.append({
                    "travel_mm": t,
                    "static_camber_deg": math.degrees(static_rad),
                    "new_camber_deg": math.degrees(new_rad),
                    "gain_deg_per_mm": gain,
                })
            except Exception:
                results.append({
                    "travel_mm": t,
                    "static_camber_deg": 0.0,
                    "new_camber_deg": 0.0,
                    "gain_deg_per_mm": 0.0,
                })
            t += step

        return results

    @staticmethod
    def run_cg_sweep(ui_data: dict, h_ro: float) -> list:
        """
        Sweep CG height and compute lateral load transfer metrics.

        Returns list of SimpleNamespace(h_cg, d_fz, fz_int, m_roll).
        """
        mass = _safe_float(ui_data.get("mass", 260))
        ay = _safe_float(ui_data.get("ay", 9.8))
        bf = _safe_float(ui_data.get("bf", 1250))
        h_min = _safe_float(ui_data.get("h_min", 250))
        h_max = _safe_float(ui_data.get("h_max", 650))
        step = max(1.0, _safe_float(ui_data.get("step", 25)))

        results = []
        h = h_min
        while h <= h_max + 1e-9:
            # Lateral load transfer per axle (simplified):  ΔFz = m·ay·h / bf
            d_fz = (mass * ay * h) / bf
            # Internal vertical force on inner tyre (quasi-static)
            fz_int = (mass * 9.81 / 2.0) - d_fz
            # Roll moment about roll axis
            m_roll = mass * ay * (h - h_ro)
            results.append(SimpleNamespace(
                h_cg=h, d_fz=d_fz, fz_int=fz_int, m_roll=m_roll,
            ))
            h += step

        return results

    @staticmethod
    def run_mass_sweep(ui_data: dict, h_ro: float) -> list:
        """
        Sweep vehicle mass and compute stability metrics.

        Returns list of dicts with keys:
            mass, m_roll, d_fz, ssf, ay_crit, margin
        """
        h_cg = _safe_float(ui_data.get("hcg", 450))
        ay = _safe_float(ui_data.get("ay", 9.8))
        bf = _safe_float(ui_data.get("bf", 1250))
        m_min = _safe_float(ui_data.get("m_min", 180))
        m_max = _safe_float(ui_data.get("m_max", 300))
        step = max(1.0, _safe_float(ui_data.get("step", 25)))

        # Static Stability Factor is geometry-only, independent of mass.
        ssf = (bf / 2.0) / h_cg if h_cg > EPSILON else 0.0
        ay_crit = ssf * 9.81
        margin = ay_crit / ay if ay > EPSILON else 0.0

        results = []
        m = m_min
        while m <= m_max + 1e-9:
            d_fz = (m * ay * h_cg) / bf
            m_roll = m * ay * (h_cg - h_ro)
            results.append({
                "mass": m,
                "m_roll": m_roll,
                "d_fz": d_fz,
                "ssf": ssf,
                "ay_crit": ay_crit,
                "margin": margin,
            })
            m += step

        return results

    @staticmethod
    def run_structural_optimization(ui_data: dict, auto_angles: int) -> list:
        """
        Sweep arm angle and compute wishbone axial forces.

        The simplified model resolves a vertical wheel load into upper and lower
        wishbone axial loads via force equilibrium:

            F_sup · sin(θ_sup) + F_inf · sin(θ_inf) = F_load
            F_sup = k_sup · F_total,  F_inf = k_inf · F_total
        """
        load = _safe_float(ui_data.get("load", 3000))
        limit = _safe_float(ui_data.get("limit", 8000))
        k_sup = _safe_float(ui_data.get("ksup", 0.5))
        k_inf = 1.0 - k_sup
        ang_min = int(_safe_float(ui_data.get("amin", 0)))
        ang_max = int(_safe_float(ui_data.get("amax", 40)))
        delta_step = 5

        if auto_angles:
            corner = ui_data.get("_corner", "FR")
            try:
                sup_in = _extract_hp_tuple(ui_data, f"{corner} Sup In")
                sup_out = _extract_hp_tuple(ui_data, f"{corner} Sup Out")
                inf_in = _extract_hp_tuple(ui_data, f"{corner} Inf In")
                inf_out = _extract_hp_tuple(ui_data, f"{corner} Inf Out")
                ang_sup = math.degrees(math.atan2(
                    abs(sup_out[1] - sup_in[1]),
                    abs(sup_out[0] - sup_in[0]),
                ))
                ang_inf = math.degrees(math.atan2(
                    abs(inf_out[1] - inf_in[1]),
                    abs(inf_out[0] - inf_in[0]),
                ))
            except Exception:
                ang_sup = _safe_float(ui_data.get("ang_sup", 10))
                ang_inf = _safe_float(ui_data.get("ang_inf", 20))
        else:
            ang_sup = _safe_float(ui_data.get("ang_sup", 10))
            ang_inf = _safe_float(ui_data.get("ang_inf", 20))

        results = []
        for delta in range(ang_min, ang_max + 1, delta_step):
            theta_s = math.radians(ang_sup + delta)
            theta_i = math.radians(ang_inf + delta)
            denom = k_sup * math.sin(theta_s) + k_inf * math.sin(theta_i)
            if abs(denom) < EPSILON:
                continue
            f_total = load / denom
            f_sup = abs(k_sup * f_total)
            f_inf = abs(k_inf * f_total)
            status = "OK" if max(f_sup, f_inf) < limit else "EXCEDE"
            results.append({
                "angle_delta": delta,
                "force_sup": f_sup,
                "force_inf": f_inf,
                "status": status,
            })

        return results


# ---------------------------------------------------------------------------
# AlignmentOrchestrator
# ---------------------------------------------------------------------------

class AlignmentOrchestrator:
    """Provides static alignment metrics from a Suspension model."""

    @staticmethod
    def static_alignment_from_suspension(sus: Suspension) -> dict:
        return SuspensionOrchestrator.get_static_alignment(sus)
