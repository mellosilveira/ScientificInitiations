from dataclasses import dataclass


@dataclass
class SuspensionCGScanParameters:
    h_min: float
    h_max: float
    h_step: float
    mass: float
    ay: float
    track: float
    h_ro: float
    scrub_radius: float
    clearance: float


@dataclass
class SuspensionMassScanParameters:
    m_min: float
    m_max: float
    m_step: float
    h_cg: float
    ay: float
    track: float
    h_ro: float


@dataclass
class ForceAngleIteratorParameters:
    f_load: float
    angle_sup_base: float
    angle_inf_base: float
    k_sup: float
    k_inf: float
    limit: float
    ang_min: int
    ang_max: int
    step: int


class UIMapper:
    @staticmethod
    def map_to_cg_params(ui_data: dict, h_ro: float) -> SuspensionCGScanParameters:
        return SuspensionCGScanParameters(
            h_min=float(ui_data.get("h_min", 250)),
            h_max=float(ui_data.get("h_max", 650)),
            h_step=float(ui_data.get("step", 25)),
            mass=float(ui_data.get("mass", 260)),
            ay=float(ui_data.get("ay", 9.8)),
            track=float(ui_data.get("bf", 1250)),
            h_ro=h_ro,
            scrub_radius=float(ui_data.get("rs", 50)),
            clearance=0.0
        )

    @staticmethod
    def map_to_mass_params(ui_data: dict, h_ro: float) -> SuspensionMassScanParameters:
        return SuspensionMassScanParameters(
            m_min=float(ui_data.get("m_min", 180)),
            m_max=float(ui_data.get("m_max", 300)),
            m_step=float(ui_data.get("step", 25)),
            h_cg=float(ui_data.get("hcg", 450)),
            ay=float(ui_data.get("ay", 9.8)),
            track=float(ui_data.get("bf", 1250)),
            h_ro=h_ro
        )

    @staticmethod
    def map_to_structural_params(ui_data: dict, ang_sup: float, ang_inf: float) -> ForceAngleIteratorParameters:
        k_sup = float(ui_data.get("ksup", 0.5))
        return ForceAngleIteratorParameters(
            f_load=float(ui_data.get("load", 3000)),
            angle_sup_base=ang_sup,
            angle_inf_base=ang_inf,
            k_sup=k_sup,
            k_inf=1.0 - k_sup,
            limit=float(ui_data.get("limit", 8000)),
            ang_min=int(ui_data.get("amin", 0)),
            ang_max=int(ui_data.get("amax", 40)),
            step=5
        )
