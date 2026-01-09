SUMMARY_2D = """
=== 2D GEOMETRY SUMMARY ===
A) Front View Logic:
- IC (Instant Center): Intersection of upper and lower arm lines. Determines the pivot point of the suspension in this view.
- Roll Center (Ro): Intersection of the line IC-W (FVSA) with the vehicle centerline (x=0). This point determines the geometric roll moment arm.
- q-factor: Reimpell's curvature metric. Used to compare how much the roll center migrates during movement.

B) Camber Gain:
- dφ: Roll angle derived from suspension travel.
- kγ: Camber factor. Negative values indicate the geometry compensates for roll (the tire leans into the turn), which improves grip.
"""

SUMMARY_3D = """
=== 3D FORCE SUMMARY ===
A) Force Distribution:
- The longitudinal force (Fx_tire, e.g., Braking) is distributed to the arms based on stiffness ratios.
- Geometric Effect: Inclined arms generate parasitic Vertical (Fy) and Lateral (Fx) forces on the chassis when reacting to braking loads.

B) Anti-Dive:
- Calculated as (Fy_suspension / Fz_needed_for_100%) * 100.
- Determines how much the suspension geometry resists diving under braking (nose dip).

C) Alignment:
- Camber/Caster/Toe are calculated by projecting 3D points onto 2D planes.
"""

SUMMARY_BAJA = """
=== BAJA DYNAMICS SUMMARY ===
A) Trends (CG Variation):
- Increasing CG height linearly increases Load Transfer (ΔFz) and Roll Moment.
- Critical Risk: If Fz_int (Internal Wheel Load) drops near zero, the inner wheel lifts off the ground, causing loss of traction and stability control.

B) Spindle Moment (Steering Feedback):
- Depends on Lateral Force and Scrub Radius.
- Large scrub radius + high lateral g = heavy steering effort for the driver.
"""