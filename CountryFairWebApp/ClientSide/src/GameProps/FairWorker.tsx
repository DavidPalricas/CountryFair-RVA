import { useEffect, useMemo, useRef } from "react";
import { useAnimations, useGLTF } from "@react-three/drei";
import { LoopRepeat } from "three";
import type { Group } from "three";
import { clone as cloneSkinned } from "three/examples/jsm/utils/SkeletonUtils.js";

/** Expects public/models/CountryFairWorker.glb, with its animation clips. */
const FAIR_WORKER_MODEL = "/models/CountryFairWorker.glb";

// This model came out of Blender already in metres (2.3 tall), unlike the tent, which
// obj2gltf exported at 790. So it needs no normalisation.
const MODEL_SCALE = 1;

// Clips come from Blender prefixed with the armature name ("Armature|Idle"), which is why the
// lookup below also matches on the segment after the "|".
type FairWorkerAnimation = "Grounded" | "Idle" | "Jump" | "Sprint" | "Walk";

type FairWorkerProps = {
    /** Position in the parent's local space. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    scale?: number;
    /** Clip to loop; an unknown one warns and leaves the model in its bind pose. */
    animation?: FairWorkerAnimation;
    /** Playback rate of the clip. */
    animationSpeed?: number;
};


/**
 * Fairground attendant standing next to a tent. Loads the shared GLTF and loops one of its
 * clips, crossfading whenever the requested animation changes.
 */
export function FairWorker({
    position,
    rotation = [0, 0, 0],
    scale = 1,
    animation = "Idle",
    animationSpeed = 1,
}: FairWorkerProps) {
    const { scene, animations } = useGLTF(FAIR_WORKER_MODEL);
    // scene.clone() shares the skeleton between clones: every worker would be pinned to the
    // original's bones, in the same place. SkeletonUtils does the rebind.
    const model = useMemo(() => cloneSkinned(scene), [scene]);

    // The mixer has to point at this clone, otherwise it animates the original skeleton.
    const modelRoot = useRef<Group>(null);
    const { actions, names } = useAnimations(animations, modelRoot);

    useEffect(() => {
        const clipName = names.find((name) => name === animation || name.split("|").pop() === animation);
        const action = clipName ? actions[clipName] : undefined;

        if (!action) {
            console.warn(`FairWorker: animacao "${animation}" nao existe no modelo. Disponiveis: ${names.join(", ")}`);
            return;
        }

        action.reset().setLoop(LoopRepeat, Infinity).fadeIn(0.3).play();
        action.timeScale = animationSpeed;

        // fadeOut instead of stop() so the animation crossfades when it changes.
        return () => {
            action.fadeOut(0.3);
        };
    }, [actions, names, animation, animationSpeed]);

    return (
        <group ref={modelRoot} position={position} rotation={rotation} scale={scale}>
            <primitive object={model} scale={MODEL_SCALE} />
        </group>
    );
}

useGLTF.preload(FAIR_WORKER_MODEL);
