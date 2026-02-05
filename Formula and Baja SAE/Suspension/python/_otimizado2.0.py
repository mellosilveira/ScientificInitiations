import tkinter as tk
from tkinter import ttk, messagebox
import math
import matplotlib.pyplot as plt

# ============================================
# CONVENÇÕES E SISTEMA DE UNIDADES
# --------------------------------------------
# - Coordenadas geométricas em mm
# - Forças em N
# - Massa em kg
# - Ângulos em graus na interface, radianos internamente
#
# Eixos:
#   x: lateral (positiva para fora do veículo)
#   y: vertical (positiva para cima)
#   z: longitudinal (positiva para frente)
# ============================================

G = 9.81  # m/s²

def deg2rad(a): return a * math.pi / 180.0
def rad2deg(a): return a * 180.0 / math.pi
def mm_to_m(x_mm): return x_mm / 1000.0

def fmt_num(x, nd=2, sufixo=""):
    """Formata número ou retorna '--' se None."""
    if x is None:
        return "--"
    try:
        return f"{float(x):.{nd}f}{sufixo}"
    except Exception:
        return "--"

def safe_float(s: str):
    """Converte entry em float com mensagem"""
    s = (s or "").strip().replace(",", ".")
    if s == "":
        raise ValueError("Campo vazio")
    return float(s)

# ============================================================
#  BLOCO 1 – ROTINAS 2D (VISTA FRONTAL)
# ============================================================

#o ponto 0 é o meio do carro então leve em consideração isso, a distancia dos braços tem q ser do meio do carro até o braço
# a vistá é longitudinal do eixo xy

def reta_pontos(x1, y1, x2, y2):
    """
    Reta em forma implícita: a x + b y + c = 0
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

def calcular_centro_instantaneo_de_rotacao_e_centro_de_rolagem(
    bf_mm,
    ux_in, uy_in, ux_out, uy_out,
    lx_in, ly_in, lx_out, ly_out
):
    """
    Saídas:
      xP, yP: Centro Instantâneo (IC) [mm]
      h_Ro: altura do Centro de Rolagem (Roll Centre) [mm]
      q: parâmetro adimensional de curvatura (métrica do projeto)
    """
    a1, b1, c1 = reta_pontos(ux_in, uy_in, ux_out, uy_out)
    a2, b2, c2 = reta_pontos(lx_in, ly_in, lx_out, ly_out)

    xP, yP = calcular_intersecao_retas(a1, b1, c1, a2, b2, c2)
    if xP is None:
        return None, None, None, None

    # Centro da roda W (vista frontal): (bf/2, 0)
    xW = bf_mm / 2.0
    yW = 0.0

    # Linha FVSA: IC -> W, intercepta x=0 (plano central) no roll center
    if abs(xP - xW) < 1e-9:
        h_Ro = None
    else:
        m = (yP - yW) / (xP - xW)
        h_Ro = m * (0 - xW) + yW

    # p = distância vertical IC -> solo
    p = yP - yW

    if h_Ro is None or abs(h_Ro) < 1e-9:
        q = None
    else:
        q = p * bf_mm / (h_Ro ** 2)

    return xP, yP, h_Ro, q

def calcular_curvatura_cinematica_de_cambagem(bf_mm, s1_mm, s2_mm, camber_externo_deg, camber_interno_deg):
    """
    Modelo genérico:
      dφ(rad) = (s1 + s2)/bf
      dφ(deg) = 57,3 * dφ(rad)
      Δγ_W,k = (γ_o - γ_i)/2
      kγ = Δγ_W,k / dφ(deg)
    """
    if abs(bf_mm) < 1e-9:
        return None, None, None, None

    dphi_rad = (s1_mm + s2_mm) / bf_mm
    dphi_deg = rad2deg(dphi_rad)

    dgamma_k = (camber_externo_deg - camber_interno_deg) / 2.0

    if abs(dphi_deg) < 1e-9:
        k_gamma = None
    else:
        k_gamma = dgamma_k / dphi_deg

    return dphi_rad, dphi_deg, dgamma_k, k_gamma

# ============================================================
#  BLOCO 2 – 3D PARAMÉTRICO (IC PROJETADO, CAMBER/CASTER/TOE, FORÇAS)
# ============================================================

def calcular_centro_instantaneo_projetado_vista_frontal(p_sup_in, p_sup_out, p_inf_in, p_inf_out):
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

def calcular_angulo_de_cambagem(p_sup, p_inf):
    x_sup, y_sup, _ = p_sup
    x_inf, y_inf, _ = p_inf
    dx = x_sup - x_inf
    dy = y_sup - y_inf
    if abs(dy) < 1e-9:
        return None
    return rad2deg(math.atan(dx / dy))

def calcular_angulo_de_caster(p_sup, p_inf):
    _, y_sup, z_sup = p_sup
    _, y_inf, z_inf = p_inf
    dz = z_sup - z_inf
    dy = y_sup - y_inf
    if abs(dy) < 1e-9:
        return None
    return rad2deg(math.atan(dz / dy))

def calcular_angulo_de_convergencia_divergencia_toe(p_front, p_rear):
    x_f, _, z_f = p_front
    x_r, _, z_r = p_rear
    dx = x_f - x_r
    dz = z_f - z_r
    if abs(dz) < 1e-9:
        return None
    return rad2deg(math.atan(dx / dz))

def decompor_forca_axial_no_braco(p_in, p_out, forca_axial_N):
    dx = p_out[0] - p_in[0]
    dy = p_out[1] - p_in[1]
    dz = p_out[2] - p_in[2]

    L = math.sqrt(dx*dx + dy*dy + dz*dz)
    if L < 1e-9:
        return None

    ex = dx / L
    ey = dy / L
    ez = dz / L

    Fx = forca_axial_N * ex
    Fy = forca_axial_N * ey
    Fz = forca_axial_N * ez

    return {"Fx": Fx, "Fy": Fy, "Fz": Fz, "Comprimento_do_Braco_mm": L}

def calcular_forcas_no_duplo_A(
    p_sup_in, p_sup_out, p_inf_in, p_inf_out,
    forca_longitudinal_na_roda_N,
    rigidez_relativa_superior=1.0,
    rigidez_relativa_inferior=1.0
):
    # Vetores
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

    # Projeção longitudinal (eixo z)
    es_z = dzs / Ls
    ei_z = dzi / Li

    denom = rigidez_relativa_superior * es_z + rigidez_relativa_inferior * ei_z
    if abs(denom) < 1e-9:
        return None

    k = forca_longitudinal_na_roda_N / denom
    F_sup_ax = k * rigidez_relativa_superior
    F_inf_ax = k * rigidez_relativa_inferior

    res_sup = decompor_forca_axial_no_braco(p_sup_in, p_sup_out, F_sup_ax)
    res_inf = decompor_forca_axial_no_braco(p_inf_in, p_inf_out, F_inf_ax)
    if res_sup is None or res_inf is None:
        return None

    total = {
        "Fx": res_sup["Fx"] + res_inf["Fx"],
        "Fy": res_sup["Fy"] + res_inf["Fy"],
        "Fz": res_sup["Fz"] + res_inf["Fz"],
    }

    return {"Braco_Superior": res_sup, "Braco_Inferior": res_inf, "Total": total}

# ============================================================
#  BLOCO 3 – CG, MOMENTO DE ROLAGEM, TRANSF. CARGA, CAPOTAMENTO
# ============================================================

def calcular_braco_de_rolagem_da_carroceria(h_CG_mm, h_Ro_mm):
    if h_Ro_mm is None:
        return None
    return h_CG_mm - h_Ro_mm

def calcular_momento_de_rolagem(massa_kg, aceleracao_lateral_mps2, h_CG_mm, h_Ro_mm):
    hBo_mm = calcular_braco_de_rolagem_da_carroceria(h_CG_mm, h_Ro_mm)
    if hBo_mm is None:
        return None
    return massa_kg * aceleracao_lateral_mps2 * mm_to_m(hBo_mm)  # N·m

def calcular_transferencia_de_carga_lateral_total(massa_kg, aceleracao_lateral_mps2, bitola_mm, h_CG_mm):
    if abs(bitola_mm) < 1e-9:
        return None
    return massa_kg * aceleracao_lateral_mps2 * (mm_to_m(h_CG_mm) / mm_to_m(bitola_mm))  # N

def calcular_criterio_geometrico_de_capotamento(bitola_mm, h_CG_mm):
    """
    Critério geométrico (estático, rígido):
      SSF = (bitola/2) / h_CG
      ay_crit = SSF * g
    """
    if abs(h_CG_mm) < 1e-9:
        return None, None
    ssf = (bitola_mm / 2.0) / h_CG_mm
    ay_crit = ssf * G
    return ssf, ay_crit

# ============================================================
#  BLOCO 4 – ESFORÇO TRIANGULAR (SUP/INF) + LIMITE
# ============================================================

def esforco_duplo_braco_por_angulo(
    forca_roda_N,
    ang_sup_deg,
    ang_inf_deg,
    k_sup=0.5,
    k_inf=0.5
):
    """
    Modelo triangular simplificado:
      - cada braço transmite uma parcela da força da roda: F_sup=k_sup*F, F_inf=k_inf*F
      - esforço axial: F_ax = F_i / cos(theta_i)  (theta em relação à vertical)
    """
    if abs(k_sup + k_inf - 1.0) > 1e-6:
        raise ValueError("k_sup + k_inf deve ser igual a 1.0")

    th_sup = math.radians(ang_sup_deg)
    th_inf = math.radians(ang_inf_deg)

    if abs(math.cos(th_sup)) < 1e-9 or abs(math.cos(th_inf)) < 1e-9:
        return None

    F_sup = k_sup * forca_roda_N
    F_inf = k_inf * forca_roda_N

    F_ax_sup = F_sup / math.cos(th_sup)
    F_ax_inf = F_inf / math.cos(th_inf)

    return {
        "F_ax_sup": F_ax_sup,
        "F_ax_inf": F_ax_inf,
        "F_ax_total": F_ax_sup + F_ax_inf
    }

# ============================================================
#  INTERFACE TKINTER (TEMA CLARO)
# ============================================================

def aplicar_tema_branco(root):
    root.configure(bg="#FFFFFF")
    style = ttk.Style(root)
    style.theme_use("clam")

    style.configure(".", background="#FFFFFF", foreground="#000000")
    style.configure("TFrame", background="#FFFFFF")
    style.configure("TLabel", background="#FFFFFF", foreground="#000000")

    style.configure("TNotebook", background="#FFFFFF", borderwidth=0)
    style.configure("TNotebook.Tab", background="#E6E6E6", foreground="#000000", padding=(10, 6))
    style.map("TNotebook.Tab", background=[("selected", "#D0D0D0")])

    style.configure("TEntry", fieldbackground="#FFFFFF", background="#FFFFFF", foreground="#000000")

    style.configure("TButton", background="#E0E0E0", foreground="#000000", padding=(10, 6))
    style.map("TButton", background=[("active", "#C8C8C8")])

    style.configure("Treeview", background="#FFFFFF", fieldbackground="#FFFFFF", foreground="#000000", rowheight=26)
    style.configure("Treeview.Heading", background="#DADADA", foreground="#000000", font=("Arial", 10, "bold"))
    style.map("Treeview", background=[("selected", "#B0C4DE")], foreground=[("selected", "#000000")])

class App:
    def __init__(self, root):
        self.root = root
        self.root.title("Baja – Suspensão Duplo A – (2D/3D/CG/Esforços)")
        self.root.geometry("1200x850")

        aplicar_tema_branco(self.root)

        notebook = ttk.Notebook(root)
        notebook.pack(fill="both", expand=True)

        self.tab2d = ttk.Frame(notebook)
        self.tab3d = ttk.Frame(notebook)
        self.tab_cg = ttk.Frame(notebook)
        self.tab_esf = ttk.Frame(notebook)
        self.tab_res = ttk.Frame(notebook)

        notebook.add(self.tab2d, text="Vista frontal 2D (Roll Centre)")
        notebook.add(self.tab3d, text="Modelo 3D + Esforços (XYZ)")
        notebook.add(self.tab_cg, text="CG e Estabilidade")
        notebook.add(self.tab_esf, text="Esforço × Ângulo (com Roll Center)")
        notebook.add(self.tab_res, text="Resultados / Explicação")

        self._last_2d = None
        self._last_forces = None
        self._dados_plot = []

        self._build_tab2d()
        self._build_tab3d()
        self._build_tab_cg()
        self._build_tab_esforcos()
        self._build_tab_res()

        # para Treeviews expandirem
        for tab in (self.tab2d, self.tab3d, self.tab_cg, self.tab_esf):
            tab.grid_rowconfigure(99, weight=1)
            tab.grid_columnconfigure(0, weight=1)

    # ---------------- ABA 2D ----------------
    def _build_tab2d(self):
        f = self.tab2d
        row = 0

        ttk.Label(f, text="Geometria 2D – Vista frontal (x lateral, y vertical) [mm]").grid(
            row=row, column=0, columnspan=4, pady=8, sticky="w"
        )
        row += 1

        ttk.Label(f, text="Bitola dianteira (bf) [mm]:").grid(row=row, column=0, sticky="e")
        self.entry_bf = ttk.Entry(f, width=12)
        self.entry_bf.insert(0, "")
        self.entry_bf.grid(row=row, column=1, sticky="w")
        row += 1

        ttk.Label(f, text="Pivô interno do braço superior (x,y) [mm]:").grid(row=row, column=0, sticky="e")
        self.ux_in = ttk.Entry(f, width=10); self.ux_in.insert(0, ""); self.ux_in.grid(row=row, column=1, sticky="w")
        self.uy_in = ttk.Entry(f, width=10); self.uy_in.insert(0, ""); self.uy_in.grid(row=row, column=2, sticky="w")
        row += 1

        ttk.Label(f, text="Pivô externo do braço superior (x,y) [mm]:").grid(row=row, column=0, sticky="e")
        self.ux_out = ttk.Entry(f, width=10); self.ux_out.insert(0, ""); self.ux_out.grid(row=row, column=1, sticky="w")
        self.uy_out = ttk.Entry(f, width=10); self.uy_out.insert(0, ""); self.uy_out.grid(row=row, column=2, sticky="w")
        row += 1

        ttk.Label(f, text="Pivô interno do braço inferior (x,y) [mm]:").grid(row=row, column=0, sticky="e")
        self.lx_in = ttk.Entry(f, width=10); self.lx_in.insert(0, ""); self.lx_in.grid(row=row, column=1, sticky="w")
        self.ly_in = ttk.Entry(f, width=10); self.ly_in.insert(0, ""); self.ly_in.grid(row=row, column=2, sticky="w")
        row += 1

        ttk.Label(f, text="Pivô externo do braço inferior (x,y) [mm]:").grid(row=row, column=0, sticky="e")
        self.lx_out = ttk.Entry(f, width=10); self.lx_out.insert(0, ""); self.lx_out.grid(row=row, column=1, sticky="w")
        self.ly_out = ttk.Entry(f, width=10); self.ly_out.insert(0, ""); self.ly_out.grid(row=row, column=2, sticky="w")
        row += 1

        ttk.Separator(f, orient="horizontal").grid(row=row, column=0, columnspan=4, sticky="ew", pady=10)
        row += 1

        ttk.Label(f, text="Curso roda externa (s1) [mm]:").grid(row=row, column=0, sticky="e")
        self.entry_s1 = ttk.Entry(f, width=10); self.entry_s1.insert(0, ""); self.entry_s1.grid(row=row, column=1, sticky="w")
        ttk.Label(f, text="Curso roda interna (s2) [mm]:").grid(row=row, column=2, sticky="e")
        self.entry_s2 = ttk.Entry(f, width=10); self.entry_s2.insert(0, ""); self.entry_s2.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Label(f, text="Cambagem roda externa (γ_o) [°]:").grid(row=row, column=0, sticky="e")
        self.entry_camber_o = ttk.Entry(f, width=10); self.entry_camber_o.insert(0, ""); self.entry_camber_o.grid(row=row, column=1, sticky="w")
        ttk.Label(f, text="Cambagem roda interna (γ_i) [°]:").grid(row=row, column=2, sticky="e")
        self.entry_camber_i = ttk.Entry(f, width=10); self.entry_camber_i.insert(0, ""); self.entry_camber_i.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Button(f, text="Calcular 2D", command=self._exec_2d).grid(row=row, column=0, pady=10)
        ttk.Button(f, text="Plotar 2D", command=self._plot_2d).grid(row=row, column=1, pady=10)
        row += 1

        self.label_roll = ttk.Label(f, text="Centro Instantâneo de Rotação e Centro de Rolagem: --")
        self.label_roll.grid(row=row, column=0, columnspan=4, sticky="w")
        row += 1

        self.label_curv = ttk.Label(f, text="Curvatura Cinemática de Cambagem: --")
        self.label_curv.grid(row=row, column=0, columnspan=4, sticky="w")

    def _exec_2d(self):
        try:
            bf = safe_float(self.entry_bf.get())

            ux_in = safe_float(self.ux_in.get());  uy_in = safe_float(self.uy_in.get())
            ux_out = safe_float(self.ux_out.get()); uy_out = safe_float(self.uy_out.get())
            lx_in = safe_float(self.lx_in.get());  ly_in = safe_float(self.ly_in.get())
            lx_out = safe_float(self.lx_out.get()); ly_out = safe_float(self.ly_out.get())

            xP, yP, h_Ro, q = calcular_centro_instantaneo_de_rotacao_e_centro_de_rolagem(
                bf, ux_in, uy_in, ux_out, uy_out, lx_in, ly_in, lx_out, ly_out
            )

            if xP is None:
                self.label_roll.config(text="Centro Instantâneo indefinido (braços paralelos).")
            else:
                self.label_roll.config(
                    text=(
                        f"IC=({fmt_num(xP,1)} , {fmt_num(yP,1)}) mm | "
                        f"h_Ro={fmt_num(h_Ro,1,' mm')} | q={fmt_num(q,3)}"
                    )
                )

            s1 = safe_float(self.entry_s1.get())
            s2 = safe_float(self.entry_s2.get())
            camber_o = safe_float(self.entry_camber_o.get())
            camber_i = safe_float(self.entry_camber_i.get())

            dphi_rad, dphi_deg, dgamma_k, k_gamma = calcular_curvatura_cinematica_de_cambagem(
                bf, s1, s2, camber_o, camber_i
            )

            if dphi_rad is None:
                self.label_curv.config(text="Curvatura: não foi possível calcular (bf=0).")
            else:
                self.label_curv.config(
                    text=(
                        f"dφ={fmt_num(dphi_deg,3,'°')} | "
                        f"Δγ_W,k={fmt_num(dgamma_k,3,'°')} | "
                        f"kγ={fmt_num(k_gamma,3)}"
                    )
                )

            self._last_2d = {"bf": bf, "IC": (xP, yP), "h_Ro": h_Ro, "q": q}

            # Atualiza automaticamente o h_Ro mostrado na aba de esforços
            if h_Ro is not None:
                self.esf_hro_calc.config(text=f"h_Ro (da aba 2D) = {fmt_num(h_Ro,1,' mm')}")
            else:
                self.esf_hro_calc.config(text="h_Ro (da aba 2D) = --")

        except ValueError:
            messagebox.showerror("Erro", "Preencha corretamente TODOS os campos numéricos da aba 2D.")

    def _plot_2d(self):
        try:
            bf = safe_float(self.entry_bf.get())
            ux_in = safe_float(self.ux_in.get()); uy_in = safe_float(self.uy_in.get())
            ux_out = safe_float(self.ux_out.get()); uy_out = safe_float(self.uy_out.get())
            lx_in = safe_float(self.lx_in.get()); ly_in = safe_float(self.ly_in.get())
            lx_out = safe_float(self.lx_out.get()); ly_out = safe_float(self.ly_out.get())

            xP, yP, h_Ro, _ = calcular_centro_instantaneo_de_rotacao_e_centro_de_rolagem(
                bf, ux_in, uy_in, ux_out, uy_out, lx_in, ly_in, lx_out, ly_out
            )

            fig, ax = plt.subplots()
            ax.plot([ux_in, ux_out], [uy_in, uy_out], "o-", label="Braço superior")
            ax.plot([lx_in, lx_out], [ly_in, ly_out], "o-", label="Braço inferior")

            if xP is not None:
                ax.plot(xP, yP, "s", label="IC")

            xW, yW = bf / 2.0, 0.0
            ax.plot(xW, yW, "o", label="Centro da roda (W)")

            if h_Ro is not None:
                ax.plot(0, h_Ro, "*", label="Roll Center (Ro)")

            ax.axvline(0, linestyle="--")
            ax.set_aspect("equal", adjustable="box")
            ax.grid(True)
            ax.set_xlabel("x [mm] (lateral)")
            ax.set_ylabel("y [mm] (vertical)")
            ax.legend()
            plt.show()

        except ValueError:
            messagebox.showerror("Erro", "Valores inválidos para plotar 2D (verifique campos).")

    # ---------------- ABA 3D ----------------
    def _build_tab3d(self):
        f = self.tab3d
        row = 0

        ttk.Label(f, text="Geometria 3D dos braços (x, y, z) [mm]").grid(
            row=row, column=0, columnspan=6, sticky="w", pady=8
        )
        row += 1

        labels_pts = [
            ("Sup_In X", "Sup_In Y", "Sup_In Z"),
            ("Sup_Out X", "Sup_Out Y", "Sup_Out Z"),
            ("Inf_In X", "Inf_In Y", "Inf_In Z"),
            ("Inf_Out X", "Inf_Out Y", "Inf_Out Z"),
        ]
        self.entries_3d = {}

        for (lx, ly, lz) in labels_pts:
            ttk.Label(f, text=f"{lx},{ly},{lz}:").grid(row=row, column=0, sticky="e")
            ex = ttk.Entry(f, width=10); ex.insert(0, ""); ex.grid(row=row, column=1, sticky="w")
            ey = ttk.Entry(f, width=10); ey.insert(0, ""); ey.grid(row=row, column=2, sticky="w")
            ez = ttk.Entry(f, width=10); ez.insert(0, ""); ez.grid(row=row, column=3, sticky="w")
            self.entries_3d[lx] = ex; self.entries_3d[ly] = ey; self.entries_3d[lz] = ez
            row += 1

        ttk.Separator(f, orient="horizontal").grid(row=row, column=0, columnspan=6, sticky="ew", pady=10)
        row += 1

        ttk.Label(f, text="Pontos da manga (para cambagem/caster) [mm]").grid(
            row=row, column=0, columnspan=6, sticky="w", pady=8
        )
        row += 1

        ttk.Label(f, text="Manga superior (x,y,z):").grid(row=row, column=0, sticky="e")
        self.cam_sup_x = ttk.Entry(f, width=10); self.cam_sup_x.insert(0, ""); self.cam_sup_x.grid(row=row, column=1, sticky="w")
        self.cam_sup_y = ttk.Entry(f, width=10); self.cam_sup_y.insert(0, ""); self.cam_sup_y.grid(row=row, column=2, sticky="w")
        self.cam_sup_z = ttk.Entry(f, width=10); self.cam_sup_z.insert(0, ""); self.cam_sup_z.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Label(f, text="Manga inferior (x,y,z):").grid(row=row, column=0, sticky="e")
        self.cam_inf_x = ttk.Entry(f, width=10); self.cam_inf_x.insert(0, ""); self.cam_inf_x.grid(row=row, column=1, sticky="w")
        self.cam_inf_y = ttk.Entry(f, width=10); self.cam_inf_y.insert(0, ""); self.cam_inf_y.grid(row=row, column=2, sticky="w")
        self.cam_inf_z = ttk.Entry(f, width=10); self.cam_inf_z.insert(0, ""); self.cam_inf_z.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Label(f, text="Pontos para toe (vista superior) [mm]").grid(
            row=row, column=0, columnspan=6, sticky="w", pady=8
        )
        row += 1

        ttk.Label(f, text="Face dianteira (x,y,z):").grid(row=row, column=0, sticky="e")
        self.toe_front_x = ttk.Entry(f, width=10); self.toe_front_x.insert(0, ""); self.toe_front_x.grid(row=row, column=1, sticky="w")
        self.toe_front_y = ttk.Entry(f, width=10); self.toe_front_y.insert(0, ""); self.toe_front_y.grid(row=row, column=2, sticky="w")
        self.toe_front_z = ttk.Entry(f, width=10); self.toe_front_z.insert(0, ""); self.toe_front_z.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Label(f, text="Face traseira (x,y,z):").grid(row=row, column=0, sticky="e")
        self.toe_rear_x = ttk.Entry(f, width=10); self.toe_rear_x.insert(0, ""); self.toe_rear_x.grid(row=row, column=1, sticky="w")
        self.toe_rear_y = ttk.Entry(f, width=10); self.toe_rear_y.insert(0, ""); self.toe_rear_y.grid(row=row, column=2, sticky="w")
        self.toe_rear_z = ttk.Entry(f, width=10); self.toe_rear_z.insert(0, ""); self.toe_rear_z.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Label(f, text="Rigidez relativa: braço superior / inferior [-]:").grid(row=row, column=0, sticky="e")
        self.rig_sup = ttk.Entry(f, width=10); self.rig_sup.insert(0, "1.0"); self.rig_sup.grid(row=row, column=1, sticky="w")
        self.rig_inf = ttk.Entry(f, width=10); self.rig_inf.insert(0, "1.0"); self.rig_inf.grid(row=row, column=2, sticky="w")
        row += 1

        ttk.Label(f, text="Força longitudinal na roda (eixo z) [N]:").grid(row=row, column=0, sticky="e")
        self.fx_long = ttk.Entry(f, width=12); self.fx_long.insert(0, ""); self.fx_long.grid(row=row, column=1, sticky="w")
        row += 1

        ttk.Button(f, text="Calcular 3D + Esforços", command=self._exec_3d).grid(row=row, column=0, pady=10)
        row += 1

        self.label_3d_geom = ttk.Label(f, text="Centro Instantâneo projetado (vista frontal): --")
        self.label_3d_geom.grid(row=row, column=0, columnspan=6, sticky="w"); row += 1

        self.label_3d_angles = ttk.Label(f, text="Cambagem / Caster / Toe: --")
        self.label_3d_angles.grid(row=row, column=0, columnspan=6, sticky="w"); row += 1

        self.label_3d_forces = ttk.Label(f, text="Componentes de força nos braços (x,y,z): --")
        self.label_3d_forces.grid(row=row, column=0, columnspan=6, sticky="w")

    def _exec_3d(self):
        try:
            def get(label): return safe_float(self.entries_3d[label].get())

            p_sup_in  = (get("Sup_In X"),  get("Sup_In Y"),  get("Sup_In Z"))
            p_sup_out = (get("Sup_Out X"), get("Sup_Out Y"), get("Sup_Out Z"))
            p_inf_in  = (get("Inf_In X"),  get("Inf_In Y"),  get("Inf_In Z"))
            p_inf_out = (get("Inf_Out X"), get("Inf_Out Y"), get("Inf_Out Z"))

            IC = calcular_centro_instantaneo_projetado_vista_frontal(p_sup_in, p_sup_out, p_inf_in, p_inf_out)

            cam_sup = (safe_float(self.cam_sup_x.get()), safe_float(self.cam_sup_y.get()), safe_float(self.cam_sup_z.get()))
            cam_inf = (safe_float(self.cam_inf_x.get()), safe_float(self.cam_inf_y.get()), safe_float(self.cam_inf_z.get()))
            cambagem = calcular_angulo_de_cambagem(cam_sup, cam_inf)
            caster   = calcular_angulo_de_caster(cam_sup, cam_inf)

            toe_front = (safe_float(self.toe_front_x.get()), safe_float(self.toe_front_y.get()), safe_float(self.toe_front_z.get()))
            toe_rear  = (safe_float(self.toe_rear_x.get()),  safe_float(self.toe_rear_y.get()),  safe_float(self.toe_rear_z.get()))
            toe = calcular_angulo_de_convergencia_divergencia_toe(toe_front, toe_rear)

            Fx_long = safe_float(self.fx_long.get())
            rig_sup = safe_float(self.rig_sup.get())
            rig_inf = safe_float(self.rig_inf.get())

            forces = calcular_forcas_no_duplo_A(p_sup_in, p_sup_out, p_inf_in, p_inf_out, Fx_long, rig_sup, rig_inf)

            if IC is None:
                self.label_3d_geom.config(text="Centro Instantâneo: indefinido (braços paralelos na projeção frontal).")
            else:
                self.label_3d_geom.config(text=f"IC (projetado) = ({fmt_num(IC[0],1)} , {fmt_num(IC[1],1)}) mm")

            self.label_3d_angles.config(
                text=f"Cambagem={fmt_num(cambagem,2,'°')} | Caster={fmt_num(caster,2,'°')} | Toe={fmt_num(toe,2,'°')}"
            )

            if forces is None:
                self.label_3d_forces.config(text="Forças: não foi possível calcular (geometria degenerada/denominador ~0).")
                self._last_forces = None
            else:
                sup = forces["Braco_Superior"]
                inf = forces["Braco_Inferior"]
                tot = forces["Total"]
                self.label_3d_forces.config(
                    text=(
                        f"Sup (Fx,Fy,Fz)=({fmt_num(sup['Fx'],1)}, {fmt_num(sup['Fy'],1)}, {fmt_num(sup['Fz'],1)}) N | "
                        f"Inf (Fx,Fy,Fz)=({fmt_num(inf['Fx'],1)}, {fmt_num(inf['Fy'],1)}, {fmt_num(inf['Fz'],1)}) N | "
                        f"Total=({fmt_num(tot['Fx'],1)}, {fmt_num(tot['Fy'],1)}, {fmt_num(tot['Fz'],1)}) N"
                    )
                )
                self._last_forces = forces

        except ValueError:
            messagebox.showerror("Erro", "Preencha corretamente TODOS os campos numéricos da aba 3D.")

    # ---------------- ABA CG ----------------
    def _build_tab_cg(self):
        f = self.tab_cg
        row = 0

        ttk.Label(
            f,
            text="Centro de Gravidade e Estabilidade Lateral",
            font=("Arial", 12, "bold")
        ).grid(row=row, column=0, columnspan=6, sticky="w", pady=8)
        row += 1

        ttk.Label(f, text="Massa [kg]:").grid(row=row, column=0, sticky="e")
        self.cg_massa = ttk.Entry(f, width=12)
        self.cg_massa.insert(0, "200")
        self.cg_massa.grid(row=row, column=1, sticky="w")

        ttk.Label(f, text="Altura do CG (h_CG) [mm]:").grid(row=row, column=2, sticky="e")
        self.cg_hcg = ttk.Entry(f, width=12)
        self.cg_hcg.insert(0, "450")
        self.cg_hcg.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Label(f, text="Bitola (track) [mm]:").grid(row=row, column=0, sticky="e")
        self.cg_track = ttk.Entry(f, width=12)
        self.cg_track.insert(0, "1400")
        self.cg_track.grid(row=row, column=1, sticky="w")

        ttk.Label(f, text="Aceleração lateral (a_y) [m/s²]:").grid(row=row, column=2, sticky="e")
        self.cg_ay = ttk.Entry(f, width=12)
        self.cg_ay.insert(0, "8.55")
        self.cg_ay.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Label(f, text="Altura do Roll Center (h_Ro) [mm] (vazio: usa da aba 2D):").grid(row=row, column=0, sticky="e")
        self.cg_hro = ttk.Entry(f, width=12)
        self.cg_hro.insert(0, "")
        self.cg_hro.grid(row=row, column=1, sticky="w")
        row += 1

        ttk.Button(f, text="Calcular CG", command=self._exec_cg).grid(row=row, column=0, pady=8)
        row += 1

        self.label_cg_out = ttk.Label(f, text="Resultados: --")
        self.label_cg_out.grid(row=row, column=0, columnspan=6, sticky="w", pady=5)

    def _get_hro(self):
        if self._last_2d is not None and self._last_2d.get("h_Ro") is not None:
            return float(self._last_2d["h_Ro"])
        s = (self.cg_hro.get() or "").strip()
        if s == "":
            return None
        return safe_float(s)

    def _exec_cg(self):
        try:
            m = safe_float(self.cg_massa.get())
            h_CG = safe_float(self.cg_hcg.get())
            track = safe_float(self.cg_track.get())
            ay = safe_float(self.cg_ay.get())
            h_Ro = self._get_hro()

            hBo = calcular_braco_de_rolagem_da_carroceria(h_CG, h_Ro)
            M_roll = calcular_momento_de_rolagem(m, ay, h_CG, h_Ro) if h_Ro is not None else None
            dFz = calcular_transferencia_de_carga_lateral_total(m, ay, track, h_CG)
            ssf, ay_crit = calcular_criterio_geometrico_de_capotamento(track, h_CG)

            self.label_cg_out.config(
                text=(
                    f"Massa={fmt_num(m,0,' kg')} | h_CG={fmt_num(h_CG,1,' mm')} | "
                    f"h_Ro={fmt_num(h_Ro,1,' mm')} | h_Bo={fmt_num(hBo,1,' mm')} | "
                    f"M_roll={fmt_num(M_roll,2,' N·m')} | ΔFz_total={fmt_num(dFz,1,' N')} | "
                    f"SSF={fmt_num(ssf,3)} | a_y,crit={fmt_num(ay_crit,2,' m/s²')}"
                )
            )

        except ValueError:
            messagebox.showerror("Erro", "Preencha corretamente os campos da aba CG.")

    # ---------------- ABA ESFORÇOS × ÂNGULO (COM RC) ----------------
    def _build_tab_esforcos(self):
        f = self.tab_esf
        row = 0

        ttk.Label(f, text="Esforço axial nos braços (modelo triangular) + ligação com Roll Center",
                  font=("Arial", 12, "bold")).grid(row=row, column=0, columnspan=6, sticky="w", pady=8)
        row += 1

        # Info h_Ro da aba 2D (auto)
        self.esf_hro_calc = ttk.Label(f, text="h_Ro (da aba 2D) = --")
        self.esf_hro_calc.grid(row=row, column=0, columnspan=6, sticky="w")
        row += 1

        ttk.Separator(f, orient="horizontal").grid(row=row, column=0, columnspan=6, sticky="ew", pady=8)
        row += 1

        # --------- Força na roda: manual OU via RC/CG ----------
        ttk.Label(f, text="Força vertical equivalente na roda (F_roda) [N]:").grid(row=row, column=0, sticky="e")
        self.esf_forca = ttk.Entry(f, width=14)
        self.esf_forca.insert(0, "3000")
        self.esf_forca.grid(row=row, column=1, sticky="w")

        self.var_add_static = tk.IntVar(value=0)
        ttk.Checkbutton(f, text="Somar carga estática (m·g/4)", variable=self.var_add_static)\
            .grid(row=row, column=2, columnspan=2, sticky="w")

        ttk.Button(f, text="Calcular F_roda via RC/CG", command=self._calc_forca_roda_via_rc)\
            .grid(row=row, column=4, sticky="w")
        row += 1

        self.label_forca_rc = ttk.Label(f, text="ΔFz_total / ΔFz_geo(RC) / ΔFz_el(h_Bo): --")
        self.label_forca_rc.grid(row=row, column=0, columnspan=6, sticky="w")
        row += 1

        ttk.Separator(f, orient="horizontal").grid(row=row, column=0, columnspan=6, sticky="ew", pady=8)
        row += 1

        # --------- Ângulos e rigidez ----------
        ttk.Label(f, text="Ângulo braço superior base [°]:").grid(row=row, column=0, sticky="e")
        self.ang_sup = ttk.Entry(f, width=10); self.ang_sup.insert(0, "10"); self.ang_sup.grid(row=row, column=1, sticky="w")

        ttk.Label(f, text="Ângulo braço inferior base [°]:").grid(row=row, column=2, sticky="e")
        self.ang_inf = ttk.Entry(f, width=10); self.ang_inf.insert(0, "20"); self.ang_inf.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Label(f, text="k_sup [-]:").grid(row=row, column=0, sticky="e")
        self.k_sup = ttk.Entry(f, width=10); self.k_sup.insert(0, "0.5"); self.k_sup.grid(row=row, column=1, sticky="w")

        ttk.Label(f, text="k_inf [-]:").grid(row=row, column=2, sticky="e")
        self.k_inf = ttk.Entry(f, width=10); self.k_inf.insert(0, "0.5"); self.k_inf.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Label(f, text="Limite estrutural por braço [N]:").grid(row=row, column=0, sticky="e")
        self.limite_estrutural = ttk.Entry(f, width=12); self.limite_estrutural.insert(0, "8000"); self.limite_estrutural.grid(row=row, column=1, sticky="w")
        row += 1

        # --------- Varredura ----------
        ttk.Label(f, text="Ângulo varrido min [°]:").grid(row=row, column=0, sticky="e")
        self.esf_ang_min = ttk.Entry(f, width=10); self.esf_ang_min.insert(0, "0"); self.esf_ang_min.grid(row=row, column=1, sticky="w")

        ttk.Label(f, text="Ângulo varrido max [°]:").grid(row=row, column=2, sticky="e")
        self.esf_ang_max = ttk.Entry(f, width=10); self.esf_ang_max.insert(0, "40"); self.esf_ang_max.grid(row=row, column=3, sticky="w")

        ttk.Label(f, text="Passo [°]:").grid(row=row, column=4, sticky="e")
        self.esf_passo = ttk.Entry(f, width=10); self.esf_passo.insert(0, "5"); self.esf_passo.grid(row=row, column=5, sticky="w")
        row += 1

        ttk.Button(f, text="Gerar tabela + plot", command=self._exec_esforcos_angulo).grid(row=row, column=0, pady=10)
        row += 1

        # --------- TABELA ----------
        cols = ("angulo", "F_sup", "F_inf", "F_total", "status")
        self.tree_esf = ttk.Treeview(f, columns=cols, show="headings", height=12)
        self.tree_esf.grid(row=row, column=0, columnspan=6, sticky="nsew", pady=8)

        headers = {
            "angulo": "ΔÂngulo [°]",
            "F_sup": "F axial sup [N]",
            "F_inf": "F axial inf [N]",
            "F_total": "F total [N]",
            "status": "Limite"
        }
        for c in cols:
            self.tree_esf.heading(c, text=headers[c])
            self.tree_esf.column(c, width=160, anchor="center")

        f.grid_rowconfigure(row, weight=1)
        for col in range(6):
            f.grid_columnconfigure(col, weight=1)

    def _calc_forca_roda_via_rc(self):
        """
        Ligação com roll center:
          - usa (m, ay, h_CG, track) e h_Ro (da aba 2D ou entrada da aba CG)
          - calcula ΔFz_total, ΔFz_geo (via RC), ΔFz_el (via h_Bo)
          - define F_roda como ΔFz_total/2 (roda externa) e opcionalmente soma m*g/4
        """
        try:
            m = safe_float(self.cg_massa.get())
            ay = safe_float(self.cg_ay.get())
            h_CG = safe_float(self.cg_hcg.get())
            track = safe_float(self.cg_track.get())

            h_Ro = self._get_hro()
            if h_Ro is None:
                raise ValueError("h_Ro indefinido: calcule na aba 2D ou preencha na aba CG.")

            dFz_total = calcular_transferencia_de_carga_lateral_total(m, ay, track, h_CG)
            dFz_geo = m * ay * (mm_to_m(h_Ro) / mm_to_m(track))  # via RC
            hBo = h_CG - h_Ro
            dFz_el = m * ay * (mm_to_m(hBo) / mm_to_m(track))    # via braço de rolagem (hBo)

            # força equivalente na roda externa: +ΔFz_total/2
            F_roda = dFz_total / 2.0

            if self.var_add_static.get() == 1:
                F_roda += (m * G) / 4.0

            self.esf_forca.delete(0, "end")
            self.esf_forca.insert(0, f"{F_roda:.1f}")

            self.label_forca_rc.config(
                text=(
                    f"ΔFz_total={fmt_num(dFz_total,1,' N')} | "
                    f"ΔFz_geo(RC)={fmt_num(dFz_geo,1,' N')} | "
                    f"ΔFz_el(h_Bo)={fmt_num(dFz_el,1,' N')} | "
                    f"F_roda usada={fmt_num(F_roda,1,' N')}"
                )
            )

        except ValueError as e:
            messagebox.showerror("Erro", str(e))

    def _exec_esforcos_angulo(self):
        try:
            F_roda = safe_float(self.esf_forca.get())
            ang_min = int(safe_float(self.esf_ang_min.get()))
            ang_max = int(safe_float(self.esf_ang_max.get()))
            passo = int(safe_float(self.esf_passo.get()))

            ang_sup_base = safe_float(self.ang_sup.get())
            ang_inf_base = safe_float(self.ang_inf.get())
            k_sup = safe_float(self.k_sup.get())
            k_inf = safe_float(self.k_inf.get())
            limite = safe_float(self.limite_estrutural.get())

            if ang_max < ang_min or passo <= 0:
                raise ValueError("Faixa/passo de ângulo inválidos.")

            for i in self.tree_esf.get_children():
                self.tree_esf.delete(i)

            self._dados_plot = []

            for dAng in range(ang_min, ang_max + 1, passo):
                ang_sup = ang_sup_base + dAng
                ang_inf = ang_inf_base + dAng

                res = esforco_duplo_braco_por_angulo(F_roda, ang_sup, ang_inf, k_sup, k_inf)
                if res is None:
                    continue

                status = "OK"
                if res["F_ax_sup"] > limite or res["F_ax_inf"] > limite:
                    status = "⚠ LIMITE"

                self.tree_esf.insert(
                    "", "end",
                    values=(
                        dAng,
                        f"{res['F_ax_sup']:.1f}",
                        f"{res['F_ax_inf']:.1f}",
                        f"{res['F_ax_total']:.1f}",
                        status
                    )
                )

                self._dados_plot.append((dAng, res["F_ax_sup"], res["F_ax_inf"]))

            self._plot_esforcos(limite)

        except ValueError as e:
            messagebox.showerror("Erro", str(e))

    def _plot_esforcos(self, limite):
        if not self._dados_plot:
            return

        ang = [d[0] for d in self._dados_plot]
        Fsup = [d[1] for d in self._dados_plot]
        Finf = [d[2] for d in self._dados_plot]

        plt.figure()
        plt.plot(ang, Fsup, label="Braço superior")
        plt.plot(ang, Finf, label="Braço inferior")
        plt.plot(ang, [limite] * len(ang), "r--", label="Limite estrutural")

        plt.xlabel("ΔÂngulo [°]")
        plt.ylabel("Força axial [N]")
        plt.grid(True)
        plt.legend()
        plt.show()

    # ---------------- ABA RESULTADOS ----------------
    def _build_tab_res(self):
        txt = tk.Text(self.tab_res, font=("Courier", 10), bg="#0E0E0E", fg="#E6E6E6", insertbackground="#E6E6E6")
        txt.pack(fill="both", expand=True)

        texto = """
RESUMO

A) Vista frontal 2D:
- IC pela interseção das retas dos braços.
- Roll Center (h_Ro): linha IC–W interceptando x=0.

B) Ligação com Roll Center (aba "Esforço × Ângulo"):
- ΔFz_total = m * a_y * h_CG / track
- Separação conceitual:
    ΔFz_geo (via RC)  = m * a_y * h_Ro / track
    ΔFz_el  (via h_Bo)= m * a_y * (h_CG - h_Ro) / track
- Força usada na roda externa por padrão:
    F_roda = ΔFz_total/2
  (opcional: somar m*g/4 para incluir carga estática)

C) Modelo triangular de braços:
- F_sup = k_sup * F_roda ;  F_inf = k_inf * F_roda  (k_sup+k_inf=1)
- F_ax = F_i / cos(theta_i)  (theta em relação à vertical)
- Aumentar o ângulo aumenta o esforço axial de forma não-linear.
"""
        txt.insert("1.0", texto)
        txt.configure(state="disabled")

if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()
