import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";
import {Text3D} from "../Text3D";


/** Expects public/models/FairRibbon.glb. */
const RIBBON__MODEL = "/models/FairRibbon.glb";

// The model comes out of Blender oversized; this factor brings it to tent scale.
const MODEL_SCALE = 0.8;

// The ribbon is almost flat: the .glb bounding box runs from z = -0.030 to z = +0.0246.
// The 180 degree rotation turns the -Z face towards the camera, so the front lands at
// 0.030 * MODEL_SCALE = 0.024. The text sits just beyond it, with only enough clearance to
// avoid z-fighting. Keeping the text in the ribbon's plane is what guarantees it never drifts
// out of alignment: because the tents carry different rotations and the camera is very wide
// angle (fov 120), any Z offset produced different parallax per tent, and the number would
// slide off the centre of the disc — sometimes left, sometimes right.
const TEXT_Z = 0.03;

// Centre of the disc: the top of the model is at y = 0.5113 and the radius (half the width in
// X) is 0.5196, so the centre falls at y ~ -0.008. With the primitive at y = 1 that gives this.
const TEXT_Y = 0.99;


type TentRibbon = {
    /** Position in the parent tent's local space. */
    position: [number, number, number];
    /** Number of the slot the tent occupies; changes when the tents swap order. */
    number: number;
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    /** Uniform multiplier applied on top of MODEL_SCALE. */
    scale?: number;
};

/** Ribbon badge hanging on a tent, showing the number of the slot the tent currently occupies. */
export function TentRibbon({ position, number, rotation = [0, 0, 0], scale = 1 }: TentRibbon) {
    const { scene: scene } = useGLTF(RIBBON__MODEL);

    // Cloning shares geometry and materials with the cached original: one ribbon per tent
    // costs a draw call, not another mesh upload.
    const model = useMemo(() => scene.clone(), [scene]);

    return (
        <group position={position} rotation={rotation} scale={scale}>

            <primitive object={model} position={[0, 1, 0]} scale={MODEL_SCALE} rotation={[0, Math.PI, 0]} />

            {/* x = 0 because the model is already centred on its origin (X bounding box: -0.5196 to +0.5196). */}
            <Text3D position={[0, TEXT_Y, TEXT_Z]} fontSize={0.3} maxWidth={1.5} anchorX="center" anchorY="middle">
                {String(number)}
            </Text3D>
        </group>
    );
}


useGLTF.preload(RIBBON__MODEL);
