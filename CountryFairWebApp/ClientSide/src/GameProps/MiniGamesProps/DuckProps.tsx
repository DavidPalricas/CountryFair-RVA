import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";
import { HoppingProp } from "./HoppingProp";


/** Expects public/models/miniGamesProps/DuckGame/Pool.glb. */
const POOL_MODEL = "/models/miniGamesProps/DuckGame/Pool.glb";

/** Expects public/models/miniGamesProps/DuckGame/Duck.glb. */
const DUCK_MODEL = "/models/miniGamesProps/DuckGame/Duck.glb";



type DuckProps = {
    /** Position in the parent tent's local space. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    scale?: number;
};

/**
 * Display of the duck mini-game in front of its tent: a pool with two ducks bobbing in it.
 * The model has no animation clips, so the motion comes from `HoppingProp`.
 */
export function DuckProps({ position, rotation = [0, 0, 0], scale = 1 }: DuckProps) {
    const { scene: poolScene } = useGLTF(POOL_MODEL);

    const { scene: duckScene } = useGLTF(DUCK_MODEL);

    const poolModel = useMemo(() => poolScene.clone(), [poolScene]);

    // One clone per duck: a single Object3D cannot sit at two places in the scene graph.
    const duckModel1 = useMemo(() => duckScene.clone(), [duckScene]);
    const duckModel2 = useMemo(() => duckScene.clone(), [duckScene]);

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <primitive object={poolModel} position={[0, 0, 0]} scale={0.2} />

            {/* Both ducks share the same hop but with different phases and rates, otherwise
                they hopped in mirror image and read as a single movement. */}
            <HoppingProp
                object={duckModel1}
                position={[-0.1, 0.13, 0]}
                rotation={[0, Math.PI, 0]}
                scale={0.04}
                speed={0.75}
                height={0.045}
                phase={0}
            />
            <HoppingProp
                object={duckModel2}
                position={[0.1, 0.13, 0]}
                rotation={[0, Math.PI / 2, 0]}
                scale={0.04}
                speed={0.6}
                height={0.035}
                phase={0.45}
            />

        </group>
    );
}


useGLTF.preload(POOL_MODEL);
useGLTF.preload(DUCK_MODEL);
