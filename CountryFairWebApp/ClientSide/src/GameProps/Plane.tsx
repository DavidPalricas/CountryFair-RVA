import { useTexture } from "@react-three/drei";
import { RepeatWrapping, SRGBColorSpace } from "three";
import { useMemo } from "react";

type PlaneProps = {
    /** World-space position of the slab centre. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    /** Side length in world units; the slab is square. */
    size?: number;
    /** Optional tint multiplied into the texture. */
    color?: string;
    /** Path under public/, e.g. "/textures/grass.jpg". */
    texture?: string;
    /** Texture tiles per side; scale it with `size` or the grass stretches. */
    textureRepeat?: number;
};

/**
 * The fairground floor. It is a thin box rather than a plane so it stays visible from a
 * grazing angle and gives the props something with thickness to stand on.
 */
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

    // useTexture caches per path, so the returned texture is shared: it has to be configured
    // for tiling and re-flagged whenever the repeat count changes.
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
