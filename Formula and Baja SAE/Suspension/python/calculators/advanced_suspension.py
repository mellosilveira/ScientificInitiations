"""Cálculos avançados de suspensão para o MudRunner Engineering Suite.
As rotinas são determinísticas, sem dependência da interface, e retornam dicionários serializáveis.
Unidades padrão: mm, kg, N, graus e segundos.
"""
from __future__ import annotations
from dataclasses import dataclass, asdict
from math import atan, atan2, degrees, radians, sin, cos, sqrt, tan
from typing import Iterable

EPS=1e-9

def _linspace(a:float,b:float,n:int):
    n=max(2,int(n)); step=(b-a)/(n-1); return [a+i*step for i in range(n)]

def ackermann_ideal(inner_deg:float, wheelbase_mm:float, track_mm:float)->float:
    """Ângulo externo ideal para uma entrada de ângulo interno."""
    d=radians(abs(inner_deg))
    if d<EPS or wheelbase_mm<=0: return 0.0
    radius=wheelbase_mm/tan(d)+track_mm/2.0
    return degrees(atan(wheelbase_mm/max(EPS, radius+track_mm/2.0)))

def ackermann_sweep(wheelbase_mm:float, track_mm:float, max_inner_deg:float=35.0, steps:int=15,
                    steering_arm_factor:float=0.94):
    """Modelo paramétrico inicial. steering_arm_factor=1 aproxima Ackermann ideal.
    O relatório marca que o resultado real deve ser substituído por solver de hardpoints quando disponível.
    """
    rows=[]
    for inner in _linspace(0,max_inner_deg,steps):
        ideal=ackermann_ideal(inner,wheelbase_mm,track_mm)
        real=ideal*steering_arm_factor
        pct=100.0 if ideal<EPS else 100.0*real/ideal
        rows.append({'inner_deg':inner,'outer_ideal_deg':ideal,'outer_real_deg':real,
                     'ackermann_pct':pct,'error_deg':real-ideal})
    return rows

def bump_rebound_sweep(travel_min_mm=-60.0, travel_max_mm=60.0, steps=25,
                       static_camber_deg=-1.0, camber_gain_deg_per_mm=-0.025,
                       static_toe_deg=0.0, bump_steer_deg_per_mm=0.002,
                       static_caster_deg=5.0, caster_gain_deg_per_mm=0.003,
                       static_kpi_deg=8.0, kpi_gain_deg_per_mm=0.001,
                       static_scrub_mm=35.0, scrub_gain_mm_per_mm=-0.02,
                       static_trail_mm=22.0, trail_gain_mm_per_mm=0.01,
                       track_change_mm_per_mm=0.02, wheelbase_change_mm_per_mm=-0.01,
                       static_rc_mm=60.0, rc_gain_mm_per_mm=0.15,
                       motion_ratio_static=0.72, motion_ratio_progressivity=0.001):
    rows=[]
    for t in _linspace(travel_min_mm,travel_max_mm,steps):
        mr=max(0.05,motion_ratio_static+motion_ratio_progressivity*t)
        rows.append({
            'travel_mm':t,'state':'BUMP' if t>EPS else ('REBOUND' if t<-EPS else 'STATIC'),
            'camber_deg':static_camber_deg+camber_gain_deg_per_mm*t,
            'toe_deg':static_toe_deg+bump_steer_deg_per_mm*t,
            'caster_deg':static_caster_deg+caster_gain_deg_per_mm*t,
            'kpi_deg':static_kpi_deg+kpi_gain_deg_per_mm*t,
            'scrub_radius_mm':static_scrub_mm+scrub_gain_mm_per_mm*t,
            'mechanical_trail_mm':static_trail_mm+trail_gain_mm_per_mm*t,
            'track_change_mm':track_change_mm_per_mm*t,
            'wheelbase_change_mm':wheelbase_change_mm_per_mm*t,
            'roll_center_mm':static_rc_mm+rc_gain_mm_per_mm*t,
            'motion_ratio':mr,'damper_travel_mm':t*mr,
        })
    return rows

def spring_rates(spring_rate_n_per_mm:float, motion_ratio:float, tire_rate_n_per_mm:float=180.0,
                 sprung_mass_corner_kg:float=65.0):
    wheel_rate=spring_rate_n_per_mm*motion_ratio**2
    ride_rate=(wheel_rate*tire_rate_n_per_mm)/(wheel_rate+tire_rate_n_per_mm) if wheel_rate+tire_rate_n_per_mm>EPS else 0
    freq=(1/(2*3.141592653589793))*sqrt(max(0,ride_rate*1000/max(EPS,sprung_mass_corner_kg)))
    return {'spring_rate_n_per_mm':spring_rate_n_per_mm,'motion_ratio':motion_ratio,
            'wheel_rate_n_per_mm':wheel_rate,'ride_rate_n_per_mm':ride_rate,'natural_frequency_hz':freq}

def roll_stiffness_distribution(front_wheel_rate:float,rear_wheel_rate:float,front_track_mm:float,rear_track_mm:float,
                                front_arb_nmm_per_rad:float=0,rear_arb_nmm_per_rad:float=0):
    kf=front_wheel_rate*(front_track_mm/1000)**2/2+front_arb_nmm_per_rad/1000
    kr=rear_wheel_rate*(rear_track_mm/1000)**2/2+rear_arb_nmm_per_rad/1000
    total=kf+kr
    return {'front_roll_stiffness_nm_per_rad':kf,'rear_roll_stiffness_nm_per_rad':kr,
            'front_distribution_pct':100*kf/total if total>EPS else 0,'rear_distribution_pct':100*kr/total if total>EPS else 0}

def anti_geometry(cg_height_mm:float,wheelbase_mm:float,side_view_ic_height_mm:float,side_view_ic_distance_mm:float,
                  brake_bias_front:float=0.6,drive_axle_share:float=1.0):
    slope=side_view_ic_height_mm/max(EPS,abs(side_view_ic_distance_mm))
    base=100*slope*wheelbase_mm/max(EPS,cg_height_mm)
    return {'anti_dive_pct':base*brake_bias_front,'anti_squat_pct':base*drive_axle_share,
            'anti_lift_pct':base*(1-brake_bias_front)}

def lateral_load_transfer(mass_kg:float,ay_mps2:float,cg_height_mm:float,track_mm:float,front_share:float=0.55):
    total=mass_kg*ay_mps2*(cg_height_mm/1000)/max(EPS,track_mm/1000)
    return {'total_transfer_n':total,'front_transfer_n':total*front_share,'rear_transfer_n':total*(1-front_share)}

def jacking_force(lateral_force_n:float,roll_center_height_mm:float,half_track_mm:float):
    return lateral_force_n*roll_center_height_mm/max(EPS,half_track_mm)

def tolerance_monte_carlo(n:int=1000, nominal:float=0.0, tolerance:float=1.0, sensitivity:float=1.0, seed:int=42):
    import random, statistics
    rng=random.Random(seed); vals=[nominal+sensitivity*rng.uniform(-tolerance,tolerance) for _ in range(max(10,n))]
    return {'samples':len(vals),'mean':statistics.mean(vals),'stdev':statistics.pstdev(vals),'min':min(vals),'max':max(vals)}
