# Relatório de refatoração

## Correções realizadas

- Corrigida a importação inexistente `orchestrators.Alinhament`.
- Padronizados os módulos de orquestração para nomes minúsculos e descritivos:
  - `Alignment.py` → `kinematics.py`
  - `Cinematic.py` → `vehicle.py`
  - `Hardpoin.py` → `serialization.py`
- Padronizada a classe `VehycleParameters` para `VehicleParameters`.
- Mantido um alias temporário `VehycleParameters` para compatibilidade com código antigo.
- Criados `__init__.py` em `models`, `calculators`, `orchestrators` e `ui_ux`.
- Criada uma fachada pública `orchestrators.Orchestrators`.
- Removida a fachada duplicada do módulo de serialização.
- Renomeado `Teste matriz estatico.py` para `static_matrix_test.py`.
- Removidos diretórios gerados (`__pycache__`) e arquivos do PyCharm (`.idea`).
- Criado `requirements.txt`.
- Criado `run.py` como ponto de entrada recomendado.
- Criado `README.md` com instalação, execução e organização.

## Validações executadas

- Compilação de todos os arquivos com `python -m compileall`.
- Importação de todos os pacotes e módulos principais.
- Importação de `main.py` e `run.py` sem inicializar a janela.
- Teste funcional básico dos modelos, alinhamento, fachada e serialização.

## Observações

- A abertura efetiva da janela Tkinter exige um computador com ambiente gráfico.
- O módulo `KinematicsOrchestrator` ainda depende de um solver cinemático no modelo `Suspension` (`clone/apply_bump` ou `solve_kinematics`) quando utilizado diretamente. A interface principal usa a varredura implementada em `SuspensionOrchestrator`.
- `models/math_2d.py` foi mantido por compatibilidade, embora exista também `calculators/math_2d.py`. Recomenda-se consolidar esses dois módulos em uma futura revisão funcional, após confirmar quais chamadas externas ainda dependem do módulo legado.

## Correção 3D — centro instantâneo longitudinal

- Corrigida a exceção `'NoneType' object has no attribute 'x'` durante **Análise 3D**.
- A causa era a interseção dos eixos internos dos braços superior e inferior, que normalmente são paralelos e retornavam `None`.
- O centro instantâneo longitudinal passou a ser calculado corretamente na vista lateral (plano Y-Z), usando as linhas entre os pivôs internos médios e os pivôs externos.
- Geometrias paralelas ou degeneradas agora retornam resultado neutro sem interromper a aplicação.
- Testado nos quatro cantos com os hardpoints padrão.
