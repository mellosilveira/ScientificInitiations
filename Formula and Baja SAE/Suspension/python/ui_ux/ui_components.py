import tkinter as tk
from tkinter import ttk

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
            header, text=title, variable=self.var_open,
            command=self._toggle, style="Section.TCheckbutton"
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

def setup_style():
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