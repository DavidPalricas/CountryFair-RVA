import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";


/** Expects public/models/BigTop.glb. */
const BIG_TOP_MODEL = "/models/BigTop.glb";

/*
  The model came out of the exporter 0.58 tall (y from -0.42 to 0.16). It is normalised to
  BASE_HEIGHT, the height a big tent needs so it reads as one next to the ferris wheel and the
  mini-game row; that way the `scale` prop stays a multiplier around 1, rather than a number
  that depends on the units the model happened to be exported in.
*/
const MODEL_HEIGHT = 0.58;
const BASE_HEIGHT = 4;
const MODEL_SCALE = BASE_HEIGHT / MODEL_HEIGHT;

/* The model pivot is not at the base but 0.42 above it; without this correction the tent sank
   into the ground in proportion to its scale. */
const PIVOT_TO_BASE = 0.42 * MODEL_SCALE;

type BigTopProps = {
    /** World-space position of the tent base, thanks to the PIVOT_TO_BASE correction. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    /** Multiplier on BASE_HEIGHT, so 1 means a 4-unit tall tent. */
    scale?: number;
};

/**
 * Decorative circus tent used to populate the fairground. It carries no pointer handlers, so
 * it stays out of R3F's interaction list and never interferes with the playable tents.
 */
export function BigTop({ position = [0, 0, 0], rotation = [0, 0, 0], scale = 1 }: BigTopProps) {
    const { scene } = useGLTF(BIG_TOP_MODEL);

    /* clone() shares geometry and materials with the original, so each extra tent costs a draw
       call and not a copy of the mesh on the GPU. */
    const model = useMemo(() => scene.clone(), [scene]);

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <primitive object={model} position={[0, PIVOT_TO_BASE, 0]} scale={MODEL_SCALE} />
        </group>
    );
}


useGLTF.preload(BIG_TOP_MODEL);
