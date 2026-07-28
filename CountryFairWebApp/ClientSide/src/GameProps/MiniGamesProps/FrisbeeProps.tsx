import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";
import { clone as skeletonClone } from "three/examples/jsm/utils/SkeletonUtils.js";

const DOG_MODEL = "/models/miniGamesProps/Frisbee/Dog.glb";

const FRISBEE_MODEL = "/models/miniGamesProps/Frisbee/Frisbee.glb";

type FrisbeeProps = {
    position: [number, number, number];
    rotation?: [number, number, number];
    scale?: [number, number, number];
};

export function FrisbeeProps({ position, rotation = [0, 0, 0], scale = [1, 1, 1] }: FrisbeeProps) {
    const { scene: dogScene } = useGLTF(DOG_MODEL);
    const { scene: frisbeeScene } = useGLTF(FRISBEE_MODEL);

    // O cão tem esqueleto: scene.clone() não faz rebind dos bones e todas as
    // cópias colapsavam na mesma posição. SkeletonUtils.clone resolve isso.
    const dog = useMemo(() => skeletonClone(dogScene), [dogScene]);
    const frisbee = useMemo(() => frisbeeScene.clone(), [frisbeeScene]);

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <primitive object={dog} position={[0, 0.1, 0]} scale={0.13} rotation={[0, -Math.PI/1.5, 0]} />
            <primitive object={frisbee} position={[-0.18, 0.35, 0.25]} scale={0.03} />
        </group>
    );
}


useGLTF.preload(DOG_MODEL);
useGLTF.preload(FRISBEE_MODEL);
