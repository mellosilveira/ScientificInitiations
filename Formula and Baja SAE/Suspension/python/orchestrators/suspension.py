from models.primitives import Point3D
from models.component_structures import ThreePointArm, TwoPointLink3D
from models.suspension import Suspension
import math

class SuspensionOrchestrator:
    """
    Atua como ponte entre a Interface Gráfica (UI) e os modelos de domínio 3D.
    Recebe dicionários de dados brutos da UI, converte para entidades 3D e 
    retorna resultados formatados prontos para exibição.
    """

    @staticmethod
    def build_suspension_from_ui(hp_dict: dict, corner: str, tire_diam: float = 584.2) -> Suspension:
        """
        Constrói o modelo 3D da suspensão a partir do dicionário de hardpoints da UI.
        Nota: A UI precisará ser atualizada para enviar pontos 'Fore' e 'Aft' para os braços.
        """
        
        def extract_point3d_from_ui_data(point_name: str) -> Point3D:
            """
            Busca as coordenadas X, Y, Z de um ponto específico no dicionário da UI
            e o converte em um objeto Point3D do nosso domínio.
            """
            key = f"{corner} {point_name}"
            # Se o ponto não existir no dict da UI, retorna origem para evitar quebra imediata
            x, y, z = hp_dict.get(key, (0.0, 0.0, 0.0)) 
            return Point3D(float(x), float(y), float(z))

        # Construção dos braços tridimensionais
        upper = ThreePointArm(
            fore_inner=extract_point3d_from_ui_data("Sup Fore In"),
            aft_inner=extract_point3d_from_ui_data("Sup Aft In"),
            outer=extract_point3d_from_ui_data("Sup Out")
        )

        lower = ThreePointArm(
            fore_inner=extract_point3d_from_ui_data("Inf Fore In"),
            aft_inner=extract_point3d_from_ui_data("Inf Aft In"),
            outer=extract_point3d_from_ui_data("Inf Out")
        )

        tie_rod = TwoPointLink3D(
            inner=extract_point3d_from_ui_data("Toe In"),
            outer=extract_point3d_from_ui_data("Toe Out")
        )

        damper = TwoPointLink3D(
            inner=extract_point3d_from_ui_data("Damper In"),
            outer=extract_point3d_from_ui_data("Damper Out")
        )

        tire_contact = extract_point3d_from_ui_data("Tire Patch")

        # Parâmetros adicionais que a UI deve fornecer ou que serão calculados depois
        camber_gap = hp_dict.get(f"{corner} Camber Gap", 0.0)
        toe_distance = hp_dict.get(f"{corner} Toe Dist", 0.0)

        return Suspension(
            upper_arm=upper,
            lower_arm=lower,
            tie_rod=tie_rod,
            damper=damper,
            tire_contact=tire_contact,
            tire_diameter=tire_diam,
            camber_gap=camber_gap,
            toe_distance=toe_distance,
            acceleration_on_shaft=False,
            brake_on_shaft=True
        )

    @staticmethod
    def get_static_alignment(sus: Suspension) -> dict:
        """
        Retorna os ângulos estáticos baseados na geometria atual.
        """
        # 1. Caster (Plano XZ) - Usando os pontos externos dos braços
        dx = sus.upper_arm.outer.x - sus.lower_arm.outer.x
        dz = sus.upper_arm.outer.z - sus.lower_arm.outer.z
        
        # Evitar divisão por zero se a manga de eixo for perfeitamente vertical
        caster_rad = math.atan2(dx, dz) if abs(dz) > 1e-6 else 0.0 
        
        # 2. Camber (Baseado no gap e diâmetro do pneu)
        camber_rad = math.asin(sus.camber_gap / sus.tire_diameter) if sus.tire_diameter > 0 else 0.0

        # 3. Toe (Aproximação angular simples baseada na distância)
        toe_rad = math.asin(sus.toe_distance / sus.tire_diameter) if sus.tire_diameter > 0 else 0.0

        return {
            "Caster (deg)": math.degrees(caster_rad),
            "Camber (deg)": math.degrees(camber_rad),
            "Toe (deg)": math.degrees(toe_rad)
        }