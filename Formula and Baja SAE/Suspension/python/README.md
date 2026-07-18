# Simas Turbo — análise de suspensão

Aplicação Python/Tkinter para análise geométrica de suspensões Formula e Baja SAE.

## Instalação

```bash
python -m venv .venv
# Windows
.venv\Scripts\activate
# Linux/macOS
source .venv/bin/activate

pip install -r requirements.txt
```

## Execução

```bash
python run.py
```

Também é possível executar `python main.py`.

## Organização

- `models/`: estruturas de domínio e primitivas geométricas.
- `calculators/`: fórmulas, parâmetros de suspensão e solucionadores.
- `orchestrators/`: coordenação dos casos de uso.
- `ui_ux/`: componentes e visualizações da interface.
- `main.py`: aplicação Tkinter legada principal.
- `run.py`: ponto de entrada recomendado.
- `static_matrix_test.py`: protótipo/teste independente da matriz estática.

## Orquestradores públicos

- `SuspensionOrchestrator`: construção e análises da suspensão.
- `AlignmentOrchestrator`: alinhamento estático.
- `KinematicsOrchestrator`: varredura de alinhamento por curso.
- `VehicleOrchestrator`: montagem e resumo dos quatro cantos.
- `SerializationOrchestrator`: serialização de hardpoints e presets.

## Observação

A interface requer ambiente gráfico. Em servidor sem display, os módulos podem ser importados e testados, mas a janela Tkinter não será aberta.
