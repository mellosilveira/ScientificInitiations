import tkinter as tk
from tkinter import ttk, messagebox
import math
import matplotlib.pyplot as plt

# ============================================
# CONVENÇÕES E SISTEMA DE UNIDADES
# --------------------------------------------
# - Todas as coordenadas geométricas em mm
# - Forças em N
# - Massa em kg
# - Ângulos em graus na interface, radianos internamente
#
# Eixos (coerentes com o código 2D original):
#   x: horizontal (lateral, positiva para fora do veículo)
#   y: vertical (positiva para cima)
#   z: longitudinal (positiva para frente)
#
# Vista frontal 2D (Reimpell): plano x–y
# Vista lateral: plano z–y
# Vista superior: plano x–z
# ============================================

def deg2rad(a):
    return a * math.pi / 180.0

def rad2deg(a):
    return a * 180.0 / math.pi

# ============================================================
#  BLOCO 1 – ROTINAS 2D (VISTA FRONTAL) – BASE REIMPELL
# ============================================================

def reta_pontos(x1, y1, x2, y2):
    """
    Retorna a, b, c da equação da reta: a x + b y + c = 0
    Dimensão: a, b, c têm dimensão de '1' (adimensional), pois
    são combinações algébricas de coordenadas.
    """
    a = y1 - y2
    b = x2 - x1
    c = x1 * y2 - x2 * y1
    return a, b, c

def calcular_intersecao_retas(a1, b1, c1, a2, b2, c2):
    det = a1 * b2 - a2 * b1
    if abs(det) < 1e-9:
        return None, None
    x = (b1 * c2 - b2 * c1) / det
    y = (c1 * a2 - c2 * a1) / det
    return x, y

def calcular_rollcentre_e_q(bf, ux_in, uy_in, ux_out, uy_out,
                            lx_in, ly_in, lx_out, ly_out):
    """
    Cálculo geométrico do Roll Centre para duplo A (vista frontal),
    seguindo a construção do Reimpell (cap. 3 - suspensão independente).

    Entradas em mm.
    bf  : bitola (mm)
    (ux_in, uy_in), (ux_out, uy_out) : pivôs braço superior
    (lx_in, ly_in), (lx_out, ly_out) : pivôs braço inferior

    Saídas:
      xP, yP: coordenadas do Instant Center (mm)
      h_ro  : altura do roll center em relação ao solo (mm)
      q     : parâmetro de curvatura (adimensional)
    """
    # Reta dos braços
    a1, b1, c1 = reta_pontos(ux_in, uy_in, ux_out, uy_out)
    a2, b2, c2 = reta_pontos(lx_in, ly_in, lx_out, ly_out)

    xP, yP = calcular_intersecao_retas(a1, b1, c1, a2, b2, c2)
    if xP is None:
        return None, None, None, None

    # Centro da roda (W) na vista frontal
    xW = bf / 2.0
    yW = 0.0  # solo

    # FVSA = linha que liga IC até W
    if abs(xP - xW) < 1e-9:
        h_ro = None
    else:
        m = (yP - yW) / (xP - xW)   # inclinação FVSA
        # Interseção da FVSA com plano médio x=0 -> Roll Centre (0, h_ro)
        h_ro = m * (0 - xW) + yW

    # Distância entre IC e W (vertical) – p no Reimpell
    p = yP - yW  # mm

    # Parâmetro de curvatura q (adimensional) ~ p * bf / h_ro^2
    if h_ro is None or abs(h_ro) < 1e-9:
        q = None
    else:
        q = p * bf / (h_ro ** 2)

    return xP, yP, h_ro, q

def calcular_curvatura_reimpell(bf, s1, s2, camber_o, camber_i):
    """
    Curvatura segundo Reimpell (Eqs. 3.5, 3.6, 3.7 – forma genérica):

    dφ = (s1 + s2) / bf       (rad)
    dφ(°) ≈ 57,3 * dφ
    Δγ_W,k = (γ_o - γ_i) / 2  (graus)
    kγ = Δγ_W,k / dφ(°)       (adimensional)

    Observações dimensionais:
    - s1, s2, bf em mm → dφ é adimensional (rad)
    - γ em graus
    """
    if abs(bf) < 1e-9:
        return None, None, None, None

    dphi_rad = (s1 + s2) / bf
    dphi_deg = rad2deg(dphi_rad)

    dgamma_k = (camber_o - camber_i) / 2.0  # graus

    if abs(dphi_deg) < 1e-9:
        k_gamma = None
    else:
        k_gamma = dgamma_k / dphi_deg  # adimensional

    return dphi_rad, dphi_deg, dgamma_k, k_gamma

# ============================================================
#  BLOCO 2 – 3D PARAMÉTRICO (IC, CAMBER/CASTER/TOE, FORÇAS)
# ============================================================

def calcular_IC_3D_frontal(p_sup_in, p_sup_out, p_inf_in, p_inf_out):
    """
    Calcula o Instant Center projetado na vista frontal (plano x–y)
    a partir de quatro pontos 3D (coordenadas em mm).

    Cada braço é representado por uma reta no plano x–y:
    L_sup: (x_sup_in, y_sup_in) -> (x_sup_out, y_sup_out)
    L_inf: (x_inf_in, y_inf_in) -> (x_inf_out, y_inf_out)
    """
    x1, y1, _ = p_sup_in
    x2, y2, _ = p_sup_out
    x3, y3, _ = p_inf_in
    x4, y4, _ = p_inf_out

    a1, b1, c1 = reta_pontos(x1, y1, x2, y2)
    a2, b2, c2 = reta_pontos(x3, y3, x4, y4)

    xP, yP = calcular_intersecao_retas(a1, b1, c1, a2, b2, c2)
    if xP is None:
        return None
    return (xP, yP)

def calcular_angulo_camber(p_sup, p_inf):
    """
    Camber a partir de dois pontos da manga (sup e inf) na vista frontal (x–y).

    tan(γ) = Δx / Δy
    γ > 0 → roda inclinada para fora (convencional)
    """
    x_sup, y_sup, _ = p_sup
    x_inf, y_inf, _ = p_inf
    dx = x_sup - x_inf
    dy = y_sup - y_inf
    if abs(dy) < 1e-9:
        return None
    return rad2deg(math.atan(dx / dy))

def calcular_angulo_caster(p_sup, p_inf):
    """
    Caster a partir da projeção do eixo de direção no plano z–y.

    tan(α) = Δz / Δy
    """
    _, y_sup, z_sup = p_sup
    _, y_inf, z_inf = p_inf
    dz = z_sup - z_inf
    dy = y_sup - y_inf
    if abs(dy) < 1e-9:
        return None
    return rad2deg(math.atan(dz / dy))

def calcular_angulo_toe(p_front, p_rear):
    """
    Toe a partir da vista superior (plano x–z):

    tan(θ) = Δx / Δz
    θ > 0 → toe-out (depende da convenção)
    """
    x_f, _, z_f = p_front
    x_r, _, z_r = p_rear
    dx = x_f - x_r
    dz = z_f - z_r
    if abs(dz) < 1e-9:
        return None
    return rad2deg(math.atan(dx / dz))

def decompor_forca_braco(p_in, p_out, F_axial):
    """
    Decomposição da força axial em um braço nas direções x, y, z do chassi.

    p_in, p_out : pontos 3D (mm)
    F_axial    : força ao longo do braço (N)

    Força vetorial:
      v = (dx, dy, dz)
      L = |v| [mm]
      e = v / L (adimensional)
      F = F_axial * e  [N]

    Dimensionalmente:
      |v| em mm, mas e é adimensional,
      logo F tem unidade N, coerente.
    """
    dx = p_out[0] - p_in[0]
    dy = p_out[1] - p_in[1]
    dz = p_out[2] - p_in[2]

    L = math.sqrt(dx*dx + dy*dy + dz*dz)
    if L < 1e-9:
        return None

    ex = dx / L
    ey = dy / L
    ez = dz / L

    Fx = F_axial * ex
    Fy = F_axial * ey
    Fz = F_axial * ez

    # Ângulos auxiliares só para visualização:
    ang_front = rad2deg(math.atan2(dx, dy)) if abs(dy) > 1e-9 else None  # plano x–y
    ang_side  = rad2deg(math.atan2(dz, dy)) if abs(dy) > 1e-9 else None  # plano z–y
    ang_top   = rad2deg(math.atan2(dx, dz)) if abs(dz) > 1e-9 else None  # plano x–z

    return {
        "Fx": Fx, "Fy": Fy, "Fz": Fz,
        "ang_front_deg": ang_front,
        "ang_side_deg": ang_side,
        "ang_top_deg": ang_top,
        "L_mm": L
    }

def calcular_forcas_duploA(p_sup_in, p_sup_out, p_inf_in, p_inf_out,
                           Fx_long_tire, rig_sup=1.0, rig_inf=1.0):
    """
    Calcula forças em x, y, z em cada braço (sup/inf) a partir de
    uma força longitudinal na roda (Fx_long_tire), distribuída entre
    os braços proporcionalmente às "rigidezes" (rig_sup, rig_inf).

    Modelo simplificado (estilo Milliken):
      - Força resultante no nó da manga é equilibrada por forças axiais.
      - Tomamos z como eixo longitudinal.
      - F_sup_ax * e_sup_z + F_inf_ax * e_inf_z = Fx_long_tire
      - F_sup_ax : F_inf_ax = rig_sup : rig_inf
    """

    # Vetores diretores
    dxs = p_sup_out[0] - p_sup_in[0]
    dys = p_sup_out[1] - p_sup_in[1]
    dzs = p_sup_out[2] - p_sup_in[2]
    Ls = math.sqrt(dxs*dxs + dys*dys + dzs*dzs)

    dxi = p_inf_out[0] - p_inf_in[0]
    dyi = p_inf_out[1] - p_inf_in[1]
    dzi = p_inf_out[2] - p_inf_in[2]
    Li = math.sqrt(dxi*dxi + dyi*dyi + dzi*dzi)

    if Ls < 1e-9 or Li < 1e-9:
        return None

    # Direções longitudinais (eixo z)
    es_z = dzs / Ls
    ei_z = dzi / Li

    # Equilíbrio na direção longitudinal z:
    # F_sup_ax = k * rig_sup, F_inf_ax = k * rig_inf
    # (k*rig_sup)*es_z + (k*rig_inf)*ei_z = Fx_long_tire
    denom = rig_sup * es_z + rig_inf * ei_z
    if abs(denom) < 1e-9:
        return None

    k = Fx_long_tire / denom
    F_sup_ax = k * rig_sup
    F_inf_ax = k * rig_inf

    # Decompor forças
    res_sup = decompor_forca_braco(p_sup_in, p_sup_out, F_sup_ax)
    res_inf = decompor_forca_braco(p_inf_in, p_inf_out, F_inf_ax)

    if res_sup is None or res_inf is None:
        return None

    Fx_tot = res_sup["Fx"] + res_inf["Fx"]
    Fy_tot = res_sup["Fy"] + res_inf["Fy"]
    Fz_tot = res_sup["Fz"] + res_inf["Fz"]

    return {
        "sup": res_sup,
        "inf": res_inf,
        "total": {"Fx": Fx_tot, "Fy": Fy_tot, "Fz": Fz_tot}
    }

def calcular_antidive(Fx_long_tire, Fz_vert_total, h_CG_mm, entre_eixos_mm):
    """
    Anti-dive (%) segundo a definição clássica usada por Reimpell:

    Fz_necessária = Fx * h_CG / L   (N)
    Anti-dive (%) = (Fz_vertical_total / Fz_necessária) * 100

    Fx_long_tire   : força longitudinal por roda (N)
    Fz_vert_total  : componente vertical transmitida pelos braços (N)
    h_CG_mm        : altura CG (mm)
    entre_eixos_mm : entre-eixos (mm)
    """
    if abs(entre_eixos_mm) < 1e-9:
        return None
    Fz_need = Fx_long_tire * (h_CG_mm / entre_eixos_mm)  # N
    if abs(Fz_need) < 1e-9:
        return None
    return (Fz_vert_total / Fz_need) * 100.0

# ============================================================
#  INTERFACE TKINTER
# ============================================================

class App:
    def __init__(self, root):
        self.root = root
        self.root.title("Baja – Suspensão Duplo A – Reimpell & Milliken")
        self.root.geometry("1200x800")

        notebook = ttk.Notebook(root)
        notebook.pack(fill="both", expand=True)

        # Abas
        self.tab2d = ttk.Frame(notebook)
        self.tab3d = ttk.Frame(notebook)
        self.tab_baja = ttk.Frame(notebook)
        self.tab_res = ttk.Frame(notebook)

        notebook.add(self.tab2d, text="Vista frontal 2D (Reimpell)")
        notebook.add(self.tab3d, text="Modelo 3D + Esforços")
        notebook.add(self.tab_baja, text="Parâmetros Baja SAE")
        notebook.add(self.tab_res, text="Resultados / Explicação")

        self._build_tab2d()
        self._build_tab3d()
        self._build_tab_baja()
        self._build_tab_res()

    # ---------- ABA 2D ----------
    def _build_tab2d(self):
        f = self.tab2d
        row = 0

        ttk.Label(
            f,
            text="Geometria 2D – Vista frontal (x lateral, y vertical) [mm]"
        ).grid(row=row, column=0, columnspan=4, pady=5, sticky="w")
        row += 1

        ttk.Label(f, text="Bitola bf [mm]:").grid(row=row, column=0, sticky="e")
        self.entry_bf = ttk.Entry(f, width=10)
        self.entry_bf.insert(0, "1200")
        self.entry_bf.grid(row=row, column=1, sticky="w")
        row += 1

        # Superior interno/externo
        ttk.Label(f, text="Sup interno (x,y) [mm]:").grid(row=row, column=0, sticky="e")
        self.ux_in = ttk.Entry(f, width=8); self.ux_in.insert(0, "450"); self.ux_in.grid(row=row, column=1)
        self.uy_in = ttk.Entry(f, width=8); self.uy_in.insert(0, "400"); self.uy_in.grid(row=row, column=2)
        row += 1

        ttk.Label(f, text="Sup externo (x,y) [mm]:").grid(row=row, column=0, sticky="e")
        self.ux_out = ttk.Entry(f, width=8); self.ux_out.insert(0, "600"); self.ux_out.grid(row=row, column=1)
        self.uy_out = ttk.Entry(f, width=8); self.uy_out.insert(0, "350"); self.uy_out.grid(row=row, column=2)
        row += 1

        # Inferior interno/externo
        ttk.Label(f, text="Inf interno (x,y) [mm]:").grid(row=row, column=0, sticky="e")
        self.lx_in = ttk.Entry(f, width=8); self.lx_in.insert(0, "450"); self.lx_in.grid(row=row, column=1)
        self.ly_in = ttk.Entry(f, width=8); self.ly_in.insert(0, "200"); self.ly_in.grid(row=row, column=2)
        row += 1

        ttk.Label(f, text="Inf externo (x,y) [mm]:").grid(row=row, column=0, sticky="e")
        self.lx_out = ttk.Entry(f, width=8); self.lx_out.insert(0, "600"); self.lx_out.grid(row=row, column=1)
        self.ly_out = ttk.Entry(f, width=8); self.ly_out.insert(0, "180"); self.ly_out.grid(row=row, column=2)
        row += 1

        # Curvatura
        ttk.Label(f, text="Curso roda externa s1 [mm]:").grid(row=row, column=0, sticky="e")
        self.entry_s1 = ttk.Entry(f, width=8); self.entry_s1.insert(0, "40"); self.entry_s1.grid(row=row, column=1)
        ttk.Label(f, text="Curso roda interna s2 [mm]:").grid(row=row, column=2, sticky="e")
        self.entry_s2 = ttk.Entry(f, width=8); self.entry_s2.insert(0, "30"); self.entry_s2.grid(row=row, column=3)
        row += 1

        ttk.Label(f, text="Camber externo γ_o [°]:").grid(row=row, column=0, sticky="e")
        self.entry_camber_o = ttk.Entry(f, width=8); self.entry_camber_o.insert(0, "-1.5"); self.entry_camber_o.grid(row=row, column=1)
        ttk.Label(f, text="Camber interno γ_i [°]:").grid(row=row, column=2, sticky="e")
        self.entry_camber_i = ttk.Entry(f, width=8); self.entry_camber_i.insert(0, "-4.0"); self.entry_camber_i.grid(row=row, column=3)
        row += 1

        ttk.Button(f, text="Calcular 2D", command=self._exec_2d).grid(row=row, column=0, pady=5)
        ttk.Button(f, text="Plotar 2D", command=self._plot_2d).grid(row=row, column=1, pady=5)
        row += 1

        self.label_roll = ttk.Label(f, text="Roll centre: --")
        self.label_roll.grid(row=row, column=0, columnspan=4, sticky="w")
        row += 1

        self.label_curv = ttk.Label(f, text="Curvatura: --")
        self.label_curv.grid(row=row, column=0, columnspan=4, sticky="w")

    def _exec_2d(self):
        try:
            bf = float(self.entry_bf.get())
            ux_in = float(self.ux_in.get()); uy_in = float(self.uy_in.get())
            ux_out = float(self.ux_out.get()); uy_out = float(self.uy_out.get())
            lx_in = float(self.lx_in.get()); ly_in = float(self.ly_in.get())
            lx_out = float(self.lx_out.get()); ly_out = float(self.ly_out.get())

            xP, yP, h_ro, q = calcular_rollcentre_e_q(
                bf, ux_in, uy_in, ux_out, uy_out, lx_in, ly_in, lx_out, ly_out
            )

            if xP is None:
                self.label_roll.config(text="IC indefinido (braços paralelos).")
            else:
                txt = f"IC=({xP:.1f}, {yP:.1f}) mm | h_Ro={'--' if h_ro is None else f'{h_ro:.1f} mm'} | q={'--' if q is None else f'{q:.3f}'}"
                self.label_roll.config(text=txt)

            s1 = float(self.entry_s1.get())
            s2 = float(self.entry_s2.get())
            camber_o = float(self.entry_camber_o.get())
            camber_i = float(self.entry_camber_i.get())

            dphi_rad, dphi_deg, dgamma_k, k_gamma = calcular_curvatura_reimpell(
                bf, s1, s2, camber_o, camber_i
            )

            if dphi_rad is None:
                self.label_curv.config(text="Curvatura: não foi possível calcular (bf=0).")
            else:
                txtc = (
                    f"dφ={dphi_deg:.3f}° | Δγ_W,k={dgamma_k:.3f}° | kγ={k_gamma:.3f}"
                    if k_gamma is not None
                    else "Curvatura: divisão por zero em dφ."
                )
                self.label_curv.config(text=txtc)

            self._last_2d = {
                "bf": bf, "ux_in": ux_in, "uy_in": uy_in,
                "ux_out": ux_out, "uy_out": uy_out,
                "lx_in": lx_in, "ly_in": ly_in,
                "lx_out": lx_out, "ly_out": ly_out,
                "IC": (xP, yP), "h_ro": h_ro, "q": q,
                "dphi_deg": dphi_deg, "dgamma_k": dgamma_k, "k_gamma": k_gamma
            }

        except ValueError:
            messagebox.showerror("Erro", "Valores inválidos na aba 2D.")

    def _plot_2d(self):
        try:
            bf = float(self.entry_bf.get())
            ux_in = float(self.ux_in.get()); uy_in = float(self.uy_in.get())
            ux_out = float(self.ux_out.get()); uy_out = float(self.uy_out.get())
            lx_in = float(self.lx_in.get()); ly_in = float(self.ly_in.get())
            lx_out = float(self.lx_out.get()); ly_out = float(self.ly_out.get())

            xP, yP, h_ro, q = calcular_rollcentre_e_q(
                bf, ux_in, uy_in, ux_out, uy_out, lx_in, ly_in, lx_out, ly_out
            )

            fig, ax = plt.subplots()
            ax.plot([ux_in, ux_out], [uy_in, uy_out], "bo-", label="Braço sup")
            ax.plot([lx_in, lx_out], [ly_in, ly_out], "ro-", label="Braço inf")

            if xP is not None:
                ax.plot(xP, yP, "ks", label="IC")

            xW = bf / 2.0
            yW = 0.0
            ax.plot(xW, yW, "mo", label="Centro da roda")

            if h_ro is not None:
                ax.plot(0, h_ro, "c*", label="Roll centre")

            ax.axvline(0, color="gray", linestyle="--")
            ax.set_aspect("equal", adjustable="box")
            ax.grid(True)
            ax.set_xlabel("x [mm] (lateral)")
            ax.set_ylabel("y [mm] (vertical)")
            ax.legend()
            plt.show()

        except ValueError:
            messagebox.showerror("Erro", "Valores inválidos para plotar 2D.")

    # ---------- ABA 3D ----------
    def _build_tab3d(self):
        f = self.tab3d
        row = 0
        ttk.Label(
            f,
            text="Geometria 3D dos braços (x lateral, y vertical, z longitudinal) [mm]"
        ).grid(row=row, column=0, columnspan=6, sticky="w", pady=5)
        row += 1

        # Entradas dos quatro pontos da suspensão (sup/inf in/out)
        labels_pts = [
            ("Sup_In X", "Sup_In Y", "Sup_In Z"),
            ("Sup_Out X", "Sup_Out Y", "Sup_Out Z"),
            ("Inf_In X", "Inf_In Y", "Inf_In Z"),
            ("Inf_Out X", "Inf_Out Y", "Inf_Out Z"),
        ]
        self.entries_3d = {}
        valores_default = [
            (450, 400, 200),  # sup in
            (600, 350, 300),  # sup out
            (450, 200, 200),  # inf in
            (600, 180, 300),  # inf out
        ]
        for i, (lx, ly, lz) in enumerate(labels_pts):
            ttk.Label(f, text=f"{lx},{ly},{lz}:").grid(row=row, column=0, sticky="e")
            ex = ttk.Entry(f, width=7); ex.insert(0, str(valores_default[i][0])); ex.grid(row=row, column=1)
            ey = ttk.Entry(f, width=7); ey.insert(0, str(valores_default[i][1])); ey.grid(row=row, column=2)
            ez = ttk.Entry(f, width=7); ez.insert(0, str(valores_default[i][2])); ez.grid(row=row, column=3)
            self.entries_3d[lx] = ex
            self.entries_3d[ly] = ey
            self.entries_3d[lz] = ez
            row += 1

        # Pontos da manga para camber/caster
        ttk.Label(f, text="Pontos da manga para camber/caster (sup, inf) [mm]:").grid(
            row=row, column=0, columnspan=4, sticky="w", pady=5
        )
        row += 1
        ttk.Label(f, text="Manga sup (x,y,z):").grid(row=row, column=0, sticky="e")
        self.cam_sup_x = ttk.Entry(f, width=7); self.cam_sup_x.insert(0, "600"); self.cam_sup_x.grid(row=row, column=1)
        self.cam_sup_y = ttk.Entry(f, width=7); self.cam_sup_y.insert(0, "350"); self.cam_sup_y.grid(row=row, column=2)
        self.cam_sup_z = ttk.Entry(f, width=7); self.cam_sup_z.insert(0, "300"); self.cam_sup_z.grid(row=row, column=3)
        row += 1
        ttk.Label(f, text="Manga inf (x,y,z):").grid(row=row, column=0, sticky="e")
        self.cam_inf_x = ttk.Entry(f, width=7); self.cam_inf_x.insert(0, "600"); self.cam_inf_x.grid(row=row, column=1)
        self.cam_inf_y = ttk.Entry(f, width=7); self.cam_inf_y.insert(0, "180"); self.cam_inf_y.grid(row=row, column=2)
        self.cam_inf_z = ttk.Entry(f, width=7); self.cam_inf_z.insert(0, "300"); self.cam_inf_z.grid(row=row, column=3)
        row += 1

        # Pontos da roda para toe
        ttk.Label(f, text="Pontos para toe (vista superior) [mm]:").grid(
            row=row, column=0, columnspan=4, sticky="w", pady=5
        )
        row += 1
        ttk.Label(f, text="Face frente (x,y,z):").grid(row=row, column=0, sticky="e")
        self.toe_front_x = ttk.Entry(f, width=7); self.toe_front_x.insert(0, "600"); self.toe_front_x.grid(row=row, column=1)
        self.toe_front_y = ttk.Entry(f, width=7); self.toe_front_y.insert(0, "180"); self.toe_front_y.grid(row=row, column=2)
        self.toe_front_z = ttk.Entry(f, width=7); self.toe_front_z.insert(0, "350"); self.toe_front_z.grid(row=row, column=3)
        row += 1
        ttk.Label(f, text="Face traseira (x,y,z):").grid(row=row, column=0, sticky="e")
        self.toe_rear_x = ttk.Entry(f, width=7); self.toe_rear_x.insert(0, "600"); self.toe_rear_x.grid(row=row, column=1)
        self.toe_rear_y = ttk.Entry(f, width=7); self.toe_rear_y.insert(0, "180"); self.toe_rear_y.grid(row=row, column=2)
        self.toe_rear_z = ttk.Entry(f, width=7); self.toe_rear_z.insert(0, "250"); self.toe_rear_z.grid(row=row, column=3)
        row += 1

        ttk.Label(f, text="Rigidez relativa braço sup/inf (adimensional):").grid(row=row, column=0, sticky="e")
        self.rig_sup = ttk.Entry(f, width=7); self.rig_sup.insert(0, "1.0"); self.rig_sup.grid(row=row, column=1)
        self.rig_inf = ttk.Entry(f, width=7); self.rig_inf.insert(0, "1.0"); self.rig_inf.grid(row=row, column=2)
        row += 1

        ttk.Label(f, text="Força longitudinal na roda (N) – eixo z:").grid(row=row, column=0, sticky="e")
        self.fx_long = ttk.Entry(f, width=10); self.fx_long.insert(0, "1200"); self.fx_long.grid(row=row, column=1)
        row += 1

        ttk.Button(f, text="Calcular 3D + Esforços", command=self._exec_3d).grid(row=row, column=0, pady=10)
        row += 1

        self.label_3d_geom = ttk.Label(f, text="Geom 3D / IC: --")
        self.label_3d_geom.grid(row=row, column=0, columnspan=6, sticky="w"); row += 1

        self.label_3d_angles = ttk.Label(f, text="Camber/Caster/Toe: --")
        self.label_3d_angles.grid(row=row, column=0, columnspan=6, sticky="w"); row += 1

        self.label_3d_forces = ttk.Label(f, text="Esforços em x,y,z (sup/inf/total): --")
        self.label_3d_forces.grid(row=row, column=0, columnspan=6, sticky="w")

    def _exec_3d(self):
        try:
            # Ler pontos 3D dos braços
            def get(label):
                return float(self.entries_3d[label].get())

            p_sup_in = (get("Sup_In X"), get("Sup_In Y"), get("Sup_In Z"))
            p_sup_out = (get("Sup_Out X"), get("Sup_Out Y"), get("Sup_Out Z"))
            p_inf_in = (get("Inf_In X"), get("Inf_In Y"), get("Inf_In Z"))
            p_inf_out = (get("Inf_Out X"), get("Inf_Out Y"), get("Inf_Out Z"))

            # IC frontal 3D projetado
            IC = calcular_IC_3D_frontal(p_sup_in, p_sup_out, p_inf_in, p_inf_out)

            # Camber / caster
            cam_sup = (
                float(self.cam_sup_x.get()),
                float(self.cam_sup_y.get()),
                float(self.cam_sup_z.get())
            )
            cam_inf = (
                float(self.cam_inf_x.get()),
                float(self.cam_inf_y.get()),
                float(self.cam_inf_z.get())
            )
            camber = calcular_angulo_camber(cam_sup, cam_inf)
            caster = calcular_angulo_caster(cam_sup, cam_inf)

            # Toe
            toe_front = (
                float(self.toe_front_x.get()),
                float(self.toe_front_y.get()),
                float(self.toe_front_z.get())
            )
            toe_rear = (
                float(self.toe_rear_x.get()),
                float(self.toe_rear_y.get()),
                float(self.toe_rear_z.get())
            )
            toe = calcular_angulo_toe(toe_front, toe_rear)

            # Forças nos braços
            Fx_long = float(self.fx_long.get())
            rig_sup = float(self.rig_sup.get())
            rig_inf = float(self.rig_inf.get())

            forces = calcular_forcas_duploA(
                p_sup_in, p_sup_out, p_inf_in, p_inf_out,
                Fx_long, rig_sup, rig_inf
            )

            # Atualizar labels
            if IC is None:
                self.label_3d_geom.config(
                    text="Geom 3D / IC: IC indefinido (braços paralelos na vista frontal)."
                )
            else:
                self.label_3d_geom.config(
                    text=f"Geom 3D / IC: IC_frontal=({IC[0]:.1f}, {IC[1]:.1f}) mm"
                )

            self.label_3d_angles.config(
                text=f"Camber={camber:.2f}° | Caster={caster:.2f}° | Toe={toe:.2f}°"
            )

            if forces is None:
                self.label_3d_forces.config(
                    text="Esforços: não foi possível calcular (geometria degenerada)."
                )
                self._last_forces = None
            else:
                sup = forces["sup"]
                inf = forces["inf"]
                tot = forces["total"]
                txtf = (
                    f"Sup(Fx,Fy,Fz)=({sup['Fx']:.1f}, {sup['Fy']:.1f}, {sup['Fz']:.1f}) N | "
                    f"Inf(Fx,Fy,Fz)=({inf['Fx']:.1f}, {inf['Fy']:.1f}, {inf['Fz']:.1f}) N | "
                    f"Total(Fx,Fy,Fz)=({tot['Fx']:.1f}, {tot['Fy']:.1f}, {tot['Fz']:.1f}) N"
                )
                self.label_3d_forces.config(text=txtf)
                self._last_forces = forces

        except ValueError:
            messagebox.showerror("Erro", "Valores inválidos na aba 3D.")

    # ---------- ABA BAJA ----------
    def _build_tab_baja(self):
        f = self.tab_baja
        row = 0
        ttk.Label(
            f,
            text="Parâmetros típicos de um Baja SAE (tudo em mm, exceto massa e força)"
        ).grid(row=row, column=0, columnspan=2, sticky="w", pady=5)
        row += 1

        self.entries_baja = {}

        def add_param(nome, default):
            nonlocal row
            ttk.Label(f, text=nome).grid(row=row, column=0, sticky="e")
            e = ttk.Entry(f, width=10)
            e.insert(0, str(default))
            e.grid(row=row, column=1, sticky="w")
            self.entries_baja[nome] = e
            row += 1

        add_param("Massa total [kg]", 200)
        add_param("Altura do CG h_CG [mm]", 500)
        add_param("Entre-eixos L [mm]", 1550)
        add_param("Bitola dianteira bf [mm]", 1200)
        add_param("Curso da suspensão [mm]", 220)
        add_param("Força de frenagem por roda Fx [N]", 1200)

        ttk.Button(
            f,
            text="Calcular Anti-Dive com forças 3D",
            command=self._exec_baja
        ).grid(row=row, column=0, columnspan=2, pady=10)

        row += 1
        self.label_baja = ttk.Label(f, text="Anti-dive: --")
        self.label_baja.grid(row=row, column=0, columnspan=2, sticky="w")

    def _exec_baja(self):
        try:
            h_CG = float(self.entries_baja["Altura do CG h_CG [mm]"].get())
            L = float(self.entries_baja["Entre-eixos L [mm]"].get())
            Fx = float(self.entries_baja["Força de frenagem por roda Fx [N]"].get())

            if not hasattr(self, "_last_forces") or self._last_forces is None:
                messagebox.showwarning(
                    "Aviso",
                    "Primeiro calcule as forças 3D na aba 'Modelo 3D + Esforços'."
                )
                return

            # Fy é vertical nesse sistema (eixo y)
            Fz_vert_total = self._last_forces["total"]["Fy"]

            anti = calcular_antidive(Fx, Fz_vert_total, h_CG, L)
            if anti is None:
                self.label_baja.config(text="Anti-dive: não foi possível calcular (dados inválidos).")
            else:
                self.label_baja.config(text=f"Anti-dive ≈ {anti:.1f} %")
        except ValueError:
            messagebox.showerror("Erro", "Parâmetros Baja inválidos.")

    # ---------- ABA RESULTADOS / EXPLICAÇÃO ----------
    def _build_tab_res(self):
        txt = tk.Text(self.tab_res, font=("Courier", 10))
        txt.pack(fill="both", expand=True)
        texto = """
RESUMO DO MODELO (Reimpell & Milliken) – Tudo em mm, N, kg

1) Vista frontal 2D (aba 1)
--------------------------------
- Entradas:
    bf                : bitola dianteira [mm]
    (ux_in, uy_in)    : pivô interno braço superior [mm]
    (ux_out, uy_out)  : pivô externo braço superior [mm]
    (lx_in, ly_in)    : pivô interno braço inferior [mm]
    (lx_out, ly_out)  : pivô externo braço inferior [mm]
    s1, s2            : curso roda externa/interna [mm]
    camber_o, camber_i: cambers em graus

- Saídas:
    IC (xP, yP)       : Instant Center [mm]
    h_Ro              : altura do Roll Centre [mm]
    q                 : parâmetro de curvatura (adimensional)
    dφ                : ângulo de rolagem [graus]
    Δγ_W,k            : variação média de cambagem [graus]
    kγ                : fator de cambagem (adimensional)

2) Modelo 3D + esforços (aba 2)
--------------------------------
- Entradas:
    Pontos 3D dos braços (x, y, z) em mm
    Pontos da manga para camber/caster
    Pontos da face da roda para toe
    Rigidez relativa braço sup/inf (adimensional)
    Força longitudinal na roda Fx (N) – ao longo de z

- Cálculos:
    IC_frontal (x, y) : Instant Center projetado [mm]
    Camber            : vista frontal (x–y)
    Caster            : vista lateral (z–y)
    Toe               : vista superior (x–z)

    Forças nos braços:
      v = (dx, dy, dz) [mm]
      L = |v| [mm]
      e = v / L (adimensional)
      F_sup_ax, F_inf_ax obtidas por equilíbrio em z:
         F_sup_ax * e_sup_z + F_inf_ax * e_inf_z ≈ Fx_long

      Fx = F_axial * (dx/L)  [N]  (componente lateral – eixo x)
      Fy = F_axial * (dy/L)  [N]  (componente vertical – eixo y)
      Fz = F_axial * (dz/L)  [N]  (componente longitudinal – eixo z)

3) Parâmetros Baja SAE e Anti-Dive (aba 3)
---------------------------------------------
- Entradas:
    h_CG [mm]         : altura do centro de gravidade
    L [mm]            : entre-eixos
    Fx [N]            : força de frenagem por roda
    Fz_vertical_total : componente vertical resultante dos braços (Fy_total)

- Anti-dive:
    Fz_necessária = Fx * h_CG / L        [N]
    Anti-dive (%) = (Fz_vertical_total / Fz_necessária) * 100

4) Efeitos da angulação dos braços nos eixos x, y, z
------------------------------------------------------
- Braço inclinado na vista frontal (x–y):
    -> gera componente vertical Fy a partir de força axial
- Braço inclinado na vista lateral (z–y):
    -> gera componente vertical Fy a partir de Fz (frenagem/aceleração)
- Braço inclinado na vista superior (x–z):
    -> gera componente lateral Fx a partir de Fz (compliance steer)

Esse modelo te deixa ver numericamente como a angulação dos braços
muda a repartição de esforços em x, y, z, o roll centre e o anti-dive.
"""
        txt.insert("1.0", texto)
        txt.configure(state="disabled")


if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()
