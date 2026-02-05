import json
import csv
import math
import tkinter as tk
from tkinter import ttk, messagebox, filedialog

import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2Tk

# Domain modules (precisam existir no seu projeto)
import models
import math_2d
import math_3d
import dynamics
import utils
import ui_strings


# =========================
# UI Helpers
# =========================

class ToolTip:
    def __init__(self, widget, text: str, wrap=260):
        self.widget = widget
        self.text = text
        self.wrap = wrap
        self.top = None
        widget.bind("<Enter>", self._show)
        widget.bind("<Leave>", self._hide)

    def _show(self, _event=None):
        if self.top or not self.text:
            return
        x = self.widget.winfo_rootx() + 20
        y = self.widget.winfo_rooty() + 20
        self.top = tk.Toplevel(self.widget)
        self.top.overrideredirect(True)
        self.top.attributes("-topmost", True)

        frm = ttk.Frame(self.top, padding=8, style="Tip.TFrame")
        frm.pack(fill="both", expand=True)

        lbl = ttk.Label(frm, text=self.text, wraplength=self.wrap, justify="left", style="Tip.TLabel")
        lbl.pack()

        self.top.geometry(f"+{x}+{y}")

    def _hide(self, _event=None):
        if self.top:
            self.top.destroy()
            self.top = None


class CollapsibleSection(ttk.Frame):
    def __init__(self, parent, title: str, *args, **kwargs):
        super().__init__(parent, *args, **kwargs)

        self.var_open = tk.BooleanVar(value=True)

        header = ttk.Frame(self)
        header.pack(fill="x")

        self.btn = ttk.Checkbutton(
            header,
            text=title,
            variable=self.var_open,
            command=self._toggle,
            style="Section.TCheckbutton"
        )
        self.btn.pack(side="left", fill="x", expand=True)

        self.body = ttk.Frame(self)
        self.body.pack(fill="x", expand=True)

    def _toggle(self):
        if self.var_open.get():
            self.body.pack(fill="x", expand=True)
        else:
            self.body.forget()


class ScrollableFrame(ttk.Frame):
    """Frame contendo canvas e scrollbar para permitir rolagem dos inputs."""
    def __init__(self, container, width=390, *args, **kwargs):
        super().__init__(container, *args, **kwargs)
        self.canvas = tk.Canvas(self, width=width, highlightthickness=0)
        scrollbar = ttk.Scrollbar(self, orient="vertical", command=self.canvas.yview)

        self.scrollable_window = ttk.Frame(self.canvas)
        self.scrollable_window.bind("<Configure>", lambda e: self.canvas.configure(scrollregion=self.canvas.bbox("all")))

        self.canvas.create_window((0, 0), window=self.scrollable_window, anchor="nw")
        self.canvas.configure(yscrollcommand=scrollbar.set)

        self.canvas.pack(side="left", fill="both", expand=True)
        scrollbar.pack(side="right", fill="y")

        self.canvas.bind("<Enter>", lambda e: self._bind_wheel())
        self.canvas.bind("<Leave>", lambda e: self._unbind_wheel())

    def _bind_wheel(self):
        self.canvas.bind_all("<MouseWheel>", self._on_mousewheel)

    def _unbind_wheel(self):
        self.canvas.unbind_all("<MouseWheel>")

    def _on_mousewheel(self, event):
        self.canvas.yview_scroll(int(-1 * (event.delta / 120)), "units")


def safe_num(s: str) -> float:
    """Aceita vírgula e ponto; levanta erro com mensagem boa."""
    if s is None:
        raise ValueError("Valor vazio.")
    s = str(s).strip().replace(",", ".")
    if s == "":
        raise ValueError("Valor vazio.")
    return float(s)


# =========================
# Main App
# =========================

class App:
    # -------------------------
    # Hardpoints helpers
    # -------------------------
    def _pair_corner(self, corner: str):
        """
        Retorna (corner_right, corner_left) do mesmo eixo.
        Ex:
          FR -> (FR, FL)
          FL -> (FR, FL)
          RR -> (RR, RL)
          RL -> (RR, RL)
        """
        c = (corner or "FR").strip().upper()
        if c.endswith("L"):
            cR = c[:-1] + "R"
            cL = c
        else:
            cR = c
            cL = c[:-1] + "L"
        return cR, cL

    def _hp_key(self, corner: str, point: str) -> str:
        return f"{corner} {point}"

    def _hp_get(self, lbl: str):
        """Retorna (x,y,z) float dos hardpoints do label."""
        if lbl not in self.entries_hp:
            raise KeyError(f"Hardpoint não encontrado: {lbl}")
        ex, ey, ez = self.entries_hp[lbl]
        return safe_num(ex.get()), safe_num(ey.get()), safe_num(ez.get())

    def _hp_set(self, lbl: str, x: float, y: float, z: float):
        """Seta (x,y,z) nos hardpoints do label."""
        if lbl not in self.entries_hp:
            raise KeyError(f"Hardpoint não encontrado: {lbl}")
        ex, ey, ez = self.entries_hp[lbl]
        ex.delete(0, tk.END); ex.insert(0, str(x))
        ey.delete(0, tk.END); ey.insert(0, str(y))
        ez.delete(0, tk.END); ez.insert(0, str(z))

    def _corner_points_2d(self, corner: str):
        """Lê os 4 hardpoints 2D (x,y) de um canto."""
        c = (corner or "FR").strip().upper()
        ui_x, ui_y, _ = self._hp_get(self._hp_key(c, "Sup In"))
        uo_x, uo_y, _ = self._hp_get(self._hp_key(c, "Sup Out"))
        li_x, li_y, _ = self._hp_get(self._hp_key(c, "Inf In"))
        lo_x, lo_y, _ = self._hp_get(self._hp_key(c, "Inf Out"))
        return (
            models.Point2D(ui_x, ui_y),
            models.Point2D(uo_x, uo_y),
            models.Point2D(li_x, li_y),
            models.Point2D(lo_x, lo_y),
        )

    def _make_geo2d_for_corner(self, corner: str):
        """Monta um SuspensionGeometry2D para um canto específico, reaproveitável."""
        upper_in, upper_out, lower_in, lower_out = self._corner_points_2d(corner)
        return models.SuspensionGeometry2D(
            track_width=self._read("bf"),
            upper_in=upper_in,
            upper_out=upper_out,
            lower_in=lower_in,
            lower_out=lower_out,
            s1=self._read("s1"),
            s2=self._read("s2"),
            camber_out_deg=self._read("cam_o"),
            camber_in_deg=self._read("cam_i"),
        )

    def _safe_call(self, fn, name: str):
        """Executa uma ação com captura de erro sem quebrar a sequência."""
        try:
            fn()
            return True
        except Exception as e:
            self._set_status(f"Erro em {name}: {e}")
            try:
                messagebox.showerror(f"Erro em {name}", str(e))
            except Exception:
                pass
            return False

    def _calc_all(self):
        """
        Executa tudo em sequência:
        2D -> 3D -> Visual 3D -> CG Sweep -> Mass Sweep -> Estrutura
        Observação: CG/Mass usam h_ro do 2D; por isso 2D vem primeiro.
        """
        ok2d = self._safe_call(self._calc_2d, "Calcular 2D")
        ok3d = self._safe_call(self._calc_3d, "Calcular 3D")
        okvis = self._safe_call(self._calc_full_3d, "Visual 3D")
        okcg = self._safe_call(self._run_cg_sweep, "CG Sweep")
        okm = self._safe_call(self._run_mass_sweep, "Mass Sweep")
        okopt = self._safe_call(self._run_opt, "Estrutura")

        if all([ok2d, ok3d, okvis, okcg, okm, okopt]):
            self._set_status("✅ Calcular TODOS concluído.")
        else:
            self._set_status("⚠️ Calcular TODOS terminou com alguns erros (veja mensagens).")

    def _goto_tab(self, tab_name: str):
        """Atalho de visualização: seleciona a aba desejada."""
        tabs = {
            "2D": getattr(self, "tab_2d", None),
            "3D": getattr(self, "tab_3d", None),
            "VIS3D": getattr(self, "tab_vis3d", None),
            "DYN": getattr(self, "tab_dyn", None),
            "OPT": getattr(self, "tab_opt", None),
        }
        t = tabs.get(tab_name)
        if t is not None:
            self.content_area.select(t)

    # =========================
    # Gerador de hardpoints (linha "Reimpell" / Duplo A)
    # =========================
    def _sign_from_corner(self, corner: str) -> float:
        c = (corner or "FR").strip().upper()
        return -1.0 if c.endswith("L") else 1.0

    def _axle_z_from_corner(self, corner: str) -> float:
        c = (corner or "FR").strip().upper()
        return self._read("z_front") if c.startswith("F") else self._read("z_rear")

    def _apply_double_a_reimpell(self):
        """
        Preenche automaticamente Sup/Inf In/Out (e também Toe/Damper simples)
        para FR/FL/RR/RL, com parâmetros “de projeto” (lateral/vertical/longitudinal).

        Ideia (bem prática para você evoluir):
        - Outboard fica no “plano da roda”: x = ±bf/2
        - Inboard fica no chassi: x = ±(bf/2 - L_arm)
        - Alturas y_in e y_out controlam inclinação dos braços na vista frontal
        - z_front / z_rear posicionam o eixo no 3D (longitudinal)
        - dz_inboard desloca inboard em z pra não ficar tudo colinear no 3D
        """
        bf = self._read("bf")
        half_track = 0.5 * bf

        # Front params
        LsF = self._read("LsF")      # lateral span upper arm (front)
        LiF = self._read("LiF")      # lateral span lower arm (front)
        ySupInF = self._read("ySupInF")
        ySupOutF = self._read("ySupOutF")
        yInfInF = self._read("yInfInF")
        yInfOutF = self._read("yInfOutF")

        # Rear params
        LsR = self._read("LsR")
        LiR = self._read("LiR")
        ySupInR = self._read("ySupInR")
        ySupOutR = self._read("ySupOutR")
        yInfInR = self._read("yInfInR")
        yInfOutR = self._read("yInfOutR")

        dz_in = self._read("dz_inboard")   # separa inboard em z
        dz_toe = self._read("dz_toe")      # separa toe em z
        dy_toe = self._read("dy_toe")      # altura do toe
        dy_damper = self._read("dy_damper")  # altura do damper in/out
        dx_toe_in = self._read("dx_toe_in")  # deslocamento lateral interno do toe in
        dx_damper_in = self._read("dx_damper_in")

        # corners
        corners = ["FR", "FL", "RR", "RL"]

        for c in corners:
            sgn = self._sign_from_corner(c)
            z_axle = self._axle_z_from_corner(c)

            if c.startswith("F"):
                Ls = LsF; Li = LiF
                ySupIn = ySupInF; ySupOut = ySupOutF
                yInfIn = yInfInF; yInfOut = yInfOutF
            else:
                Ls = LsR; Li = LiR
                ySupIn = ySupInR; ySupOut = ySupOutR
                yInfIn = yInfInR; yInfOut = yInfOutR

            # Outboard (na roda)
            x_out = sgn * half_track
            z_out = z_axle

            # Inboard (no chassi)
            x_sup_in = sgn * (half_track - Ls)
            x_inf_in = sgn * (half_track - Li)

            z_sup_in = z_axle - dz_in
            z_inf_in = z_axle + dz_in

            # Define hardpoints do Duplo A
            self._hp_set(self._hp_key(c, "Sup Out"), x_out, ySupOut, z_out)
            self._hp_set(self._hp_key(c, "Inf Out"), x_out, yInfOut, z_out)

            self._hp_set(self._hp_key(c, "Sup In"), x_sup_in, ySupIn, z_sup_in)
            self._hp_set(self._hp_key(c, "Inf In"), x_inf_in, yInfIn, z_inf_in)

            # Toe link (simples: no meio da altura, com z separado)
            # Toe Out: próximo do outboard, Toe In: mais interno
            self._hp_set(self._hp_key(c, "Toe Out"), x_out, dy_toe, z_axle + dz_toe)
            self._hp_set(self._hp_key(c, "Toe In"), sgn * (half_track - dx_toe_in), dy_toe, z_axle - dz_toe)

            # Damper (simples: inboard alto, outboard médio)
            self._hp_set(self._hp_key(c, "Damper Out"), x_out, ySupOut + 0.5 * (dy_damper), z_axle)
            self._hp_set(self._hp_key(c, "Damper In"), sgn * (half_track - dx_damper_in), ySupIn + dy_damper, z_axle)

        self._set_status("Hardpoints Duplo A gerados (estilo Reimpell simplificado) para FR/FL/RR/RL.")
        # já atualiza os gráficos
        self._safe_call(self._calc_2d, "Calcular 2D (auto)")
        self._safe_call(self._calc_full_3d, "Visual 3D (auto)")

    # =========================
    # 3D helpers (autoscale + grid fixo 600x600)
    # =========================
    def _autoscale_3d_equal(self, ax, points, pad_factor=0.35, min_range=400.0):
        """
        Autoscale 3D "forte" para sempre enquadrar todos os pontos.
        IMPORTANTE: seu plot usa ax.scatter(x, z, y) -> eixos do gráfico:
          X_graf = x (lateral)
          Y_graf = z (longitudinal)
          Z_graf = y (vertical)
        """
        if not points:
            return

        xs = [p.x for p in points]
        ys = [p.y for p in points]  # vertical real
        zs = [p.z for p in points]  # longitudinal real

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

        # setlim no espaço do gráfico: X -> x, Y -> z, Z -> y
        ax.set_xlim(cx - half, cx + half)
        ax.set_ylim(cz - half, cz + half)
        ax.set_zlim(cy - half, cy + half)

    def _draw_ground_mesh_fixed_600(self, ax, step=50.0):
        """
        Desenha uma malha fixa de 600x600 mm no chão (Z_graf = 0):
          X: -300 .. +300
          Y_graf (longitudinal): -300 .. +300
        step: espaçamento das linhas (mm)
        """
        half = 300.0  # 600/2
        step = float(step) if step else 50.0
        if step <= 0:
            step = 50.0

        # linhas paralelas ao "longitudinal" (Y do gráfico)
        x = -half
        while x <= half + 1e-9:
            ax.plot([x, x], [-half, half], [0, 0],
                    linestyle=":", linewidth=0.8, alpha=0.55, color="gray")
            x += step

        # linhas paralelas ao "lateral" (X do gráfico)
        z = -half
        while z <= half + 1e-9:
            ax.plot([-half, half], [z, z], [0, 0],
                    linestyle=":", linewidth=0.8, alpha=0.55, color="gray")
            z += step

    # -------------------------
    # CSV hardpoints
    # -------------------------
    def _export_hardpoints_csv(self):
        """
        Exporta SOMENTE os hardpoints + bitola para CSV.
        Inclui Toe/Damper (total 32 hardpoints).
        Formato:
        key,x,y,z
        bf,1250,,
        FR Sup In,450,420,200
        ...
        """
        path = filedialog.asksaveasfilename(
            defaultextension=".csv",
            filetypes=[("CSV", "*.csv")]
        )
        if not path:
            return

        try:
            rows = []
            rows.append(["bf", self.entries["bf"].get(), "", ""])

            for lbl in getattr(self, "hp_keys", []):
                x, y, z = self._hp_get(lbl)
                rows.append([lbl, f"{x}", f"{y}", f"{z}"])

            with open(path, "w", newline="", encoding="utf-8") as f:
                w = csv.writer(f, delimiter=",")
                w.writerow(["key", "x", "y", "z"])
                w.writerows(rows)

            self._set_status("Hardpoints exportados para CSV.")
        except Exception as e:
            self._set_status(f"Erro export CSV: {e}")
            messagebox.showerror("Erro", str(e))

    def _import_hardpoints_csv(self):
        """
        Importa hardpoints + bf (se existir) do CSV e preenche automaticamente.
        Aceita separador ',' e decimal com vírgula (safe_num).
        """
        path = filedialog.askopenfilename(filetypes=[("CSV", "*.csv")])
        if not path:
            return

        try:
            got_any = False
            with open(path, "r", encoding="utf-8") as f:
                rdr = csv.DictReader(f)
                for row in rdr:
                    key = (row.get("key") or "").strip()
                    if not key:
                        continue

                    if key == "bf":
                        bf_val = row.get("x", "")
                        if bf_val:
                            self._set("bf", safe_num(bf_val))
                            got_any = True
                        continue

                    if key in self.entries_hp:
                        x = safe_num(row.get("x", "0"))
                        y = safe_num(row.get("y", "0"))
                        z = safe_num(row.get("z", "0"))
                        self._hp_set(key, x, y, z)
                        got_any = True

            if got_any:
                self._set_status("Hardpoints importados do CSV.")
            else:
                self._set_status("CSV lido, mas não encontrou chaves válidas.")
                messagebox.showwarning("Aviso", "Não encontrei chaves (key) válidas no CSV.")
        except Exception as e:
            self._set_status(f"Erro import CSV: {e}")
            messagebox.showerror("Erro", str(e))

    def _compute_arm_angles_deg(self, corner: str):
        """
        Calcula ângulo do braço superior e inferior na vista frontal (X lateral, Y vertical).
        Ângulo = atan2(dY, dX) em graus. Usa pontos In->Out.
        """
        sup_in_x, sup_in_y, _ = self._hp_get(self._hp_key(corner, "Sup In"))
        sup_out_x, sup_out_y, _ = self._hp_get(self._hp_key(corner, "Sup Out"))

        inf_in_x, inf_in_y, _ = self._hp_get(self._hp_key(corner, "Inf In"))
        inf_out_x, inf_out_y, _ = self._hp_get(self._hp_key(corner, "Inf Out"))

        ang_sup = math.degrees(math.atan2((sup_out_y - sup_in_y), (sup_out_x - sup_in_x)))
        ang_inf = math.degrees(math.atan2((inf_out_y - inf_in_y), (inf_out_x - inf_in_x)))
        return ang_sup, ang_inf

    # -------------------------
    # Init
    # -------------------------
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("Suíte de Engenharia de Suspensão (2D/3D/Visual/Dinâmica/Estrutura)")
        self.root.geometry("1400x900")

        # Estado: 3D view
        self.free_view = False
        self._view_lock_cid = None

        # Estado: resultados
        self.last_2d_results = None
        self.last_3d_results = None

        # Entradas
        self.entries = {}
        self.entries_hp = {}
        self.hp_keys = []

        # Corner usado nos cálculos 2D/3D/Opt
        self.var_corner = tk.StringVar(value="FR")

        # UI
        self._setup_style()
        self._build_topbar()

        self.paned = tk.PanedWindow(root, orient="horizontal", sashwidth=6, bg="#b0b0b0")
        self.paned.pack(fill="both", expand=True)

        self.sidebar_container = ScrollableFrame(self.paned, width=410)
        self.paned.add(self.sidebar_container, minsize=400, width=420)

        self.content_area = ttk.Notebook(self.paned)
        self.paned.add(self.content_area, minsize=700)

        self._init_sidebar_inputs(self.sidebar_container.scrollable_window)
        self._init_result_tabs()
        self._build_statusbar()

    # -------------------------
    # Style
    # -------------------------
    def _setup_style(self):
        style = ttk.Style()
        style.theme_use("clam")

        style.configure("TLabelFrame", font=("Arial", 10, "bold"))
        style.configure("TButton", font=("Arial", 9, "bold"), padding=6)
        style.configure("Action.TButton", foreground="#1f5cff")
        style.configure("Reset.TButton", foreground="#b00020")

        style.configure("Top.TFrame", padding=8)
        style.configure("Section.TCheckbutton", font=("Arial", 10, "bold"))
        style.configure("Card.TFrame", padding=10, relief="solid", borderwidth=1)

        style.configure("Tip.TFrame", relief="solid", borderwidth=1)
        style.configure("Tip.TLabel", font=("Arial", 9))

        style.configure("Status.TLabel", font=("Arial", 9))
        style.configure("Header.TLabel", font=("Arial", 11, "bold"))

    # -------------------------
    # Topbar + Statusbar
    # -------------------------
    def _build_topbar(self):
        bar = ttk.Frame(self.root, style="Top.TFrame")
        bar.pack(fill="x")

        ttk.Label(bar, text="Ações rápidas", style="Header.TLabel").pack(side="left", padx=(6, 16))

        ttk.Button(bar, text="Calcular 2D", style="Action.TButton", command=self._calc_2d).pack(side="left", padx=4)
        ttk.Button(bar, text="Calcular 3D", style="Action.TButton", command=self._calc_3d).pack(side="left", padx=4)
        ttk.Button(bar, text="Visual 3D", style="Action.TButton", command=self._calc_full_3d).pack(side="left", padx=4)
        ttk.Button(bar, text="CG Sweep", command=self._run_cg_sweep).pack(side="left", padx=4)
        ttk.Button(bar, text="Mass Sweep", command=self._run_mass_sweep).pack(side="left", padx=4)
        ttk.Button(bar, text="Estrutura", command=self._run_opt).pack(side="left", padx=4)

        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=10)

        ttk.Button(bar, text="Salvar preset", command=self._save_preset).pack(side="left", padx=4)
        ttk.Button(bar, text="Carregar preset", command=self._load_preset).pack(side="left", padx=4)
        ttk.Button(bar, text="Reset defaults", command=self._reset_defaults).pack(side="left", padx=4)

        # CANTO DIREITO: CALCULAR + VISUAL
        right = ttk.Frame(bar)
        right.pack(side="right", padx=8)

        btn_calc = ttk.Menubutton(right, text="Calcular ▾", style="Action.TButton")
        m_calc = tk.Menu(btn_calc, tearoff=0)
        btn_calc["menu"] = m_calc
        m_calc.add_command(label="Calcular TODOS", command=self._calc_all)
        m_calc.add_separator()
        m_calc.add_command(label="Calcular 2D", command=self._calc_2d)
        m_calc.add_command(label="Calcular 3D", command=self._calc_3d)
        m_calc.add_command(label="Calcular Visual 3D", command=self._calc_full_3d)
        m_calc.add_separator()
        m_calc.add_command(label="CG Sweep", command=self._run_cg_sweep)
        m_calc.add_command(label="Mass Sweep", command=self._run_mass_sweep)
        m_calc.add_separator()
        m_calc.add_command(label="Estrutura", command=self._run_opt)
        btn_calc.pack(side="left", padx=(0, 6))

        btn_vis = ttk.Menubutton(right, text="Visual ▾")
        m_vis = tk.Menu(btn_vis, tearoff=0)
        btn_vis["menu"] = m_vis
        m_vis.add_command(label="Ir para Resultados 2D", command=lambda: self._goto_tab("2D"))
        m_vis.add_command(label="Ir para Resultados 3D", command=lambda: self._goto_tab("3D"))
        m_vis.add_command(label="Ir para Visualização 3D", command=lambda: self._goto_tab("VIS3D"))
        m_vis.add_separator()
        m_vis.add_command(label="Ir para Dinâmica Veicular", command=lambda: self._goto_tab("DYN"))
        m_vis.add_command(label="Ir para Otimização", command=lambda: self._goto_tab("OPT"))
        btn_vis.pack(side="left")

    def _build_statusbar(self):
        self.status_var = tk.StringVar(value="Pronto.")
        bar = ttk.Frame(self.root)
        bar.pack(fill="x", side="bottom")
        ttk.Label(bar, textvariable=self.status_var, style="Status.TLabel").pack(side="left", padx=8, pady=3)

    def _set_status(self, msg: str):
        if hasattr(self, "status_var"):
            self.status_var.set(msg)

    # -------------------------
    # Entry builders
    # -------------------------
    def _create_entry(self, parent, key, label, default, row, col, width=10, tip=None):
        ttk.Label(parent, text=label).grid(row=row, column=col, sticky="e", padx=3, pady=2)
        e = ttk.Entry(parent, width=width)
        e.insert(0, str(default))
        e.grid(row=row, column=col + 1, sticky="w", padx=3, pady=2)
        self.entries[key] = e
        if tip:
            ToolTip(e, tip)
        return e

    def _read(self, key) -> float:
        if key not in self.entries:
            raise KeyError(f"Campo não encontrado: {key}")
        return safe_num(self.entries[key].get())

    def _set(self, key, value):
        e = self.entries.get(key)
        if not e:
            return
        e.delete(0, tk.END)
        e.insert(0, str(value))

    # -------------------------
    # Sidebar Sections
    # -------------------------
    def _init_sidebar_inputs(self, parent):
        # 1) Geometria (Hardpoints) MTOMBO
        sec_geo = CollapsibleSection(parent, "1) Geometria (Hardpoints) MTOMBO [mm]")
        sec_geo.pack(fill="x", padx=6, pady=6)
        f_geo = sec_geo.body

        self._create_entry(
            f_geo, "bf", "Bitola (bf):", 1250, 0, 0, width=12,
            tip="Distância entre centros das rodas (track). Use mm."
        )

        ttk.Label(f_geo, text="Canto p/ cálculo:").grid(row=0, column=2, sticky="e", padx=3, pady=2)
        cb = ttk.Combobox(f_geo, values=["FR", "FL", "RR", "RL"], textvariable=self.var_corner, width=6, state="readonly")
        cb.grid(row=0, column=3, sticky="w", padx=3, pady=2)
        ToolTip(cb, "Define qual canto (FR/FL/RR/RL) será usado nos cálculos 2D/3D e ângulos automáticos.")

        # =========================
        # Gerador Duplo A (paramétrico)
        # =========================
        gen = CollapsibleSection(f_geo, "Gerador Duplo A (Reimpell - parametrizado)")
        gen.grid(row=1, column=0, columnspan=4, sticky="ew", padx=0, pady=(10, 6))
        f_gen = gen.body

        # (opcional, mas recomendado)
        for c in range(4):
            f_geo.grid_columnconfigure(c, weight=1)

        # Longitudinal (posiciona eixo no 3D)
        self._create_entry(f_gen, "z_front", "Z eixo dianteiro:", 775, 0, 0, width=10,
                           tip="Posição longitudinal do eixo dianteiro (mm).")
        self._create_entry(f_gen, "z_rear", "Z eixo traseiro:", -775, 0, 2, width=10,
                           tip="Posição longitudinal do eixo traseiro (mm).")

        # Front lateral spans
        self._create_entry(f_gen, "LsF", "Front L sup:", 450, 1, 0, width=10,
                           tip="Distância lateral entre inboard e outboard do braço superior (dianteiro).")
        self._create_entry(f_gen, "LiF", "Front L inf:", 520, 1, 2, width=10,
                           tip="Distância lateral entre inboard e outboard do braço inferior (dianteiro).")

        # Front heights
        self._create_entry(f_gen, "ySupInF", "Front y sup in:", 420, 2, 0, width=10)
        self._create_entry(f_gen, "ySupOutF", "Front y sup out:", 390, 2, 2, width=10)
        self._create_entry(f_gen, "yInfInF", "Front y inf in:", 210, 3, 0, width=10)
        self._create_entry(f_gen, "yInfOutF", "Front y inf out:", 190, 3, 2, width=10)

        # Rear lateral spans
        self._create_entry(f_gen, "LsR", "Rear L sup:", 430, 4, 0, width=10)
        self._create_entry(f_gen, "LiR", "Rear L inf:", 500, 4, 2, width=10)

        # Rear heights
        self._create_entry(f_gen, "ySupInR", "Rear y sup in:", 410, 5, 0, width=10)
        self._create_entry(f_gen, "ySupOutR", "Rear y sup out:", 380, 5, 2, width=10)
        self._create_entry(f_gen, "yInfInR", "Rear y inf in:", 200, 6, 0, width=10)
        self._create_entry(f_gen, "yInfOutR", "Rear y inf out:", 180, 6, 2, width=10)

        # 3D separations + Toe/Damper helpers
        self._create_entry(f_gen, "dz_inboard", "dz inboard:", 80, 7, 0, width=10,
                           tip="Desloca inboards em z (um vai +dz, outro -dz) para 3D não colinear.")
        self._create_entry(f_gen, "dz_toe", "dz toe:", 60, 7, 2, width=10)

        self._create_entry(f_gen, "dy_toe", "y toe:", 260, 8, 0, width=10)
        self._create_entry(f_gen, "dx_toe_in", "dx toe in:", 520, 8, 2, width=10,
                           tip="Distância lateral (da borda/roda) para posicionar Toe In (mm).")

        self._create_entry(f_gen, "dy_damper", "dy damper:", 100, 9, 0, width=10,
                           tip="Eleva Damper In acima de y sup in (mm).")
        self._create_entry(f_gen, "dx_damper_in", "dx damper in:", 380, 9, 2, width=10)

        ttk.Button(f_gen, text="Gerar Hardpoints Duplo A (Reimpell) + Plot 2D/3D",
                   style="Action.TButton", command=self._apply_double_a_reimpell)\
            .grid(row=10, column=0, columnspan=4, sticky="ew", pady=(8, 4))

        # =========================
        # Hardpoints (32)
        # =========================
        ttk.Label(f_geo, text="X").grid(row=999, column=1)  # placeholders (não usado)
        ttk.Label(f_geo, text="Y").grid(row=999, column=2)
        ttk.Label(f_geo, text="Z").grid(row=999, column=3)

        corners = ["FR", "FL", "RR", "RL"]
        pts = ["Sup In", "Sup Out", "Inf In", "Inf Out",
               "Toe In", "Toe Out", "Damper In", "Damper Out"]

        # Defaults base (FR) - ficam como “fallback”
        base_defaults = {
            "Sup In":     (450, 420, 200),
            "Sup Out":    (625, 390, 300),
            "Inf In":     (430, 210, 200),
            "Inf Out":    (625, 190, 300),
            "Toe In":     (520, 260, 120),
            "Toe Out":    (650, 260, 180),
            "Damper In":  (380, 520, 150),
            "Damper Out": (610, 430, 260),
        }

        def mirror_x(x):
            return -x

        r = 20  # começa depois do gerador
        self.hp_keys = []

        for corner in corners:
            ttk.Label(f_geo, text=f"== {corner} ==", font=("Arial", 9, "bold"))\
                .grid(row=r, column=0, columnspan=4, sticky="w", pady=(10, 2))
            r += 1

            for p in pts:
                lbl = self._hp_key(corner, p)
                dx, dy, dz = base_defaults[p]
                if corner in ("FL", "RL"):
                    dx = mirror_x(dx)

                ttk.Label(f_geo, text=lbl + ":").grid(row=r, column=0, sticky="e")
                ex = ttk.Entry(f_geo, width=8); ex.insert(0, str(dx)); ex.grid(row=r, column=1)
                ey = ttk.Entry(f_geo, width=8); ey.insert(0, str(dy)); ey.grid(row=r, column=2)
                ez = ttk.Entry(f_geo, width=8); ez.insert(0, str(dz)); ez.grid(row=r, column=3)

                self.entries_hp[lbl] = (ex, ey, ez)
                self.hp_keys.append(lbl)

                ToolTip(ex, f"{lbl} X (mm)")
                ToolTip(ey, f"{lbl} Y (mm)")
                ToolTip(ez, f"{lbl} Z (mm)")
                r += 1

        frm_csv = ttk.Frame(f_geo)
        frm_csv.grid(row=r, column=0, columnspan=4, sticky="ew", pady=(10, 2))
        ttk.Button(frm_csv, text="Exportar Hardpoints CSV", command=self._export_hardpoints_csv).pack(side="left", padx=4)
        ttk.Button(frm_csv, text="Importar Hardpoints CSV", command=self._import_hardpoints_csv).pack(side="left", padx=4)

        # 2) Cinemática
        sec_kin = CollapsibleSection(parent, "2) Cinemática & Alinhamento")
        sec_kin.pack(fill="x", padx=6, pady=6)
        f_kin = sec_kin.body

        self._create_entry(f_kin, "s1", "Curso Ext (mm):", 40, 0, 0)
        self._create_entry(f_kin, "s2", "Curso Int (mm):", 30, 0, 2)
        self._create_entry(f_kin, "cam_o", "Camber Ext (°):", -1.5, 1, 0)
        self._create_entry(f_kin, "cam_i", "Camber Int (°):", -4.0, 1, 2)

        ttk.Label(f_kin, text="Auxiliares 3D").grid(row=2, column=0, columnspan=4, pady=(8, 2), sticky="w")
        self._create_entry(f_kin, "spindle_sup_z", "Spindle Sup Z:", 300, 3, 0)
        self._create_entry(f_kin, "spindle_inf_z", "Spindle Inf Z:", 300, 3, 2)
        self._create_entry(f_kin, "toe_f_x", "Toe Front X:", 600, 4, 0)
        self._create_entry(f_kin, "toe_r_x", "Toe Rear X:", 600, 4, 2)

        # 3) Dinâmica
        sec_dyn = CollapsibleSection(parent, "3) Dinâmica Veicular (Baja)")
        sec_dyn.pack(fill="x", padx=6, pady=6)
        f_dyn = sec_dyn.body

        self._create_entry(f_dyn, "mass", "Massa (kg):", 200, 0, 0)
        self._create_entry(f_dyn, "hcg", "Altura CG (mm):", 450, 0, 2)
        self._create_entry(f_dyn, "ay", "Ay (m/s²):", 9.8, 1, 0)
        self._create_entry(f_dyn, "wb", "Entre-eixos (mm):", 1550, 1, 2)
        self._create_entry(f_dyn, "fx", "Fx Pneu (N):", 1200, 2, 0)
        self._create_entry(f_dyn, "rs", "Scrub (mm):", 50, 2, 2)

        ttk.Label(f_dyn, text="Varredura (Simulação)").grid(row=3, column=0, columnspan=4, pady=(8, 2), sticky="w")
        self._create_entry(f_dyn, "h_min", "CG Min:", 250, 4, 0)
        self._create_entry(f_dyn, "h_max", "CG Max:", 650, 4, 2)
        self._create_entry(f_dyn, "m_min", "Massa Min:", 180, 5, 0)
        self._create_entry(f_dyn, "m_max", "Massa Max:", 240, 5, 2)
        self._create_entry(f_dyn, "step", "Passo:", 25, 6, 0)

        # 4) Estrutura
        sec_opt = CollapsibleSection(parent, "4) Otimização Estrutural")
        sec_opt.pack(fill="x", padx=6, pady=6)
        f_opt = sec_opt.body

        self._create_entry(f_opt, "load", "Carga F [N]:", 3000, 0, 0)
        self.var_static = tk.IntVar(value=0)
        ttk.Checkbutton(f_opt, text="+ Estática (mg/4)", variable=self.var_static)\
            .grid(row=0, column=2, columnspan=2, sticky="w")

        self.var_auto_arm_angles = tk.IntVar(value=1)
        ttk.Checkbutton(
            f_opt,
            text="Usar ângulos automáticos (do canto selecionado)",
            variable=self.var_auto_arm_angles
        ).grid(row=1, column=0, columnspan=4, sticky="w", pady=(2, 6))

        self._create_entry(f_opt, "ang_sup", "Ang Base Sup:", 10, 2, 0)
        self._create_entry(f_opt, "ang_inf", "Ang Base Inf:", 20, 2, 2)
        self._create_entry(f_opt, "limit", "Limite (N):", 8000, 3, 0)
        self._create_entry(f_opt, "ksup", "K Sup:", 0.5, 3, 2)
        self._create_entry(f_opt, "amin", "Varredura Min:", 0, 4, 0)
        self._create_entry(f_opt, "amax", "Varredura Max:", 40, 4, 2)

    # -------------------------
    # Tabs
    # -------------------------
    def _init_result_tabs(self):
        self.tab_2d = ttk.Frame(self.content_area)
        self.tab_3d = ttk.Frame(self.content_area)
        self.tab_vis3d = ttk.Frame(self.content_area)
        self.tab_dyn = ttk.Frame(self.content_area)
        self.tab_opt = ttk.Frame(self.content_area)

        self.content_area.add(self.tab_2d, text="Resultados 2D")
        self.content_area.add(self.tab_3d, text="Resultados 3D")
        self.content_area.add(self.tab_vis3d, text="Visualização 3D")
        self.content_area.add(self.tab_dyn, text="Dinâmica Veicular")
        self.content_area.add(self.tab_opt, text="Otimização")

        self._setup_res_2d(self.tab_2d)
        self._setup_res_3d(self.tab_3d)
        self._setup_res_vis3d(self.tab_vis3d)
        self._setup_res_dyn(self.tab_dyn)
        self._setup_res_opt(self.tab_opt)

    def _setup_res_2d(self, parent):
        card = ttk.Frame(parent, style="Card.TFrame")
        card.pack(fill="x", padx=12, pady=10)

        ttk.Label(card, text="Principais resultados", style="Header.TLabel").pack(anchor="w")
        self.lbl_2d_res = ttk.Label(card, text="Calcule para ver resultados...", font=("Courier", 11), justify="left")
        self.lbl_2d_res.pack(anchor="w", pady=(6, 0))

        self.fig_2d, self.ax_2d = plt.subplots(figsize=(6, 5))
        self.canvas_2d = FigureCanvasTkAgg(self.fig_2d, master=parent)
        self.canvas_2d.get_tk_widget().pack(fill="both", expand=True, padx=12, pady=8)

        txt = tk.Text(parent, height=6, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", getattr(ui_strings, "SUMMARY_2D", ""))
        txt.config(state="disabled")
        txt.pack(fill="x", side="bottom")

    def _setup_res_3d(self, parent):
        card = ttk.Frame(parent, style="Card.TFrame")
        card.pack(fill="x", padx=12, pady=10)

        ttk.Label(card, text="Forças e alinhamento", style="Header.TLabel").pack(anchor="w")
        self.lbl_3d_res = ttk.Label(card, text="Calcule para ver resultados...", font=("Courier", 11), justify="left")
        self.lbl_3d_res.pack(anchor="w", pady=(6, 0))

        txt = tk.Text(parent, height=10, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", getattr(ui_strings, "SUMMARY_3D", ""))
        txt.config(state="disabled")
        txt.pack(fill="x", side="bottom")

    def _setup_res_vis3d(self, parent):
        ctrl = ttk.Frame(parent, padding=8)
        ctrl.pack(fill="x")

        ttk.Button(ctrl, text="GERAR DIAGRAMA 3D", style="Action.TButton",
                   command=self._calc_full_3d).pack(side="left", fill="x", expand=True, padx=2)

        ttk.Button(ctrl, text="Resetar Visualização", style="Reset.TButton",
                   command=self._reset_view).pack(side="right", padx=2)

        self.btn_free_view = ttk.Button(ctrl, text="Visualização: FIXA (YZ)", command=self._toggle_free_view)
        self.btn_free_view.pack(side="right", padx=8)

        # slider do espaçamento do grid 600x600
        self.var_grid_step = tk.DoubleVar(value=50.0)
        frm_grid = ttk.Frame(ctrl)
        frm_grid.pack(side="left", padx=(10, 10))
        ttk.Label(frm_grid, text="Grid 600×600 (step mm):").grid(row=0, column=0, sticky="e")
        s_step = ttk.Scale(frm_grid, from_=10, to=100, variable=self.var_grid_step, orient="horizontal", length=160)
        s_step.grid(row=0, column=1, sticky="w", padx=6)
        ToolTip(s_step, "Espaçamento das linhas do grid fixo 600×600 (mm). Solte o mouse para redesenhar.")
        s_step.bind("<ButtonRelease-1>", lambda e: self._calc_full_3d())

        self.lbl_steer_data = ttk.Label(parent, text="Dados: --", font=("Courier", 10))
        self.lbl_steer_data.pack(pady=5)

        self.fig_vis3d = plt.figure(figsize=(6, 5))
        self.ax_vis3d = self.fig_vis3d.add_subplot(111, projection="3d")

        self.canvas_vis3d = FigureCanvasTkAgg(self.fig_vis3d, master=parent)
        self.canvas_vis3d.get_tk_widget().pack(fill="both", expand=True, padx=10, pady=(0, 0))

        toolbar = NavigationToolbar2Tk(self.canvas_vis3d, parent)
        toolbar.update()
        toolbar.pack(fill="x", padx=10, pady=(0, 8))

        self._apply_view_mode()
        self._reset_view()

    def _setup_res_dyn(self, parent):
        nb = ttk.Notebook(parent)
        nb.pack(fill="both", expand=True)

        f_cg = ttk.Frame(nb)
        nb.add(f_cg, text="Varredura de CG")

        self.fig_cg, (self.ax_cg1, self.ax_cg2) = plt.subplots(1, 2, figsize=(8, 3.5))
        self.canvas_cg = FigureCanvasTkAgg(self.fig_cg, master=f_cg)
        self.canvas_cg.get_tk_widget().pack(fill="both", expand=True, padx=10, pady=8)

        cols_cg = ("CG", "dFz", "Fz_int", "M_Roll")
        self.tree_cg = ttk.Treeview(f_cg, columns=cols_cg, show="headings", height=7)
        for c in cols_cg:
            self.tree_cg.heading(c, text=c)
            self.tree_cg.column(c, width=110, anchor="center")
        self.tree_cg.pack(fill="x", padx=10, pady=(0, 10))

        f_mass = ttk.Frame(nb)
        nb.add(f_mass, text="Varredura de Massa")

        cols_m = ("Massa", "M_Roll", "dFz", "SSF", "Ay_Crit", "Margem")
        self.tree_mass = ttk.Treeview(f_mass, columns=cols_m, show="headings")
        for c in cols_m:
            self.tree_mass.heading(c, text=c)
            self.tree_mass.column(c, width=120, anchor="center")
        self.tree_mass.pack(fill="both", expand=True, padx=10, pady=10)

        txt = tk.Text(parent, height=5, bg="#f0f0f0", font=("Arial", 9))
        txt.insert("1.0", getattr(ui_strings, "SUMMARY_BAJA", ""))
        txt.config(state="disabled")
        txt.pack(fill="x", side="bottom")

    def _setup_res_opt(self, parent):
        paned = tk.PanedWindow(parent, orient="vertical")
        paned.pack(fill="both", expand=True, padx=12, pady=10)

        f_tab = ttk.Frame(paned)
        cols = ("dAngle", "F_Sup", "F_Inf", "Status")
        self.tree_opt = ttk.Treeview(f_tab, columns=cols, show="headings")
        for c in cols:
            self.tree_opt.heading(c, text=c)
            self.tree_opt.column(c, width=120, anchor="center")
        self.tree_opt.pack(fill="both", expand=True)
        paned.add(f_tab, minsize=180)

        f_graph = ttk.Frame(paned)
        self.fig_opt, self.ax_opt = plt.subplots(figsize=(6, 4))
        self.canvas_opt = FigureCanvasTkAgg(self.fig_opt, master=f_graph)
        self.canvas_opt.get_tk_widget().pack(fill="both", expand=True)
        paned.add(f_graph)

    # =========================
    # 3D view mode controls
    # =========================
    def _toggle_free_view(self):
        self.free_view = not self.free_view
        self._apply_view_mode()
        if self.free_view:
            self.btn_free_view.config(text="Visualização: LIVRE (3D)")
            self._set_status("Visualização livre ativada (rotação habilitada).")
        else:
            self.btn_free_view.config(text="Visualização: FIXA (YZ)")
            self._reset_view()
            self._set_status("Visualização fixa ativada (plano YZ, sem rotação).")

    def _apply_view_mode(self):
        if not hasattr(self, "ax_vis3d") or not hasattr(self, "canvas_vis3d"):
            return

        if self._view_lock_cid is not None:
            try:
                self.canvas_vis3d.mpl_disconnect(self._view_lock_cid)
            except Exception:
                pass
            self._view_lock_cid = None

        if self.free_view:
            try:
                self.ax_vis3d.mouse_init(rotate_btn=1, zoom_btn=3)
            except Exception:
                pass
        else:
            try:
                self.ax_vis3d.mouse_init(rotate_btn=None, zoom_btn=3)
            except Exception:
                pass
            self._view_lock_cid = self.canvas_vis3d.mpl_connect(
                "button_release_event",
                lambda evt: self._enforce_yz_view()
            )

    def _enforce_yz_view(self):
        if self.free_view:
            return
        if not hasattr(self, "ax_vis3d") or not hasattr(self, "canvas_vis3d"):
            return

        try:
            dist = self.ax_vis3d.dist
        except Exception:
            dist = None

        self.ax_vis3d.view_init(elev=0, azim=0)

        if dist is not None:
            try:
                self.ax_vis3d.dist = dist
            except Exception:
                pass

        self.canvas_vis3d.draw_idle()

    def _reset_view(self):
        if not hasattr(self, "ax_vis3d") or not hasattr(self, "canvas_vis3d"):
            return
        if self.free_view:
            self.ax_vis3d.view_init(elev=20, azim=-60)
        else:
            self.ax_vis3d.view_init(elev=0, azim=0)
        self.canvas_vis3d.draw_idle()

    # =========================
    # Domain gathering
    # =========================
    def _get_point3d_from_input(self, label: str):
        x, y, z = self._hp_get(label)
        return models.Point3D(x, y, z)

    # =========================
    # Actions: 2D / 3D / Visual / Dynamics / Opt
    # =========================
    def _calc_2d(self):
        try:
            corner_sel = (self.var_corner.get().strip() or "FR").upper()
            corner_R, corner_L = self._pair_corner(corner_sel)

            geoR = self._make_geo2d_for_corner(corner_R)
            geoL = self._make_geo2d_for_corner(corner_L)

            rcR = math_2d.calculate_roll_center(geoR)
            rcL = math_2d.calculate_roll_center(geoL)
            camR = math_2d.calculate_camber_gain(geoR)

            self.last_2d_results = rcR if corner_sel.endswith("R") else rcL

            def fmt_ic(rc):
                return f"({rc.ic.x:.1f}, {rc.ic.y:.1f})" if getattr(rc, "ic", None) else "Paralelo"

            def fmt_hro(rc):
                h = getattr(rc, "h_ro", None)
                return f"{h:.1f} mm" if h is not None else "--"

            self.lbl_2d_res.config(text=(
                f"Eixo: {corner_R[:-1]}  |  Canto selecionado: {corner_sel}\n"
                f"== DIREITA ({corner_R}) ==\n"
                f"IC: {fmt_ic(rcR)} mm\n"
                f"Altura RC (h_Ro): {fmt_hro(rcR)}\n"
                f"q: {getattr(rcR, 'q_factor', '--')}\n"
                f"\n== ESQUERDA ({corner_L}) ==\n"
                f"IC: {fmt_ic(rcL)} mm\n"
                f"Altura RC (h_Ro): {fmt_hro(rcL)}\n"
                f"q: {getattr(rcL, 'q_factor', '--')}\n"
                f"\nRolagem (dPhi - ref. lado R): {camR.d_phi_deg:.3f} deg\n"
                f"Fator Camber (kGamma - ref. lado R): {camR.k_gamma:.3f}"
            ))

            ax = self.ax_2d
            ax.clear()

            def plot_corner(geo, rc, label_prefix, color):
                ax.plot([geo.upper_in.x, geo.upper_out.x], [geo.upper_in.y, geo.upper_out.y],
                        marker="o", label=f"{label_prefix} Braço Sup", color=color, linestyle="--")
                ax.plot([geo.lower_in.x, geo.lower_out.x], [geo.lower_in.y, geo.lower_out.y],
                        marker="o", label=f"{label_prefix} Braço Inf", color=color, linestyle="-")

                if getattr(rc, "ic", None):
                    ax.plot(rc.ic.x, rc.ic.y, marker="s", linestyle="None", label=f"{label_prefix} IC", color=color)
                h_ro_val = getattr(rc, "h_ro", None)
                if h_ro_val is not None:
                    ax.plot(0, h_ro_val, marker="*", linestyle="None", markersize=12,
                            label=f"{label_prefix} RC (no centro)", color=color)

            plot_corner(geoR, rcR, f"{corner_R}", "tab:blue")
            plot_corner(geoL, rcL, f"{corner_L}", "tab:red")

            ax.axvline(0, linestyle=":", linewidth=1)
            ax.set_title(f"Vista Frontal 2D (dois lados): {corner_R} + {corner_L}")
            ax.set_xlabel("X lateral (mm)")
            ax.set_ylabel("Y vertical (mm)")
            ax.grid(True)
            ax.legend(ncols=2, fontsize=8)
            ax.set_aspect("equal", adjustable="datalim")

            self.canvas_2d.draw()
            self._set_status("2D calculado (mostrando esquerda e direita).")
            self.content_area.select(self.tab_2d)

        except Exception as e:
            self._set_status(f"Erro 2D: {e}")
            messagebox.showerror("Erro 2D", str(e))

    def _calc_3d(self):
        try:
            corner = (self.var_corner.get().strip() or "FR").upper()
            sup_in = self._get_point3d_from_input(self._hp_key(corner, "Sup In"))
            sup_out = self._get_point3d_from_input(self._hp_key(corner, "Sup Out"))
            inf_in = self._get_point3d_from_input(self._hp_key(corner, "Inf In"))
            inf_out = self._get_point3d_from_input(self._hp_key(corner, "Inf Out"))

            sup_out_x, sup_out_y, _ = self._hp_get(self._hp_key(corner, "Sup Out"))
            inf_out_x, inf_out_y, _ = self._hp_get(self._hp_key(corner, "Inf Out"))

            geo3d = models.SuspensionGeometry3D(
                sup_in=sup_in,
                sup_out=sup_out,
                inf_in=inf_in,
                inf_out=inf_out,
                spindle_sup=models.Point3D(sup_out_x, sup_out_y, self._read("spindle_sup_z")),
                spindle_inf=models.Point3D(inf_out_x, inf_out_y, self._read("spindle_inf_z")),
                toe_front=models.Point3D(self._read("toe_f_x"), 0, 0),
                toe_rear=models.Point3D(self._read("toe_r_x"), 0, 0),
                stiffness_ratio_sup=1.0,
                stiffness_ratio_inf=1.0,
                fx_tire=self._read("fx"),
            )

            forces = math_3d.calculate_forces(geo3d)
            self.last_3d_results = forces

            if forces:
                self.lbl_3d_res.config(text=(
                    f"Canto: {corner}\n"
                    "== FORÇAS AXIAIS ==\n"
                    f"Sup: {forces.upper.axial:.1f} N\n"
                    f"Inf: {forces.lower.axial:.1f} N\n"
                ))
                self._set_status("3D calculado com sucesso.")
                self.content_area.select(self.tab_3d)
            else:
                self._set_status("Erro: geometria 3D inválida.")
                self.lbl_3d_res.config(text="Erro: Geometria 3D inválida.")
        except Exception as e:
            self._set_status(f"Erro 3D: {e}")
            messagebox.showerror("Erro 3D", str(e))

    def _calc_full_3d(self):
        """
        - Mostra FR, FL, RR, RL
        - Plota 32 hardpoints (Sup/Inf + Toe + Damper)
        - Não conecta SupOut com InfOut (sem upright em linha)
        - Grid fixo 600x600 no chão (step configurável)
        - Autoscale robusto (RR/RL nunca somem)
        """
        try:
            ax = self.ax_vis3d
            ax.clear()

            corners = ["FR", "FL", "RR", "RL"]
            pts_types = ["Sup In", "Sup Out", "Inf In", "Inf Out", "Toe In", "Toe Out", "Damper In", "Damper Out"]

            pts = {}
            plotted_points = []
            missing = []

            for c in corners:
                for p in pts_types:
                    key = f"{c} {p}"
                    try:
                        pts[key] = self._get_point3d_from_input(key)
                    except Exception:
                        missing.append(key)

            def plot_link(p1_name, p2_name, color, style="--", linewidth=2):
                if p1_name in pts and p2_name in pts:
                    p1 = pts[p1_name]
                    p2 = pts[p2_name]
                    ax.plot([p1.x, p2.x], [p1.z, p2.z], [p1.y, p2.y],
                            color=color, linestyle=style, linewidth=linewidth)

            for c in corners:
                col = "blue" if c.endswith("R") else "red"
                plot_link(f"{c} Sup In", f"{c} Sup Out", col, "--", 2)
                plot_link(f"{c} Inf In", f"{c} Inf Out", col, "--", 2)
                plot_link(f"{c} Toe In", f"{c} Toe Out", "green", "--", 2)
                plot_link(f"{c} Damper In", f"{c} Damper Out", "purple", "-", 2)

            for c in corners:
                for p in pts_types:
                    name = f"{c} {p}"
                    if name not in pts:
                        continue
                    pt = pts[name]
                    plotted_points.append(pt)

                    if "Toe" in name:
                        color = "green"
                    elif "Damper" in name:
                        color = "purple"
                    else:
                        color = "black"

                    ax.scatter(pt.x, pt.z, pt.y, c=color, marker="o", s=35)
                    ax.text(pt.x, pt.z, pt.y + 10, name, fontsize=7)

            step = float(getattr(self, "var_grid_step", tk.DoubleVar(value=50.0)).get())
            self._draw_ground_mesh_fixed_600(ax, step=step)

            self._autoscale_3d_equal(ax, plotted_points, pad_factor=0.35, min_range=400.0)

            ax.set_xlabel("X (Lateral)")
            ax.set_ylabel("Z (Longitudinal)")
            ax.set_zlabel("Y (Vertical)")

            info = []
            try:
                wb = self._read("wb")
                bf = self._read("bf")
                rs = self._read("rs")
                info.append(f"wb={wb:.0f} mm | bf={bf:.0f} mm | scrub={rs:.0f} mm")
            except Exception:
                pass
            if missing:
                info.append(f"⚠️ Faltando {len(missing)} hardpoints (não plotados).")

            self.lbl_steer_data.config(text="\n".join(info) if info else "Dados: --")

            self.canvas_vis3d.draw_idle()
            if not self.free_view:
                self._enforce_yz_view()

            self._set_status("Visualização 3D gerada (FR/FL/RR/RL + 32 hardpoints).")
            self.content_area.select(self.tab_vis3d)

        except Exception as e:
            self._set_status(f"Erro Visual 3D: {e}")
            messagebox.showerror("Erro", str(e))

    def _run_cg_sweep(self):
        try:
            h_ro = getattr(self.last_2d_results, "h_ro", None) if self.last_2d_results else None
            if h_ro is None:
                h_ro = 100.0

            params = models.SuspensionCGScanParameters(
                h_min=self._read("h_min"),
                h_max=self._read("h_max"),
                h_step=self._read("step"),
                mass=self._read("mass"),
                ay=self._read("ay"),
                track=self._read("bf"),
                h_ro=h_ro,
                scrub_radius=self._read("rs"),
                clearance=0.0
            )
            results = list(dynamics.calculate_baja_cg_sweep(params))

            for i in self.tree_cg.get_children():
                self.tree_cg.delete(i)
            for r in results:
                self.tree_cg.insert("", "end", values=(
                    f"{r.h_cg:.1f}", f"{r.d_fz:.1f}", f"{r.fz_int:.1f}", f"{r.m_roll:.1f}"
                ))

            cgs = [r.h_cg for r in results]
            self.ax_cg1.clear()
            self.ax_cg2.clear()
            self.ax_cg1.plot(cgs, [r.d_fz for r in results], marker="o")
            self.ax_cg1.set_title("Transf. Carga vs CG")
            self.ax_cg1.grid(True)

            self.ax_cg2.plot(cgs, [r.m_roll for r in results], marker="o")
            self.ax_cg2.set_title("Momento Rolagem vs CG")
            self.ax_cg2.grid(True)

            self.canvas_cg.draw()
            self._set_status("Varredura de CG concluída.")
            self.content_area.select(self.tab_dyn)
        except Exception as e:
            self._set_status(f"Erro CG sweep: {e}")
            messagebox.showerror("Erro", str(e))

    def _run_mass_sweep(self):
        try:
            if not hasattr(self, "tree_mass"):
                raise RuntimeError("tree_mass não existe (aba Dinâmica não montada).")

            h_ro = getattr(self.last_2d_results, "h_ro", None) if self.last_2d_results else None
            if h_ro is None:
                h_ro = 100.0

            m_min = self._read("m_min")
            m_max = self._read("m_max")
            step = self._read("step")

            if step <= 0:
                raise ValueError("Passo (step) deve ser > 0.")
            if m_max < m_min:
                raise ValueError("Massa Max deve ser maior ou igual à Massa Min.")

            params = models.SuspensionMassScanParameters(
                m_min=m_min,
                m_max=m_max,
                m_step=step,
                h_cg=self._read("hcg"),
                ay=self._read("ay"),
                track=self._read("bf"),
                h_ro=h_ro
            )

            results = list(dynamics.calculate_mass_sweep(params))

            for i in self.tree_mass.get_children():
                self.tree_mass.delete(i)

            for r in results:
                margin = getattr(r, "margin", None)
                self.tree_mass.insert("", "end", values=(
                    f"{r.mass:.0f}",
                    f"{r.m_roll:.1f}",
                    f"{r.d_fz:.1f}",
                    f"{r.ssf:.3f}",
                    f"{getattr(r, 'ay_crit', 0):.2f}",
                    f"{margin:.2f}" if margin is not None else "--"
                ))

            self._set_status("Varredura de massa concluída.")
            self.content_area.select(self.tab_dyn)

        except Exception as e:
            self._set_status(f"Erro mass sweep: {e}")
            messagebox.showerror("Erro", str(e))

    def _run_opt(self):
        try:
            corner = self.var_corner.get().strip() or "FR"

            if getattr(self, "var_auto_arm_angles", tk.IntVar(value=1)).get() == 1:
                ang_sup, ang_inf = self._compute_arm_angles_deg(corner)
                self._set("ang_sup", f"{ang_sup:.3f}")
                self._set("ang_inf", f"{ang_inf:.3f}")
            else:
                ang_sup = self._read("ang_sup")
                ang_inf = self._read("ang_inf")

            results = list(dynamics.calculate_force_vs_angle_sweep(
                models.ForceAngleIteratorParameters(
                    f_load=self._read("load"),
                    angle_sup_base=ang_sup,
                    angle_inf_base=ang_inf,
                    k_sup=self._read("ksup"),
                    k_inf=1.0 - self._read("ksup"),
                    limit=self._read("limit"),
                    ang_min=int(self._read("amin")),
                    ang_max=int(self._read("amax")),
                    step=int(5)
                )
            ))

            for i in self.tree_opt.get_children():
                self.tree_opt.delete(i)

            angles, f_sup, f_inf = [], [], []
            limit = self._read("limit")

            for r in results:
                self.tree_opt.insert("", "end", values=(
                    f"{r.angle_delta}", f"{r.force_sup:.1f}", f"{r.force_inf:.1f}", r.status
                ))
                angles.append(r.angle_delta)
                f_sup.append(r.force_sup)
                f_inf.append(r.force_inf)

            self.ax_opt.clear()
            self.ax_opt.plot(angles, f_sup, label="Sup", marker="o")
            self.ax_opt.plot(angles, f_inf, label="Inf", marker="o")
            self.ax_opt.axhline(limit, linestyle="--")
            self.ax_opt.legend()
            self.ax_opt.grid(True)
            self.canvas_opt.draw()

            self._set_status("Análise estrutural concluída.")
            self.content_area.select(self.tab_opt)
        except Exception as e:
            self._set_status(f"Erro Opt: {e}")
            messagebox.showerror("Erro Opt", str(e))

    # =========================
    # Presets (JSON geral)
    # =========================
    def _collect_preset(self) -> dict:
        data = {k: e.get() for k, e in self.entries.items()}
        hp = {}
        for lbl, (ex, ey, ez) in self.entries_hp.items():
            hp[lbl] = {"x": ex.get(), "y": ey.get(), "z": ez.get()}
        data["_hardpoints"] = hp
        data["_static"] = int(getattr(self, "var_static", tk.IntVar(value=0)).get())
        data["_corner"] = self.var_corner.get()
        return data

    def _apply_preset(self, data: dict):
        for k, e in self.entries.items():
            if k in data:
                e.delete(0, tk.END)
                e.insert(0, str(data[k]))

        hp = data.get("_hardpoints", {})
        for lbl, coords in hp.items():
            if lbl in self.entries_hp:
                ex, ey, ez = self.entries_hp[lbl]
                ex.delete(0, tk.END); ex.insert(0, str(coords.get("x", ex.get())))
                ey.delete(0, tk.END); ey.insert(0, str(coords.get("y", ey.get())))
                ez.delete(0, tk.END); ez.insert(0, str(coords.get("z", ez.get())))

        if hasattr(self, "var_static"):
            self.var_static.set(int(data.get("_static", 0)))

        if "_corner" in data:
            try:
                self.var_corner.set(str(data["_corner"]))
            except Exception:
                pass

    def _save_preset(self):
        path = filedialog.asksaveasfilename(defaultextension=".json", filetypes=[("JSON", "*.json")])
        if not path:
            return
        try:
            with open(path, "w", encoding="utf-8") as f:
                json.dump(self._collect_preset(), f, indent=2, ensure_ascii=False)
            self._set_status("Preset salvo.")
        except Exception as e:
            self._set_status(f"Erro ao salvar preset: {e}")
            messagebox.showerror("Erro", str(e))

    def _load_preset(self):
        path = filedialog.askopenfilename(filetypes=[("JSON", "*.json")])
        if not path:
            return
        try:
            with open(path, "r", encoding="utf-8") as f:
                data = json.load(f)
            self._apply_preset(data)
            self._set_status("Preset carregado.")
        except Exception as e:
            self._set_status(f"Erro ao carregar preset: {e}")
            messagebox.showerror("Erro", str(e))

    def _reset_defaults(self):
        defaults = {
            "bf": 1250, "s1": 40, "s2": 30, "cam_o": -1.5, "cam_i": -4.0,
            "spindle_sup_z": 300, "spindle_inf_z": 300, "toe_f_x": 600, "toe_r_x": 600,
            "mass": 200, "hcg": 450, "ay": 9.8, "wb": 1550, "fx": 1200, "rs": 50,
            "h_min": 250, "h_max": 650, "m_min": 180, "m_max": 240, "step": 25,
            "load": 3000, "ang_sup": 10, "ang_inf": 20, "limit": 8000, "ksup": 0.5,
            "amin": 0, "amax": 40,

            # gerador
            "z_front": 775, "z_rear": -775,
            "LsF": 450, "LiF": 520,
            "ySupInF": 420, "ySupOutF": 390, "yInfInF": 210, "yInfOutF": 190,
            "LsR": 430, "LiR": 500,
            "ySupInR": 410, "ySupOutR": 380, "yInfInR": 200, "yInfOutR": 180,
            "dz_inboard": 80, "dz_toe": 60, "dy_toe": 260, "dx_toe_in": 520,
            "dy_damper": 100, "dx_damper_in": 380
        }
        for k, v in defaults.items():
            if k in self.entries:
                self._set(k, v)

        self.var_corner.set("FR")
        self._set_status("Defaults restaurados.")


if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()
