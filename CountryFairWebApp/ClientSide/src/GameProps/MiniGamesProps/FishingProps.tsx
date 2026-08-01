import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";
import { HoppingProp } from "./HoppingProp";


/** Expects public/models/miniGamesProps/Fishing/Bucket.glb. */
const BUCKET_MODEL = "/models/miniGamesProps/Fishing/Bucket.glb";

/** Expects public/models/miniGamesProps/Fishing/FishRoad.glb. */
const FISH_ROAD_MODEL = "/models/miniGamesProps/Fishing/FishRoad.glb";

/** Expects public/models/miniGamesProps/Fishing/RedLionFish.glb. */
const RED_LION_FISH_MODEL = "/models/miniGamesProps/Fishing/RedLionFish.glb";

type FishingProps = {
    /** Position in the parent tent's local space. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    scale?: number;
};

/**
 * Display of the fishing mini-game in front of its tent: rod, bucket and a fish flapping
 * above it. The fish motion comes from `HoppingProp`, tuned differently from the ducks.
 */
export function FishingProps({ position, rotation = [0, 0, 0], scale = 1 }: FishingProps) {
    const { scene: bucketScene } = useGLTF(BUCKET_MODEL);

    const { scene: fishRoadScene } = useGLTF(FISH_ROAD_MODEL);

    const { scene: redLionFishScene } = useGLTF(RED_LION_FISH_MODEL);

    // Cloned so each tent gets its own node tree while sharing geometry and materials with the
    // cached original.
    const bucketModel = useMemo(() => bucketScene.clone(), [bucketScene]);

    const fishRoadModel = useMemo(() => fishRoadScene.clone(), [fishRoadScene]);

    const redLionFishModel = useMemo(() => redLionFishScene.clone(), [redLionFishScene]);

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <primitive object={bucketModel} position={[1, 0, 0]} scale={0.18} rotation={[0, 0, 0]} />
            <primitive object={fishRoadModel} position={[0.8, 0, 0]} scale={0.2} rotation={[0,-Math.PI/2, 0]} />

            {/* The fish hops higher and faster than the ducks, with little time resting, so it
                reads as a fish thrashing out of water. */}
            <HoppingProp
                object={redLionFishModel}
                position={[1, 0.12, 0]}
                rotation={[0, Math.PI, 0]}
                scale={0.045}
                speed={1.1}
                height={0.07}
                airTime={0.75}
                tilt={0.4}
                sway={0.14}
            />

        </group>
    );
}

useGLTF.preload(BUCKET_MODEL);
useGLTF.preload(RED_LION_FISH_MODEL);
useGLTF.preload(FISH_ROAD_MODEL);
