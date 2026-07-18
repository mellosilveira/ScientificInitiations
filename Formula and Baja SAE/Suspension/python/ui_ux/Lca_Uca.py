# visualization.py
from __future__ import annotations

import math
from dataclasses import dataclass
from typing import Iterable, List, Optional

# tenta usar Point3D do seu projeto
try:
    from models import Point3D
except Exception:
    @dataclass
    class Point3D:
        x: float
        y: float
        z: float


def plot_suspension_link(ax, p1: Point3D, p2: Point3D, color="k", style="-", lw: float = 2.0, alpha: float = 0.95):
    """
    Seu app está plotando (x, z, y) para ficar:
      X = lateral
      Z = longitudinal
      Y = vertical
    Então mantemos isso aqui.
    """
    ax.plot([p1.x, p2.x], [p1.z, p2.z], [p1.y, p2.y],
            linestyle=style, linewidth=lw, alpha=alpha, color=color)


def draw_ground_mesh(ax, step: float = 50.0, extent: float = 1000.0):
    """
    Desenha uma malha no "chão" (Y=0) no plano X-Z.
    """
    step = max(5.0, float(step))
    extent = max(step * 5.0, float(extent))

    # linhas paralelas ao eixo Z (varia X)
    x = -extent
    while x <= extent + 1e-9:
        ax.plot([x, x], [-extent, extent], [0, 0], linewidth=0.6, alpha=0.35, color="gray")
        x += step

    # linhas paralelas ao eixo X (varia Z)
    z = -extent
    while z <= extent + 1e-9:
        ax.plot([-extent, extent], [z, z], [0, 0], linewidth=0.6, alpha=0.35, color="gray")
        z += step


def autoscale_3d_equal(ax, points: Iterable[Point3D], pad: float = 0.08):
    """
    Ajusta limites 3D com escala igual em X/Z/Y (usando X, Z e Y do teu plot).
    """
    pts = list(points)
    if not pts:
        return

    xs = [p.x for p in pts]
    ys = [p.y for p in pts]
    zs = [p.z for p in pts]

    xmin, xmax = min(xs), max(xs)
    ymin, ymax = min(ys), max(ys)
    zmin, zmax = min(zs), max(zs)

    cx = 0.5 * (xmin + xmax)
    cy = 0.5 * (ymin + ymax)
    cz = 0.5 * (zmin + zmax)

    dx = (xmax - xmin)
    dy = (ymax - ymin)
    dz = (zmax - zmin)

    r = 0.5 * max(dx, dy, dz)
    r = r * (1.0 + float(pad)) + 1e-9

    ax.set_xlim(cx - r, cx + r)
    ax.set_zlim(cy - r, cy + r)  # zlim do matplotlib = eixo vertical do teu plot (que é Y)
    ax.set_ylim(cz - r, cz + r)  # ylim do matplotlib = eixo do meio (que é Z no teu plot)


# -----------------------------------------------------------------------------
# Um "desenhador" pronto de braços (4 cantos)
# -----------------------------------------------------------------------------
def draw_double_wishbone_corner(ax, corner: str, getp, *, show_labels=True):
    """
    getp(corner, name) -> Point3D ou None
    names: "Sup In","Sup Out","Inf In","Inf Out","Toe In","Toe Out","Damper In","Damper Out"
    """
    # cores por lado
    col = "tab:blue" if corner.endswith("R") else "tab:red"

    def link(n1, n2, c="k", style="-", lw=2.0):
        p1 = getp(corner, n1)
        p2 = getp(corner, n2)
        if p1 is None or p2 is None:
            return
        plot_suspension_link(ax, p1, p2, color=c, style=style, lw=lw)

    # braços
    link("Sup In", "Sup Out", c=col, style="--", lw=2.2)
    link("Inf In", "Inf Out", c=col, style="-", lw=2.6)

    # tirante / amortecedor
    link("Toe In", "Toe Out", c="tab:green", style="--", lw=2.0)
    link("Damper In", "Damper Out", c="tab:purple", style="-", lw=2.0)

    if show_labels:
        p = getp(corner, "Sup Out") or getp(corner, "Inf Out")
        if p is not None:
            ax.text(p.x, p.z, p.y, f" {corner}", fontsize=9)