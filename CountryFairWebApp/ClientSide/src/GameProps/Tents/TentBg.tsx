type TentBgProps = {
    /** Position in the parent tent's local space. */
    position: [number, number, number];
    /** Euler rotation in radians. */
    rotation?: [number, number, number];
    /** Box size as [width, height, depth], in the parent's units. */
    dimensions?: [number, number, number];

};


/**
 * Opaque black box placed inside a tent so the interior reads as dark depth instead of
 * showing the scene through the open front of the model.
 */
export function TentBg({ position, rotation = [0, 0, 0], dimensions = [1, 1, 1] }: TentBgProps) {
    return (
        <mesh position={position} rotation={rotation} scale={1}>
            <boxGeometry args={dimensions} />
            <meshStandardMaterial color="black" />
        </mesh>

    );
}
