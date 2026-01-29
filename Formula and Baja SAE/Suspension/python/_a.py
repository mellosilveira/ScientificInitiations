import tkinter as tk
from tkinter import ttk, messagebox
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
import numpy as np

# Constante da Gravidade
G = 9.81

# ========= LISTAS GLOBAIS =========
list_h = []
list_Mphi = []
list_dFz = []
list_Fz_ext = []  # Carga Vertical na Roda Externa
list_Fz_int = []  # Carga Vertical na Roda Interna
list_Msp = []     # Momento na Manga


# ========= FUNÇÃO DE PLOTAGEM =========
def plot_analysis():
    # Limpa o frame de gráficos antes de plotar um novo
    for widget in frame_plot.winfo_children():
        widget.destroy()

    if not list_h:
        tk.Label(frame_plot, text="Execute o cálculo para gerar o gráfico.", fg="red").pack(pady=20)
        return

    try:
        # 1. Obter o CG de Referência
        h_ref_mm = float(entry_h_ref.get())

        # 2. Encontrar o índice do CG mais próximo do valor de referência
        h_np = np.array(list_h, dtype=float)
        idx_ref = int(np.argmin(np.abs(h_np - h_ref_mm)))

        # 3. Valores de Referência (no ponto mais próximo)
        Mphi_ref = float(list_Mphi[idx_ref])
        dFz_ref = float(list_dFz[idx_ref])
        Fz_ext_ref = float(list_Fz_ext[idx_ref])

        # 4. Cálculo da Comparação (Melhoria/Piora)
        # Evita divisão por zero
        Mphi_array = np.array(list_Mphi, dtype=float)
        dFz_array = np.array(list_dFz, dtype=float)
        Fz_ext_array = np.array(list_Fz_ext, dtype=float)

        if Mphi_ref == 0 or dFz_ref == 0 or Fz_ext_ref == 0:
            messagebox.showerror(
                "Erro de Plotagem",
                "Os valores de referência não podem ser zero (evita divisão por zero). "
                "Ajuste o CG de referência ou os parâmetros de cálculo."
            )
            return

        Mphi_comp = (Mphi_array - Mphi_ref) / Mphi_ref * 100
        dFz_comp = (dFz_array - dFz_ref) / dFz_ref * 100
        Fz_ext_comp = (Fz_ext_array - Fz_ext_ref) / Fz_ext_ref * 100

        # 5. Plotagem (3 Gráficos)
        fig, (ax1, ax2, ax3) = plt.subplots(1, 3, figsize=(12, 4))
        fig.suptitle(f"Comparação de Tendências vs. CG de Referência ({h_ref_mm:.1f} mm)")

        # Gráfico 1: Momento de Rolagem (Mphi)
        ax1.plot(list_h, Mphi_comp, 'r-')
        ax1.axhline(0, color='black', linestyle='--')
        ax1.axvline(h_ref_mm, color='gray', linestyle=':')
        ax1.set_title("Momento de Rolagem (Mφ)")
        ax1.set_xlabel("CG (mm)")
        ax1.set_ylabel("Variação Percentual (%)")
        ax1.grid(True)

        # Gráfico 2: Transferência de Carga (ΔFz)
        ax2.plot(list_h, dFz_comp, 'b-')
        ax2.axhline(0, color='black', linestyle='--')
        ax2.axvline(h_ref_mm, color='gray', linestyle=':')
        ax2.set_title("Transferência de Carga (ΔFz)")
        ax2.set_xlabel("CG (mm)")
        ax2.set_ylabel("Variação Percentual (%)")
        ax2.grid(True)

        # Gráfico 3: Carga Vertical Externa (Fz_ext)
        ax3.plot(list_h, Fz_ext_comp, 'g-')
        ax3.axhline(0, color='black', linestyle='--')
        ax3.axvline(h_ref_mm, color='gray', linestyle=':')
        ax3.set_title("Carga Vertical Externa (Fz_ext)")
        ax3.set_xlabel("CG (mm)")
        ax3.set_ylabel("Variação Percentual (%)")
        ax3.grid(True)

        plt.tight_layout(rect=[0, 0.03, 1, 0.95])

        # 6. Integração com Tkinter
        canvas = FigureCanvasTkAgg(fig, master=frame_plot)
        canvas_widget = canvas.get_tk_widget()
        canvas_widget.pack(fill=tk.BOTH, expand=True)
        canvas.draw()

        # Fecha a figura no Matplotlib para não acumular em memória
        plt.close(fig)

    except ValueError:
        messagebox.showerror("Erro de Entrada", "O CG de Referência deve ser um número válido.")
    except Exception as e:
        messagebox.showerror("Erro de Plotagem", f"Ocorreu um erro ao gerar o gráfico: {e}")


# ========= EXIBIR VETORES =========
def show_vectors():
    text_vectors.delete("1.0", tk.END)

    if not list_h:
        text_vectors.insert(tk.END, "Nenhum dado de cálculo disponível. Execute o cálculo primeiro.")
        return

    def show(label, vec):
        display_vec = vec[:10] + ["..."] if len(vec) > 10 else vec
        text_vectors.insert(tk.END, f"{label}:\n{display_vec}\n\n")

    show("h (mm)", [round(x, 1) for x in list_h])
    show("Momento de Rolagem Mφ (Nm)", [round(x, 2) for x in list_Mphi])
    show("Transferência ΔFz (N)", [round(x, 2) for x in list_dFz])
    show("Carga Vertical Externa Fz_ext (N)", [round(x, 2) for x in list_Fz_ext])
    show("Carga Vertical Interna Fz_int (N)", [round(x, 2) for x in list_Fz_int])
    show("Momento na Manga Msp (Nm)", [round(x, 2) for x in list_Msp])


# ========= EXIBIR ANÁLISE AUTOMÁTICA =========
def show_analysis():
    text_analysis.delete("1.0", tk.END)

    if not list_h:
        text_analysis.insert(tk.END, "ERRO: Nenhum dado de cálculo disponível para análise.")
        return

    h_min = min(list_h)
    h_max = max(list_h)
    dFz_min = min(list_dFz)
    dFz_max = max(list_dFz)
    Mphi_min = min(list_Mphi)
    Mphi_max = max(list_Mphi)
    Fz_ext_max = max(list_Fz_ext)
    Fz_int_min = min(list_Fz_int)

    analysis = f"""
================ ANÁLISE DE TENDÊNCIA (V4.0) ================

Variação do CG: {h_min:.1f} mm a {h_max:.1f} mm

1) Transferência de Carga (ΔFz):
   • De {dFz_min:.1f} N para {dFz_max:.1f} N
   • CONSEQUÊNCIA: Aumenta a diferença de carga entre os pneus.

2) Momento de Rolagem (Mφ):
   • De {Mphi_min:.1f} Nm para {Mphi_max:.1f} Nm
   • CONSEQUÊNCIA: Maior rolagem do veículo.

3) Cargas Verticais nas Rodas (Fz):
   • Carga Máxima Externa: {Fz_ext_max:.1f} N
   • Carga Mínima Interna: {Fz_int_min:.1f} N
   • CONCLUSÃO: A carga mínima interna é crítica. Se Fz_int_min for próximo de zero, o pneu interno está **levantando** ou perdendo aderência, o que é perigoso.

4) Momento na Manga (Msp):
   • O Momento na Manga (Msp) é **indireto** e depende da Força Lateral ($F_y$) e do Scrub Radius ($r_s$). Como $F_y$ é constante neste modelo, $M_{{sp}}$ é constante.
   • NOTA: Em modelos mais complexos, $F_y$ varia com a carga vertical, o que faria $M_{{sp}}$ variar.

================ RESUMO DE PROJETO =================
O aumento do CG prejudica a estabilidade ao aumentar ΔFz e Mφ, e ao reduzir a carga vertical no pneu interno.
"""
    text_analysis.insert(tk.END, analysis)


# ========= CÁLCULO PRINCIPAL =========
def calcular():
    try:
        # 1. Inputs
        try:
            h_min = float(entry_h_min.get()) / 1000
            h_max = float(entry_h_max.get()) / 1000
            passo = float(entry_h_step.get()) / 1000

            m = float(entry_massa.get())
            ay = float(entry_ay.get())
            track = float(entry_track.get()) / 1000
            hro = float(entry_hro.get()) / 1000

            # Scrub Radius
            rs = float(entry_rs.get()) / 1000  # Scrub Radius (m)

            # Vão livre (m)
            vao_livre = float(entry_vao_livre.get()) / 1000

            if h_min >= h_max or passo <= 0:
                raise ValueError("Erro nos valores de min/max/passo.")

        except ValueError:
            raise ValueError("Verifique se todos os campos são números válidos.")

        # 2. Reset
        list_h.clear()
        list_Mphi.clear()
        list_dFz.clear()
        list_Fz_ext.clear()
        list_Fz_int.clear()
        list_Msp.clear()
        tree.delete(*tree.get_children())

        # 3. Loop
        h = h_min
        while h <= h_max + 1e-9:

            # (1) Momento de Rolagem (Mphi)
            Mphi = m * ay * (h - hro)

            # (2) Transferência de Carga (dFz)
            dFz = m * ay * (h / track)

            # (3) Cargas Verticais nas Rodas
            Fz_base = (m * G) / 4
            Fz_ext = Fz_base + (dFz / 2)
            Fz_int = Fz_base - (dFz / 2)

            # (4) Momento na Manga (Msp = Fy * (rs + vão livre))
            Fy_roda = (m * ay) / 2
            lever_arm = rs + vao_livre  # braço efetivo incluindo vão livre
            Msp = Fy_roda * lever_arm

            # Guardar dados (h * 1000 para voltar a mm)
            list_h.append(h * 1000)
            list_Mphi.append(Mphi)
            list_dFz.append(dFz)
            list_Fz_ext.append(Fz_ext)
            list_Fz_int.append(Fz_int)
            list_Msp.append(Msp)

            # Tabela
            tree.insert(
                "",
                "end",
                values=(
                    round(h * 1000, 1),
                    round(Mphi, 2),
                    round(dFz, 2),
                    round(Fz_ext, 1),
                    round(Fz_int, 1),
                    round(Msp, 2),
                ),
            )

            h += passo

        # 4. Exibir resultados e plotar
        show_vectors()
        show_analysis()
        plot_analysis()
        messagebox.showinfo("Sucesso", "Cálculo e Gráfico gerados com sucesso!")

    except ValueError as ve:
        messagebox.showerror("Erro de Entrada", str(ve))
    except Exception as e:
        messagebox.showerror("Erro Inesperado", f"Ocorreu um erro: {e}")


# ========= INTERFACE TKINTER =========
root = tk.Tk()
root.title("Analisador Dinâmico – Tendência CG (v4.0 Completo)")
root.geometry("1200x800")

notebook = ttk.Notebook(root)
notebook.pack(fill="both", expand=True)

# ========= ABA 1: Cálculo & Geometria =========
frame1 = tk.Frame(notebook)
notebook.add(frame1, text="Cálculo & Geometria")

# Frame para Inputs
frame_inputs = tk.Frame(frame1)
frame_inputs.pack(pady=10)

labels = [
    "CG mínimo (mm):", "CG máximo (mm):", "Passo (mm):",
    "Massa Total (kg):", "Aceleração ay (m/s²):",
    "Track (Bitola) (mm):", "Roll Center hRo (mm):",
    "Scrub Radius rs (mm):", "Vão livre (mm):"
]

entries = []
for i, lab in enumerate(labels):
    tk.Label(frame_inputs, text=lab).grid(row=i, column=0, sticky="e", pady=3)
    ent = tk.Entry(frame_inputs)
    ent.grid(row=i, column=1, pady=3)
    entries.append(ent)

(entry_h_min, entry_h_max, entry_h_step,
 entry_massa, entry_ay, entry_track,
 entry_hro, entry_rs, entry_vao_livre) = entries

# Valores padrão
defaults = ["250", "450", "25", "300", "9.8", "1300", "100", "60", "0"]
for ent, val in zip(entries, defaults):
    ent.insert(0, val)

btn_calc = tk.Button(
    frame1,
    text="CALCULAR DINÂMICA",
    bg="#0055aa",
    fg="white",
    font=("Arial", 11, "bold"),
    command=calcular
)
btn_calc.pack(pady=5)

# Tabela
cols = ["CG (mm)", "M_Roll (Nm)", "ΔFz (N)", "Fz_ext (N)", "Fz_int (N)", "Msp (Nm)"]
tree = ttk.Treeview(frame1, columns=cols, show="headings", height=15)
for col in cols:
    tree.heading(col, text=col)
    tree.column(col, width=120, anchor="center")
tree.pack(pady=10)

# ========= ABA 2: Gráfico & Comparação =========
frame4 = tk.Frame(notebook)
notebook.add(frame4, text="Gráfico & Comparação")

# Frame de Controle (CG de Referência)
frame_control = tk.Frame(frame4)
frame_control.pack(pady=10)

tk.Label(
    frame_control,
    text="CG de Referência (mm):",
    font=("Arial", 10, "bold")
).pack(side=tk.LEFT, padx=5)

entry_h_ref = tk.Entry(frame_control, width=10)
entry_h_ref.insert(0, "350")
entry_h_ref.pack(side=tk.LEFT, padx=5)

btn_plot = tk.Button(frame_control, text="ATUALIZAR GRÁFICO", command=plot_analysis)
btn_plot.pack(side=tk.LEFT, padx=10)

# Frame para o Gráfico
frame_plot = tk.Frame(frame4)
frame_plot.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)

# ========= ABA 3: Vetores =========
frame2 = tk.Frame(notebook)
notebook.add(frame2, text="Dados Brutos")
text_vectors = tk.Text(frame2, font=("Courier", 10))
text_vectors.pack(fill="both", expand=True, padx=10, pady=10)

# ========= ABA 4: Análises do Comportamento =========
frame3 = tk.Frame(notebook)
notebook.add(frame3, text="Relatório Técnico")
text_analysis = tk.Text(frame3, font=("Arial", 11))
text_analysis.pack(fill="both", expand=True, padx=10, pady=10)

root.mainloop()
