import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";


const BOW_MODEL = "/models/miniGamesProps/Archery/Bow.glb";

const BALLOON_MODEL = "/models/miniGamesProps/Archery/Balloon.glb";

const ARROW_MODEL = "/models/miniGamesProps/Archery/Arrow.glb";

type ArcheryProps = {
    position: [number, number, number];
    rotation?: [number, number, number];
    scale?: number;
};

export function ArcheryProps({ position, rotation = [0, 0, 0], scale = 1 }: ArcheryProps) {
    const { scene: bowScene } = useGLTF(BOW_MODEL);

    const { scene: balloonScene } = useGLTF(BALLOON_MODEL);

    const { scene: arrowScene } = useGLTF(ARROW_MODEL);

    const bowModel = useMemo(() => bowScene.clone(), [bowScene]);

    const balloonModel = useMemo(() => balloonScene.clone(), [balloonScene]);

    const arrowModel = useMemo(() => arrowScene.clone(), [arrowScene]);

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <primitive object={bowModel} position={[0, 1, 0]} scale={0.2} rotation={[0, Math.PI, 0]} /> 
            <primitive object={arrowModel} position={[0, 1, 0]} scale={0.2} rotation={[0,Math.PI/2, 0]} />
            <primitive object={balloonModel} position={[-0.1, 1, 0]} scale={0.2} />

        </group>
    );
}


useGLTF.preload(BOW_MODEL);
useGLTF.preload(ARROW_MODEL);
useGLTF.preload(BALLOON_MODEL);
