import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";


const POOL_MODEL = "/models/miniGamesProps/DuckGame/Pool.glb";

const DUCK_MODEL = "/models/miniGamesProps/DuckGame/Duck.glb";



type DuckProps = {
    position: [number, number, number];
    rotation?: [number, number, number];
    scale?: number;
};

export function DuckProps({ position, rotation = [0, 0, 0], scale = 1 }: DuckProps) {
    const { scene: poolScene } = useGLTF(POOL_MODEL);

    const { scene: duckScene } = useGLTF(DUCK_MODEL);

    const poolModel = useMemo(() => poolScene.clone(), [poolScene]);

    const duckModel1 = useMemo(() => duckScene.clone(), [duckScene]);
    const duckModel2 = useMemo(() => duckScene.clone(), [duckScene]);

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <primitive object={poolModel} position={[0, 0, 0]} scale={0.2} /> 
            <primitive object={duckModel1} position={[-0.1, 0.13, 0]} scale={0.04} rotation={[0, Math.PI, 0] }/>
            <primitive object={duckModel2} position={[0.1, 0.13, 0]} scale={0.04} rotation={[0, Math.PI/2, 0] }/>

        </group>
    );
}


useGLTF.preload(POOL_MODEL);
useGLTF.preload(DUCK_MODEL);
