import json
import csv
import math
import tkinter as tk
from tkinter import ttk, messagebox, filedialog

import matplotlib
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2Tk

# =============================================================================
# IMPORTS DE ARQUITETURA (Refatorado para usar APENAS o Orquestrador)
# =============================================================================
try:
    from orchestrators.suspension import SuspensionOrchestrator
except ImportError as e:
    print(f"ERRO CRÍTICO: Orquestrador não encontrado ({e}). A aplicação não funcionará.")
    SuspensionOrchestrator = None

try:
    from models.primitives import Point3D, Point2D, Vector3D
    from models.lines import LineCoefficients3D
    _models_ok = True
except ImportError:
    _models_ok = False
    Point3D = Point2D = Vector3D = LineCoefficients3D = None

# =============================================================================
# IMPORTS DE UI/UX (se existir) — mas agora com fallback interno completo
# =============================================================================
try:
    import ui_ux.ui_components as uic
except Exception as e:
    uic = None
    print(f"AVISO: ui_components não encontrado ({e}). Usando fallback interno.")

try:
    import ui_ux.visualization as vis
except Exception as e:
    vis = None

# =============================================================================
# UI/UX FALLBACK (se uic não existir): tema cinza + seções colapsáveis + scroll
# =============================================================================
_BG = "#d0d0d0"        # fundo cinza claro (pedido)
_PANEL = "#e0e0e0"     # painéis
_PANEL2 = "#f0f0f0"    # cards
_TEXT = "#111111"
_ACCENT = "#5a5a5a"


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
        x = self.widget.winfo_rootx() + 16
        y = self.widget.winfo_rooty() + 18
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
    """
    Seção colapsável (abre/fecha) para deixar a UI intuitiva e reduzir poluição visual.
    """
    def __init__(self, parent, title: str, initially_open=True):
        super().__init__(parent, style="Section.TFrame")
        self._open = tk.BooleanVar(value=bool(initially_open))

        header = ttk.Frame(self, style="SectionHeader.TFrame")
        header.pack(fill="x")

        self._btn = ttk.Button(
            header,
            text=("▼ " if self._open.get() else "► ") + title,
            command=self.toggle,
            style="SectionHeader.TButton"
        )
        self._btn.pack(side="left", fill="x", expand=True)

        self.body = ttk.Frame(self, style="SectionBody.TFrame")
        if self._open.get():
            self.body.pack(fill="x", expand=True)

    def toggle(self):
        self._open.set(not self._open.get())
        if self._open.get():
            self.body.pack(fill="x", expand=True)
        else:
            self.body.pack_forget()
        txt = self._btn.cget("text")
        # troca o primeiro símbolo
        if txt.startswith("▼ "):
            self._btn.config(text="► " + txt[2:])
        elif txt.startswith("► "):
            self._btn.config(text="▼ " + txt[2:])


class ScrollableFrame(ttk.Frame):
    def __init__(self, parent, width=420, height=800):
        super().__init__(parent, style="Scroll.TFrame")
        self.canvas = tk.Canvas(self, borderwidth=0, highlightthickness=0, bg=_BG)
        self.vsb = ttk.Scrollbar(self, orient="vertical", command=self.canvas.yview)
        self.canvas.configure(yscrollcommand=self.vsb.set)

        self.vsb.pack(side="right", fill="y")
        self.canvas.pack(side="left", fill="both", expand=True)

        self.scrollable_window = ttk.Frame(self.canvas, style="ScrollInner.TFrame")
        self.window_id = self.canvas.create_window((0, 0), window=self.scrollable_window, anchor="nw")

        self.scrollable_window.bind("<Configure>", self._on_frame_configure)
        self.canvas.bind("<Configure>", self._on_canvas_configure)

        # mousewheel
        self.canvas.bind_all("<MouseWheel>", self._on_mousewheel)

    def _on_frame_configure(self, _event=None):
        self.canvas.configure(scrollregion=self.canvas.bbox("all"))

    def _on_canvas_configure(self, event):
        # mantém largura do conteúdo igual à largura do canvas (UI mais limpa)
        self.canvas.itemconfigure(self.window_id, width=event.width)

    def _on_mousewheel(self, event):
        # scroll natural
        self.canvas.yview_scroll(int(-1 * (event.delta / 120)), "units")


def setup_style(root: tk.Tk):
    """
    Tema cinza e consistente usando ttk.
    """
    try:
        style = ttk.Style(root)
        # 'clam' costuma ser mais previsível
        style.theme_use("clam")
    except Exception:
        style = ttk.Style(root)

    root.configure(bg=_BG)

    # Base
    style.configure(".", background=_BG, foreground=_TEXT, font=("Segoe UI", 9))
    style.configure("TFrame", background=_BG)
    style.configure("TLabelframe", background=_BG, foreground=_TEXT)
    style.configure("TLabelframe.Label", background=_BG, foreground=_TEXT, font=("Segoe UI", 9, "bold"))
    style.configure("TLabel", background=_BG, foreground=_TEXT)
    style.configure("TButton", padding=(10, 6), background=_PANEL, foreground=_TEXT)
    style.map("TButton", background=[("active", _PANEL2)], foreground=[("disabled", "#777777")])

    # Entradas/Combobox
    style.configure("TEntry", fieldbackground="#ffffff", background=_BG, foreground=_TEXT)
    style.configure("TCombobox", fieldbackground="#ffffff")

    # Notebook
    style.configure("TNotebook", background=_BG, tabmargins=(6, 6, 6, 0))
    style.configure("TNotebook.Tab", background=_PANEL, padding=(12, 6))
    style.map("TNotebook.Tab", background=[("selected", _PANEL2)])

    # Seções colapsáveis
    style.configure("Section.TFrame", background=_BG)
    style.configure("SectionHeader.TFrame", background=_PANEL)
    style.configure("SectionHeader.TButton", background=_PANEL, anchor="w")
    style.map("SectionHeader.TButton", background=[("active", _PANEL2)])
    style.configure("SectionBody.TFrame", background=_BG)

    # Scroll
    style.configure("Scroll.TFrame", background=_BG)
    style.configure("ScrollInner.TFrame", background=_BG)

    # Tooltip
    style.configure("Tip.TFrame", background="#2b2b2b")
    style.configure("Tip.TLabel", background="#2b2b2b", foreground="#ffffff")

    # Treeview (tabelas) — mais “cara de app”
    style.configure(
        "Treeview",
        background="#ffffff",
        fieldbackground="#ffffff",
        foreground=_TEXT,
        rowheight=24
    )
    style.configure("Treeview.Heading", background=_PANEL, foreground=_TEXT, font=("Segoe UI", 9, "bold"))
    style.map("Treeview", background=[("selected", "#c7d7ff")])

    return style


# Se existir ui_components externo, usa ele; senão usa fallback interno
if uic is None:
    class uic:
        ToolTip = ToolTip
        CollapsibleSection = CollapsibleSection
        ScrollableFrame = ScrollableFrame

        @staticmethod
        def setup_style():
            # root ainda não existe aqui; o App chamará setup_style(root)
            return


def safe_num(s: str) -> float:
    if s is None: raise ValueError("Valor vazio.")
    s = str(s).strip().replace(",", ".")
    if s == "": raise ValueError("Valor vazio.")
    return float(s)


def _atan2_deg(num: float, den: float) -> float:
    return math.degrees(math.atan2(num, den))


def zebra_stripes(tv, colors=("#ffffff", "#eef2ff")):
    for i, row in enumerate(tv.get_children()):
        tv.item(row, tags=("even" if i % 2 == 0 else "odd",))
    tv.tag_configure("even", background=colors[0])
    tv.tag_configure("odd",  background=colors[1])


def autosize_treeview_columns(tv, min_width=80, max_width=300, pad=20):
    for col in tv["columns"]:
        children = tv.get_children()
        if children:
            w = max(
                len(str(tv.set(item, col))) * 8 + pad
                for item in children
            )
        else:
            w = min_width
        tv.column(col, width=max(min_width, min(max_width, w)))


def copy_treeview_to_clipboard(root, tv):
    cols = tv["columns"]
    lines = ["\t".join(cols)]
    for row in tv.get_children():
        lines.append("\t".join(str(v) for v in tv.item(row, "values")))
    root.clipboard_clear()
    root.clipboard_append("\n".join(lines))


# =============================================================================
# Vetores para cálculo de momentos da Panhard
# =============================================================================
class Vec3:
    __slots__ = ("x", "y", "z")

    def __init__(self, x, y, z):
        self.x = float(x)
        self.y = float(y)
        self.z = float(z)

    def norm(self):
        return math.sqrt(self.x * self.x + self.y * self.y + self.z * self.z)

    def unit(self):
        n = self.norm()
        if n < 1e-12:
            raise ValueError("Vetor com norma ~0.")
        return Vec3(self.x / n, self.y / n, self.z / n)

    def scale(self, k):
        return Vec3(self.x * k, self.y * k, self.z * k)


def cross(a: Vec3, b: Vec3) -> Vec3:
    return Vec3(
        a.y * b.z - a.z * b.y,
        a.z * b.x - a.x * b.z,
        a.x * b.y - a.y * b.x
    )


class App:
    def __init__(self, root: tk.Tk):
        self.root = root
        self.root.title("Simas Turbo")
        self.root.geometry("1920x1080")
        self.root.configure(bg=_BG)

        # aplica tema cinza (mesmo se tiver uic externo, a UI fica consistente)
        setup_style(self.root)

        # Modo do VIS 3D (AUTO herda FRONT/REAR, ou força DOUBLE_A/RIGID)
        self.var_vis3d_mode = tk.StringVar(value="AUTO")  # AUTO | DOUBLE_A | RIGID

        # Estado: 3D view
        self.free_view = False
        self._view_lock_cid = None

        # Estado: resultados
        self.last_2d_results = {"front": None, "rear": None}
        self.last_3d_results = None
        self.last_panhard = {"front": None, "rear": None}
        self.last_alignment = {"FRONT": None, "REAR": None, "CORNER": None}

        self.entries = {}
        self.entries_hp = {}
        self.hp_keys = []

        # Variáveis de controle
        self.var_corner = tk.StringVar(value="FR")
        self.var_front_type = tk.StringVar(value="DOUBLE_A")
        self.var_rear_type = tk.StringVar(value="RIGID")

        self.UI_LABEL = {
            "bf": "Bitola  [mm]",
            "wb": "Entre-eixos [mm]",
            "s1": "Curso Extensão (s1) [mm]",
            "s2": "Curso Compressão (s2) [mm]",
            "cam_o": "Cambagem Extensão [deg]", 
            "cam_i": "Cambagem Compressão [deg]",
            "spindle_sup_z": "Spindle Sup Z [mm]", 
            "spindle_inf_z": "Spindle Inf Z [mm]",
            "toe_f_x": "Toe Front X [mm]", 
            "toe_r_x": "Toe Rear X [mm]",
            "mass": "Massa [kg]",
            "hcg": "AlturaCG.(hCG) [mm]",
            "ay": "Aceleração lateral [m/s²]", 
            "fx": "Força longitudinal pneu (Fx) [N]",
            "rs": "Scrub Radius [mm]", 
            "h_min": "CG mín [mm]", 
            "h_max": "CG máx [mm]",
            "m_min": "Massa mín [kg]",
            "m_max": "Massa máx [kg]",
            "step": "Passo varredura",
            "front_share": "Distribuição lateral no eixo dianteiro [0..1]",
            "pf_ch_x": "Panhard Fr — Chassi X [mm]", 
            "pf_ch_y": "Panhard Fr — Chassi Y [mm]", 
            "pf_ch_z": "Panhard Fr — Chassi Z [mm]",
            "pf_ax_x": "Panhard Fr — Eixo X [mm]", 
            "pf_ax_y": "Panhard Fr — Eixo Y [mm]", 
            "pf_ax_z": "Panhard Fr — Eixo Z [mm]",
            "pf_ref_ax_x": "Ref FR (Eixo) X [mm]",
            "pf_ref_ax_y": "Ref FR (Eixo) Y [mm]",
            "pf_ref_ax_z": "Ref FR (Eixo) Z [mm]",
            "pf_ref_ch_x": "Ref FR (Chassi) X [mm]", 
            "pf_ref_ch_y": "Ref FR (Chassi) Y [mm]", 
            "pf_ref_ch_z": "Ref FR (Chassi) Z [mm]",
            "pr_ch_x": "Panhard Rr — Chassi X [mm]",
            "pr_ch_y": "Panhard Rr — Chassi Y [mm]",
            "pr_ch_z": "Panhard Rr — Chassi Z [mm]",
            "pr_ax_x": "Panhard Rr — Eixo X [mm]", 
            "pr_ax_y": "Panhard Rr — Eixo Y [mm]", 
            "pr_ax_z": "Panhard Rr — Eixo Z [mm]",
            "pr_ref_ax_x": "Ref RR (Eixo) X [mm]",
            "pr_ref_ax_y": "Ref RR (Eixo) Y [mm]",
            "pr_ref_ax_z": "Ref RR (Eixo) Z [mm]",
            "pr_ref_ch_x": "Ref RR (Chassi) X [mm]", 
            "pr_ref_ch_y": "Ref RR (Chassi) Y [mm]", 
            "pr_ref_ch_z": "Ref RR (Chassi) Z [mm]",
            "load": "Carga aplicada F [N]",
            "ang_sup": "Ângulo base braço superior [deg]",
            "ang_inf": "Ângulo base braço inferior [deg]", 
            "limit": "Limite de força [N]",
            "ksup": "Distribuição no braço sup (Ksup) [0..1]",
            "amin": "Varredura ângulo mínima [deg]", 
            "amax": "Varredura ângulo máxima [deg]",
        }

        self._build_topbar()

        self.paned = tk.PanedWindow(root, orient="horizontal", sashwidth=10, bg=_BG)
        self.paned.pack(fill="both", expand=True)

        # Sidebar com scroll
        self.sidebar_container = ScrollableFrame(self.paned, width=430)
        self.paned.add(self.sidebar_container, minsize=420, width=680)
        sidebar_parent = self.sidebar_container.scrollable_window

        self.content_area = ttk.Notebook(self.paned)
        self.paned.add(self.content_area, minsize=700)

        self._init_sidebar_inputs(sidebar_parent)
        self._init_result_tabs()
        self._build_statusbar()

    # =========================================================================
    # HELPERS DE UI
    # =========================================================================
    def _create_entry(self, parent, key, label, default, row, col, width=10, tip=None):
        label_txt = self.UI_LABEL.get(key, label)
        ttk.Label(parent, text=label_txt).grid(row=row, column=col, sticky="e", padx=3, pady=2)
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
        if not e: return
        e.delete(0, tk.END)
        e.insert(0, str(value))

    def _set_status(self, msg: str):
        if hasattr(self, "status_var"):
            self.status_var.set(msg)

    def _safe_call(self, fn, name: str):
        try:
            fn()
            return True
        except Exception as e:
            self._set_status(f"Erro em {name}: {e}")
            messagebox.showerror(f"Erro em {name}", str(e))
            return False

    # =========================================================================
    # HARDPOINTS HELPERS
    # =========================================================================
    def _corner_name(self, corner: str) -> str:
        corner = (corner or "").strip().upper()
        return {"FR": "FR ", "FL": "FL ", "RR": "RR ", "RL": "RL "}.get(corner, corner)

    def _point_name(self, p: str) -> str:
        return {
            "Sup In": "UCA — Inboard", 
            "Sup Out": "UCA — Outboard", 
            "Inf In": "LCA — Inboard",
            "Inf Out": "LCA — Outboard", 
            "Toe In": "Tie Rod — Inboard", 
            "Toe Out": "Tie Rod — Outboard",
            "Damper In": "Damper — Inboard", 
            "Damper Out": "Damper — Outboard"
        }.get(p, p)

    def _hp_pretty_label(self, corner: str, p: str) -> str:
        return f"{self._corner_name(corner)} | {self._point_name(p)}"

    def _hp_key(self, corner: str, point: str) -> str:
        return f"{corner} {point}"

    def _hp_get(self, lbl: str):
        ex, ey, ez = self.entries_hp[lbl]
        return safe_num(ex.get()), safe_num(ey.get()), safe_num(ez.get())

    def _hp_set(self, lbl: str, x: float, y: float, z: float):
        ex, ey, ez = self.entries_hp[lbl]
        ex.delete(0, tk.END); ex.insert(0, str(x))
        ey.delete(0, tk.END); ey.insert(0, str(y))
        ez.delete(0, tk.END); ez.insert(0, str(z))

    def _get_point3d_from_input(self, label: str):
        x, y, z = self._hp_get(label)
        return Point3D(x, y, z)

    # =========================================================================
    # NOVO: ALINHAMENTO (Camber/Caster/Toe)
    # =========================================================================
    def _calc_alignment_corner(self, corner: str) -> dict:
        c = (corner or "FR").strip().upper()

        sx, sy, sz = self._hp_get(self._hp_key(c, "Sup Out"))
        ix, iy, iz = self._hp_get(self._hp_key(c, "Inf Out"))

        dx = sx - ix
        dy = sy - iy
        dz = sz - iz

        camber_deg = _atan2_deg(dx, dy) if abs(dy) > 1e-12 else 0.0
        caster_deg = _atan2_deg(dz, dy) if abs(dy) > 1e-12 else 0.0

        tinx, tiny, tinz = self._hp_get(self._hp_key(c, "Toe In"))
        toutx, touty, toutz = self._hp_get(self._hp_key(c, "Toe Out"))

        dx_toe = toutx - tinx
        dz_toe = toutz - tinz
        toe_deg = _atan2_deg(dx_toe, dz_toe) if abs(dz_toe) > 1e-12 else 0.0

        return {"corner": c, "camber_deg": camber_deg, "caster_deg": caster_deg, "toe_deg": toe_deg}

    def _calc_alignment_axle(self, axis: str) -> dict:
        axis = (axis or "FRONT").strip().upper()
        corners = ("FR", "FL") if axis == "FRONT" else ("RR", "RL")
        per = {c: self._calc_alignment_corner(c) for c in corners}

        def avg(k):
            vals = [per[c][k] for c in corners]
            return sum(vals) / len(vals) if vals else 0.0

        return {
            "axis": axis,
            "per_corner": per,
            "avg_camber_deg": avg("camber_deg"),
            "avg_caster_deg": avg("caster_deg"),
            "avg_toe_deg": avg("toe_deg"),
        }

    # =========================================================================
    # TOPBAR / STATUS
    # =========================================================================
    def _build_topbar(self):
        bar = ttk.Frame(self.root)
        bar.pack(fill="x")

        ttk.Label(bar, text="Ações rápidas", font=("Segoe UI", 10, "bold")).pack(side="left", padx=(10, 12))

        actions = [
            ("2D (F/R)",       self._calc_2d_front_rear),
            ("Análise 3D",     self._calc_3d),
            ("Visual 3D",      self._calc_full_3d),
            ("Alinhamento",    self._calc_alignment_tab),
            ("Cinemática",     self._run_kinematic_sweep),
            ("Panhard",        self._calc_panhard_moments),
            ("CG Sweep",       self._run_cg_sweep),
            ("Mass Sweep",     self._run_mass_sweep),
            ("Estrutura",      self._run_opt),
        ]
        for txt, cmd in actions:
            ttk.Button(bar, text=txt, command=cmd).pack(side="left", padx=4, pady=6)

        ttk.Separator(bar, orient="vertical").pack(side="left", fill="y", padx=12)

        ttk.Button(bar, text="Salvar preset", command=self._save_preset).pack(side="left", padx=4)
        ttk.Button(bar, text="Carregar preset", command=self._load_preset).pack(side="left", padx=4)
        ttk.Button(bar, text="Reset defaults", command=self._reset_defaults).pack(side="left", padx=4)

    def _build_statusbar(self):
        self.status_var = tk.StringVar(value="Pronto.")
        bar = ttk.Frame(self.root)
        bar.pack(fill="x", side="bottom")
        ttk.Label(bar, textvariable=self.status_var).pack(side="left", padx=10, pady=4)

    # =========================================================================
    # SIDEBAR INPUTS (agora tudo em seções colapsáveis)
    # =========================================================================
    def _init_sidebar_inputs(self, parent):
        # 1) Geometria
        sec_geo = CollapsibleSection(parent, "1) Geometria (Hardpoints) [mm]", initially_open=True)
        sec_geo.pack(fill="x", padx=8, pady=8)
        f_geo = sec_geo.body

        self._create_entry(f_geo, "bf", "Bitola (bf):", 1250, 0, 0, width=12, tip="Distância entre centros das rodas (track). Use mm.")
        self._create_entry(f_geo, "wb", "Entre-eixos (wb):", 1550, 0, 2, width=12)
        
        ttk.Label(f_geo, text="Canto p/ cálculo 3D:").grid(row=1, column=0, sticky="e", padx=3, pady=2)
        cb = ttk.Combobox(f_geo, values=["FR", "FL", "RR", "RL"], textvariable=self.var_corner, width=6, state="readonly")
        cb.grid(row=1, column=1, sticky="w", padx=3, pady=2)
        ToolTip(cb, "Define qual canto será usado nos cálculos 3D (forças/alinhamento).")

        ttk.Label(f_geo, text="Tipo DIANTEIRO:").grid(row=1, column=2, sticky="e", padx=3, pady=2)
        cbf = ttk.Combobox(f_geo, values=["DOUBLE_A", "RIGID"], textvariable=self.var_front_type, width=10, state="readonly")
        cbf.grid(row=1, column=3, sticky="w", padx=3, pady=2)

        ttk.Label(f_geo, text="Tipo TRASEIRO:").grid(row=2, column=2, sticky="e", padx=3, pady=2)
        cbr = ttk.Combobox(f_geo, values=["DOUBLE_A", "RIGID"], textvariable=self.var_rear_type, width=10, state="readonly")
        cbr.grid(row=2, column=3, sticky="w", padx=3, pady=2)

        ttk.Label(f_geo, text="X").grid(row=6, column=1)
        ttk.Label(f_geo, text="Y").grid(row=6, column=2)
        ttk.Label(f_geo, text="Z").grid(row=6, column=3)

        corners = ["FR", "FL", "RR", "RL"]
        pts = ["Sup In", "Sup Out", "Inf In", "Inf Out", "Toe In", "Toe Out", "Damper In", "Damper Out"]
        base_defaults = {
            "Sup In": (450, 420, 200), "Sup Out": (625, 390, 300),
            "Inf In": (430, 210, 200), "Inf Out": (625, 190, 300),
            "Toe In": (520, 260, 120), "Toe Out": (650, 260, 180),
            "Damper In": (380, 520, 150), "Damper Out": (610, 430, 260),
        }

        r = 7
        self.hp_keys = []
        for corner in corners:
            ttk.Label(f_geo, text=f"== {self._corner_name(corner)} ==", font=("Segoe UI", 9, "bold")).grid(
                row=r, column=0, columnspan=4, sticky="w", pady=(10, 2)
            )
            r += 1
            for p in pts:
                lbl = self._hp_key(corner, p)
                dx, dy, dz = base_defaults[p]
                if corner in ("FL", "RL"): dx = -dx
                ttk.Label(f_geo, text=self._hp_pretty_label(corner, p) + ":").grid(row=r, column=0, sticky="e")
                ex = ttk.Entry(f_geo, width=8); ex.insert(0, str(dx)); ex.grid(row=r, column=1)
                ey = ttk.Entry(f_geo, width=8); ey.insert(0, str(dy)); ey.grid(row=r, column=2)
                ez = ttk.Entry(f_geo, width=8); ez.insert(0, str(dz)); ez.grid(row=r, column=3)
                self.entries_hp[lbl] = (ex, ey, ez)
                self.hp_keys.append(lbl)
                r += 1

        frm_csv = ttk.Frame(f_geo)
        frm_csv.grid(row=r, column=0, columnspan=4, sticky="ew", pady=(10, 2))
        ttk.Button(frm_csv, text="Export CSV", command=self._export_hardpoints_csv).pack(side="left", padx=4)
        ttk.Button(frm_csv, text="Import CSV", command=self._import_hardpoints_csv).pack(side="left", padx=4)

        # 2) Cinemática
        sec_kin = CollapsibleSection(parent, "2) Cinemática & Alinhamento", initially_open=False)
        sec_kin.pack(fill="x", padx=8, pady=8)
        f_kin = sec_kin.body

        self._create_entry(f_kin, "s1", "Curso Ext:", 40, 0, 0)
        self._create_entry(f_kin, "s2", "Curso Int:", 30, 0, 2)
        self._create_entry(f_kin, "cam_o", "Cam Ext:", -1.5, 1, 0)
        self._create_entry(f_kin, "cam_i", "Cam Int:", -4.0, 1, 2)
        self._create_entry(f_kin, "spindle_sup_z", "Spindle Sup Z:", 300, 3, 0)
        self._create_entry(f_kin, "spindle_inf_z", "Spindle Inf Z:", 300, 3, 2)
        self._create_entry(f_kin, "toe_f_x", "Toe F X:", 600, 4, 0)
        self._create_entry(f_kin, "toe_r_x", "Toe R X:", 600, 4, 2)

        # 3) Dinâmica
        sec_dyn = CollapsibleSection(parent, "3) Dinâmica Veicular", initially_open=False)
        sec_dyn.pack(fill="x", padx=8, pady=8)
        f_dyn = sec_dyn.body

        self._create_entry(f_dyn, "mass", "Massa (kg):", 260, 0, 0)
        self._create_entry(f_dyn, "hcg", "Altura CG:", 450, 0, 2)
        self._create_entry(f_dyn, "ay", "Ay (m/s²):", 9.8, 1, 0)
        self._create_entry(f_dyn, "fx", "Fx Pneu (N):", 1200, 2, 0)
        self._create_entry(f_dyn, "rs", "Scrub (mm):", 50, 2, 2)
        self._create_entry(f_dyn, "h_min", "CG Min:", 250, 4, 0)
        self._create_entry(f_dyn, "h_max", "CG Max:", 650, 4, 2)
        self._create_entry(f_dyn, "m_min", "Massa Min:", 180, 5, 0)
        self._create_entry(f_dyn, "m_max", "Massa Max:", 300, 5, 2)
        self._create_entry(f_dyn, "step", "Passo:", 25, 6, 0)

        # 4) Panhard
        sec_pan = CollapsibleSection(parent, "4) Panhard (Front/Rear) + Momentos (suportes)", initially_open=False)
        sec_pan.pack(fill="x", padx=8, pady=8)
        f_pan = sec_pan.body

        ttk.Label(f_pan, text="Distribuição lateral (fração no eixo dianteiro):").grid(
            row=0, column=0, sticky="e", padx=3, pady=2
        )
        self._create_entry(
            f_pan, "front_share", "Front share:", 0.55, 0, 2, width=10,
            tip="Fy_front = front_share*(m*ay). Fy_rear = (1-front_share)*(m*ay)."
        )

        ttk.Separator(f_pan, orient="horizontal").grid(row=1, column=0, columnspan=4, sticky="ew", pady=(8, 8))

        ttk.Label(f_pan, text="FRONT PANHARD", font=("Segoe UI", 9, "bold")).grid(row=2, column=0, columnspan=4, sticky="w")
        self._create_entry(f_pan, "pf_ch_x", "Chassi X:", -250, 3, 0, width=9)
        self._create_entry(f_pan, "pf_ch_y", "Chassi Y:", 260, 3, 2, width=9)
        self._create_entry(f_pan, "pf_ch_z", "Chassi Z:", 775, 4, 0, width=9)
        self._create_entry(f_pan, "pf_ax_x", "Eixo X:", 250, 4, 2, width=9)
        self._create_entry(f_pan, "pf_ax_y", "Eixo Y:", 240, 5, 0, width=9)
        self._create_entry(f_pan, "pf_ax_z", "Eixo Z:", 775, 5, 2, width=9)

        ttk.Label(f_pan, text="Ref FRONT (p/ momento)", font=("Segoe UI", 9, "bold")).grid(
            row=6, column=0, columnspan=4, sticky="w", pady=(6, 0)
        )
        self._create_entry(f_pan, "pf_ref_ax_x", "Ref eixo X:", 0, 7, 0, width=9)
        self._create_entry(f_pan, "pf_ref_ax_y", "Ref eixo Y:", 220, 7, 2, width=9)
        self._create_entry(f_pan, "pf_ref_ax_z", "Ref eixo Z:", 775, 8, 0, width=9)
        self._create_entry(f_pan, "pf_ref_ch_x", "Ref chassi X:", 0, 8, 2, width=9)
        self._create_entry(f_pan, "pf_ref_ch_y", "Ref chassi Y:", 300, 9, 0, width=9)
        self._create_entry(f_pan, "pf_ref_ch_z", "Ref chassi Z:", 775, 9, 2, width=9)
        ttk.Separator(f_pan, orient="horizontal").grid(row=10, column=0, columnspan=4, sticky="ew", pady=(8, 8))

        ttk.Label(f_pan, text="REAR PANHARD", font=("Segoe UI", 9, "bold")).grid(row=11, column=0, columnspan=4, sticky="w")
        self._create_entry(f_pan, "pr_ch_x", "Chassi X:", 250, 12, 0, width=9)
        self._create_entry(f_pan, "pr_ch_y", "Chassi Y:", 260, 12, 2, width=9)
        self._create_entry(f_pan, "pr_ch_z", "Chassi Z:", -775, 13, 0, width=9)
        self._create_entry(f_pan, "pr_ax_x", "Eixo X:", -250, 13, 2, width=9)
        self._create_entry(f_pan, "pr_ax_y", "Eixo Y:", 240, 14, 0, width=9)
        self._create_entry(f_pan, "pr_ax_z", "Eixo Z:", -775, 14, 2, width=9)

        ttk.Label(f_pan, text="Ref REAR (p/ momento)", font=("Segoe UI", 9, "bold")).grid(
            row=15, column=0, columnspan=4, sticky="w", pady=(6, 0)
        )
        self._create_entry(f_pan, "pr_ref_ax_x", "Ref eixo X:", 0, 16, 0, width=9)
        self._create_entry(f_pan, "pr_ref_ax_y", "Ref eixo Y:", 220, 16, 2, width=9)
        self._create_entry(f_pan, "pr_ref_ax_z", "Ref eixo Z:", -775, 17, 0, width=9)
        self._create_entry(f_pan, "pr_ref_ch_x", "Ref chassi X:", 0, 17, 2, width=9)
        self._create_entry(f_pan, "pr_ref_ch_y", "Ref chassi Y:", 300, 18, 0, width=9)
        self._create_entry(f_pan, "pr_ref_ch_z", "Ref chassi Z:", -775, 18, 2, width=9)

        # 5) Otimização
        sec_opt = CollapsibleSection(parent, "5) Otimização Estrutural", initially_open=False)
        sec_opt.pack(fill="x", padx=8, pady=8)
        f_opt = sec_opt.body

        self._create_entry(f_opt, "load", "Carga F [N]:", 3000, 0, 0)
        self.var_static = tk.IntVar(value=0)
        ttk.Checkbutton(f_opt, text="+ Estática", variable=self.var_static).grid(row=0, column=2, sticky="w")
        self.var_auto_arm_angles = tk.IntVar(value=1)
        ttk.Checkbutton(f_opt, text="Usar ângulos automáticos", variable=self.var_auto_arm_angles).grid(row=1, column=0, columnspan=4, sticky="w")
        self._create_entry(f_opt, "ang_sup", "Ang Base Sup:", 10, 2, 0)
        self._create_entry(f_opt, "ang_inf", "Ang Base Inf:", 20, 2, 2)
        self._create_entry(f_opt, "limit", "Limite (N):", 8000, 3, 0)
        self._create_entry(f_opt, "ksup", "K Sup:", 0.5, 3, 2)
        self._create_entry(f_opt, "amin", "Varredura Min:", 0, 4, 0)
        self._create_entry(f_opt, "amax", "Varredura Max:", 40, 4, 2)

    # =========================================================================
    # 2D
    # =========================================================================

    def _calc_2d_front_rear(self):
        if not SuspensionOrchestrator:
            return
        self._safe_call(self._do_calc_2d, "Cálculo 2D")
        self.content_area.select(self.tab_2d)

    def _do_calc_2d(self):
        ui_data = self._collect_preset()

        for axis, ax_obj, axis_key in [
            ("FRONT", self.ax_2d_front, "front"),
            ("REAR",  self.ax_2d_rear,  "rear"),
        ]:
            ax_obj.clear()
            corners = ("FR", "FL") if axis == "FRONT" else ("RR", "RL")
            mode_var = self.var_front_type if axis == "FRONT" else self.var_rear_type
            mode = mode_var.get().strip().upper()

            if mode == "RIGID":
                # Rigid axle: draw axle beam + approximate Panhard RC
                corner_L, corner_R = corners[1], corners[0]
                xr, yr, _ = self._hp_get(self._hp_key(corner_R, "Inf Out"))
                xl, yl, _ = self._hp_get(self._hp_key(corner_L, "Inf Out"))
                ax_obj.plot([xl, xr], [yl, yr], "o-", linewidth=2, label="Eixo rígido")

                if axis == "FRONT":
                    h_ro = 0.5 * (self._read("pf_ch_y") + self._read("pf_ax_y"))
                else:
                    h_ro = 0.5 * (self._read("pr_ch_y") + self._read("pr_ax_y"))

                ax_obj.plot(0, h_ro, "*", markersize=14, color="tab:orange", label=f"RC ≈ {h_ro:.1f} mm")
                ax_obj.axvline(0, linestyle=":", color="k")
                ax_obj.set_title(f"{axis} 2D — Eixo Rígido")
                self.last_2d_results[axis_key] = {"h_ro": h_ro}
            else:
                # Double A-arm: compute roll centre via domain model
                res = SuspensionOrchestrator.run_2d_analysis(ui_data, axis)
                h_ro = res.get("h_ro", 0.0)
                self.last_2d_results[axis_key] = {"h_ro": h_ro}

                colors_map = {corners[0]: "tab:blue", corners[1]: "tab:red"}
                for c in corners:
                    col = colors_map[c]
                    try:
                        ui_x, ui_y, _ = self._hp_get(self._hp_key(c, "Sup In"))
                        uo_x, uo_y, _ = self._hp_get(self._hp_key(c, "Sup Out"))
                        li_x, li_y, _ = self._hp_get(self._hp_key(c, "Inf In"))
                        lo_x, lo_y, _ = self._hp_get(self._hp_key(c, "Inf Out"))
                        ax_obj.plot([ui_x, uo_x], [ui_y, uo_y], "o--", color=col, label=f"{c} Upper")
                        ax_obj.plot([li_x, lo_x], [li_y, lo_y], "o-",  color=col, label=f"{c} Lower")
                    except Exception:
                        pass
                    rc = res["corners"].get(c)
                    if rc and rc.instantaneous_center:
                        ic = rc.instantaneous_center
                        ax_obj.plot(ic.x, ic.y, "s", color=col, markersize=8)
                        ax_obj.annotate(f"IC {c}", (ic.x, ic.y), fontsize=7, color=col)
                    if rc and rc.roll_center_point:
                        rcp = rc.roll_center_point
                        ax_obj.plot(rcp.x, rcp.y, "*", color=col, markersize=12)

                ax_obj.plot(0, h_ro, "D", color="k", markersize=10, label=f"RC avg {h_ro:.1f} mm")
                ax_obj.set_title(f"{axis} 2D — Double A-arm")

            ax_obj.axvline(0, linestyle=":", color="k", linewidth=0.8)
            ax_obj.axhline(0, linestyle="-", color="gray", linewidth=0.5)
            ax_obj.grid(True)
            ax_obj.legend(fontsize=7)
            ax_obj.set_xlabel("X lateral [mm]")
            ax_obj.set_ylabel("Y vertical [mm]")

        self._update_2d_summary_label()
        self.canvas_2d.draw()
        self._set_status("✅ 2D calculado (Front + Rear).")

    def _update_2d_summary_label(self):
        f = self.last_2d_results.get("front", {}).get("h_ro")
        r = self.last_2d_results.get("rear", {}).get("h_ro")
        f_str = f"{f:.1f} mm" if f is not None else "--"
        r_str = f"{r:.1f} mm" if r is not None else "--"
        self.lbl_2d_res.config(text=f"FRONT h_Ro={f_str} | REAR h_Ro={r_str}")

        def h(v):
            return "--" if (v is None or getattr(v, "h_ro", None) is None) else f"{v.h_ro:.1f} mm"

        self.lbl_2d_res.config(text=(
            f"FRONT type={self.var_front_type.get()} | h_Ro={h(f)}\n"
            f"REAR  type={self.var_rear_type.get()} | h_Ro={h(r)}"
        ))

    # =========================================================================
    # ABA: ALINHAMENTO
    # =========================================================================
    def _calc_alignment_tab(self):
        self._safe_call(self._alignment_calc_all, "Alinhamento")
        self.content_area.select(self.tab_align)

    def _alignment_clear(self):
        for i in self.tree_align.get_children():
            self.tree_align.delete(i)

    def _alignment_calc_all(self):
        self._alignment_clear()
        corners = ["FR", "FL", "RR", "RL"]
        for c in corners:
            d = self._calc_alignment_corner(c)
            self.tree_align.insert("", "end", values=(
                c,
                f"{d['camber_deg']:.3f}",
                f"{d['caster_deg']:.3f}",
                f"{d['toe_deg']:.3f}",
            ))
        zebra_stripes(self.tree_align)
        autosize_treeview_columns(self.tree_align)

        front = self._calc_alignment_axle("FRONT")
        rear = self._calc_alignment_axle("REAR")
        self.last_alignment["FRONT"] = front
        self.last_alignment["REAR"] = rear

        self.lbl_align_summary.config(text=(
            f"FRONT: Cam={front['avg_camber_deg']:.3f}° Cas={front['avg_caster_deg']:.3f}° Toe={front['avg_toe_deg']:.3f}°   |   "
            f"REAR: Cam={rear['avg_camber_deg']:.3f}° Cas={rear['avg_caster_deg']:.3f}° Toe={rear['avg_toe_deg']:.3f}°"
        ))
        self._set_status("✅ Alinhamento calculado (4 cantos + médias).")

    # =========================================================================
    # PANHARD
    # =========================================================================
    def _calc_panhard_moments(self):
        for i in self.tree_panhard.get_children():
            self.tree_panhard.delete(i)

        share_front = max(0.0, min(1.0, self._read("front_share")))
        share_rear = 1.0 - share_front

        self.last_panhard["front"] = self._calc_panhard_one(axis="FRONT", share=share_front)
        self.last_panhard["rear"] = self._calc_panhard_one(axis="REAR", share=share_rear)

        def line(axis, d):
            if not d:
                return f"{axis}: --"
            return f"{axis}: Fy={d['Fy_axle']:.1f}N | F_axial={d['F_axial']:.1f}N | ux={d['ux']:.3f}"

        self.lbl_panhard_res.config(
            text=line("FRONT", self.last_panhard["front"]) + "   ||   " + line("REAR", self.last_panhard["rear"])
        )
        zebra_stripes(self.tree_panhard)
        autosize_treeview_columns(self.tree_panhard)
        self._set_status("✅ Panhard calculada (força + momentos nos suportes).")
        self.content_area.select(self.tab_panhard)

    def _calc_panhard_one(self, axis: str, share: float):
        if axis == "FRONT":
            ch = (self._read("pf_ch_x"), self._read("pf_ch_y"), self._read("pf_ch_z"))
            axp = (self._read("pf_ax_x"), self._read("pf_ax_y"), self._read("pf_ax_z"))
            ref_ax = (self._read("pf_ref_ax_x"), self._read("pf_ref_ax_y"), self._read("pf_ref_ax_z"))
            ref_ch = (self._read("pf_ref_ch_x"), self._read("pf_ref_ch_y"), self._read("pf_ref_ch_z"))
        else:
            ch = (self._read("pr_ch_x"), self._read("pr_ch_y"), self._read("pr_ch_z"))
            axp = (self._read("pr_ax_x"), self._read("pr_ax_y"), self._read("pr_ax_z"))
            ref_ax = (self._read("pr_ref_ax_x"), self._read("pr_ref_ax_y"), self._read("pr_ref_ax_z"))
            ref_ch = (self._read("pr_ref_ch_x"), self._read("pr_ref_ch_y"), self._read("pr_ref_ch_z"))

        m = self._read("mass")
        ay = self._read("ay")
        Fy_total = m * ay
        Fy_axle = share * Fy_total

        v = Vec3(ch[0] - axp[0], ch[1] - axp[1], ch[2] - axp[2]).unit()
        ux = v.x
        if abs(ux) < 1e-6:
            raise ValueError(f"{axis}: Panhard com ux≈0 (quase sem componente lateral).")

        F_axial = Fy_axle / ux

        F_on_axle = v.scale(F_axial)
        F_on_ch = F_on_axle.scale(-1.0)

        r_ax = Vec3(axp[0] - ref_ax[0], axp[1] - ref_ax[1], axp[2] - ref_ax[2])
        r_ch = Vec3(ch[0] - ref_ch[0], ch[1] - ref_ch[1], ch[2] - ref_ch[2])

        M_ax = cross(r_ax, F_on_axle)
        M_ch = cross(r_ch, F_on_ch)

        self.tree_panhard.insert("", "end", values=(
            axis,
            f"{Fy_axle:.1f}",
            f"{F_axial:.1f}",
            f"{ux:.3f}",
            f"{M_ax.x:.1f}", f"{M_ax.y:.1f}", f"{M_ax.z:.1f}",
            f"{M_ch.x:.1f}", f"{M_ch.y:.1f}", f"{M_ch.z:.1f}",
        ))

        return {"Fy_axle": Fy_axle, "F_axial": F_axial, "ux": ux, "M_ax": M_ax, "M_ch": M_ch}

    # =========================================================================
    # 3D (domínio) + VISUALIZAÇÃO
    # =========================================================================
    def _calc_3d(self):
        if not SuspensionOrchestrator:
            messagebox.showerror("Erro", "Orquestrador não importado.")
            return
        self._safe_call(self._do_calc_3d, "Cálculo 3D")
        self.content_area.select(self.tab_3d)

    def _do_calc_3d(self):
        corner = (self.var_corner.get().strip() or "FR").upper()
        ui_data = self._collect_preset()

        result = SuspensionOrchestrator.run_full_3d_analysis(ui_data, corner)
        self.last_3d_results = result

        m = result["metrics"]
        kp = m.kingpin_parameters
        rc = m.roll_center_parameters
        ta = m.tire_angles
        ad = m.anti_dive_parameters
        aq = m.anti_squat_parameters

        lines = [
            f"═══════ Canto: {corner} ═══════",
            "",
            "── Ângulos do Pneu ──────────────────",
            f"  Cambagem (KPI)  : {math.degrees(ta.camber):+.3f} °",
            f"  Convergência     : {math.degrees(ta.toe):+.3f} °",
            "",
            "── Geometria do Kingpin ─────────────",
            f"  Inclinação (KPI) : {math.degrees(kp.inclination):+.3f} °",
            f"  Caster           : {math.degrees(kp.caster):+.3f} °",
            f"  Scrub Radius     : {kp.scrub_radius:+.2f} mm",
            f"  Mechanical Trail : {kp.mechanical_trail:+.2f} mm",
            "",
            "── Centro de Rolamento ──────────────",
        ]

        if rc.roll_center_point is not None:
            lines += [
                f"  h_RC             : {rc.roll_center_point.y:.2f} mm",
                f"  Q Factor (Reimp) : {rc.q_factor:.4f}" if rc.q_factor is not None else "  Q Factor        : --",
            ]
            if rc.instantaneous_center is not None:
                ic = rc.instantaneous_center
                lines.append(f"  IC (x,y)         : ({ic.x:.1f}, {ic.y:.1f}) mm")
        else:
            lines.append("  Roll Center      : paralelo / não calculado")

        lines += [
            "",
            "── Geometria Longitudinal ───────────",
            f"  Anti-Dive        : {ad.percentage:+.1f} %",
            f"  Anti-Squat       : {aq.percentage:+.1f} %",
        ]

        st = result.get("steering")
        if st is not None:
            lines += [
                "",
                "── Ackermann / Direção ──────────────",
                f"  Ângulo interno   : {math.degrees(st.inner_angle):.2f} °",
                f"  Ângulo externo   : {math.degrees(st.outer_angle):.2f} °",
                f"  Ideal externo    : {math.degrees(st.ideal_outer_angle):.2f} °",
                f"  % Ackermann      : {st.ackermann_percentage:.1f} %",
                f"  Raio de curva    : {st.turning_radius:.0f} mm",
            ]

        cg_tuple = result.get("camber_gain")
        if cg_tuple is not None:
            static_rad, new_rad, gain = cg_tuple
            lines += [
                "",
                "── Ganho de Câmbagem (20 mm bump) ───",
                f"  Câmbagem estática: {math.degrees(static_rad):+.3f} °",
                f"  Câmbagem nova    : {math.degrees(new_rad):+.3f} °",
                f"  Ganho            : {gain:+.4f} °/mm",
            ]

        self.lbl_3d_res.config(text="\n".join(lines))
        self._set_status(f"✅ Análise 3D completa — canto {corner}.")

    def _calc_full_3d(self):
        ax = self.ax_vis3d
        ax.clear()

        corners = ["FR", "FL", "RR", "RL"]
        pts_types = ["Sup In", "Sup Out", "Inf In", "Inf Out", "Toe In", "Toe Out", "Damper In", "Damper Out"]

        pts = {}
        for c in corners:
            for p in pts_types:
                key = self._hp_key(c, p)
                try:
                    pts[key] = self._get_point3d_from_input(key)
                except Exception:
                    pts[key] = None

        def getp(corner: str, name: str):
            return pts.get(f"{corner} {name}")

        def safe_link(p1, p2, color="k", style="-", lw=2.0, alpha=0.95):
            if p1 is None or p2 is None:
                return
            if vis is not None and hasattr(vis, "plot_suspension_link"):
                vis.plot_suspension_link(ax, p1, p2, color, style)
            else:
                ax.plot([p1.x, p2.x], [p1.z, p2.z], [p1.y, p2.y], linestyle=style, linewidth=lw, alpha=alpha)

        def scatter_pt(p, s=18, marker="o"):
            if p is None:
                return
            ax.scatter(p.x, p.z, p.y, s=s, marker=marker, c="k")

        def draw_double_a_corner(corner: str):
            col = "tab:blue" if corner.endswith("R") else "tab:red"

            safe_link(getp(corner, "Sup In"), getp(corner, "Sup Out"), col, "--", lw=2.2)
            safe_link(getp(corner, "Inf In"), getp(corner, "Inf Out"), col, "-", lw=2.6)

            safe_link(getp(corner, "Toe In"), getp(corner, "Toe Out"), "tab:green", "--", lw=2.0)
            safe_link(getp(corner, "Damper In"), getp(corner, "Damper Out"), "tab:purple", "-", lw=2.0)

            for name in pts_types:
                p = getp(corner, name)
                if p is None:
                    continue
                if "Sup" in name:
                    scatter_pt(p, s=55, marker="o")
                elif "Inf" in name:
                    scatter_pt(p, s=55, marker="s")
                elif "Toe" in name:
                    scatter_pt(p, s=55, marker="^")
                elif "Damper" in name:
                    scatter_pt(p, s=55, marker="D")

            pso = getp(corner, "Sup Out")
            pio = getp(corner, "Inf Out")
            ptxt = pso if pso is not None else pio
            if ptxt is not None:
                ax.text(ptxt.x, ptxt.z, ptxt.y, f" {corner}", fontsize=9)

        def draw_rigid_axle(axis: str):
            if Point3D is None:
                return

            if axis == "FRONT":
                left_corner, right_corner = "FL", "FR"
                ch  = Point3D(self._read("pf_ch_x"), self._read("pf_ch_y"), self._read("pf_ch_z"))
                axp = Point3D(self._read("pf_ax_x"), self._read("pf_ax_y"), self._read("pf_ax_z"))
            else:
                left_corner, right_corner = "RL", "RR"
                ch  = Point3D(self._read("pr_ch_x"), self._read("pr_ch_y"), self._read("pr_ch_z"))
                axp = Point3D(self._read("pr_ax_x"), self._read("pr_ax_y"), self._read("pr_ax_z"))

            pl = getp(left_corner, "Inf Out")
            pr = getp(right_corner, "Inf Out")
            safe_link(pl, pr, "black", "-", lw=3.2)

            scatter_pt(pl, s=90, marker="s")
            scatter_pt(pr, s=90, marker="s")

            safe_link(axp, ch, "orange", "-", lw=3.2)
            ax.scatter(axp.x, axp.z, axp.y, s=110, marker="X", c="k")
            ax.scatter(ch.x, ch.z, ch.y, s=110, marker="X", c="k")

        mode = (self.var_vis3d_mode.get() or "AUTO").strip().upper()

        def axis_mode(axis: str) -> str:
            if mode in ("DOUBLE_A", "RIGID"):
                return mode
            if axis == "FRONT":
                return (self.var_front_type.get() or "DOUBLE_A").strip().upper()
            return (self.var_rear_type.get() or "DOUBLE_A").strip().upper()

        front_mode = axis_mode("FRONT")
        rear_mode = axis_mode("REAR")

        draw_double_a_corner("FR")
        draw_double_a_corner("FL")
        if front_mode == "RIGID":
            draw_rigid_axle("FRONT")

        draw_double_a_corner("RR")
        draw_double_a_corner("RL")
        if rear_mode == "RIGID":
            draw_rigid_axle("REAR")

        valid_pts = [p for p in pts.values() if p is not None]
        if Point3D is not None:
            try:
                pf_ch = Point3D(self._read("pf_ch_x"), self._read("pf_ch_y"), self._read("pf_ch_z"))
                pf_ax = Point3D(self._read("pf_ax_x"), self._read("pf_ax_y"), self._read("pf_ax_z"))
                pr_ch = Point3D(self._read("pr_ch_x"), self._read("pr_ch_y"), self._read("pr_ch_z"))
                pr_ax = Point3D(self._read("pr_ax_x"), self._read("pr_ax_y"), self._read("pr_ax_z"))
                valid_pts += [pf_ch, pf_ax, pr_ch, pr_ax]
            except Exception:
                pass

        step = float(getattr(self, "var_grid_step", tk.DoubleVar(value=50.0)).get())
        if vis is not None and hasattr(vis, "draw_ground_mesh"):
            vis.draw_ground_mesh(ax, step=step)
        else:
            ax.grid(True)

        if valid_pts and vis is not None and hasattr(vis, "autoscale_3d_equal"):
            vis.autoscale_3d_equal(ax, valid_pts)
        elif valid_pts:
            xs = [p.x for p in valid_pts]
            ys = [p.y for p in valid_pts]
            zs = [p.z for p in valid_pts]
            ax.set_xlim(min(xs), max(xs))
            ax.set_ylim(min(zs), max(zs))
            ax.set_zlim(min(ys), max(ys))

        ax.set_title(f"Visualização 3D — (sempre 4 cantos) | FRONT={front_mode} | REAR={rear_mode} | mode={mode}")
        ax.set_xlabel("X (Lateral)")
        ax.set_ylabel("Z (Longitudinal)")
        ax.set_zlabel("Y (Vertical)")

        self.canvas_vis3d.draw_idle()
        if not self.free_view:
            self._enforce_fixed_iso_view()

        self._set_status("✅ Visual 3D atualizado (4 cantos garantidos).")
        self.content_area.select(self.tab_vis3d)

    # =========================================================================
    # DYNAMICS
    # =========================================================================
    def _run_cg_sweep(self):
        if not SuspensionOrchestrator:
            return
        self._safe_call(self._do_cg_sweep, "CG Sweep")
        self.content_area.select(self.tab_dyn)

    def _do_cg_sweep(self):
        h_ro = (self.last_2d_results.get("front") or {}).get("h_ro") or 100.0
        ui_data = self._collect_preset()
        results = SuspensionOrchestrator.run_cg_sweep(ui_data, h_ro)

        for i in self.tree_cg.get_children():
            self.tree_cg.delete(i)

        h_vals, dfz_vals, m_vals = [], [], []
        for r in results:
            self.tree_cg.insert("", "end", values=(
                f"{r.h_cg:.1f}", f"{r.d_fz:.1f}", f"{r.fz_int:.1f}", f"{r.m_roll:.1f}",
            ))
            h_vals.append(r.h_cg)
            dfz_vals.append(r.d_fz)
            m_vals.append(r.m_roll)

        zebra_stripes(self.tree_cg)
        autosize_treeview_columns(self.tree_cg)

        self.ax_cg1.clear()
        self.ax_cg1.plot(h_vals, dfz_vals, "o-", color="tab:blue")
        self.ax_cg1.set_xlabel("h_CG [mm]")
        self.ax_cg1.set_ylabel("ΔFz [N]")
        self.ax_cg1.set_title("Transferência de Carga vs CG")
        self.ax_cg1.grid(True)

        self.ax_cg2.clear()
        self.ax_cg2.plot(h_vals, m_vals, "o-", color="tab:orange")
        self.ax_cg2.set_xlabel("h_CG [mm]")
        self.ax_cg2.set_ylabel("M_roll [N·mm]")
        self.ax_cg2.set_title("Momento de Rolamento vs CG")
        self.ax_cg2.grid(True)

        self.canvas_cg.draw()
        self._set_status(f"✅ CG Sweep concluído ({len(results)} pontos).")

    def _run_mass_sweep(self):
        if not SuspensionOrchestrator:
            return
        self._safe_call(self._do_mass_sweep, "Mass Sweep")
        self.content_area.select(self.tab_dyn)

    def _do_mass_sweep(self):
        h_ro = (self.last_2d_results.get("front") or {}).get("h_ro") or 100.0
        ui_data = self._collect_preset()
        results = SuspensionOrchestrator.run_mass_sweep(ui_data, h_ro)

        for i in self.tree_mass.get_children():
            self.tree_mass.delete(i)

        for r in results:
            self.tree_mass.insert("", "end", values=(
                f"{r['mass']:.0f}",
                f"{r['m_roll']:.1f}",
                f"{r['d_fz']:.1f}",
                f"{r['ssf']:.3f}",
                f"{r['ay_crit']:.2f}",
                f"{r['margin']:.2f}",
            ))

        zebra_stripes(self.tree_mass)
        autosize_treeview_columns(self.tree_mass)
        self._set_status(f"✅ Mass Sweep concluído ({len(results)} pontos).")

    def _run_opt(self):
        if not SuspensionOrchestrator: return
        ui_data = self._collect_preset()
        
        # Deixando o Orquestrador extrair os ângulos diretamente dos hardpoints via ui_data
        results = SuspensionOrchestrator.run_structural_optimization(ui_data, self.var_auto_arm_angles.get())
        
        for i in self.tree_opt.get_children():
            self.tree_opt.delete(i)
            
        angles, f_sup, f_inf = [], [], []
        for r in results:
            self.tree_opt.insert("", "end", values=(f"{r['angle_delta']}", f"{r['force_sup']:.1f}", f"{r['force_inf']:.1f}", r['status']))
            angles.append(r['angle_delta'])
            f_sup.append(r['force_sup'])
            f_inf.append(r['force_inf'])

        zebra_stripes(self.tree_opt)
        autosize_treeview_columns(self.tree_opt)

        self.ax_opt.clear()
        self.ax_opt.plot(angles, f_sup, label="F Sup", marker="o")
        self.ax_opt.plot(angles, f_inf, label="F Inf", marker="o")
        self.ax_opt.axhline(self._read("limit"), linestyle="--", color="r")
        self.ax_opt.grid(True)
        self.ax_opt.legend()
        self.canvas_opt.draw()
        self.content_area.select(self.tab_opt)

    # =========================================================================
    # VIEW CONTROL (3D)
    # =========================================================================
    def _toggle_free_view(self):
        self.free_view = not self.free_view
        self._apply_view_mode()
        self.btn_free_view.config(text="Visualização: LIVRE (3D)" if self.free_view else "Visualização: FIXA (ISO)")

    def _apply_view_mode(self):
        if not hasattr(self, "ax_vis3d"): return
        if self._view_lock_cid:
            self.canvas_vis3d.mpl_disconnect(self._view_lock_cid)
            self._view_lock_cid = None
        if self.free_view:
            self.ax_vis3d.mouse_init()
        else:
            self.ax_vis3d.mouse_init(rotate_btn=None)
            self._view_lock_cid = self.canvas_vis3d.mpl_connect(
                "button_release_event", lambda e: self._enforce_fixed_iso_view()
            )
            self._enforce_fixed_iso_view()

    def _enforce_fixed_iso_view(self):
        if self.free_view:
            return
        self.ax_vis3d.view_init(elev=22, azim=-55)
        self.canvas_vis3d.draw_idle()

    def _reset_view(self):
        if self.free_view:
            self.ax_vis3d.view_init(elev=20, azim=-60)
        else:
            self.ax_vis3d.view_init(elev=22, azim=-55)
        self.canvas_vis3d.draw_idle()

    # =========================================================================
    # SETUP DAS ABAS (agora com “tabelas colapsáveis” + botões úteis)
    # =========================================================================
    def _init_result_tabs(self):
        self.tab_2d      = ttk.Frame(self.content_area)
        self.tab_3d      = ttk.Frame(self.content_area)
        self.tab_vis3d   = ttk.Frame(self.content_area)
        self.tab_align   = ttk.Frame(self.content_area)
        self.tab_kinswp  = ttk.Frame(self.content_area)
        self.tab_panhard = ttk.Frame(self.content_area)
        self.tab_dyn     = ttk.Frame(self.content_area)
        self.tab_opt     = ttk.Frame(self.content_area)

        self.content_area.add(self.tab_2d,      text="Resultados 2D")
        self.content_area.add(self.tab_3d,      text="Análise 3D Completa")
        self.content_area.add(self.tab_vis3d,   text="Visualização 3D")
        self.content_area.add(self.tab_align,   text="Alinhamento")
        self.content_area.add(self.tab_kinswp,  text="Cinemática (Sweep)")
        self.content_area.add(self.tab_panhard, text="Panhard")
        self.content_area.add(self.tab_dyn,     text="Dinâmica")
        self.content_area.add(self.tab_opt,     text="Otimização")

        self._setup_res_2d(self.tab_2d)
        self._setup_res_3d(self.tab_3d)
        self._setup_res_vis3d(self.tab_vis3d)
        self._setup_res_align(self.tab_align)
        self._setup_res_kinswp(self.tab_kinswp)
        self._setup_res_panhard(self.tab_panhard)
        self._setup_res_dyn(self.tab_dyn)
        self._setup_res_opt(self.tab_opt)

    def _setup_res_2d(self, parent):
        sec = CollapsibleSection(parent, "Resumo 2D", initially_open=True)
        sec.pack(fill="x", padx=12, pady=(12, 6))
        self.lbl_2d_res = ttk.Label(sec.body, text="Aguardando cálculo...", font=("Courier", 10))
        self.lbl_2d_res.pack(anchor="w", padx=6, pady=6)

        sec_plot = CollapsibleSection(parent, "Gráfico 2D (Front/Rear)", initially_open=True)
        sec_plot.pack(fill="both", expand=True, padx=12, pady=(6, 12))

        self.fig_2d, (self.ax_2d_front, self.ax_2d_rear) = plt.subplots(1, 2, figsize=(10, 4))
        self.canvas_2d = FigureCanvasTkAgg(self.fig_2d, master=sec_plot.body)
        self.canvas_2d.get_tk_widget().pack(fill="both", expand=True, padx=6, pady=6)

    def _setup_res_3d(self, parent):
        sec = CollapsibleSection(parent, "Resumo 3D (forças + alinhamento do canto)", initially_open=True)
        sec.pack(fill="x", padx=12, pady=12)
        self.lbl_3d_res = ttk.Label(sec.body, text="Aguardando cálculo...", font=("Courier", 10))
        self.lbl_3d_res.pack(anchor="w", padx=6, pady=6)

    def _setup_res_vis3d(self, parent):
        ctrl = ttk.Frame(parent, padding=10)
        ctrl.pack(fill="x")

        ttk.Label(ctrl, text="Modo Visual 3D:", font=("Segoe UI", 9, "bold")).pack(side="left")
        cbm = ttk.Combobox(
            ctrl,
            values=["AUTO", "DOUBLE_A", "RIGID"],
            textvariable=self.var_vis3d_mode,
            width=10,
            state="readonly"
        )
        cbm.pack(side="left", padx=6)
        ToolTip(cbm, "AUTO: usa tipo FRONT/REAR. DOUBLE_A: força braços. RIGID: força eixo rígido.")

        ttk.Button(ctrl, text="GERAR DIAGRAMA", command=self._calc_full_3d).pack(side="left", padx=8)
        ttk.Button(ctrl, text="Reset View", command=self._reset_view).pack(side="right")

        self.btn_free_view = ttk.Button(ctrl, text="Visualização: FIXA (ISO)", command=self._toggle_free_view)
        self.btn_free_view.pack(side="right", padx=8)

        self.var_grid_step = tk.DoubleVar(value=50.0)
        ttk.Label(ctrl, text="Grid:").pack(side="left", padx=(14, 4))
        ttk.Scale(ctrl, from_=10, to=100, variable=self.var_grid_step, orient="horizontal", length=220).pack(side="left")

        sec_plot = CollapsibleSection(parent, "Cena 3D", initially_open=True)
        sec_plot.pack(fill="both", expand=True, padx=12, pady=(0, 12))

        self.fig_vis3d = plt.figure(figsize=(5, 5))
        self.ax_vis3d = self.fig_vis3d.add_subplot(111, projection="3d")
        self.canvas_vis3d = FigureCanvasTkAgg(self.fig_vis3d, master=sec_plot.body)
        self.canvas_vis3d.get_tk_widget().pack(fill="both", expand=True)
        NavigationToolbar2Tk(self.canvas_vis3d, sec_plot.body).pack(fill="x")
        self._apply_view_mode()

    def _setup_res_align(self, parent):
        top = ttk.Frame(parent, padding=10)
        top.pack(fill="x")
        ttk.Label(top, text="Alinhamento (Hardpoints)", font=("Segoe UI", 11, "bold")).pack(side="left")
        ttk.Button(top, text="Calcular", command=lambda: self._safe_call(self._alignment_calc_all, "Alinhamento")).pack(side="left", padx=10)

        ttk.Button(top, text="Copiar tabela", command=lambda: copy_treeview_to_clipboard(self.root, self.tree_align)).pack(side="left", padx=6)
        self.lbl_align_summary = ttk.Label(top, text="--", font=("Courier", 10))
        self.lbl_align_summary.pack(side="right")

        sec_tbl = CollapsibleSection(parent, "Tabela (abre/fecha)", initially_open=True)
        sec_tbl.pack(fill="both", expand=True, padx=12, pady=(0, 12))

        cols = ("Corner", "Camber_deg", "Caster_deg", "Toe_deg")
        self.tree_align = ttk.Treeview(sec_tbl.body, columns=cols, show="headings", height=14)
        for c in cols:
            self.tree_align.heading(c, text=c)
            self.tree_align.column(c, width=150, anchor="center")
        self.tree_align.pack(fill="both", expand=True)

    def _setup_res_kinswp(self, parent):
        top = ttk.Frame(parent, padding=10)
        top.pack(fill="x")
        ttk.Label(top, text="Cinemática: Câmbagem vs Curso", font=("Segoe UI", 11, "bold")).pack(side="left")
        ttk.Button(top, text="Calcular Sweep", command=self._run_kinematic_sweep).pack(side="left", padx=10)
        ttk.Button(top, text="Copiar tabela", command=lambda: copy_treeview_to_clipboard(self.root, self.tree_kinswp)).pack(side="left", padx=6)

        sec_plot = CollapsibleSection(parent, "Gráfico Câmbagem vs Curso (abre/fecha)", initially_open=True)
        sec_plot.pack(fill="both", expand=True, padx=12, pady=(6, 4))
        self.fig_kinswp, self.ax_kinswp = plt.subplots(figsize=(7, 3.5))
        self.canvas_kinswp = FigureCanvasTkAgg(self.fig_kinswp, master=sec_plot.body)
        self.canvas_kinswp.get_tk_widget().pack(fill="both", expand=True)

        sec_tbl = CollapsibleSection(parent, "Tabela Cinemática (abre/fecha)", initially_open=True)
        sec_tbl.pack(fill="x", padx=12, pady=(0, 12))

        cols_k = ("Curso_mm", "Camb_estatica_deg", "Camb_nova_deg", "Ganho_deg_per_mm")
        self.tree_kinswp = ttk.Treeview(sec_tbl.body, columns=cols_k, show="headings", height=10)
        for c in cols_k:
            self.tree_kinswp.heading(c, text=c)
            self.tree_kinswp.column(c, anchor="center", width=160)
        self.tree_kinswp.pack(fill="x")

    def _run_kinematic_sweep(self):
        if not SuspensionOrchestrator:
            return
        self._safe_call(self._do_kinematic_sweep, "Cinemática Sweep")
        self.content_area.select(self.tab_kinswp)

    def _do_kinematic_sweep(self):
        corner = (self.var_corner.get().strip() or "FR").upper()
        ui_data = self._collect_preset()

        results = SuspensionOrchestrator.run_kinematic_sweep(
            ui_data, corner,
            travel_min=-40.0, travel_max=40.0, n_steps=17,
        )

        for i in self.tree_kinswp.get_children():
            self.tree_kinswp.delete(i)

        travels, cambers = [], []
        for r in results:
            self.tree_kinswp.insert("", "end", values=(
                f"{r['travel_mm']:.1f}",
                f"{r['static_camber_deg']:.3f}",
                f"{r['new_camber_deg']:.3f}",
                f"{r['gain_deg_per_mm']:.4f}",
            ))
            travels.append(r["travel_mm"])
            cambers.append(r["new_camber_deg"])

        zebra_stripes(self.tree_kinswp)
        autosize_treeview_columns(self.tree_kinswp)

        self.ax_kinswp.clear()
        if travels:
            static_val = results[len(results) // 2]["static_camber_deg"]
            self.ax_kinswp.plot(travels, cambers, "o-", label="Câmbagem vs Curso")
            self.ax_kinswp.axhline(static_val, linestyle="--", color="gray", label=f"Estática {static_val:.2f}°")
            self.ax_kinswp.axvline(0, linestyle=":", color="k")
        self.ax_kinswp.set_xlabel("Curso vertical [mm]  (+ = bump)")
        self.ax_kinswp.set_ylabel("Câmbagem [°]")
        self.ax_kinswp.set_title(f"Câmbagem Cinemática — canto {corner}")
        self.ax_kinswp.grid(True)
        self.ax_kinswp.legend(fontsize=8)
        self.canvas_kinswp.draw()
        self._set_status(f"✅ Cinemática sweep — canto {corner} ({len(results)} pontos).")

    def _setup_res_panhard(self, parent):
        top = ttk.Frame(parent, padding=10)
        top.pack(fill="x")
        self.lbl_panhard_res = ttk.Label(top, text="Aguardando cálculo...", font=("Courier", 10))
        self.lbl_panhard_res.pack(side="left")

        ttk.Button(top, text="Copiar tabela", command=lambda: copy_treeview_to_clipboard(self.root, self.tree_panhard)).pack(side="right")

        sec_tbl = CollapsibleSection(parent, "Tabela Panhard (abre/fecha)", initially_open=True)
        sec_tbl.pack(fill="both", expand=True, padx=12, pady=(0, 12))

        cols = ("Eixo", "Fy_axle", "F_axial", "ux",
                "Mx_ax", "My_ax", "Mz_ax",
                "Mx_ch", "My_ch", "Mz_ch")
        self.tree_panhard = ttk.Treeview(sec_tbl.body, columns=cols, show="headings", height=12)
        for c in cols:
            self.tree_panhard.heading(c, text=c)
            self.tree_panhard.column(c, anchor="center", width=110)
        self.tree_panhard.pack(fill="both", expand=True)

    def _setup_res_dyn(self, parent):
        nb = ttk.Notebook(parent)
        nb.pack(fill="both", expand=True, padx=8, pady=8)
        f_cg = ttk.Frame(nb)
        nb.add(f_cg, text="CG Sweep")
        f_mass = ttk.Frame(nb)
        nb.add(f_mass, text="Mass Sweep")

        sec_plot = CollapsibleSection(f_cg, "Gráficos (abre/fecha)", initially_open=True)
        sec_plot.pack(fill="both", expand=True, padx=8, pady=8)

        self.fig_cg, (self.ax_cg1, self.ax_cg2) = plt.subplots(1, 2, figsize=(8, 3))
        self.canvas_cg = FigureCanvasTkAgg(self.fig_cg, master=sec_plot.body)
        self.canvas_cg.get_tk_widget().pack(fill="both", expand=True)

        sec_tbl = CollapsibleSection(f_cg, "Tabela CG Sweep (abre/fecha)", initially_open=True)
        sec_tbl.pack(fill="x", padx=8, pady=(0, 8))

        self.tree_cg = ttk.Treeview(sec_tbl.body, columns=("CG", "dFz", "Fz_int", "M_Roll"), show="headings", height=6)
        for c in ("CG", "dFz", "Fz_int", "M_Roll"):
            self.tree_cg.heading(c, text=c)
            self.tree_cg.column(c, anchor="center", width=120)
        self.tree_cg.pack(fill="x")

        top_m = ttk.Frame(f_mass, padding=8)
        top_m.pack(fill="x")
        ttk.Button(top_m, text="Copiar tabela", command=lambda: copy_treeview_to_clipboard(self.root, self.tree_mass)).pack(side="right")

        sec_tbl2 = CollapsibleSection(f_mass, "Tabela Mass Sweep (abre/fecha)", initially_open=True)
        sec_tbl2.pack(fill="both", expand=True, padx=8, pady=(0, 8))

        cols_m = ("Massa", "M_Roll", "dFz", "SSF", "Ay_Crit", "Margem")
        self.tree_mass = ttk.Treeview(sec_tbl2.body, columns=cols_m, show="headings")
        for c in cols_m:
            self.tree_mass.heading(c, text=c)
            self.tree_mass.column(c, anchor="center", width=120)
        self.tree_mass.pack(fill="both", expand=True)

    def _setup_res_opt(self, parent):
        top = ttk.Frame(parent, padding=10)
        top.pack(fill="x")
        ttk.Button(top, text="Copiar tabela", command=lambda: copy_treeview_to_clipboard(self.root, self.tree_opt)).pack(side="left")

        sec_tbl = CollapsibleSection(parent, "Tabela (abre/fecha)", initially_open=True)
        sec_tbl.pack(fill="both", expand=True, padx=12, pady=(0, 8))

        self.tree_opt = ttk.Treeview(sec_tbl.body, columns=("dAng", "FSup", "FInf", "Sts"), show="headings", height=10)
        for c in ("dAng", "FSup", "FInf", "Sts"):
            self.tree_opt.heading(c, text=c)
            self.tree_opt.column(c, anchor="center", width=120)
        self.tree_opt.pack(fill="both", expand=True)

        sec_plot = CollapsibleSection(parent, "Gráfico (abre/fecha)", initially_open=True)
        sec_plot.pack(fill="both", expand=True, padx=12, pady=(0, 12))

        self.fig_opt, self.ax_opt = plt.subplots(figsize=(5, 3))
        self.canvas_opt = FigureCanvasTkAgg(self.fig_opt, master=sec_plot.body)
        self.canvas_opt.get_tk_widget().pack(fill="both", expand=True)

    # =========================================================================
    # PERSISTÊNCIA E UTILITÁRIOS
    # =========================================================================
    def _collect_preset(self) -> dict:
        """
        Build a flat dictionary suitable for the orchestrator methods.

        Hardpoints are stored as (x, y, z) float tuples keyed by
        "<corner> <point>" so orchestrators can call:
            ui_data["FR Sup In"]  →  (450.0, 420.0, 200.0)
        Scalar entries are kept as strings (safe_num handles conversion).
        """
        data = {k: e.get() for k, e in self.entries.items()}
        for lbl, (ex, ey, ez) in self.entries_hp.items():
            try:
                data[lbl] = (float(ex.get()), float(ey.get()), float(ez.get()))
            except ValueError:
                data[lbl] = (0.0, 0.0, 0.0)
        data["_corner"]     = self.var_corner.get()
        data["_front_type"] = self.var_front_type.get()
        data["_rear_type"]  = self.var_rear_type.get()
        data["_static"]     = getattr(self, "var_static", tk.IntVar(value=0)).get()
        data["_vis3d_mode"] = self.var_vis3d_mode.get()
        return data

    def _apply_preset(self, data: dict):
        # Scalar fields
        for k, e in self.entries.items():
            if k in data:
                e.delete(0, tk.END)
                e.insert(0, str(data[k]))
        # Hardpoints — support both tuple and {"x":, "y":, "z":} formats
        for lbl, (ex, ey, ez) in self.entries_hp.items():
            if lbl not in data:
                continue
            v = data[lbl]
            if isinstance(v, (list, tuple)) and len(v) == 3:
                xv, yv, zv = v
            elif isinstance(v, dict):
                xv = v.get("x", ex.get())
                yv = v.get("y", ey.get())
                zv = v.get("z", ez.get())
            else:
                continue
            ex.delete(0, tk.END); ex.insert(0, str(xv))
            ey.delete(0, tk.END); ey.insert(0, str(yv))
            ez.delete(0, tk.END); ez.insert(0, str(zv))

    def _save_preset(self):
        path = filedialog.asksaveasfilename(defaultextension=".json", filetypes=[("JSON", "*.json")])
        if not path:
            return
        raw = self._collect_preset()
        # Convert tuples to lists for JSON serialisation
        serialisable = {
            k: list(v) if isinstance(v, tuple) else v
            for k, v in raw.items()
        }
        with open(path, "w", encoding="utf-8") as f:
            json.dump(serialisable, f, indent=2, ensure_ascii=False)

    def _load_preset(self):
        path = filedialog.askopenfilename(filetypes=[("JSON", "*.json")])
        if path:
            with open(path, "r", encoding="utf-8") as f:
                self._apply_preset(json.load(f))

    def _reset_defaults(self):
        defaults_entries = {
            "bf": 1250, "wb": 1550,
            "s1": 40, "s2": 30,
            "cam_o": -1.5, "cam_i": -4.0,
            "spindle_sup_z": 300, "spindle_inf_z": 300,
            "toe_f_x": 600, "toe_r_x": 600,
            "mass": 260, "hcg": 450, "ay": 9.8, "fx": 1200, "rs": 50,
            "h_min": 250, "h_max": 650,
            "m_min": 180, "m_max": 300, "step": 25,
            "front_share": 0.55,
            "load": 3000, "ang_sup": 10, "ang_inf": 20,
            "limit": 8000, "ksup": 0.5, "amin": 0, "amax": 40,
        }
        for k, v in defaults_entries.items():
            self._set(k, v)

        base_defaults = {
            "Sup In": (450, 420, 200), "Sup Out": (625, 390, 300),
            "Inf In": (430, 210, 200), "Inf Out": (625, 190, 300),
            "Toe In": (520, 260, 120), "Toe Out": (650, 260, 180),
            "Damper In": (380, 520, 150), "Damper Out": (610, 430, 260),
        }
        for corner in ("FR", "FL", "RR", "RL"):
            for p, (dx, dy, dz) in base_defaults.items():
                if corner in ("FL", "RL"):
                    dx = -dx
                lbl = self._hp_key(corner, p)
                if lbl in self.entries_hp:
                    self._hp_set(lbl, dx, dy, dz)

        self._set_status("✅ Valores padrão restaurados.")

    def _export_hardpoints_csv(self):
        path = filedialog.asksaveasfilename(
            defaultextension=".csv",
            filetypes=[("CSV", "*.csv")],
        )
        if not path:
            return
        with open(path, "w", newline="", encoding="utf-8") as f:
            writer = csv.writer(f)
            writer.writerow(["label", "x", "y", "z"])
            for lbl, (ex, ey, ez) in self.entries_hp.items():
                writer.writerow([lbl, ex.get(), ey.get(), ez.get()])
        self._set_status(f"✅ Hardpoints exportados → {path}")

    def _import_hardpoints_csv(self):
        path = filedialog.askopenfilename(filetypes=[("CSV", "*.csv")])
        if not path:
            return
        with open(path, newline="", encoding="utf-8") as f:
            reader = csv.DictReader(f)
            for row in reader:
                lbl = row.get("label", "").strip()
                if lbl in self.entries_hp:
                    self._hp_set(
                        lbl,
                        row.get("x", "0"),
                        row.get("y", "0"),
                        row.get("z", "0"),
                    )
        self._set_status(f"✅ Hardpoints importados de {path}")

if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()