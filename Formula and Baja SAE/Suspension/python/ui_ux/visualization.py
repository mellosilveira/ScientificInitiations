import matplotlib.pyplot as plt
# Se necessário importar models aqui, use try/except ou passe apenas objetos simples

def autoscale_3d_equal(ax, points, pad_factor=0.35, min_range=400.0):
    """Autoscale 3D para manter proporção igual."""
    if not points:
        return
    xs = [p.x for p in points]
    ys = [p.y for p in points]
    zs = [p.z for p in points]

    xmin, xmax = min(xs), max(xs)
    ymin, ymax = min(ys), max(ys)
    zmin, zmax = min(zs), max(zs)

    cx = 0.5 * (xmin + xmax)
    cy = 0.5 * (ymin + ymax)
    cz = 0.5 * (zmin + zmax)

    rx = max(1e-9, (xmax - xmin))
    ry = max(1e-9, (ymax - ymin))
    rz = max(1e-9, (zmax - zmin))

    rmax = max(rx, ry, rz, float(min_range))
    rmax *= (1.0 + float(pad_factor))
    half = 0.5 * rmax

    ax.set_xlim(cx - half, cx + half)
    ax.set_ylim(cz - half, cz + half)
    ax.set_zlim(cy - half, cy + half)

def draw_ground_mesh(ax, step=50.0):
    """Desenha o grid de chão."""
    half = 300.0
    step = float(step) if step > 0 else 50.0
    
    # Grid fixo
    curr = -half
    while curr <= half + 1e-9:
        # Linhas em X
        ax.plot([curr, curr], [-half, half], [0, 0], ":", lw=0.8, alpha=0.55, color="gray")
        # Linhas em Y (Z no gráfico)
        ax.plot([-half, half], [curr, curr], [0, 0], ":", lw=0.8, alpha=0.55, color="gray")
        curr += step

def plot_suspension_link(ax, p1, p2, color, style="--", linewidth=2):
    """Abstração para desenhar link entre dois Point3D."""
    ax.plot([p1.x, p2.x], [p1.z, p2.z], [p1.y, p2.y],
            color=color, linestyle=style, linewidth=linewidth)