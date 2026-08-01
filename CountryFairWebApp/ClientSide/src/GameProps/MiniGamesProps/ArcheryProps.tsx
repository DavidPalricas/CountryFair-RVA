import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";


/** Expects public/models/miniGamesProps/Archery/Bow.glb. */
const BOW_MODEL = "/models/miniGamesProps/Archery/Bow.glb";

/** Expects public/models/miniGamesProps/Archery/Balloon.glb. */
const BALLOON_MODEL = "/models/miniGamesProps/Archery/Balloon.glb";

/** Expects public/models/miniGamesProps/Archery/Arrow.glb. */
const ARROW_MODEL = "/models/miniGamesProps/Archery/Arrow.glb";

type ArcheryProps = {
    /** Position in the parent tent's local space. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    scale?: number;
};

/**
 * Static display of the archery mini-game in front of its tent: bow, arrow and a balloon
 * target, so the tent is recognisable without any label.
 */
export function ArcheryProps({ position, rotation = [0, 0, 0], scale = 1 }: ArcheryProps) {
    const { scene: bowScene } = useGLTF(BOW_MODEL);

    const { scene: balloonScene } = useGLTF(BALLOON_MODEL);

    const { scene: arrowScene } = useGLTF(ARROW_MODEL);

    // Cloned so each tent gets its own node tree while sharing geometry and materials with the
    // cached original.
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
