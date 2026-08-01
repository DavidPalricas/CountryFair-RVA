import { useEffect, useMemo, useRef } from "react";
import { useAnimations, useGLTF } from "@react-three/drei";
import { LoopRepeat } from "three";
import type { Group } from "three";
import { clone as cloneSkinned } from "three/examples/jsm/utils/SkeletonUtils.js";

/** Expects public/models/FerrisWheel.glb, with its rotation clips baked in. */
const Ferris_WHEEL_MODEL = "/models/FerrisWheel.glb";

const MODEL_SCALE = 1;

type FerrisWheelProps = {
    /** World-space position of the wheel base. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    scale?: number;
    /** Playback rate of every clip; below 1 the wheel turns slowly, as a real one does. */
    animationSpeed?: number;
};


/**
 * The fair's landmark at the back of the scene, turning continuously.
 *
 * Note it takes up a large footprint on the ground — see the BIG_TOPS layout notes in
 * `GameScreen`, where no tent is allowed to enter the box it occupies.
 */
export function FerrisWheel({
    position,
    rotation = [0, 0, 0],
    scale = 1,
    animationSpeed = 1,
}: FerrisWheelProps) {
    const { scene, animations } = useGLTF(Ferris_WHEEL_MODEL);
    // scene.clone() shares the skeleton between clones: every instance would be pinned to the
    // original's bones, in the same place. SkeletonUtils does the rebind.
    const model = useMemo(() => cloneSkinned(scene), [scene]);

    // The mixer has to point at this clone, otherwise it animates the original model.
    const modelRoot = useRef<Group>(null);
    const { actions, names } = useAnimations(animations, modelRoot);

    useEffect(() => {
        // The model ships one rotation clip per part (the wheel, plus each cabin, which
        // counter-rotates to stay upright), so they all play simultaneously.
        const playing = names
            .map((name) => actions[name])
            .filter((action) => action !== null && action !== undefined);

        for (const action of playing) {
            action.reset().setLoop(LoopRepeat, Infinity).play();
            action.timeScale = animationSpeed;
        }

        return () => {
            for (const action of playing) {
                action.stop();
            }
        };
    }, [actions, names, animationSpeed]);

    return (
        <group ref={modelRoot} position={position} rotation={rotation} scale={scale}>
            <primitive object={model} scale={MODEL_SCALE} />
        </group>
    );
}

useGLTF.preload(Ferris_WHEEL_MODEL);
