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
#  INTERFACE TKINTER (TEMA ESCURO)
# ============================================================

def aplicar_tema_branco(root):
    root.configure(bg="#FFFFFF")
    style = ttk.Style(root)
    style.theme_use("clam")

    # Estilo base
    style.configure(
        ".",
        background="#FFFFFF",
        foreground="#000000"
    )

    # Frames e labels
    style.configure("TFrame", background="#FFFFFF")
    style.configure("TLabel", background="#FFFFFF", foreground="#000000")

    # Notebook
    style.configure(
        "TNotebook",
        background="#FFFFFF",
        borderwidth=0
    )

    style.configure(
        "TNotebook.Tab",
        background="#E6E6E6",
        foreground="#000000",
        padding=(10, 6)
    )

    style.map(
        "TNotebook.Tab",
        background=[("selected", "#D0D0D0")]
    )

    # Entradas de texto
    style.configure(
        "TEntry",
        fieldbackground="#FFFFFF",
        background="#FFFFFF",
        foreground="#000000"
    )

    # Botões
    style.configure(
        "TButton",
        background="#E0E0E0",
        foreground="#000000",
        padding=(10, 6)
    )

    style.map(
        "TButton",
        background=[("active", "#C8C8C8")]
    )

    # -----------------------------
    # TREEVIEW (TABELAS)
    # -----------------------------
    style.configure(
        "Treeview",
        background="#FFFFFF",
        fieldbackground="#FFFFFF",
        foreground="#000000",
        rowheight=26
    )

    style.configure(
        "Treeview.Heading",
        background="#DADADA",
        foreground="#000000",
        font=("Arial", 10, "bold")
    )

    style.map(
        "Treeview",
        background=[("selected", "#B0C4DE")],  # azul claro ao selecionar
        foreground=[("selected", "#000000")]
    )

class App:
    def __init__(self, root):
        self.root = root
        self.root.title("Baja – Suspensão Duplo A – (2D/3D/CG)")
        self.root.geometry("1200x850")



        notebook = ttk.Notebook(root)
        notebook.pack(fill="both", expand=True)

        self.tab2d = ttk.Frame(notebook)
        self.tab3d = ttk.Frame(notebook)
        self.tab_cg = ttk.Frame(notebook)
        self.tab_res = ttk.Frame(notebook)

        notebook.add(self.tab2d, text="Vista frontal 2D (Roll Centre)")
        notebook.add(self.tab3d, text="Modelo 3D + Esforços")
        notebook.add(self.tab_cg, text="CG e Estabilidade")
        notebook.add(self.tab_res, text="Resultados / Explicação")

        self._last_2d = None
        self._last_forces = None

        self._build_tab2d()
        self._build_tab3d()
        self._build_tab_cg()
        self._build_tab_res()

    # ---------- ABA 2D ----------
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

    # ---------- ABA 3D ----------
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

    # ---------- ABA CG / ESTABILIDADE (COM MASSA VARIÁVEL + TABELA) ----------
    def _build_tab_cg(self):
        f = self.tab_cg
        row = 0

        ttk.Label(
            f,
            text="Centro de Gravidade e Estabilidade Lateral",
            font=("Arial", 12, "bold")
        ).grid(row=row, column=0, columnspan=6, sticky="w", pady=8)
        row += 1

        ttk.Label(f, text="Massa inicial [kg]:").grid(row=row, column=0, sticky="e")
        self.cg_massa_ini = ttk.Entry(f, width=12)
        self.cg_massa_ini.insert(0, "180")
        self.cg_massa_ini.grid(row=row, column=1, sticky="w")

        ttk.Label(f, text="Massa final [kg]:").grid(row=row, column=2, sticky="e")
        self.cg_massa_fim = ttk.Entry(f, width=12)
        self.cg_massa_fim.insert(0, "240")
        self.cg_massa_fim.grid(row=row, column=3, sticky="w")
        row += 1

        ttk.Label(f, text="Altura do CG (h_CG) [mm]:").grid(row=row, column=0, sticky="e")
        self.cg_hcg = ttk.Entry(f, width=12)
        self.cg_hcg.insert(0, "450")
        self.cg_hcg.grid(row=row, column=1, sticky="w")
        row += 1

        ttk.Label(f, text="Bitola (track) [mm]:").grid(row=row, column=0, sticky="e")
        self.cg_track = ttk.Entry(f, width=12)
        self.cg_track.insert(0, "1400")
        self.cg_track.grid(row=row, column=1, sticky="w")
        row += 1

        ttk.Label(f, text="Aceleração lateral (a_y) [m/s²]:").grid(row=row, column=0, sticky="e")
        self.cg_ay = ttk.Entry(f, width=12)
        self.cg_ay.insert(0, "8.55")
        self.cg_ay.grid(row=row, column=1, sticky="w")
        row += 1

        ttk.Label(
            f,
            text="Altura do Roll Center (h_Ro) [mm] (se vazio, usa da aba 2D):"
        ).grid(row=row, column=0, sticky="e")
        self.cg_hro = ttk.Entry(f, width=12)
        self.cg_hro.insert(0, "")
        self.cg_hro.grid(row=row, column=1, sticky="w")
        row += 1

        ttk.Button(
            f,
            text="Calcular (massa fixa)",
            command=self._exec_cg
        ).grid(row=row, column=0, pady=8)

        ttk.Button(
            f,
            text="Gerar tabela (massa variável)",
            command=self._exec_cg_tabela
        ).grid(row=row, column=1, pady=8)
        row += 1

        self.label_cg_out = ttk.Label(f, text="Resultados (massa fixa): --")
        self.label_cg_out.grid(row=row, column=0, columnspan=6, sticky="w", pady=5)
        row += 1

        cols = (
            "massa", "H_rolagem(H_roll)", "Braço de rolagem(H_bo)", "M_Rolagem",
            "Transf_carga(ΔFz )", "Fator de estabilidade", "aycrit", "margem"
        )

        self.tree_cg = ttk.Treeview(f, columns=cols, show="headings", height=10)
        self.tree_cg.grid(row=row, column=0, columnspan=6, sticky="nsew", pady=8)

        headers = {
            "massa": "Massa [kg]",
            "H_rolagem(H_roll)": "H_rolagem(H_roll)mm",
            "Braço de rolagem(H_bo)": "Braço de rolagem(H_bo)[mm]",
            "M_Rolagem": "M_Rolagem[N·m]",
            "Transf_carga(ΔFz )": "ΔFz [N]",
            "Fator de estabilidade": "Fator de estabilidade [-]",
            "aycrit": "a_y,crit [m/s²]",
            "margem": "a_y / a_y,crit"
        }

        for c in cols:
            self.tree_cg.heading(c, text=headers[c])
            self.tree_cg.column(c, width=120, anchor="center")

    def _get_hro_para_cg(self):
        if self._last_2d is not None and self._last_2d.get("h_Ro") is not None:
            return float(self._last_2d["h_Ro"])
        return safe_float(self.cg_hro.get())

    def _exec_cg(self):
        try:
            m = safe_float(self.cg_massa_ini.get())
            h_CG = safe_float(self.cg_hcg.get())
            track = safe_float(self.cg_track.get())
            ay = safe_float(self.cg_ay.get())
            h_Ro = self._get_hro_para_cg()

            hBo = calcular_braco_de_rolagem_da_carroceria(h_CG, h_Ro)
            M_roll = calcular_momento_de_rolagem(m, ay, h_CG, h_Ro)
            dFz = calcular_transferencia_de_carga_lateral_total(m, ay, track, h_CG)
            ssf, ay_crit = calcular_criterio_geometrico_de_capotamento(track, h_CG)

            margem = ay / ay_crit if ay_crit else None
            status = "OK" if ay < ay_crit else "RISCO (critério geométrico)"

            self.label_cg_out.config(
                text=(
                    f"Massa={fmt_num(m, 0, ' kg')} | "
                    f"h_Ro={fmt_num(h_Ro, 1, ' mm')} | "
                    f"h_Bo={fmt_num(hBo, 1, ' mm')} | "
                    f"M_roll={fmt_num(M_roll, 2, ' N·m')} | "
                    f"ΔFz={fmt_num(dFz, 1, ' N')} | "
                    f"SSF={fmt_num(ssf, 3)} | "
                    f"a_y,crit={fmt_num(ay_crit, 2, ' m/s²')} | "
                    f"margem={fmt_num(margem, 2)} | {status}"
                )
            )

        except ValueError:
            messagebox.showerror("Erro", "Preencha corretamente os campos da aba CG.")

    def _exec_cg_tabela(self):
        try:
            m_ini = int(safe_float(self.cg_massa_ini.get()))
            m_fim = int(safe_float(self.cg_massa_fim.get()))
            h_CG = safe_float(self.cg_hcg.get())
            track = safe_float(self.cg_track.get())
            ay = safe_float(self.cg_ay.get())
            h_Ro = self._get_hro_para_cg()

            if m_fim < m_ini:
                raise ValueError

            for item in self.tree_cg.get_children():
                self.tree_cg.delete(item)

            for m in range(m_ini, m_fim + 1, 5):
                hBo = calcular_braco_de_rolagem_da_carroceria(h_CG, h_Ro)
                M_roll = calcular_momento_de_rolagem(m, ay, h_CG, h_Ro)
                dFz = calcular_transferencia_de_carga_lateral_total(m, ay, track, h_CG)
                ssf, ay_crit = calcular_criterio_geometrico_de_capotamento(track, h_CG)
                margem = ay / ay_crit if ay_crit else None

                self.tree_cg.insert(
                    "", "end",
                    values=(
                        m,
                        fmt_num(h_Ro, 1),
                        fmt_num(hBo, 1),
                        fmt_num(M_roll, 1),
                        fmt_num(dFz, 1),
                        fmt_num(ssf, 3),
                        fmt_num(ay_crit, 2),
                        fmt_num(margem, 2)
                    )
                )

        except ValueError:
            messagebox.showerror("Erro", "Verifique massas, CG, bitola e aceleração lateral.")

    # ---------- ABA RESULTADOS ----------
    def _build_tab_res(self):
        txt = tk.Text(self.tab_res, font=("Courier", 10), bg="#0E0E0E", fg="#E6E6E6", insertbackground="#E6E6E6")
        txt.pack(fill="both", expand=True)

        texto = """
RESUMO (o que cada bloco te diz)

A) Vista frontal 2D:
- Calcula o Centro Instantâneo (IC) pela interseção das retas dos braços.
- A partir do IC e do centro da roda (W), calcula a altura do Roll Center (h_Ro)
  pela interseção da linha IC–W com o plano central do veículo (x=0).

B) CG e estabilidade:
- Braço de rolagem da carroceria: h_Bo = h_CG - h_Ro.
- Momento de rolagem:
    M_roll = m * a_y * (h_Bo)   [N·m]
- Transferência total de carga lateral (modelo estático):
    ΔFz_total = m * a_y * h_CG / track   [N]
- Critério geométrico de capotamento (estático):
    SSF = (track/2)/h_CG
    a_y,crit = SSF * g
  Se a_y >= a_y,crit: tendência a levantar a roda interna (critério geométrico).

C) Modelo 3D + esforços:
- Calcula cambagem/caster/toe por projeções simples.
- Decompõe forças axiais em x,y,z usando o versor do braço.

D) Fator de cambagem (kγ):
- kγ = Δγ_W,k / dφ
- kγ negativo indica que a geometria está “compensando” parte da rolagem.
E) O fator de estabilidade quanto mais próximo de 1 maior o risco de capotamento
"""
        txt.insert("1.0", texto)
        txt.configure(state="disabled")

if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()
