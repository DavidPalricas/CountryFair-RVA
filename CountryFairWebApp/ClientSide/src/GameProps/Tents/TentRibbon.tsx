import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";
import {Text3D} from "../Text3D";


const RIBBON__MODEL = "/models/FairRibbon.glb";

// O modelo vem grande do Blender; este factor poe-o a escala da tenda.
const MODEL_SCALE = 0.8;

// O ribbon e quase plano: a bounding box do .glb vai de z = -0.030 a z = +0.0246.
// A rotacao de 180 graus poe a face -Z virada a camara, logo a frente fica a
// 0.030 * MODEL_SCALE = 0.024. O texto assenta logo a seguir, so com folga para
// evitar z-fighting. Manter o texto no plano do ribbon e o que garante que ele
// nunca se desalinha: como as tendas tem rotacoes diferentes e a camara e muito
// grande-angular (fov 120), qualquer offset em Z produzia paralaxe distinta em
// cada tenda, e o numero sairia do centro do disco umas vezes para a esquerda,
// outras para a direita.
const TEXT_Z = 0.03;

// Centro do disco: o topo do modelo esta em y = 0.5113 e o raio (metade da largura
// em X) e 0.5196, logo o centro cai em y ~ -0.008. Com o primitive em y = 1 da isto.
const TEXT_Y = 0.99;


type TentRibbon = {
    position: [number, number, number];
    rotation?: [number, number, number];
    scale?: number;
};

export function TentRibbon({ position, rotation = [0, 0, 0], scale = 1 }: TentRibbon) {
    const { scene: scene } = useGLTF(RIBBON__MODEL);


    const model = useMemo(() => scene.clone(), [scene]);


    return (
        <group position={position} rotation={rotation} scale={scale}>
      
            <primitive object={model} position={[0, 1, 0]} scale={MODEL_SCALE} rotation={[0, Math.PI, 0]} />

            {/* x = 0 porque o modelo ja esta centrado na sua origem (bounding box em X: -0.5196 a +0.5196). */}
            <Text3D position={[0, TEXT_Y, TEXT_Z]} fontSize={0.3} maxWidth={1.5} anchorX="center" anchorY="middle">
                1
            </Text3D>
        </group>
    );
}


useGLTF.preload(RIBBON__MODEL);
