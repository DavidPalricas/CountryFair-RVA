import { useTexture } from "@react-three/drei";
import { RepeatWrapping, SRGBColorSpace } from "three";
import { useMemo } from "react";

type PlaneProps = {
    position: [number, number, number];
    rotation?: [number, number, number];
    size?: number;
    color?: string;
    texture?: string;
    textureRepeat?: number;
};

export function Plane({
    position,
    rotation = [0, 0, 0],
    size = 1,
    color,
    texture = "/textures/grass.jpg",
    textureRepeat = 10,
}: PlaneProps) {
    const planeHeight: number = 0.1;

    const map = useTexture(texture);

    useMemo(() => {
        map.wrapS = RepeatWrapping;
        map.wrapT = RepeatWrapping;
        map.repeat.set(textureRepeat, textureRepeat);
        map.colorSpace = SRGBColorSpace;
        map.needsUpdate = true;
    }, [map, textureRepeat]);

    return (
        <mesh position={position} rotation={rotation}>
            <boxGeometry args={[size, planeHeight, size]} />
            <meshStandardMaterial map={map} color={color} />
        </mesh>
    );
}
