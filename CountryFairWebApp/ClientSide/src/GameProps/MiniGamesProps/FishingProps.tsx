import { useMemo } from "react";
import { useGLTF } from "@react-three/drei";


const BUCKET_MODEL = "/models/miniGamesProps/Fishing/Bucket.glb";

const FISH_ROAD_MODEL = "/models/miniGamesProps/Fishing/FishRoad.glb";

const RED_LION_FISH_MODEL = "/models/miniGamesProps/Fishing/RedLionFish.glb";

type FishingProps = {
    position: [number, number, number];
    rotation?: [number, number, number];
    scale?: number;
};

export function FishingProps({ position, rotation = [0, 0, 0], scale = 1 }: FishingProps) {
    const { scene: bucketScene } = useGLTF(BUCKET_MODEL);

    const { scene: fishRoadScene } = useGLTF(FISH_ROAD_MODEL);

    const { scene: redLionFishScene } = useGLTF(RED_LION_FISH_MODEL);

    const bucketModel = useMemo(() => bucketScene.clone(), [bucketScene]);

    const fishRoadModel = useMemo(() => fishRoadScene.clone(), [fishRoadScene]);

    const redLionFishModel = useMemo(() => redLionFishScene.clone(), [redLionFishScene]);

    return (
        <group position={position} rotation={rotation} scale={scale}>
            <primitive object={bucketModel} position={[0, 0, 0]} scale={0.18} rotation={[0, 0, 0]} /> 
            <primitive object={fishRoadModel} position={[-0.1, 0, 0]} scale={0.2} rotation={[0,-Math.PI/2, 0]} />
            <primitive object={redLionFishModel } position={[0, 0.12, 0]} scale={0.05} rotation={[0,Math.PI, 0]} />

        </group>
    );
}


useGLTF.preload(BUCKET_MODEL);
useGLTF.preload(RED_LION_FISH_MODEL);
useGLTF.preload(FISH_ROAD_MODEL);
