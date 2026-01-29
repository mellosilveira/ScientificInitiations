import tkinter as tk
from tkinter import ttk, messagebox
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
from mpl_toolkits.mplot3d import Axes3D

# Import modules
import models
import math_2d
import math_3d
import dynamics
import utils
import ui_strings


class ScrollableFrame(ttk.Frame):
    """
    Frame contendo canvas e scrollbar para permitir rolagem dos inputs.
    """

    def __init__(self, container, *args, **kwargs):
        super().__init__(container, *args, **kwargs)
        canvas = tk.Canvas(self, width=380)
        scrollbar = ttk.Scrollbar(self, orient="vertical", command=canvas.yview)
        self.scrollable_window = ttk.Frame(canvas)
        self.scrollable_window.bind("<Configure>", lambda e: canvas.configure(scrollregion=canvas.bbox("all")))
        canvas.create_window((0, 0), window=self.scrollable_window, anchor="nw")
        canvas.configure(yscrollcommand=scrollbar.set)
        canvas.pack(side="left", fill="both", expand=True)
        scrollbar.pack(side="right", fill="y")
        canvas.bind_all("<MouseWheel>", lambda event: canvas.yview_scroll(int(-1 * (event.delta / 120)), "units"))


class App:
    def __init__(self, root):
        self.root = root
        self.root.title("Suíte de Engenharia de Suspensão (Completa)")
        self.root.geometry("1400x900")

        # Estilos
        style = ttk.Style()
        style.theme_use('clam')
        style.configure("TLabelFrame", font=("Arial", 10, "bold"))
        style.configure("Action.TButton", foreground="blue", font=("Arial", 9, "bold"))
        style.configure("Reset.TButton", foreground="red", font=("Arial", 8))

        # Layout Principal
        self.paned = tk.PanedWindow(root, orient="horizontal", sashwidth=5)
        self.paned.pack(fill="both", expand=True)

        self.sidebar_container = ScrollableFrame(self.paned)
        self.paned.add(self.sidebar_container, minsize=400, width=420)

        self.content_area = ttk.Notebook(self.paned)
        self.paned.add(self.content_area, minsize=600)

        # Estado Compartilhado
        self.last_2d_results = None
        self.entries_hp = {}

        # Inicialização
        self._init_sidebar_inputs(self.sidebar_container.scrollable_window)
        self._init_result_tabs()

    def _create_entry(self, parent, label, row, col, default, width=8):
        ttk.Label(parent, text=label).grid(row=row, column=col, sticky="e", padx=2, pady=2)
        e = ttk.Entry(parent, width=width)
        e.insert(0, str(default))
        e.grid(row=row, column=col + 1, sticky="w", padx=2, pady=2)
        return e

    # ==========================================
    # INPUTS (SIDEBAR)
    # ==========================================
    def _init_sidebar_inputs(self, parent):
        # 1. Geometria
        f_geo = ttk.LabelFrame(parent, text="1. Geometria (Hardpoints) [mm]")
        f_geo.pack(fill="x", padx=5, pady=5)
        self.e_bf = self._create_entry(f_geo, "Bitola (bf):", 0, 0, "1250", width=12)

        ttk.Label(f_geo, text="X").grid(row=1, column=1)
        ttk.Label(f_geo, text="Y").grid(row=1, column=2)
        ttk.Label(f_geo, text="Z").grid(row=1, column=3)

        labels = ["Sup In:", "Sup Out:", "Inf In:", "Inf Out:"]
        defaults = [("450", "420", "200"), ("625", "390", "300"), ("430", "210", "200"), ("625", "190", "300")]

        for i, (lbl, (dx, dy, dz)) in enumerate(zip(labels, defaults)):
            row = i + 2
            ttk.Label(f_geo, text=lbl).grid(row=row, column=0, sticky="e")
            ex = ttk.Entry(f_geo, width=7);
            ex.insert(0, dx);
            ex.grid(row=row, column=1)
            ey = ttk.Entry(f_geo, width=7);
            ey.insert(0, dy);
            ey.grid(row=row, column=2)
            ez = ttk.Entry(f_geo, width=7);
            ez.insert(0, dz);
            ez.grid(row=row, column=3)
            self.entries_hp[lbl] = (ex, ey, ez)

        # 2. Cinemática
        f_kin = ttk.LabelFrame(parent, text="2. Cinemática & Alinhamento")
        f_kin.pack(fill="x", padx=5, pady=5)
        self.e_s1 = self._create_entry(f_kin, "Curso Ext (mm):", 0, 0, "40")
        self.e_s2 = self._create_entry(f_kin, "Curso Int (mm):", 0, 2, "30")
        self.e_cam_o = self._create_entry(f_kin, "Camber Ext (°):", 1, 0, "-1.5")
        self.e_cam_i = self._create_entry(f_kin, "Camber Int (°):", 1, 2, "-4.0")
        ttk.Label(f_kin, text="-- Auxiliares 3D --").grid(row=2, column=0, columnspan=4, pady=5)
        self.e_spindle_sup_z = self._create_entry(f_kin, "Spindle Sup Z:", 3, 0, "300")
        self.e_spindle_inf_z = self._create_entry(f_kin, "Spindle Inf Z:", 3, 2, "300")
        self.e_toe_f_x = self._create_entry(f_kin, "Toe Front X:", 4, 0, "600")
        self.e_toe_r_x = self._create_entry(f_kin, "Toe Rear X:", 4, 2, "600")

        # 3. Dinâmica
        f_dyn = ttk.LabelFrame(parent, text="3. Dinâmica Veicular (Baja)")
        f_dyn.pack(fill="x", padx=5, pady=5)
        self.e_mass = self._create_entry(f_dyn, "Massa (kg):", 0, 0, "200")
        self.e_hcg = self._create_entry(f_dyn, "Altura CG (mm):", 0, 2, "450")
        self.e_ay = self._create_entry(f_dyn, "Ay (m/s²):", 1, 0, "9.8")
        self.e_wb = self._create_entry(f_dyn, "Entre-eixos (mm):", 1, 2, "1550")
        self.e_fx = self._create_entry(f_dyn, "Fx Pneu (N):", 2, 0, "1200")
        self.e_rs = self._create_entry(f_dyn, "Scrub (mm):", 2, 2, "50")

        ttk.Label(f_dyn, text="-- Varredura --").grid(row=3, column=0, columnspan=4, pady=5)
        self.e_h_min = self._create_entry(f_dyn, "CG Min:", 4, 0, "250")
        self.e_h_max = self._create_entry(f_dyn, "CG Max:", 4, 2, "650")
        self.e_m_min = self._create_entry(f_dyn, "Massa Min:", 5, 0, "180")
        self.e_m_max = self._create_entry(f_dyn, "Massa Max:", 5, 2, "240")
        self.e_step = self._create_entry(f_dyn, "Passo:", 6, 0, "25")

        # 4. Otimização
        f_opt = ttk.LabelFrame(parent, text="4. Otimização Estrutural")
        f_opt.pack(fill="x", padx=5, pady=5)
        self.e_load = self._create_entry(f_opt, "Carga F [N]:", 0, 0, "3000")
        self.var_static = tk.IntVar()
        ttk.Checkbutton(f_opt, text="+ Estática (mg/4)", variable=self.var_static).grid(row=0, column=2, columnspan=2)
        self.e_ang_sup = self._create_entry(f_opt, "Ang Base Sup:", 2, 0, "10")
        self.e_ang_inf = self._create_entry(f_opt, "Ang Base Inf:", 2, 2, "20")
        self.e_limit = self._create_entry(f_opt, "Limite (N):", 3, 0, "8000")
        self.e_ksup = self._create_entry(f_opt, "K Sup:", 3, 2, "0.5")
        self.e_amin = self._create_entry(f_opt, "Varredura Min:", 4, 0, "0")
        self.e_amax = self._create_entry(f_opt, "Varredura Max:", 4, 2, "40")

    # ==========================================
    # RESULTADOS E ABAS
    # ==========================================
    def _init_result_tabs(self):
        self.tab_2d = ttk.Frame(self.content_area);
        self.content_area.add(self.tab_2d, text="Resultados 2D");
        self._setup_res_2d(self.tab_2d)
        self.tab_3d = ttk.Frame(self.content_area);
        self.content_area.add(self.tab_3d, text="Resultados 3D");
        self._setup_res_3d(self.tab_3d)
        self.tab_vis = ttk.Frame(self.content_area);
        self.content_area.add(self.tab_vis, text="Visualização 3D");
        self._setup_res_vis(self.tab_vis)
        self.tab_dyn = ttk.Frame(self.content_area);
        self.content_area.add(self.tab_dyn, text="Dinâmica");
        self._setup_res_dyn(self.tab_dyn)
        self.tab_opt = ttk.Frame(self.content_area);
        self.content_area.add(self.tab_opt, text="Otimização");
        self._setup_res_opt(self.tab_opt)

    def _setup_res_2d(self, parent):
        f = ttk.Frame(parent);
        f.pack(fill='x', padx=10, pady=10)
        ttk.Button(f, text="CALCULAR GEOMETRIA 2D", style="Action.TButton", command=self._calc_2d).pack(fill='x',
                                                                                                        ipady=5)
        self.lbl_2d_res = ttk.Label(parent, text="Clique em calcular...", font=("Courier", 11));
        self.lbl_2d_res.pack(pady=5, padx=10, anchor="w")
        self.fig_2d, self.ax_2d = plt.subplots(figsize=(6, 5))
        self.canvas_2d = FigureCanvasTkAgg(self.fig_2d, master=parent)
        self.canvas_2d.get_tk_widget().pack(fill="both", expand=True, padx=10, pady=5)

    def _setup_res_3d(self, parent):
        f = ttk.Frame(parent);
        f.pack(fill='x', padx=10, pady=10)
        ttk.Button(f, text="CALCULAR FORÇAS 3D", style="Action.TButton", command=self._calc_3d).pack(fill='x', ipady=5)
        self.lbl_3d_res = ttk.Label(parent, text="Clique em calcular...", font=("Courier", 11));
        self.lbl_3d_res.pack(pady=10, padx=10, anchor="w")

    def _setup_res_vis(self, parent):
        # Frame de Controle
        f_ctrl = ttk.Frame(parent)
        f_ctrl.pack(fill='x', padx=10, pady=5)

        # Botão Calcular
        ttk.Button(f_ctrl, text="GERAR DIAGRAMA 3D (Ref: Pneu TR Esq)", style="Action.TButton",
                   command=self._calc_full_3d).pack(side="left", fill='x', expand=True, padx=2)

        # Botão Resetar Visualização (Novo)
        ttk.Button(f_ctrl, text="Resetar Visualização", style="Reset.TButton",
                   command=self._reset_view).pack(side="right", padx=2)

        self.lbl_steer_data = ttk.Label(parent, text="Dados: --", font=("Courier", 10))
        self.lbl_steer_data.pack(pady=5)

        self.fig_3d = plt.figure(figsize=(6, 5))
        self.ax_3d = self.fig_3d.add_subplot(111, projection='3d')
        self.canvas_3d = FigureCanvasTkAgg(self.fig_3d, master=parent)
        self.canvas_3d.get_tk_widget().pack(fill="both", expand=True)

    def _setup_res_dyn(self, parent):
        nb = ttk.Notebook(parent);
        nb.pack(fill="both", expand=True)
        f_cg = ttk.Frame(nb);
        nb.add(f_cg, text="Varredura de CG")
        f_c = ttk.Frame(f_cg);
        f_c.pack(fill='x', padx=10, pady=5)
        ttk.Button(f_c, text="RODAR SIMULAÇÃO CG", style="Action.TButton", command=self._run_cg_sweep).pack(fill='x',
                                                                                                            ipady=3)
        self.fig_cg, (self.ax_cg1, self.ax_cg2) = plt.subplots(1, 2, figsize=(8, 3.5))
        self.canvas_cg = FigureCanvasTkAgg(self.fig_cg, master=f_cg);
        self.canvas_cg.get_tk_widget().pack(fill="both", expand=True)
        self.tree_cg = ttk.Treeview(f_cg, columns=("CG", "dFz", "Fz_int", "M_Roll"), show="headings", height=5);
        self.tree_cg.pack(fill="x")
        for c in ("CG", "dFz", "Fz_int", "M_Roll"): self.tree_cg.heading(c, text=c)

        f_m = ttk.Frame(nb);
        nb.add(f_m, text="Varredura de Massa")
        f_cm = ttk.Frame(f_m);
        f_cm.pack(fill='x', padx=10, pady=5)
        ttk.Button(f_cm, text="RODAR SIMULAÇÃO MASSA", style="Action.TButton", command=self._run_mass_sweep).pack(
            fill='x', ipady=3)
        self.tree_mass = ttk.Treeview(f_m, columns=("Massa", "M_Roll", "dFz", "SSF"), show="headings");
        self.tree_mass.pack(fill="both", expand=True)
        for c in ("Massa", "M_Roll", "dFz", "SSF"): self.tree_mass.heading(c, text=c)

    def _setup_res_opt(self, parent):
        f = ttk.Frame(parent);
        f.pack(fill='x', padx=10, pady=10)
        ttk.Button(f, text="1. Auto Calc Carga", command=self._calc_load_from_dynamics).pack(side="left", fill='x',
                                                                                             expand=True, padx=2)
        ttk.Button(f, text="2. ANALISAR ESTRUTURA", style="Action.TButton", command=self._run_opt).pack(side="left",
                                                                                                        fill='x',
                                                                                                        expand=True,
                                                                                                        padx=2)
        paned = tk.PanedWindow(parent, orient="vertical");
        paned.pack(fill="both", expand=True)
        f_tab = ttk.Frame(paned);
        self.tree_opt = ttk.Treeview(f_tab, columns=("dAngle", "F_Sup", "F_Inf", "Status"), show="headings");
        self.tree_opt.pack(fill="both", expand=True);
        paned.add(f_tab, minsize=150)
        for c in ("dAngle", "F_Sup", "F_Inf", "Status"): self.tree_opt.heading(c, text=c)
        f_graph = ttk.Frame(paned);
        self.fig_opt, self.ax_opt = plt.subplots(figsize=(5, 4));
        self.canvas_opt = FigureCanvasTkAgg(self.fig_opt, master=f_graph);
        self.canvas_opt.get_tk_widget().pack(fill="both", expand=True);
        paned.add(f_graph)

    # ==========================================
    # LÓGICA
    # ==========================================
    def _get_point3d(self, key):
        e = self.entries_hp[key]
        return models.Point3D(utils.safe_float(e[0].get()), utils.safe_float(e[1].get()), utils.safe_float(e[2].get()))

    def _reset_view(self):
        """Restaura a câmera do gráfico 3D para a posição padrão."""
        if hasattr(self, 'ax_3d'):
            self.ax_3d.view_init(elev=20, azim=-60)
            self.canvas_3d.draw()

    def _calc_2d(self):
        try:
            geo = models.SuspensionGeometry2D(utils.safe_float(self.e_bf.get()),
                                              models.Point2D(utils.safe_float(self.entries_hp["Sup In:"][0].get()),
                                                             utils.safe_float(self.entries_hp["Sup In:"][1].get())),
                                              models.Point2D(utils.safe_float(self.entries_hp["Sup Out:"][0].get()),
                                                             utils.safe_float(self.entries_hp["Sup Out:"][1].get())),
                                              models.Point2D(utils.safe_float(self.entries_hp["Inf In:"][0].get()),
                                                             utils.safe_float(self.entries_hp["Inf In:"][1].get())),
                                              models.Point2D(utils.safe_float(self.entries_hp["Inf Out:"][0].get()),
                                                             utils.safe_float(self.entries_hp["Inf Out:"][1].get())),
                                              utils.safe_float(self.e_s1.get()), utils.safe_float(self.e_s2.get()),
                                              utils.safe_float(self.e_cam_o.get()),
                                              utils.safe_float(self.e_cam_i.get()))

            res = math_2d.calculate_roll_center(geo)
            cam_res = math_2d.calculate_camber_gain(geo)
            self.last_2d_results = res

            ic_str = f"({res.ic.x:.1f}, {res.ic.y:.1f})" if res.ic else "Paralelo"
            h_ro_str = f"{res.h_ro:.1f}" if res.h_ro is not None else "--"
            self.lbl_2d_res.config(
                text=f"IC: {ic_str} mm\nh_Ro: {h_ro_str} mm\nq: {res.q_factor}\ndPhi: {cam_res.d_phi_deg:.3f}°\nkGamma: {cam_res.k_gamma:.3f}")

            self.ax_2d.clear()
            self.ax_2d.plot([geo.upper_in.x, geo.upper_out.x], [geo.upper_in.y, geo.upper_out.y], 'b-o', label="Sup")
            self.ax_2d.plot([geo.lower_in.x, geo.lower_out.x], [geo.lower_in.y, geo.lower_out.y], 'r-o', label="Inf")
            if res.ic: self.ax_2d.plot(res.ic.x, res.ic.y, 'ks')
            if res.h_ro: self.ax_2d.plot(0, res.h_ro, 'm*')
            self.ax_2d.grid(True);
            self.ax_2d.legend();
            self.canvas_2d.draw()
        except Exception as e:
            messagebox.showerror("Error", str(e))

    def _calc_3d(self):
        try:
            geo = models.SuspensionGeometry3D(
                self._get_point3d("Sup In:"), self._get_point3d("Sup Out:"),
                self._get_point3d("Inf In:"), self._get_point3d("Inf Out:"),
                models.Point3D(utils.safe_float(self.entries_hp["Sup Out:"][0].get()),
                               utils.safe_float(self.entries_hp["Sup Out:"][1].get()),
                               utils.safe_float(self.e_spindle_sup_z.get())),
                models.Point3D(utils.safe_float(self.entries_hp["Inf Out:"][0].get()),
                               utils.safe_float(self.entries_hp["Inf Out:"][1].get()),
                               utils.safe_float(self.e_spindle_inf_z.get())),
                models.Point3D(utils.safe_float(self.e_toe_f_x.get()), 0, 0),
                models.Point3D(utils.safe_float(self.e_toe_r_x.get()), 0, 0),
                1.0, 1.0, utils.safe_float(self.e_fx.get()))

            f = math_3d.calculate_forces(geo)
            self.lbl_3d_res.config(
                text=f"F_Sup Axial: {f.upper.axial:.1f} N\nF_Inf Axial: {f.lower.axial:.1f} N" if f else "Error")
        except Exception as e:
            messagebox.showerror("Error", str(e))

    def _calc_full_3d(self):
        """
        Gera a visualização 3D completa com anotações, linhas tracejadas e projeções.
        Visualização Avançada:
        - Rótulos de texto para cada ponto.
        - Linhas tracejadas (dashed) para os braços.
        - Linhas de projeção (ground drop lines) para referência espacial.
        """
        try:
            # 1. Recuperar Geometria
            geo = models.VehicleGeometry(
                utils.safe_float(self.e_wb.get()), utils.safe_float(self.e_bf.get()), utils.safe_float(self.e_bf.get()),
                self._get_point3d("Sup In:"), self._get_point3d("Sup Out:"),
                self._get_point3d("Inf In:"), self._get_point3d("Inf Out:"),
                models.Point3D(utils.safe_float(self.e_toe_f_x.get()) - 100, 200, 100),
                models.Point3D(utils.safe_float(self.e_toe_f_x.get()), 200, 100),
                utils.safe_float(self.e_rs.get()) * 3
            )
            ro = math_3d.calculate_3d_roll_center(geo)
            pts = math_3d.generate_full_car_plot_points(geo, ro)
            kpi = math_3d.calculate_kingpin_metrics(geo)

            self.lbl_steer_data.config(
                text=f"KPI: {kpi.kpi_deg:.1f}° | Scrub: {kpi.scrub_radius:.1f} mm\nRo Local: ({ro.x:.1f}, {ro.y:.1f}, {ro.z:.1f})")

            # 2. Configurar Plot
            self.ax_3d.clear()

            # Helper para desenhar linhas conectando pontos nomeados
            def plot_link(p1_name, p2_name, color, style='-', linewidth=2):
                if p1_name in pts and p2_name in pts:
                    p1 = pts[p1_name]
                    p2 = pts[p2_name]
                    # Mapeamento de eixos: Modelo(X,Y,Z) -> Matplotlib(X,Z,Y) pois Z é vertical no plot default
                    self.ax_3d.plot([p1.x, p2.x], [p1.z, p2.z], [p1.y, p2.y], color=color, linestyle=style,
                                    linewidth=linewidth)

            # Helper para linhas de projeção (Drop lines) até o chão (Y=0)
            def plot_projection(p_name):
                if p_name in pts:
                    p = pts[p_name]
                    # Linha do ponto até (x, z, 0)
                    self.ax_3d.plot([p.x, p.x], [p.z, p.z], [p.y, 0], color='gray', linestyle=':', linewidth=0.8,
                                    alpha=0.5)

            # 3. Desenhar Conexões (Geometria)
            # Frente Direita (FR)
            plot_link("FR Sup In", "FR Sup Out", "blue", "--")  # Braço Superior Tracejado
            plot_link("FR Inf In", "FR Inf Out", "blue", "--")  # Braço Inferior Tracejado
            plot_link("FR Sup Out", "FR Inf Out", "black", "-")  # Manga de Eixo (Sólido)
            plot_link("FR Tie In", "FR Tie Out", "green", "--")  # Direção

            # Frente Esquerda (FL)
            plot_link("FL Sup In", "FL Sup Out", "red", "--")
            plot_link("FL Inf In", "FL Inf Out", "red", "--")
            plot_link("FL Sup Out", "FL Inf Out", "black", "-")

            # 4. Plotar Pontos e Rótulos
            for name, p in pts.items():
                # Escolher cor/marcador
                c = 'red' if "Roll" in name else 'black' if "Wheel" in name else 'blue'
                m = '*' if "Roll" in name else 'o'

                # Scatter (Ponto)
                self.ax_3d.scatter(p.x, p.z, p.y, c=c, marker=m, s=40)

                # Rótulo de Texto (Label)
                # Adiciona um pequeno offset em Z (visual Y) para não cobrir o ponto
                self.ax_3d.text(p.x, p.z, p.y + 10, name, fontsize=8, color='black')

                # Adicionar projeção para o chão para melhor noção de altura
                if "Wheel" not in name and "Roll" not in name:
                    plot_projection(name)

            # 5. Configurações Finais de Eixos
            self.ax_3d.set_xlabel('X (Lateral)')
            self.ax_3d.set_ylabel('Z (Longitudinal)')
            self.ax_3d.set_zlabel('Y (Vertical)')

            # Ajustar limites para manter proporção (Aspect Ratio Hack)
            # Matplotlib 3D não tem 'equal' aspect ratio perfeito automático
            limits = self.ax_3d.get_w_lims() if hasattr(self.ax_3d, 'get_w_lims') else (self.ax_3d.get_xlim(),
                                                                                        self.ax_3d.get_ylim(),
                                                                                        self.ax_3d.get_zlim())
            # Força o Z a começar de 0 (Chão)
            self.ax_3d.set_zlim(bottom=0)

            self.canvas_3d.draw()

        except Exception as e:
            messagebox.showerror("Error", str(e))

    def _run_cg_sweep(self):
        h_ro = self.last_2d_results.ro.y if (self.last_2d_results and self.last_2d_results.ro) else 100.0
        params = models.SuspensionCGScanParameters(
            utils.safe_float(self.e_h_min.get()), utils.safe_float(self.e_h_max.get()),
            utils.safe_float(self.e_step.get()),
            utils.safe_float(self.e_mass.get()), utils.safe_float(self.e_ay.get()), utils.safe_float(self.e_bf.get()),
            h_ro, utils.safe_float(self.e_rs.get()), 0.0)

        res = list(dynamics.calculate_baja_cg_sweep(params))
        cgs = [r.h_cg for r in res]
        self.ax_cg1.clear();
        self.ax_cg1.plot(cgs, [r.d_fz for r in res]);
        self.ax_cg2.clear();
        self.ax_cg2.plot(cgs, [r.m_roll for r in res]);
        self.canvas_cg.draw()

    def _run_mass_sweep(self):
        h_ro = self.last_2d_results.ro.y if (self.last_2d_results and self.last_2d_results.ro) else 100.0
        params = models.SuspensionMassScanParameters(
            utils.safe_float(self.e_m_min.get()), utils.safe_float(self.e_m_max.get()),
            utils.safe_float(self.e_step.get()),
            utils.safe_float(self.e_hcg.get()), utils.safe_float(self.e_ay.get()), utils.safe_float(self.e_bf.get()),
            h_ro)

        res = list(dynamics.calculate_mass_sweep(params))
        for i in self.tree_mass.get_children(): self.tree_mass.delete(i)
        for r in res: self.tree_mass.insert("", "end", values=(f"{r.mass:.0f}", f"{r.m_roll:.1f}", f"{r.d_fz:.1f}",
                                                               f"{r.ssf:.3f}"))

    def _calc_load_from_dynamics(self):
        h_ro = self.last_2d_results.ro.y if (self.last_2d_results and self.last_2d_results.ro) else 100.0
        comps = dynamics.calculate_load_transfer_components(
            utils.safe_float(self.e_mass.get()), utils.safe_float(self.e_ay.get()), utils.safe_float(self.e_bf.get()),
            utils.safe_float(self.e_hcg.get()), h_ro)
        f = comps.d_fz_total / 2.0
        if self.var_static.get(): f += (utils.safe_float(self.e_mass.get()) * 9.81) / 4.0
        self.e_load.delete(0, tk.END);
        self.e_load.insert(0, f"{f:.1f}")

    def _run_opt(self):
        res = list(dynamics.calculate_force_vs_angle_sweep(models.ForceAngleIteratorParameters(
            utils.safe_float(self.e_load.get()), utils.safe_float(self.e_ang_sup.get()),
            utils.safe_float(self.e_ang_inf.get()),
            utils.safe_float(self.e_ksup.get()), 1.0 - utils.safe_float(self.e_ksup.get()),
            utils.safe_float(self.e_limit.get()),
            int(utils.safe_float(self.e_amin.get())), int(utils.safe_float(self.e_amax.get())), 5)))

        ang = [r.angle_delta for r in res]
        self.ax_opt.clear();
        self.ax_opt.plot(ang, [r.force_sup for r in res], label="Sup");
        self.ax_opt.plot(ang, [r.force_inf for r in res], label="Inf");
        self.ax_opt.legend();
        self.canvas_opt.draw()


if __name__ == "__main__":
    root = tk.Tk()
    app = App(root)
    root.mainloop()