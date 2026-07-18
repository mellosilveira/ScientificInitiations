"""Ponto de entrada recomendado para a aplicação."""

from main import App
import tkinter as tk


def main() -> None:
    root = tk.Tk()
    App(root)
    root.mainloop()


if __name__ == "__main__":
    main()
