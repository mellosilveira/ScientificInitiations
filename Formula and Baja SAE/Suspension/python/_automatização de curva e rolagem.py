import tkinter as tk
from tkinter import ttk, messagebox
import math
import matplotlib.pyplot as plt


# ============================================================
#   FUNÇÕES DE CÁLCULO – ROLL CENTRE / CURVATURA (REIMPELL)
# ============================================================

def calcular_rollcentre_e_q(bf, ux_in, uy_in, ux_out, uy_out,
                            lx_in, ly_in, lx_out, ly_out):
    """Cálculo geométrico do Roll Centre para duplo A (vista frontal)."""

    def reta_pontos(x1, y1, x2, y2):
        # ax + by + c = 0
        a = y1 - y2
        b = x2 - x1
        c = x1 * y2 - x2 * y1
        return a, b, c

    a1, b1, c1 = reta_pontos(ux_in, uy_in, ux_out, uy_out)
    a2, b2, c2 = reta_pontos(lx_in, ly_in, lx_out, ly_out)

    det = a1 * b2 - a2 * b1

    if abs(det) < 1e-9:
        return None, None, None, None

    xP = (b1 * c2 - b2 * c1) / det
    yP = (c1 * a2 - c2 * a1) / det

    xW = bf / 2.0
    yW = 0.0

    if abs(xP - xW) < 1e-9:
        h_ro = None
    else:
        m = (yP - yW) / (xP - xW)
        h_ro = m * (0 - xW) + yW

    p = yP - yW

    if h_ro is None or abs(h_ro) < 1e-9:
        q = None
    else:
        q = p * bf / (h_ro ** 2)

    return xP, yP, h_ro, q


def calcular_curvatura_reimpell(bf, s1, s2, camber_o, camber_i):
    """Curvatura segundo Reimpell (Eqs. 3.5, 3.6, 3.7)."""
    if abs(bf) < 1e-9:
        return None, None, None, None

    dphi_rad = (s1 + s2) / bf
    dphi_deg = 57.3 * dphi_rad

    dgamma_k = (camber_o - camber_i) / 2.0

    if abs(dphi_deg) < 1e-9:
        k_gamma = None
    else:
        k_gamma = dgamma_k / dphi_deg

    return dphi_rad, dphi_deg, dgamma_k, k_gamma


# ============================================================
#   FUNÇÕES 3D – ENGENHARIA REVERSA PARA CAMBER/CASTER/TOE
# ============================================================

def calcular_angulo_camber(p_sup, p_inf):
    x_sup, y_sup, _ = p_sup
    x_inf, y_inf, _ = p_inf
    dx = x_sup - x_inf
    dy = y_sup - y_inf
    if abs(dy) < 1e-9:
        return None
    return math.degrees(math.atan(dx / dy))


def calcular_angulo_caster(p_sup, p_inf):
    _, y_sup, z_sup = p_sup
    _, y_inf, z_inf = p_inf
    dz = z_sup - z_inf
    dy = y_sup - y_inf
    if abs(dy) < 1e-9:
        return None
    return math.degrees(math.atan(dz / dy))


def calcular_angulo_toe(p_front, p_rear):
    x_f, _, z_f = p_front
    x_r, _, z_r = p_rear
    dx = x_f - x_r
    dz = z_f - z_r
    if abs(dz) < 1e-9:
        return None
    return math.degrees(math.atan(dx / dz))


# ============================================================
#   CALLBACKS DA ABA 1
# ============================================================

def carregar_padrao_baja():
    """Preenche automaticamente os valores típicos Baja 2D."""
    entry_bf.delete(0, tk.END)
    entry_bf.insert(0, "1250")

    entry_ux_in.delete(0, tk.END);   entry_ux_in.insert(0, "450")
    entry_uy_in.delete(0, tk.END);   entry_uy_in.insert(0, "420")

    entry_ux_out.delete(0, tk.END);  entry_ux_out.insert(0, "625")
    entry_uy_out.delete(0, tk.END);  entry_uy_out.insert(0, "390")

    entry_lx_in.delete(0, tk.END);   entry_lx_in.insert(0, "430")
    entry_ly_in.delete(0, tk.END);   entry_ly_in.insert(0, "210")

    entry_lx_out.delete(0, tk.END);  entry_lx_out.insert(0, "625")
    entry_ly_out.delete(0, tk.END);  entry_ly_out.insert(0, "190")

    entry_s1.delete(0, tk.END); entry_s1.insert(0, "40")
    entry_s2.delete(0, tk.END); entry_s2.insert(0, "30")

    entry_camber_o.delete(0, tk.END); entry_camber_o.insert(0, "-1.5")
    entry_camber_i.delete(0, tk.END); entry_camber_i.insert(0, "-4.0")


def executar_aba1():
    """Executa cálculos 2D + (opcional) 3D."""
    try:
        bf = float(entry_bf.get())

        ux_in = float(entry_ux_in.get());  uy_in = float(entry_uy_in.get())
        ux_out = float(entry_ux_out.get()); uy_out = float(entry_uy_out.get())
        lx_in = float(entry_lx_in.get());  ly_in = float(entry_ly_in.get())
        lx_out = float(entry_lx_out.get()); ly_out = float(entry_ly_out.get())

        # ----------------------------
        #   CÁLCULO DO ROLL CENTRE
        # ----------------------------
        xP, yP, h_ro, q = calcular_rollcentre_e_q(
            bf, ux_in, uy_in, ux_out, uy_out,
            lx_in, ly_in, lx_out, ly_out
        )

        if xP is None:
            label_roll_results.config(text="IC indefinido (braços paralelos).")
        else:
            txt = f"IC=({xP:.1f},{yP:.1f}) | h_Ro={'--' if h_ro is None else f'{h_ro:.1f} mm'} | q={'--' if q is None else f'{q:.3f}'}"
            label_roll_results.config(text=txt)

        # ----------------------------
        #   CURVATURA DE ROLAGEM
        # ----------------------------
        s1 = float(entry_s1.get())
        s2 = float(entry_s2.get())
        camber_o = float(entry_camber_o.get())
        camber_i = float(entry_camber_i.get())

        dphi_rad, dphi_deg, dgamma_k, k_gamma = calcular_curvatura_reimpell(
            bf, s1, s2, camber_o, camber_i
        )

        if dphi_rad is not None:
            txt_c = f"dφ={dphi_deg:.3f}° | dγ_W,k={dgamma_k:.3f}° | kγ={k_gamma:.3f if k_gamma else '--'}"
            label_curv_results.config(text=txt_c)
        else:
            label_curv_results.config(text="Não foi possível calcular curvatura.")

        # ----------------------------
        #   OPCIONAL: CÁLCULO 3D XYZ
        # ----------------------------
        try:
            # Superior e inferior da manga
            xs_up = float(entry_x_sup_1.get())
            ys_up = float(entry_y_sup_1.get())
            zs_up = float(entry_z_sup_1.get())

            xs_lo = float(entry_x_inf_1.get())
            ys_lo = float(entry_y_inf_1.get())
            zs_lo = float(entry_z_inf_1.get())

            p_sup = (xs_up, ys_up, zs_up)
            p_inf = (xs_lo, ys_lo, zs_lo)

            # Eixo de direção
            z_sup_c = float(entry_z_caster_sup_1.get())
            y_sup_c = float(entry_y_caster_sup_1.get())
            z_inf_c = float(entry_z_caster_inf_1.get())
            y_inf_c = float(entry_y_caster_inf_1.get())

            p_cast_sup = (0, y_sup_c, z_sup_c)
            p_cast_inf = (0, y_inf_c, z_inf_c)

            # Toe
            xf = float(entry_x_front_1.get())
            yf = float(entry_y_front_1.get())
            zf = float(entry_z_front_1.get())

            xr = float(entry_x_rear_1.get())
            yr = float(entry_y_rear_1.get())
            zr = float(entry_z_rear_1.get())

            p_front = (xf, yf, zf)
            p_rear = (xr, yr, zr)

            camber = calcular_angulo_camber(p_sup, p_inf)
            caster = calcular_angulo_caster(p_cast_sup, p_cast_inf)
            toe = calcular_angulo_toe(p_front, p_rear)

            txt3d = f"Camber={camber:.2f}° | Caster={caster:.2f}° | Toe={toe:.2f}°"
            label_xyz_results.config(text=txt3d)

        except:
            label_xyz_results.config(text="XYZ não preenchido (opcional).")

        # ----------------------------
        #   PLOT 2D
        # ----------------------------
        fig, ax = plt.subplots()
        ax.plot([ux_in, ux_out], [uy_in, uy_out], "bo-", label="Braço sup")
        ax.plot([lx_in, lx_out], [ly_in, ly_out], "ro-", label="Braço inf")
        if xP is not None:
            ax.plot(xP, yP, "ks", label="IC")
        xW = bf / 2
        ax.plot(xW, 0, "mo", label="Centro roda")

        if h_ro is not None:
            ax.plot(0, h_ro, "c*", label="Roll centre")

        ax.axvline(0, color="gray", linestyle="--")
        ax.legend()
        ax.set_aspect("equal")
        ax.grid(True)
        plt.show()

    except ValueError:
        messagebox.showerror("Erro", "Valores inválidos na Aba 1.")


# ============================================================
#   ABA 2 – XYZ COMPLETO (engenharia reversa)
# ============================================================

def executar_aba2():
    try:
        xs_up = float(entry_x_sup_2.get()); ys_up = float(entry_y_sup_2.get()); zs_up = float(entry_z_sup_2.get())
        xs_lo = float(entry_x_inf_2.get()); ys_lo = float(entry_y_inf_2.get()); zs_lo = float(entry_z_inf_2.get())
        p_sup = (xs_up, ys_up, zs_up)
        p_inf = (xs_lo, ys_lo, zs_lo)

        z_sup_c = float(entry_z_caster_sup_2.get())
        y_sup_c = float(entry_y_caster_sup_2.get())
        z_inf_c = float(entry_z_caster_inf_2.get())
        y_inf_c = float(entry_y_caster_inf_2.get())
        p_cast_sup = (0, y_sup_c, z_sup_c)
        p_cast_inf = (0, y_inf_c, z_inf_c)

        xf = float(entry_x_front_2.get())
        yf = float(entry_y_front_2.get())
        zf = float(entry_z_front_2.get())
        xr = float(entry_x_rear_2.get())
        yr = float(entry_y_rear_2.get())
        zr = float(entry_z_rear_2.get())
        p_front = (xf, yf, zf)
        p_rear = (xr, yr, zr)

        camber = calcular_angulo_camber(p_sup, p_inf)
        caster = calcular_angulo_caster(p_cast_sup, p_cast_inf)
        toe = calcular_angulo_toe(p_front, p_rear)

        label_output_aba2.config(
            text=f"Camber={camber:.2f}° | Caster={caster:.2f}° | Toe={toe:.2f}°"
        )

    except:
        messagebox.showerror("Erro", "Dados XYZ inválidos.")


# ============================================================
#   INTERFACE – JANELA 1200×900
# ============================================================

root = tk.Tk()
root.title("Baja – Suspensão Duplo A – Automação Completa")
root.geometry("1200x900")

notebook = ttk.Notebook(root)
notebook.pack(fill="both", expand=True)


# ============================================================
#   ABA 1 – GEOMETRIA 2D + XYZ OPCIONAL
# ============================================================

aba1 = ttk.Frame(notebook)
notebook.add(aba1, text="Aba 1 – Geometria 2D + XYZ Opcional")

row = 0
# --------------------------
#   BLOCO 1 – 2D
# --------------------------
ttk.Label(aba1, text="Bitola bf [mm]:").grid(row=row, column=0, sticky="e")
entry_bf = ttk.Entry(aba1, width=10); entry_bf.grid(row=row, column=1); row += 1

ttk.Label(aba1, text="Sup interno (x,y):").grid(row=row, column=0, sticky="e")
entry_ux_in = ttk.Entry(aba1, width=10); entry_ux_in.grid(row=row, column=1)
entry_uy_in = ttk.Entry(aba1, width=10); entry_uy_in.grid(row=row, column=2); row += 1

ttk.Label(aba1, text="Sup externo (x,y):").grid(row=row, column=0, sticky="e")
entry_ux_out = ttk.Entry(aba1, width=10); entry_ux_out.grid(row=row, column=1)
entry_uy_out = ttk.Entry(aba1, width=10); entry_uy_out.grid(row=row, column=2); row += 1

ttk.Label(aba1, text="Inf interno (x,y):").grid(row=row, column=0, sticky="e")
entry_lx_in = ttk.Entry(aba1, width=10); entry_lx_in.grid(row=row, column=1)
entry_ly_in = ttk.Entry(aba1, width=10); entry_ly_in.grid(row=row, column=2); row += 1

ttk.Label(aba1, text="Inf externo (x,y):").grid(row=row, column=0, sticky="e")
entry_lx_out = ttk.Entry(aba1, width=10); entry_lx_out.grid(row=row, column=1)
entry_ly_out = ttk.Entry(aba1, width=10); entry_ly_out.grid(row=row, column=2); row += 1

ttk.Button(aba1, text="Carregar padrão Baja", command=carregar_padrao_baja).grid(row=row, column=0)
ttk.Button(aba1, text="Calcular", command=executar_aba1).grid(row=row, column=1); row += 1

label_roll_results = ttk.Label(aba1, text="Roll centre: --")
label_roll_results.grid(row=row, column=0, columnspan=4, sticky="w"); row += 1

label_curv_results = ttk.Label(aba1, text="Curvatura: --")
label_curv_results.grid(row=row, column=0, columnspan=4, sticky="w"); row += 1

# --------------------------
#   BLOCO 2 – XYZ OPCIONAL
# --------------------------
ttk.Label(aba1, text="XYZ (opcional) – Superior (x,y,z):").grid(row=row, column=0)
entry_x_sup_1 = ttk.Entry(aba1, width=10); entry_x_sup_1.grid(row=row, column=1)
entry_y_sup_1 = ttk.Entry(aba1, width=10); entry_y_sup_1.grid(row=row, column=2)
entry_z_sup_1 = ttk.Entry(aba1, width=10); entry_z_sup_1.grid(row=row, column=3); row += 1

ttk.Label(aba1, text="XYZ – Inferior (x,y,z):").grid(row=row, column=0)
entry_x_inf_1 = ttk.Entry(aba1, width=10); entry_x_inf_1.grid(row=row, column=1)
entry_y_inf_1 = ttk.Entry(aba1, width=10); entry_y_inf_1.grid(row=row, column=2)
entry_z_inf_1 = ttk.Entry(aba1, width=10); entry_z_inf_1.grid(row=row, column=3); row += 1

ttk.Label(aba1, text="Caster sup (z,y):").grid(row=row, column=0)
entry_z_caster_sup_1 = ttk.Entry(aba1, width=10); entry_z_caster_sup_1.grid(row=row, column=1)
entry_y_caster_sup_1 = ttk.Entry(aba1, width=10); entry_y_caster_sup_1.grid(row=row, column=2); row += 1

ttk.Label(aba1, text="Caster inf (z,y):").grid(row=row, column=0)
entry_z_caster_inf_1 = ttk.Entry(aba1, width=10); entry_z_caster_inf_1.grid(row=row, column=1)
entry_y_caster_inf_1 = ttk.Entry(aba1, width=10); entry_y_caster_inf_1.grid(row=row, column=2); row += 1

ttk.Label(aba1, text="Toe – frente (x,y,z):").grid(row=row, column=0)
entry_x_front_1 = ttk.Entry(aba1, width=10); entry_x_front_1.grid(row=row, column=1)
entry_y_front_1 = ttk.Entry(aba1, width=10); entry_y_front_1.grid(row=row, column=2)
entry_z_front_1 = ttk.Entry(aba1, width=10); entry_z_front_1.grid(row=row, column=3); row += 1

ttk.Label(aba1, text="Toe – traseira (x,y,z):").grid(row=row, column=0)
entry_x_rear_1 = ttk.Entry(aba1, width=10); entry_x_rear_1.grid(row=row, column=1)
entry_y_rear_1 = ttk.Entry(aba1, width=10); entry_y_rear_1.grid(row=row, column=2)
entry_z_rear_1 = ttk.Entry(aba1, width=10); entry_z_rear_1.grid(row=row, column=3); row += 1

label_xyz_results = ttk.Label(aba1, text="Resultados XYZ (opcional): --")
label_xyz_results.grid(row=row, column=0, columnspan=4, sticky="w")


# ============================================================
#   ABA 2 – XYZ COMPLETO
# ============================================================

aba2 = ttk.Frame(notebook)
notebook.add(aba2, text="Aba 2 – Engenharia reversa XYZ")

r = 0
ttk.Label(aba2, text="Superior (x,y,z):").grid(row=r, column=0)
entry_x_sup_2 = ttk.Entry(aba2, width=10); entry_x_sup_2.grid(row=r, column=1)
entry_y_sup_2 = ttk.Entry(aba2, width=10); entry_y_sup_2.grid(row=r, column=2)
entry_z_sup_2 = ttk.Entry(aba2, width=10); entry_z_sup_2.grid(row=r, column=3); r += 1

ttk.Label(aba2, text="Inferior (x,y,z):").grid(row=r, column=0)
entry_x_inf_2 = ttk.Entry(aba2, width=10); entry_x_inf_2.grid(row=r, column=1)
entry_y_inf_2 = ttk.Entry(aba2, width=10); entry_y_inf_2.grid(row=r, column=2)
entry_z_inf_2 = ttk.Entry(aba2, width=10); entry_z_inf_2.grid(row=r, column=3); r += 1

ttk.Label(aba2, text="Caster sup (z,y):").grid(row=r, column=0)
entry_z_caster_sup_2 = ttk.Entry(aba2, width=10); entry_z_caster_sup_2.grid(row=r, column=1)
entry_y_caster_sup_2 = ttk.Entry(aba2, width=10); entry_y_caster_sup_2.grid(row=r, column=2); r += 1

ttk.Label(aba2, text="Caster inf (z,y):").grid(row=r, column=0)
entry_z_caster_inf_2 = ttk.Entry(aba2, width=10); entry_z_caster_inf_2.grid(row=r, column=1)
entry_y_caster_inf_2 = ttk.Entry(aba2, width=10); entry_y_caster_inf_2.grid(row=r, column=2); r += 1

ttk.Label(aba2, text="Toe – frente (x,y,z):").grid(row=r, column=0)
entry_x_front_2 = ttk.Entry(aba2, width=10); entry_x_front_2.grid(row=r, column=1)
entry_y_front_2 = ttk.Entry(aba2, width=10); entry_y_front_2.grid(row=r, column=2)
entry_z_front_2 = ttk.Entry(aba2, width=10); entry_z_front_2.grid(row=r, column=3); r += 1

ttk.Label(aba2, text="Toe – traseira (x,y,z):").grid(row=r, column=0)
entry_x_rear_2 = ttk.Entry(aba2, width=10); entry_x_rear_2.grid(row=r, column=1)
entry_y_rear_2 = ttk.Entry(aba2, width=10); entry_y_rear_2.grid(row=r, column=2)
entry_z_rear_2 = ttk.Entry(aba2, width=10); entry_z_rear_2.grid(row=r, column=3); r += 1

ttk.Button(aba2, text="Calcular XYZ", command=executar_aba2).grid(row=r, column=0, pady=10)
label_output_aba2 = ttk.Label(aba2, text="Resultados: --")
label_output_aba2.grid(row=r, column=1, columnspan=3, sticky="w")


# ============================================================
#   ABA 3 – EXPLICAÇÃO DAS VARIÁVEIS
# ============================================================

aba3 = ttk.Frame(notebook)
notebook.add(aba3, text="Aba 3 – Explicação das Variáveis")

texto = """
=====================================
EXPLICAÇÃO DAS VARIÁVEIS – ABA 1 (2D)
=====================================

bf  = bitola dianteira (mm)
x_in, y_in  = coordenadas do pivô interno do braço (superior ou inferior)
x_out, y_out = coordenadas do pivô externo na manga

IC = Instant Center → ponto onde as linhas dos braços se cruzam
Ro = Roll Centre → interseção da FVSA com o plano médio

FVSA (Front View Swing Arm) = linha P–W

s1 = curso da roda externa em curva
s2 = curso da roda interna

dφ = ângulo de rolagem do corpo
dγ_W,k = variação média de cambagem
kγ = fator de cambagem (Reimpell Eq. 3.5)


=====================================
EXPLICAÇÃO DAS VARIÁVEIS – XYZ (3D)
=====================================

(x,y,z) = coordenadas reais de cada ponto da suspensão no veículo/CAD

Camber:
  obtido da inclinação da manga no plano frontal (x–y)

Caster:
  inclinação do eixo de direção no plano lateral (z–y)

Toe:
  inclinação da roda no plano superior (x–z)

Superior (x,y,z):
  ponto esférico superior da manga

Inferior (x,y,z):
  ponto esférico inferior da manga

Toe – frente/traseira:
  dois pontos na face da roda vistos de cima

=====================================
"""
ttk.Label(aba3, text=texto, justify="left", font=("Courier", 10)).pack(padx=20, pady=20)

root.mainloop()
